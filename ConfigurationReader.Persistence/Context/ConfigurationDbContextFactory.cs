using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ConfigurationReader.Persistence.Context;

/// <summary>
/// Design-time factory for EF Core migrations
/// This allows migrations to be created without needing the full application context
/// </summary>
public class ConfigurationDbContextFactory : IDesignTimeDbContextFactory<ConfigurationDbContext>
{
    public ConfigurationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ConfigurationDbContext>();

        // ✅ PostgreSQL için design-time connection string
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=ConfigurationDb;Username=postgres;Password=postgres",
            b => b.MigrationsAssembly(typeof(ConfigurationDbContext).Assembly.FullName));

        return new ConfigurationDbContext(optionsBuilder.Options);
    }
}