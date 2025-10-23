// ConfigurationReader.Library/IConfigurationReader.cs
namespace ConfigurationReader.Library;

public interface IConfigurationReader
{
    /// <summary>
    /// Gets configuration value by key
    /// </summary>
    T GetValue<T>(string key);

    /// <summary>
    /// Gets configuration value by key with default value
    /// </summary>
    T GetValue<T>(string key, T defaultValue);

    /// <summary>
    /// Gets configuration value asynchronously
    /// </summary>
    Task<T> GetValueAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets configuration value asynchronously with default value
    /// </summary>
    Task<T> GetValueAsync<T>(string key, T defaultValue, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes all configurations from database
    /// </summary>
    Task RefreshAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all configurations for the application
    /// </summary>
    IReadOnlyDictionary<string, string> GetAll();
}