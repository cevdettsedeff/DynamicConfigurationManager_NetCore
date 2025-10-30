// ConfigurationReader.Library/ConfigurationReader.cs
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using Npgsql;
using StackExchange.Redis;

namespace ConfigurationReader.Library;

public class ConfigurationReader : IConfigurationReader, IDisposable
{
    private readonly ConcurrentDictionary<string, string> _cache = new();
    private readonly IDatabase? _redisDb;
    private readonly string _connectionString;
    private readonly string _applicationName;
    private readonly TimeSpan _cacheExpiration;
    private readonly Timer _refreshTimer;
    private readonly bool _enableLogging;

    // 3 Parametreli Constructor (Minimalist)
    public ConfigurationReader(
        string applicationName,
        string connectionString,
        int refreshTimerIntervalInMs)
    {
        if (string.IsNullOrEmpty(applicationName))
            throw new ArgumentException("ApplicationName cannot be null or empty", nameof(applicationName));

        if (string.IsNullOrEmpty(connectionString))
            throw new ArgumentException("ConnectionString cannot be null or empty", nameof(connectionString));

        if (refreshTimerIntervalInMs <= 0)
            throw new ArgumentException("RefreshTimerIntervalInMs must be greater than 0", nameof(refreshTimerIntervalInMs));

        _applicationName = applicationName;
        _connectionString = connectionString;
        _cacheExpiration = TimeSpan.FromMinutes(5);
        _enableLogging = false;

        RefreshAsync().Wait();

        var interval = TimeSpan.FromMilliseconds(refreshTimerIntervalInMs);
        _refreshTimer = new Timer(
            async _ => await RefreshAsync(),
            null,
            interval,
            interval
        );

        Log($"ConfigurationReader initialized for '{_applicationName}' (Refresh: {refreshTimerIntervalInMs}ms)");
    }

    // Options ile Constructor (DI için)
    public ConfigurationReader(ConfigurationReaderOptions options)
    {
        if (string.IsNullOrEmpty(options.ApplicationName))
            throw new ArgumentException("ApplicationName is required", nameof(options));

        if (string.IsNullOrEmpty(options.ConnectionString))
            throw new ArgumentException("ConnectionString is required", nameof(options));

        _connectionString = options.ConnectionString;
        _applicationName = options.ApplicationName;
        _cacheExpiration = TimeSpan.FromMinutes(options.CacheExpirationMinutes);
        _enableLogging = options.EnableLogging;

        if (!string.IsNullOrEmpty(options.RedisConnectionString))
        {
            try
            {
                var redis = ConnectionMultiplexer.Connect(options.RedisConnectionString);
                _redisDb = redis.GetDatabase();
                Log("Redis connected successfully");
            }
            catch (Exception ex)
            {
                Log($"Redis connection failed: {ex.Message}");
            }
        }

        RefreshAsync().Wait();

        var interval = TimeSpan.FromMilliseconds(options.RefreshTimerIntervalInMs);
        _refreshTimer = new Timer(
            async _ => await RefreshAsync(),
            null,
            interval,
            interval
        );

        Log($"ConfigurationReader initialized for '{_applicationName}' (Refresh: {options.RefreshTimerIntervalInMs}ms)");
    }

    // ✅ GetValue<T>(string key)
    public T GetValue<T>(string key)
    {
        if (_cache.TryGetValue(key, out var cachedValue))
        {
            Log($"[MEMORY HIT] {key}");
            return ConvertValue<T>(cachedValue);
        }

        if (_redisDb != null)
        {
            try
            {
                var redisKey = GetRedisKey(key);
                var redisValue = _redisDb.StringGet(redisKey);

                if (redisValue.HasValue)
                {
                    Log($"[REDIS HIT] {key}");
                    _cache.TryAdd(key, redisValue!);
                    return ConvertValue<T>(redisValue!);
                }
            }
            catch (Exception ex)
            {
                Log($"Redis read error: {ex.Message}");
            }
        }

        Log($"[DATABASE HIT] {key}");
        var dbValue = GetFromDatabase(key);

        _cache.TryAdd(key, dbValue);

        if (_redisDb != null)
        {
            try
            {
                _redisDb.StringSet(GetRedisKey(key), dbValue, _cacheExpiration);
            }
            catch (Exception ex)
            {
                Log($"Redis write error: {ex.Message}");
            }
        }

        return ConvertValue<T>(dbValue);
    }

    // ✅ GetValue<T>(string key, T defaultValue) - YENİ!
    public T GetValue<T>(string key, T defaultValue)
    {
        try
        {
            return GetValue<T>(key);
        }
        catch (KeyNotFoundException)
        {
            Log($"Configuration '{key}' not found for application '{_applicationName}', using default value: {defaultValue}");
            return defaultValue;
        }
        catch (Exception ex)
        {
            Log($"Error reading '{key}': {ex.Message}, using default value: {defaultValue}");
            return defaultValue;
        }
    }

    // ✅ GetValueAsync<T>(string key)
    public async Task<T> GetValueAsync<T>(string key)
    {
        return await Task.Run(() => GetValue<T>(key));
    }

    // ✅ GetValueAsync<T>(string key, T defaultValue) - YENİ!
    public async Task<T> GetValueAsync<T>(string key, T defaultValue)
    {
        return await Task.Run(() => GetValue(key, defaultValue));
    }

    // ✅ TryGetValue<T>
    public bool TryGetValue<T>(string key, out T value)
    {
        try
        {
            value = GetValue<T>(key);
            return true;
        }
        catch
        {
            value = default!;
            return false;
        }
    }

    // ✅ RefreshAsync
    public async Task RefreshAsync()
    {
        try
        {
            Log("Refreshing configurations from database...");

            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            // ✅ SADECE BU APPLICATION'IN CONFIG'LERİNİ ÇEK!
            var sql = @"
                SELECT ""Name"", ""Value"" 
                FROM ""ConfigurationItems"" 
                WHERE ""ApplicationName"" = @ApplicationName 
                  AND ""IsActive"" = true";

            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("ApplicationName", _applicationName);

            using var reader = await command.ExecuteReaderAsync();

            var newCache = new ConcurrentDictionary<string, string>();

            while (await reader.ReadAsync())
            {
                var name = reader.GetString(0);
                var value = reader.GetString(1);

                newCache.TryAdd(name, value);

                if (_redisDb != null)
                {
                    try
                    {
                        await _redisDb.StringSetAsync(GetRedisKey(name), value, _cacheExpiration);
                    }
                    catch (Exception ex)
                    {
                        Log($"Redis write error for key '{name}': {ex.Message}");
                    }
                }
            }

            _cache.Clear();
            foreach (var kvp in newCache)
            {
                _cache.TryAdd(kvp.Key, kvp.Value);
            }

            Log($"Successfully refreshed {_cache.Count} configurations for application '{_applicationName}'");
        }
        catch (Exception ex)
        {
            Log($"Refresh error: {ex.Message}");
            throw;
        }
    }

    // ✅ GetAll
    public IReadOnlyDictionary<string, string> GetAll()
    {
        Log($"Getting all configurations for application '{_applicationName}'");
        return new ReadOnlyDictionary<string, string>(_cache);
    }

    private string GetFromDatabase(string key)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        // ✅ SADECE BU APPLICATION'IN CONFIG'İNİ ÇEK!
        var sql = @"
            SELECT ""Value"" 
            FROM ""ConfigurationItems"" 
            WHERE ""ApplicationName"" = @ApplicationName 
              AND ""Name"" = @Name 
              AND ""IsActive"" = true";

        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("ApplicationName", _applicationName);
        command.Parameters.AddWithValue("Name", key);

        var result = command.ExecuteScalar();

        if (result == null)
            throw new KeyNotFoundException(
                $"Configuration '{key}' not found for application '{_applicationName}'");

        return result.ToString()!;
    }

    private string GetRedisKey(string key)
    {
        // ✅ Redis key'e application name ekle (collision prevention)
        return $"config:{_applicationName}:{key}";
    }

    private T ConvertValue<T>(string value)
    {
        var targetType = typeof(T);

        if (targetType == typeof(string))
            return (T)(object)value;

        if (targetType == typeof(int))
            return (T)(object)int.Parse(value);

        if (targetType == typeof(bool))
            return (T)(object)bool.Parse(value);

        if (targetType == typeof(double))
            return (T)(object)double.Parse(value);

        if (targetType == typeof(long))
            return (T)(object)long.Parse(value);

        if (targetType == typeof(decimal))
            return (T)(object)decimal.Parse(value);

        throw new NotSupportedException($"Type {targetType.Name} is not supported");
    }

    private void Log(string message)
    {
        if (_enableLogging)
        {
            Console.WriteLine($"[ConfigurationReader] {message}");
        }
    }

    public void Dispose()
    {
        _refreshTimer?.Dispose();
    }
}