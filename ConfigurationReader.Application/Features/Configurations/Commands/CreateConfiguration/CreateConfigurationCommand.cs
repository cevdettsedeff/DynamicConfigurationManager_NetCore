using ConfigurationReader.Application.Common;
using ConfigurationReader.Application.DTOs;
using MediatR;

namespace ConfigurationReader.Application.Features.Configurations.Commands.CreateConfiguration;

public class CreateConfigurationCommand : IRequest<Result<ConfigurationItemDto>>
{
    public CreateConfigurationDto Dto { get; set; }

    public CreateConfigurationCommand(CreateConfigurationDto dto)
    {
        Dto = dto;
    }
}