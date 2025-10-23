// ConfigurationReader.Infrastructure/BackgroundServices/ConfigurationRefreshService.cs
using ConfigurationReader.Domain.Interfaces;
using ConfigurationReader.Infrastructure.Cache;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ConfigurationReader.Infrastructure.BackgroundServices;

public class ConfigurationRefreshService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ConfigurationRefreshService> _logger;
    private readonly RedisConfigurationCache? _cache;
    private readonly int _intervalSeconds;

    public ConfigurationRefreshService(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<ConfigurationRefreshService> logger,
        RedisConfigurationCache? cache = null)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _cache = cache;

        _intervalSeconds = configuration.GetValue<int>("ConfigurationRefreshIntervalSeconds", 30);

        _logger.LogInformation(
            "ConfigurationRefreshService initialized with {Interval} seconds interval. Redis: {HasRedis}",
            _intervalSeconds,
            _cache != null);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ConfigurationRefreshService is starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(_intervalSeconds), stoppingToken);

                using var scope = _serviceProvider.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IConfigurationRepository>();

                var configs = await repository.GetAllAsync(stoppingToken);

                _logger.LogInformation(
                    "Configuration refresh completed. Total configs: {Count}",
                    configs.Count);

                // ✅ Redis cache'i güncelle (varsa)
                if (_cache != null)
                {
                    foreach (var config in configs)
                    {
                        var cacheKey = $"config:{config.ApplicationName}:{config.Name}";
                        await _cache.SetAsync(cacheKey, config.Value, TimeSpan.FromMinutes(5));
                    }

                    _logger.LogDebug("Redis cache updated with {Count} configurations", configs.Count);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("ConfigurationRefreshService is stopping");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during configuration refresh");
            }
        }

        _logger.LogInformation("ConfigurationRefreshService has stopped");
    }
}