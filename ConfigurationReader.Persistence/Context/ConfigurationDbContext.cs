using ConfigurationReader.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace ConfigurationReader.Persistence.Context;

public class ConfigurationDbContext : DbContext
{
    public ConfigurationDbContext(DbContextOptions<ConfigurationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ConfigurationItem> ConfigurationItems => Set<ConfigurationItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all IEntityTypeConfiguration from current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Automatically update UpdatedAt for modified entities
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        // Automatically update UpdatedAt for modified entities
        UpdateTimestamps();
        return base.SaveChanges();
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<ConfigurationItem>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        var now = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                // Yeni kayıt: CreatedAt ve UpdatedAt set et
                var createdAtProperty = entry.Property(nameof(ConfigurationItem.CreatedAt));
                var createdAtValue = (DateTime?)createdAtProperty.CurrentValue;

                // Eğer CreatedAt set edilmemişse (default value ise)
                if (!createdAtValue.HasValue ||
                    createdAtValue.Value == DateTime.MinValue ||
                    createdAtValue.Value == default)
                {
                    createdAtProperty.CurrentValue = now;
                }

                // UpdatedAt'i her zaman set et
                entry.Property(nameof(ConfigurationItem.UpdatedAt)).CurrentValue = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                // Güncelleme: Sadece UpdatedAt
                entry.Property(nameof(ConfigurationItem.UpdatedAt)).CurrentValue = now;

                // CreatedAt'in değişmemesini garanti et
                entry.Property(nameof(ConfigurationItem.CreatedAt)).IsModified = false;
            }
        }
    }
}