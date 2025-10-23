using ConfigurationReader.Application.Common;
using ConfigurationReader.Application.DTOs;
using MediatR;

namespace ConfigurationReader.Application.Features.Configurations.Queries.GetAllConfigurations;

public class GetAllConfigurationsQuery : IRequest<Result<List<ConfigurationItemDto>>>
{
    public string? ApplicationName { get; set; }

    public GetAllConfigurationsQuery(string? applicationName = null)
    {
        ApplicationName = applicationName;
    }
}