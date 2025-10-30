// Sample.ServiceA/Controllers/ConfigController.cs
using ConfigurationReader.Library;
using Microsoft.AspNetCore.Mvc;

namespace Sample.ServiceA.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConfigController : ControllerBase
{
    private readonly IConfigurationReader _configReader;
    private readonly ILogger<ConfigController> _logger;

    public ConfigController(
        IConfigurationReader configReader,
        ILogger<ConfigController> logger)
    {
        _configReader = configReader;
        _logger = logger;
    }

    /// <summary>
    /// Get specific configuration value
    /// </summary>
    [HttpGet("{key}")]
    public async Task<IActionResult> GetValue(string key)
    {
        try
        {
            var value = await _configReader.GetValueAsync<string>(key);
            return Ok(new { key, value });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = $"Configuration key '{key}' not found" });
        }
    }

    /// <summary>
    /// Manually refresh configurations from database
    /// </summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        _logger.LogInformation("Manual refresh requested");
        await _configReader.RefreshAsync();

        var configs = _configReader. GetAll();
        return Ok(new
        {
            message = "Configurations refreshed successfully",
            count = configs.Count,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Get all configurations
    /// </summary>
    [HttpGet]
    public IActionResult GetAll()
    {
        var configs = _configReader.GetAll();
        return Ok(new
        {
            applicationName = "SERVICE-A",
            count = configs.Count,
            configurations = configs
        });
    }
}