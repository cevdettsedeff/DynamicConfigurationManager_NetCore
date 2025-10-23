// ConfigurationReader.Infrastructure/Caching/RedisConfigurationCache.cs
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace ConfigurationReader.Infrastructure.Cache;

public class RedisConfigurationCache
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;
    private readonly ILogger<RedisConfigurationCache>? _logger;

    public RedisConfigurationCache(
        IConnectionMultiplexer redis,
        ILogger<RedisConfigurationCache>? logger = null)
    {
        _redis = redis;
        _database = redis.GetDatabase();
        _logger = logger;

        _logger?.LogInformation("RedisConfigurationCache initialized");
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var value = await _database.StringGetAsync(key);
            if (value.IsNullOrEmpty)
            {
                _logger?.LogDebug("Cache miss for key: {Key}", key);
                return default;
            }

            _logger?.LogDebug("Cache hit for key: {Key}", key);
            return JsonSerializer.Deserialize<T>(value!);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting cache for key: {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            await _database.StringSetAsync(key, json, expiry);
            _logger?.LogDebug("Cached key: {Key} with expiry: {Expiry}", key, expiry);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error setting cache for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _database.KeyDeleteAsync(key);
            _logger?.LogDebug("Removed cache for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error removing cache for key: {Key}", key);
        }
    }

    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            return await _database.KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error checking cache existence for key: {Key}", key);
            return false;
        }
    }

    public async Task ClearPatternAsync(string pattern)
    {
        try
        {
            var endpoints = _redis.GetEndPoints();
            var server = _redis.GetServer(endpoints.First());

            var keys = server.Keys(pattern: pattern);
            foreach (var key in keys)
            {
                await _database.KeyDeleteAsync(key);
            }

            _logger?.LogInformation("Cleared cache pattern: {Pattern}", pattern);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error clearing cache pattern: {Pattern}", pattern);
        }
    }
}