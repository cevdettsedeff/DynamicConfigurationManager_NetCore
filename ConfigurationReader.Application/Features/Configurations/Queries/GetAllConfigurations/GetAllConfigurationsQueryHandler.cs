using AutoMapper;
using ConfigurationReader.Application.Common;
using ConfigurationReader.Application.DTOs;
using ConfigurationReader.Domain.Interfaces;
using MediatR;

namespace ConfigurationReader.Application.Features.Configurations.Queries.GetAllConfigurations;

public class GetAllConfigurationsQueryHandler
    : IRequestHandler<GetAllConfigurationsQuery, Result<List<ConfigurationItemDto>>>
{
    private readonly IConfigurationRepository _repository;
    private readonly IMapper _mapper;

    public GetAllConfigurationsQueryHandler(
        IConfigurationRepository repository,
        IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<List<ConfigurationItemDto>>> Handle(
        GetAllConfigurationsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var configurations = string.IsNullOrWhiteSpace(request.ApplicationName)
                ? await _repository.GetAllAsync(cancellationToken)
                : await _repository.GetByApplicationAsync(request.ApplicationName, false, cancellationToken);

            var dtos = _mapper.Map<List<ConfigurationItemDto>>(configurations);

            return Result<List<ConfigurationItemDto>>.Success(dtos,
                $"Found {dtos.Count} configurations");
        }
        catch (Exception ex)
        {
            return Result<List<ConfigurationItemDto>>.Failure(
                $"Error retrieving configurations: {ex.Message}");
        }
    }
}