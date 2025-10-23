// ConfigurationReader.Library/ConfigurationReaderOptions.cs
namespace ConfigurationReader.Library;

public class ConfigurationReaderOptions
{
    /// <summary>
    /// Application name (e.g., "SERVICE-A", "SERVICE-B")
    /// </summary>
    public string ApplicationName { get; set; } = string.Empty;

    /// <summary>
    /// PostgreSQL connection string
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Redis connection string (optional, for caching)
    /// </summary>
    public string? RedisConnectionString { get; set; }

    /// <summary>
    /// Auto-refresh interval in seconds (0 = disabled)
    /// </summary>
    public int RefreshIntervalSeconds { get; set; } = 60;

    /// <summary>
    /// Cache expiration in minutes (Redis only)
    /// </summary>
    public int CacheExpirationMinutes { get; set; } = 5;

    /// <summary>
    /// Enable logging
    /// </summary>
    public bool EnableLogging { get; set; } = true;
}