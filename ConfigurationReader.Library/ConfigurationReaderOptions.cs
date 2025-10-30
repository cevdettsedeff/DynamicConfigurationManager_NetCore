// ConfigurationReader.Library/ConfigurationReaderOptions.cs
namespace ConfigurationReader.Library;

/// <summary>
/// Configuration options for ConfigurationReader
/// </summary>
public class ConfigurationReaderOptions
{
    /// <summary>
    /// Application name (e.g., "SERVICE-A", "CONSOLE-APP")
    /// REQUIRED
    /// </summary>
    public string ApplicationName { get; set; } = string.Empty;

    /// <summary>
    /// PostgreSQL connection string
    /// REQUIRED
    /// Example: "Host=localhost;Port=5432;Database=ConfigurationDb;Username=postgres;Password=postgres"
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Background refresh interval in milliseconds
    /// REQUIRED
    /// Default: 30000 (30 seconds)
    /// Example: 60000 = 1 minute, 5000 = 5 seconds
    /// </summary>
    public int RefreshTimerIntervalInMs { get; set; } = 30000;

    /// <summary>
    /// Redis connection string (OPTIONAL, for multi-instance caching)
    /// Example: "localhost:6379,abortConnect=false"
    /// If null, Redis cache will be disabled
    /// </summary>
    public string? RedisConnectionString { get; set; }

    /// <summary>
    /// Cache expiration in minutes (Redis only)
    /// Default: 5 minutes
    /// Only used when Redis is enabled
    /// </summary>
    public int CacheExpirationMinutes { get; set; } = 5;

    /// <summary>
    /// Enable console logging
    /// Default: false
    /// Set to true for debugging
    /// </summary>
    public bool EnableLogging { get; set; } = false;
}