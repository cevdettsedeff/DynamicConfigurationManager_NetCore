using AutoMapper;
using ConfigurationReader.Application.Common;
using ConfigurationReader.Application.DTOs;
using ConfigurationReader.Domain.Exceptions;
using ConfigurationReader.Domain.Interfaces;
using MediatR;

namespace ConfigurationReader.Application.Features.Configurations.Queries.GetConfigurationByKey;

public class GetConfigurationByKeyQueryHandler
    : IRequestHandler<GetConfigurationByKeyQuery, Result<ConfigurationItemDto>>
{
    private readonly IConfigurationRepository _repository;
    private readonly IMapper _mapper;

    public GetConfigurationByKeyQueryHandler(
        IConfigurationRepository repository,
        IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<ConfigurationItemDto>> Handle(
        GetConfigurationByKeyQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var configuration = await _repository.GetByKeyAsync(
                request.ApplicationName,
                request.Key,
                cancellationToken);

            if (configuration == null)
            {
                return Result<ConfigurationItemDto>.Failure(
                    $"Configuration '{request.Key}' not found for application '{request.ApplicationName}'");
            }

            var dto = _mapper.Map<ConfigurationItemDto>(configuration);
            return Result<ConfigurationItemDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<ConfigurationItemDto>.Failure(
                $"Error retrieving configuration: {ex.Message}");
        }
    }
}