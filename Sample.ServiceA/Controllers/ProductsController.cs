// Sample.ServiceA/Controllers/ProductsController.cs
using Microsoft.AspNetCore.Mvc;
using Sample.ServiceA.Services;

namespace Sample.ServiceA.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductService productService,
        ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Get all products (respects MaxItemCount configuration)
    /// </summary>
    [HttpGet]
    public IActionResult GetProducts()
    {
        _logger.LogInformation("Getting products");
        var products = _productService.GetProducts();
        return Ok(new
        {
            count = products.Count,
            data = products
        });
    }

    /// <summary>
    /// Get service information and current configuration values
    /// </summary>
    [HttpGet("info")]
    public IActionResult GetInfo()
    {
        var info = _productService.GetServiceInfo();
        return Ok(info);
    }

    /// <summary>
    /// Get all loaded configurations
    /// </summary>
    [HttpGet("configurations")]
    public IActionResult GetConfigurations()
    {
        var configs = _productService.GetAllConfigurations();
        return Ok(new
        {
            applicationName = "SERVICE-A",
            count = configs.Count,
            configurations = configs
        });
    }
}