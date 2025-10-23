using Sample.ServiceA.Models;

namespace Sample.ServiceA.Services;

public interface IProductService
{
    List<Product> GetProducts();
    ServiceInfo GetServiceInfo();
    Dictionary<string, string> GetAllConfigurations();
}