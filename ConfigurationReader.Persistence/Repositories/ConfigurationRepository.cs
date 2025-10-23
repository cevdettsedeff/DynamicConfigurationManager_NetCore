using ConfigurationReader.Domain.Entities;
using ConfigurationReader.Domain.Interfaces; 
using ConfigurationReader.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ConfigurationReader.Persistence.Repositories;

public class ConfigurationRepository : IConfigurationRepository 
{
    private readonly ConfigurationDbContext _context;

    public ConfigurationRepository(ConfigurationDbContext context)
    {
        _context = context;
    }

    public async Task<ConfigurationItem?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _context.ConfigurationItems
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<ConfigurationItem?> GetByKeyAsync(
        string applicationName,
        string key,
        CancellationToken cancellationToken = default)
    {
        return await _context.ConfigurationItems
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.ApplicationName == applicationName &&
                     x.Name == key &&
                     x.IsActive,
                cancellationToken);
    }

    public async Task<IReadOnlyList<ConfigurationItem>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.ConfigurationItems
            .AsNoTracking()
            .OrderBy(x => x.ApplicationName)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ConfigurationItem>> GetByApplicationAsync(
        string applicationName,
        bool onlyActive = true,
        CancellationToken cancellationToken = default)
    {
        var query = _context.ConfigurationItems.AsNoTracking()
            .Where(x => x.ApplicationName == applicationName);

        if (onlyActive)
        {
            query = query.Where(x => x.IsActive);
        }

        return await query
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ConfigurationItem>> GetChangedSinceAsync(
        DateTime since,
        CancellationToken cancellationToken = default)
    {
        return await _context.ConfigurationItems
            .AsNoTracking()
            .Where(x => x.UpdatedAt > since)
            .OrderBy(x => x.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<ConfigurationItem> AddAsync(
        ConfigurationItem item,
        CancellationToken cancellationToken = default)
    {
        await _context.ConfigurationItems.AddAsync(item, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return item;
    }

    public async Task UpdateAsync(
        ConfigurationItem item,
        CancellationToken cancellationToken = default)
    {
        _context.ConfigurationItems.Update(item);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var item = await _context.ConfigurationItems.FindAsync(new object[] { id }, cancellationToken);
        if (item != null)
        {
            _context.ConfigurationItems.Remove(item);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(
        string applicationName,
        string name,
        CancellationToken cancellationToken = default)
    {
        return await _context.ConfigurationItems
            .AnyAsync(
                x => x.ApplicationName == applicationName && x.Name == name,
                cancellationToken);
    }

    public async Task<List<string>> GetApplicationNamesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.ConfigurationItems
            .Select(x => x.ApplicationName)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);
    }
}