using ConfigurationReader.Application.Common;
using ConfigurationReader.Domain.Interfaces;
using MediatR;

namespace ConfigurationReader.Application.Features.Configurations.Commands.DeleteConfiguration;

public class DeleteConfigurationCommandHandler
    : IRequestHandler<DeleteConfigurationCommand, Result>
{
    private readonly IConfigurationRepository _repository;

    public DeleteConfigurationCommandHandler(IConfigurationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(
        DeleteConfigurationCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (entity == null)
            {
                return Result.Failure($"Configuration with Id {request.Id} not found");
            }

            await _repository.DeleteAsync(request.Id, cancellationToken);

            return Result.Success("Configuration deleted successfully");
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error deleting configuration: {ex.Message}");
        }
    }
}