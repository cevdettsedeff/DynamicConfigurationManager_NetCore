// ConfigurationReader.Infrastructure/DependencyInjection.cs
using ConfigurationReader.Infrastructure.BackgroundServices;
using ConfigurationReader.Infrastructure.Cache;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace ConfigurationReader.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ✅ Redis Configuration
        var redisConnectionString = configuration.GetConnectionString("Redis");

        if (!string.IsNullOrWhiteSpace(redisConnectionString))
        {
            try
            {
                // Connection Multiplexer
                services.AddSingleton<IConnectionMultiplexer>(sp =>
                {
                    var configOptions = ConfigurationOptions.Parse(redisConnectionString);
                    configOptions.AbortOnConnectFail = false;
                    return ConnectionMultiplexer.Connect(configOptions);
                });

                // ✅ RedisConfigurationCache
                services.AddSingleton<RedisConfigurationCache>();

                Console.WriteLine($"[INFO] Redis configured: {redisConnectionString}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARNING] Redis connection failed: {ex.Message}");
                Console.WriteLine("[INFO] Continuing without Redis cache");
            }
        }
        else
        {
            Console.WriteLine("[INFO] Redis connection string not found, skipping Redis configuration");
        }

        // ✅ Background Services
        services.AddHostedService<ConfigurationRefreshService>();

        return services;
    }
}