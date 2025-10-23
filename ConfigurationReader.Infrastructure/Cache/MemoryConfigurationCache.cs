using ConfigurationReader.Domain.Entities;
using ConfigurationReader.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ConfigurationReader.Infrastructure.Cache;

public class MemoryConfigurationCache : IConfigurationCache
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<MemoryConfigurationCache> _logger;
    private readonly MemoryCacheEntryOptions _cacheOptions;

    public MemoryConfigurationCache(
        IMemoryCache cache,
        ILogger<MemoryConfigurationCache> logger)
    {
        _cache = cache;
        _logger = logger;

        _cacheOptions = new MemoryCacheEntryOptions
        {
            Priority = CacheItemPriority.High,
            SlidingExpiration = TimeSpan.FromHours(1)
        };
    }

    public Task<ConfigurationItem?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var item = _cache.Get<ConfigurationItem>(key);
            _logger.LogDebug("Cache GET: {Key} - {Found}", key, item != null ? "HIT" : "MISS");
            return Task.FromResult(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting item from cache: {Key}", key);
            return Task.FromResult<ConfigurationItem?>(null);
        }
    }

    public Task SetAsync(string key, ConfigurationItem item, CancellationToken cancellationToken = default)
    {
        try
        {
            _cache.Set(key, item, _cacheOptions);
            _logger.LogDebug("Cache SET: {Key}", key);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting item in cache: {Key}", key);
            return Task.CompletedTask;
        }
    }

    public Task SetManyAsync(IEnumerable<ConfigurationItem> items, CancellationToken cancellationToken = default)
    {
        try
        {
            foreach (var item in items)
            {
                var key = $"{item.ApplicationName}:{item.Name}";
                _cache.Set(key, item, _cacheOptions);
            }
            _logger.LogInformation("Cache SET MANY: {Count} items", items.Count());
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting multiple items in cache");
            return Task.CompletedTask;
        }
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            _cache.Remove(key);
            _logger.LogDebug("Cache REMOVE: {Key}", key);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing item from cache: {Key}", key);
            return Task.CompletedTask;
        }
    }

    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // IMemoryCache doesn't have a Clear method
            // We would need to track keys separately or use a different implementation
            _logger.LogWarning("Memory cache clear requested but not implemented");
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache");
            return Task.CompletedTask;
        }
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var exists = _cache.TryGetValue(key, out _);
            return Task.FromResult(exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking cache existence: {Key}", key);
            return Task.FromResult(false);
        }
    }
}