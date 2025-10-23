using ConfigurationReader.Domain.Entities;

namespace ConfigurationReader.Domain.Interfaces;

public interface IConfigurationCache
{
    Task<ConfigurationItem?> GetAsync(string key, CancellationToken cancellationToken = default);

    Task SetAsync(string key, ConfigurationItem item, CancellationToken cancellationToken = default);

    Task SetManyAsync(
        IEnumerable<ConfigurationItem> items,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    Task ClearAsync(CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
}