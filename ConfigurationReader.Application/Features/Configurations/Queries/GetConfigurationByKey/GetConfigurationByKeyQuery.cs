using ConfigurationReader.Application.Common;
using ConfigurationReader.Application.DTOs;
using MediatR;

namespace ConfigurationReader.Application.Features.Configurations.Queries.GetConfigurationByKey;

public class GetConfigurationByKeyQuery : IRequest<Result<ConfigurationItemDto>>
{
    public string ApplicationName { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;

    public GetConfigurationByKeyQuery(string applicationName, string key)
    {
        ApplicationName = applicationName;
        Key = key;
    }
}