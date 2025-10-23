// ConfigurationReader.Library/ServiceCollectionExtensions.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ConfigurationReader.Library;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds ConfigurationReader as a singleton service
    /// </summary>
    public static IServiceCollection AddConfigurationReader(
        this IServiceCollection services,
        Action<ConfigurationReaderOptions> configureOptions)
    {
        if (configureOptions == null)
            throw new ArgumentNullException(nameof(configureOptions));

        var options = new ConfigurationReaderOptions();
        configureOptions(options);

        // Validate options
        if (string.IsNullOrWhiteSpace(options.ApplicationName))
            throw new ArgumentException("ApplicationName is required");

        if (string.IsNullOrWhiteSpace(options.ConnectionString))
            throw new ArgumentException("ConnectionString is required");

        services.AddSingleton<IConfigurationReader>(provider =>
        {
            var logger = provider.GetService<ILogger<ConfigurationReader>>();
            return new ConfigurationReader(options, logger);
        });

        return services;
    }
}