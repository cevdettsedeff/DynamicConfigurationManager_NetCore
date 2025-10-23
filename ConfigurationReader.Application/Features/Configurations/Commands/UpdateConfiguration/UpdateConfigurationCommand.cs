using ConfigurationReader.Application.Common;
using ConfigurationReader.Application.DTOs;
using MediatR;

namespace ConfigurationReader.Application.Features.Configurations.Commands.UpdateConfiguration;

public class UpdateConfigurationCommand : IRequest<Result<ConfigurationItemDto>>
{
    public int Id { get; set; }
    public UpdateConfigurationDto Dto { get; set; } = null!;

    public UpdateConfigurationCommand(int id, UpdateConfigurationDto dto)
    {
        Id = id;
        Dto = dto;
    }
}