// ConfigurationReader.Library/ServiceCollectionExtensions.cs
using Microsoft.Extensions.DependencyInjection;

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

        // Create and configure options
        var options = new ConfigurationReaderOptions();
        configureOptions(options);

        // Validate options
        if (string.IsNullOrWhiteSpace(options.ApplicationName))
            throw new ArgumentException("ApplicationName is required");

        if (string.IsNullOrWhiteSpace(options.ConnectionString))
            throw new ArgumentException("ConnectionString is required");

        // Register as singleton
        services.AddSingleton<IConfigurationReader>(provider =>
        {
            // ✅ Options ile constructor çağır (logger yok)
            return new ConfigurationReader(options);
        });

        return services;
    }
}