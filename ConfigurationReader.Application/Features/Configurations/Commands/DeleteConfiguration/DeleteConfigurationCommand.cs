using ConfigurationReader.Application.Common;
using MediatR;

namespace ConfigurationReader.Application.Features.Configurations.Commands.DeleteConfiguration;

public class DeleteConfigurationCommand : IRequest<Result<bool>>
{
    public int Id { get; set; }

    public DeleteConfigurationCommand(int id)
    {
        Id = id;
    }
}