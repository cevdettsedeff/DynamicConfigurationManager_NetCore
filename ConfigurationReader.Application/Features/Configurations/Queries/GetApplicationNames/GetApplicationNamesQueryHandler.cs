using ConfigurationReader.Application.Common;
using ConfigurationReader.Domain.Interfaces;
using MediatR;

namespace ConfigurationReader.Application.Features.Configurations.Queries.GetApplicationNames;

public class GetApplicationNamesQueryHandler
    : IRequestHandler<GetApplicationNamesQuery, Result<List<string>>>
{
    private readonly IConfigurationRepository _repository;

    public GetApplicationNamesQueryHandler(IConfigurationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<string>>> Handle(
        GetApplicationNamesQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var names = await _repository.GetApplicationNamesAsync(cancellationToken);
            return Result<List<string>>.Success(names, $"Found {names.Count} applications");
        }
        catch (Exception ex)
        {
            return Result<List<string>>.Failure(
                $"Error retrieving application names: {ex.Message}");
        }
    }
}