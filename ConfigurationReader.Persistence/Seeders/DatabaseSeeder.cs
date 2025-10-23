using Bogus;
using ConfigurationReader.Domain.Entities;
using ConfigurationReader.Domain.Enums;
using ConfigurationReader.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ConfigurationReader.Persistence.Seeders;

public class DatabaseSeeder
{
    private readonly ConfigurationDbContext _context;
    private readonly ILogger<DatabaseSeeder>? _logger;

    public DatabaseSeeder(ConfigurationDbContext context, ILogger<DatabaseSeeder>? logger = null)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync(bool force = false)
    {
        try
        {
            var hasData = await _context.ConfigurationItems.AnyAsync();

            if (hasData && !force)
            {
                _logger?.LogInformation("Database already contains data. Skipping seed.");
                return;
            }

            if (force && hasData)
            {
                _logger?.LogInformation("Force seed: Clearing existing data...");
                var existing = await _context.ConfigurationItems.ToListAsync();
                _context.ConfigurationItems.RemoveRange(existing);
                await _context.SaveChangesAsync();
            }

            _logger?.LogInformation("Starting database seed...");

            await SeedPredefinedConfigurationsAsync();
            await SeedBogusConfigurationsAsync();

            await _context.SaveChangesAsync();

            var totalCount = await _context.ConfigurationItems.CountAsync();
            _logger?.LogInformation("Database seeded successfully. Total configurations: {Count}", totalCount);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private async Task SeedPredefinedConfigurationsAsync()
    {
        var configs = new[]
        {
            // ✅ SERVICE-A Configurations
            ConfigurationItem.Create("SiteName", ConfigurationType.String, "soty.io", "SERVICE-A", true),
            ConfigurationItem.Create("MaxItemCount", ConfigurationType.Int, "3", "SERVICE-A", true),
            ConfigurationItem.Create("ConnectionTimeout", ConfigurationType.Double, "30.5", "SERVICE-A", true),
            ConfigurationItem.Create("IsFeatureEnabled", ConfigurationType.Bool, "true", "SERVICE-A", true),
            ConfigurationItem.Create("ServiceName", ConfigurationType.String, "SERVICE-A", "SERVICE-A", true),

            // ✅ SERVICE-B Configurations
            ConfigurationItem.Create("ApiUrl", ConfigurationType.String, "https://api.example.com", "SERVICE-B", true),
            ConfigurationItem.Create("ApiKey", ConfigurationType.String, "sk-test-key-123456789", "SERVICE-B", true),
            ConfigurationItem.Create("RetryCount", ConfigurationType.Int, "3", "SERVICE-B", true),
            ConfigurationItem.Create("EnableCache", ConfigurationType.Bool, "true", "SERVICE-B", true),

            // ✅ SHARED Configurations
            ConfigurationItem.Create("Environment", ConfigurationType.String, "Development", "SHARED", true),
            ConfigurationItem.Create("LogLevel", ConfigurationType.String, "Information", "SHARED", true),
        };

        await _context.ConfigurationItems.AddRangeAsync(configs);
        _logger?.LogInformation("Added {Count} predefined configurations", configs.Length);
    }

    private async Task SeedBogusConfigurationsAsync()
    {
        var applications = new[] { "SERVICE-A", "SERVICE-B", "SERVICE-C", "SHARED" };
        var types = new[] { ConfigurationType.String, ConfigurationType.Int, ConfigurationType.Bool, ConfigurationType.Double };

        var faker = new Faker();
        var configs = new List<ConfigurationItem>();

        for (int i = 0; i < 20; i++)
        {
            var type = faker.PickRandom(types);
            var name = faker.Hacker.Verb() + faker.Hacker.Noun();
            var app = faker.PickRandom(applications);
            var isActive = faker.Random.Bool(0.8f);

            var value = type switch
            {
                ConfigurationType.String => faker.Lorem.Word(),
                ConfigurationType.Int => faker.Random.Int(1, 1000).ToString(),
                ConfigurationType.Bool => faker.Random.Bool().ToString().ToLower(),
                ConfigurationType.Double => faker.Random.Double(0.1, 100.0).ToString("F2"),
                _ => faker.Lorem.Word()
            };

            configs.Add(ConfigurationItem.Create(name, type, value, app, isActive));
        }

        await _context.ConfigurationItems.AddRangeAsync(configs);
        _logger?.LogInformation("Added {Count} Bogus configurations", configs.Count);
    }
}