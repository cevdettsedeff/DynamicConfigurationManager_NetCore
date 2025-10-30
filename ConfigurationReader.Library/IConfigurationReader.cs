// ConfigurationReader.Library/IConfigurationReader.cs
namespace ConfigurationReader.Library;

/// <summary>
/// Interface for reading configurations from database with multi-level caching
/// </summary>
public interface IConfigurationReader
{
    /// <summary>
    /// Gets configuration value by key (synchronous)
    /// Throws KeyNotFoundException if config not found
    /// </summary>
    T GetValue<T>(string key);

    /// <summary>
    /// Gets configuration value by key with default value
    /// Returns default if config not found (does not throw exception)
    /// </summary>
    T GetValue<T>(string key, T defaultValue);

    /// <summary>
    /// Gets configuration value by key (asynchronous)
    /// Throws KeyNotFoundException if config not found
    /// </summary>
    Task<T> GetValueAsync<T>(string key);

    /// <summary>
    /// Gets configuration value by key with default value (asynchronous)
    /// Returns default if config not found (does not throw exception)
    /// </summary>
    Task<T> GetValueAsync<T>(string key, T defaultValue);

    /// <summary>
    /// Tries to get configuration value by key
    /// Returns false if config not found (does not throw exception)
    /// </summary>
    bool TryGetValue<T>(string key, out T value);

    /// <summary>
    /// Refreshes all configurations from database
    /// Updates in-memory and Redis cache
    /// </summary>
    Task RefreshAsync();

    /// <summary>
    /// Gets all configurations for the current application
    /// Returns a read-only dictionary of all cached configurations
    /// </summary>
    IReadOnlyDictionary<string, string> GetAll();
}