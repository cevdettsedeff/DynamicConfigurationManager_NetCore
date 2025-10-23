using ConfigurationReader.Library;
using Sample.ServiceA.Models;

namespace Sample.ServiceA.Services;

public class ProductService : IProductService
{
    private readonly IConfigurationReader _configReader;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IConfigurationReader configReader,
        ILogger<ProductService> logger)
    {
        _configReader = configReader;
        _logger = logger;
    }

    public List<Product> GetProducts()
    {
        // ✅ Dynamic configuration kullanımı
        var maxItems = _configReader.GetValue<int>("MaxItemCount");
        var isFeatureEnabled = _configReader.GetValue<bool>("IsFeatureEnabled");

        _logger.LogInformation(
            "Getting products with MaxItemCount: {MaxItems}, FeatureEnabled: {IsEnabled}",
            maxItems, isFeatureEnabled);

        var products = new List<Product>
        {
            new() { Id = 1, Name = "Laptop", Price = 1500m, Category = "Electronics", IsAvailable = true },
            new() { Id = 2, Name = "Mouse", Price = 25m, Category = "Electronics", IsAvailable = true },
            new() { Id = 3, Name = "Keyboard", Price = 75m, Category = "Electronics", IsAvailable = true },
            new() { Id = 4, Name = "Monitor", Price = 300m, Category = "Electronics", IsAvailable = false },
            new() { Id = 5, Name = "Headphones", Price = 100m, Category = "Electronics", IsAvailable = true }
        };

        // Apply dynamic configuration
        var result = products.Take(maxItems).ToList();

        if (!isFeatureEnabled)
        {
            _logger.LogWarning("Feature is disabled, returning empty list");
            return new List<Product>();
        }

        return result;
    }

    public ServiceInfo GetServiceInfo()
    {
        return new ServiceInfo
        {
            ServiceName = _configReader.GetValue<string>("ServiceName", "SERVICE-A"),
            SiteName = _configReader.GetValue<string>("SiteName"),
            MaxItemCount = _configReader.GetValue<int>("MaxItemCount"),
            ConnectionTimeout = _configReader.GetValue<double>("ConnectionTimeout"),
            IsFeatureEnabled = _configReader.GetValue<bool>("IsFeatureEnabled"),
            LoadedAt = DateTime.UtcNow
        };
    }

    public Dictionary<string, string> GetAllConfigurations()
    {
        return _configReader.GetAll().ToDictionary(x => x.Key, x => x.Value);
    }
}