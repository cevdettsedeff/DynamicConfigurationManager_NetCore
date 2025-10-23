using AutoMapper;
using ConfigurationReader.Application.Common;
using ConfigurationReader.Application.DTOs;
using ConfigurationReader.Domain.Entities;
using ConfigurationReader.Domain.Enums;
using ConfigurationReader.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace ConfigurationReader.Application.Features.Configurations.Commands.CreateConfiguration;

public class CreateConfigurationCommandHandler
    : IRequestHandler<CreateConfigurationCommand, Result<ConfigurationItemDto>>
{
    private readonly IConfigurationRepository _repository;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateConfigurationCommand> _validator;

    public CreateConfigurationCommandHandler(
        IConfigurationRepository repository,
        IMapper mapper,
        IValidator<CreateConfigurationCommand> validator)
    {
        _repository = repository;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<Result<ConfigurationItemDto>> Handle(
        CreateConfigurationCommand request,
        CancellationToken cancellationToken)
    {
        // Validate
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return Result<ConfigurationItemDto>.Failure(errors);
        }

        try
        {
            // Check if already exists
            var exists = await _repository.ExistsAsync(
                request.Dto.ApplicationName,
                request.Dto.Name,
                cancellationToken);

            if (exists)
            {
                return Result<ConfigurationItemDto>.Failure(
                    $"Configuration '{request.Dto.Name}' already exists for application '{request.Dto.ApplicationName}'");
            }

            // Create entity using factory method
            var type = Enum.Parse<ConfigurationType>(request.Dto.Type, ignoreCase: true);
            var entity = ConfigurationItem.Create(
                request.Dto.Name,
                type,
                request.Dto.Value,
                request.Dto.ApplicationName,
                request.Dto.IsActive
            );

            // Save to database
            var created = await _repository.AddAsync(entity, cancellationToken);

            // Map to DTO
            var dto = _mapper.Map<ConfigurationItemDto>(created);

            return Result<ConfigurationItemDto>.Success(dto, "Configuration created successfully");
        }
        catch (Exception ex)
        {
            return Result<ConfigurationItemDto>.Failure(
                $"Error creating configuration: {ex.Message}");
        }
    }
}