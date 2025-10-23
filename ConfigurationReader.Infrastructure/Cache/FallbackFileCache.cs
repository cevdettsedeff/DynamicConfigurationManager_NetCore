// ConfigurationReader.Infrastructure/Cache/FallbackFileCache.cs
using ConfigurationReader.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ConfigurationReader.Infrastructure.Cache;

public class FallbackFileCache
{
    private readonly string _cacheDirectory;
    private readonly ILogger<FallbackFileCache> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public FallbackFileCache(ILogger<FallbackFileCache> logger, string? cacheDirectory = null)
    {
        _logger = logger;
        _cacheDirectory = cacheDirectory ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cache");

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        // Ensure directory exists
        if (!Directory.Exists(_cacheDirectory))
        {
            Directory.CreateDirectory(_cacheDirectory);
        }
    }

    public async Task SaveAsync(string applicationName, IEnumerable<ConfigurationItem> items)
    {
        try
        {
            var fileName = GetFileName(applicationName);
            var json = JsonSerializer.Serialize(items, _jsonOptions);
            await File.WriteAllTextAsync(fileName, json);

            _logger.LogInformation("Saved fallback cache for {ApplicationName}: {Count} items",
                applicationName, items.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving fallback cache for {ApplicationName}", applicationName);
        }
    }

    public async Task<IEnumerable<ConfigurationItem>> LoadAsync(string applicationName)
    {
        try
        {
            var fileName = GetFileName(applicationName);
            if (!File.Exists(fileName))
            {
                _logger.LogWarning("Fallback cache file not found for {ApplicationName}", applicationName);
                return Enumerable.Empty<ConfigurationItem>();
            }

            var json = await File.ReadAllTextAsync(fileName);
            var items = JsonSerializer.Deserialize<List<ConfigurationItem>>(json);

            _logger.LogInformation("Loaded fallback cache for {ApplicationName}: {Count} items",
                applicationName, items?.Count ?? 0);

            return items ?? Enumerable.Empty<ConfigurationItem>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading fallback cache for {ApplicationName}", applicationName);
            return Enumerable.Empty<ConfigurationItem>();
        }
    }

    public bool Exists(string applicationName)
    {
        var fileName = GetFileName(applicationName);
        return File.Exists(fileName);
    }

    public Task DeleteAsync(string applicationName) 
    {
        try
        {
            var fileName = GetFileName(applicationName);
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
                _logger.LogInformation("Deleted fallback cache for {ApplicationName}", applicationName);
            }

            return Task.CompletedTask; 
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting fallback cache for {ApplicationName}", applicationName);
            return Task.CompletedTask; 
        }
    }

    private string GetFileName(string applicationName)
    {
        // Sanitize application name for file system
        var sanitized = string.Join("_", applicationName.Split(Path.GetInvalidFileNameChars()));
        return Path.Combine(_cacheDirectory, $"{sanitized}.json");
    }
}