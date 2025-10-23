// ConfigurationReader.Library/ConfigurationReader.cs
using ConfigurationReader.Library.Models;
using Microsoft.Extensions.Logging;
using Npgsql;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Text.Json;

namespace ConfigurationReader.Library;

public class ConfigurationReader : IConfigurationReader, IDisposable
{
    private readonly ConfigurationReaderOptions _options;
    private readonly ILogger<ConfigurationReader>? _logger;
    private readonly ConcurrentDictionary<string, string> _cache;
    private readonly Timer? _refreshTimer;
    private readonly IConnectionMultiplexer? _redis;
    private readonly IDatabase? _redisDb;
    private bool _disposed;

    public ConfigurationReader(ConfigurationReaderOptions options, ILogger<ConfigurationReader>? logger = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;
        _cache = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // Redis connection (optional)
        if (!string.IsNullOrWhiteSpace(_options.RedisConnectionString))
        {
            try
            {
                _redis = ConnectionMultiplexer.Connect(_options.RedisConnectionString);
                _redisDb = _redis.GetDatabase();
                _logger?.LogInformation("Redis connection established");
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to connect to Redis, continuing without cache");
            }
        }

        // Initial load
        _ = Task.Run(() => RefreshAsync());

        // Auto-refresh timer
        if (_options.RefreshIntervalSeconds > 0)
        {
            _refreshTimer = new Timer(
                async _ => await RefreshAsync(),
                null,
                TimeSpan.FromSeconds(_options.RefreshIntervalSeconds),
                TimeSpan.FromSeconds(_options.RefreshIntervalSeconds));

            _logger?.LogInformation(
                "Auto-refresh enabled: every {Interval} seconds",
                _options.RefreshIntervalSeconds);
        }
    }

    public T GetValue<T>(string key)
    {
        return GetValueAsync<T>(key).GetAwaiter().GetResult();
    }

    public T GetValue<T>(string key, T defaultValue)
    {
        return GetValueAsync(key, defaultValue).GetAwaiter().GetResult();
    }

    public async Task<T> GetValueAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or empty", nameof(key));

        // Try in-memory cache first
        if (_cache.TryGetValue(key, out var cachedValue))
        {
            return ConvertValue<T>(cachedValue);
        }

        // Try Redis cache
        if (_redisDb != null)
        {
            try
            {
                var redisKey = GetRedisKey(key);
                var redisValue = await _redisDb.StringGetAsync(redisKey);

                if (redisValue.HasValue)
                {
                    var value = redisValue.ToString();
                    _cache.TryAdd(key, value);
                    return ConvertValue<T>(value);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Redis cache read failed for key: {Key}", key);
            }
        }

        // Fallback to database
        var dbValue = await GetFromDatabaseAsync(key, cancellationToken);
        if (dbValue != null)
        {
            _cache.TryAdd(key, dbValue);
            await CacheToRedisAsync(key, dbValue);
            return ConvertValue<T>(dbValue);
        }

        throw new KeyNotFoundException($"Configuration key '{key}' not found for application '{_options.ApplicationName}'");
    }

    public async Task<T> GetValueAsync<T>(string key, T defaultValue, CancellationToken cancellationToken = default)
    {
        try
        {
            return await GetValueAsync<T>(key, cancellationToken);
        }
        catch (KeyNotFoundException)
        {
            _logger?.LogWarning("Configuration key '{Key}' not found, returning default value", key);
            return defaultValue;
        }
    }

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogInformation("Refreshing configurations for '{ApplicationName}'", _options.ApplicationName);

            var configs = await LoadAllFromDatabaseAsync(cancellationToken);

            _cache.Clear();
            foreach (var config in configs)
            {
                _cache.TryAdd(config.Name, config.Value);
                await CacheToRedisAsync(config.Name, config.Value);
            }

            _logger?.LogInformation("Loaded {Count} configurations", configs.Count);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to refresh configurations");
        }
    }

    public IReadOnlyDictionary<string, string> GetAll()
    {
        return _cache.ToDictionary(x => x.Key, x => x.Value);
    }

    private async Task<string?> GetFromDatabaseAsync(string key, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_options.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        var sql = @"
            SELECT ""Value"" 
            FROM ""ConfigurationItems"" 
            WHERE ""ApplicationName"" = @ApplicationName 
              AND ""Name"" = @Name 
              AND ""IsActive"" = true
            LIMIT 1";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("ApplicationName", _options.ApplicationName);
        command.Parameters.AddWithValue("Name", key);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result?.ToString();
    }

    private async Task<List<ConfigurationItem>> LoadAllFromDatabaseAsync(CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_options.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        var sql = @"
            SELECT ""Id"", ""Name"", ""Type"", ""Value"", ""IsActive"", ""ApplicationName"", ""UpdatedAt""
            FROM ""ConfigurationItems"" 
            WHERE ""ApplicationName"" = @ApplicationName 
              AND ""IsActive"" = true";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("ApplicationName", _options.ApplicationName);

        var configs = new List<ConfigurationItem>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            configs.Add(new ConfigurationItem
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Type = reader.GetString(2),
                Value = reader.GetString(3),
                IsActive = reader.GetBoolean(4),
                ApplicationName = reader.GetString(5),
                UpdatedAt = reader.GetDateTime(6)
            });
        }

        return configs;
    }

    private async Task CacheToRedisAsync(string key, string value)
    {
        if (_redisDb == null) return;

        try
        {
            var redisKey = GetRedisKey(key);
            var expiry = TimeSpan.FromMinutes(_options.CacheExpirationMinutes);
            await _redisDb.StringSetAsync(redisKey, value, expiry);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to cache to Redis: {Key}", key);
        }
    }

    private string GetRedisKey(string key)
    {
        return $"config:{_options.ApplicationName}:{key}";
    }

    private T ConvertValue<T>(string value)
    {
        var targetType = typeof(T);

        // Handle nullable types
        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        try
        {
            if (underlyingType == typeof(string))
                return (T)(object)value;

            if (underlyingType == typeof(int))
                return (T)(object)int.Parse(value);

            if (underlyingType == typeof(long))
                return (T)(object)long.Parse(value);

            if (underlyingType == typeof(double))
                return (T)(object)double.Parse(value);

            if (underlyingType == typeof(bool))
                return (T)(object)bool.Parse(value);

            if (underlyingType == typeof(DateTime))
                return (T)(object)DateTime.Parse(value);

            // Try JSON deserialization for complex types
            return JsonSerializer.Deserialize<T>(value)
                   ?? throw new InvalidOperationException($"Failed to deserialize value to type {typeof(T).Name}");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to convert value '{Value}' to type {Type}", value, typeof(T).Name);
            throw new InvalidCastException($"Cannot convert '{value}' to type {typeof(T).Name}", ex);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        _refreshTimer?.Dispose();
        _redis?.Dispose();
        _disposed = true;

        GC.SuppressFinalize(this);
    }
}