// ConfigurationReader.Domain/Interfaces/IConfigurationRepository.cs
using ConfigurationReader.Domain.Entities;

namespace ConfigurationReader.Domain.Interfaces;

public interface IConfigurationRepository
{
    Task<ConfigurationItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<ConfigurationItem?> GetByKeyAsync(
        string applicationName,
        string key,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ConfigurationItem>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ConfigurationItem>> GetByApplicationAsync(
        string applicationName,
        bool onlyActive = true,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ConfigurationItem>> GetChangedSinceAsync(
        DateTime since,
        CancellationToken cancellationToken = default);

    Task<ConfigurationItem> AddAsync(
        ConfigurationItem item,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        ConfigurationItem item,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        string applicationName,
        string name,
        CancellationToken cancellationToken = default);

    Task<List<string>> GetApplicationNamesAsync(
        CancellationToken cancellationToken = default);
}