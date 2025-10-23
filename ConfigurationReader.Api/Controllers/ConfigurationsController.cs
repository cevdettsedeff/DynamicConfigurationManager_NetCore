using ConfigurationReader.Application.Common;
using ConfigurationReader.Application.DTOs;
using ConfigurationReader.Application.Features.Configurations.Commands.CreateConfiguration;
using ConfigurationReader.Application.Features.Configurations.Commands.DeleteConfiguration;
using ConfigurationReader.Application.Features.Configurations.Commands.UpdateConfiguration;
using ConfigurationReader.Application.Features.Configurations.Queries.GetAllConfigurations;
using ConfigurationReader.Application.Features.Configurations.Queries.GetApplicationNames;
using ConfigurationReader.Application.Features.Configurations.Queries.GetConfigurationById;
using ConfigurationReader.Application.Features.Configurations.Queries.GetConfigurationByKey;
using ConfigurationReader.Infrastructure.Cache;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

[ApiController]
[Route("api/[controller]")]
public class ConfigurationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly RedisConfigurationCache? _cache;
    private readonly ILogger<ConfigurationsController> _logger;

    public ConfigurationsController(
        IMediator mediator,
        ILogger<ConfigurationsController> logger,
        RedisConfigurationCache? cache = null)
    {
        _mediator = mediator;
        _logger = logger;
        _cache = cache;
    }

    /// <summary>
    /// Get all configurations, optionally filtered by application name
    /// </summary>
    /// <param name="applicationName">Optional application name filter</param>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<ConfigurationItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? applicationName = null)
    {
        _logger.LogInformation("GetAll called with applicationName: {ApplicationName}",
            applicationName ?? "ALL");

        var query = new GetAllConfigurationsQuery(applicationName);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(result);

        _logger.LogInformation("Returned {Count} configurations", result.Data?.Count ?? 0);
        return Ok(result);
    }

    /// <summary>
    /// Get configuration by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Result<ConfigurationItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        _logger.LogInformation("GetById called with id: {Id}", id);

        var query = new GetConfigurationByIdQuery(id);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Get configuration by application name and key (with Redis cache)
    /// </summary>
    [HttpGet("{applicationName}/{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByKey(string applicationName, string key)
    {
        _logger.LogInformation("GetByKey called: {ApplicationName}/{Key}", applicationName, key);

        // Try Redis cache first
        if (_cache != null)
        {
            var cacheKey = $"config:{applicationName}:{key}";
            var cachedValue = await _cache.GetAsync<string>(cacheKey);

            if (cachedValue != null)
            {
                _logger.LogDebug("Cache hit for {Key}", cacheKey);
                return Ok(new
                {
                    isSuccess = true,
                    message = "Configuration retrieved from cache",
                    data = new { applicationName, name = key, value = cachedValue },
                    source = "redis"
                });
            }
        }

        // Fallback to database
        var query = new GetConfigurationByKeyQuery(applicationName, key);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return NotFound(result);

        // Cache in Redis
        if (_cache != null && result.Data != null)
        {
            var cacheKey = $"config:{applicationName}:{key}";
            await _cache.SetAsync(cacheKey, result.Data.Value, TimeSpan.FromMinutes(5));
        }

        return Ok(new
        {
            result.IsSuccess,
            result.Message,
            result.Data,
            source = "database"
        });
    }

    /// <summary>
    /// Create new configuration
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Result<ConfigurationItemDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateConfigurationDto dto)
    {
        _logger.LogInformation("Create called for {ApplicationName}/{Name}",
            dto.ApplicationName, dto.Name);

        var command = new CreateConfigurationCommand(dto);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        // Clear Redis cache for this app
        if (_cache != null && result.Data != null)
        {
            await _cache.ClearPatternAsync($"config:{result.Data.ApplicationName}:*");
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
    }

    /// <summary>
    /// Update existing configuration
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Result<ConfigurationItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateConfigurationDto dto)
    {
        _logger.LogInformation("Update called for id: {Id}", id);

        var command = new UpdateConfigurationCommand(id, dto);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        // Clear Redis cache
        if (_cache != null && result.Data != null)
        {
            var cacheKey = $"config:{result.Data.ApplicationName}:{result.Data.Name}";
            await _cache.RemoveAsync(cacheKey);
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete configuration
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("Delete called for id: {Id}", id);

        var command = new DeleteConfigurationCommand(id);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Get list of all application names
    /// </summary>
    [HttpGet("applications")]
    [ProducesResponseType(typeof(Result<List<string>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetApplicationNames()
    {
        _logger.LogInformation("GetApplicationNames called");

        var query = new GetApplicationNamesQuery();
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Clear Redis cache for specific pattern or all
    /// </summary>
    [HttpPost("cache/clear")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ClearCache([FromQuery] string? pattern = null)
    {
        _logger.LogInformation("ClearCache called with pattern: {Pattern}", pattern ?? "ALL");

        if (_cache == null)
        {
            return Ok(new { message = "Redis cache is not configured" });
        }

        try
        {
            if (string.IsNullOrWhiteSpace(pattern))
            {
                await _cache.ClearPatternAsync("config:*");
                return Ok(new { message = "All configuration cache cleared" });
            }
            else
            {
                await _cache.ClearPatternAsync($"config:{pattern}*");
                return Ok(new { message = $"Cache cleared for pattern: {pattern}" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache");
            return StatusCode(500, new { error = "Failed to clear cache", detail = ex.Message });
        }
    }
}