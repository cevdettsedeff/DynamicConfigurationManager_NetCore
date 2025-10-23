// ConfigurationReader.Application/Features/Configurations/Commands/UpdateConfiguration/UpdateConfigurationCommandHandler.cs
using AutoMapper;
using ConfigurationReader.Application.Common;
using ConfigurationReader.Application.DTOs;
using ConfigurationReader.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace ConfigurationReader.Application.Features.Configurations.Commands.UpdateConfiguration;

public class UpdateConfigurationCommandHandler
    : IRequestHandler<UpdateConfigurationCommand, Result<ConfigurationItemDto>>
{
    private readonly IConfigurationRepository _repository;
    private readonly IMapper _mapper;
    private readonly IValidator<UpdateConfigurationCommand> _validator;

    public UpdateConfigurationCommandHandler(
        IConfigurationRepository repository,
        IMapper mapper,
        IValidator<UpdateConfigurationCommand> validator)
    {
        _repository = repository;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<Result<ConfigurationItemDto>> Handle(
        UpdateConfigurationCommand request,
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
            // Get existing entity
            var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (entity == null)
            {
                return Result<ConfigurationItemDto>.Failure(
                    $"Configuration with Id {request.Id} not found");
            }

            // ✅ Update using domain methods (Type değişmemeli!)
            entity.UpdateValue(request.Dto.Value);

            if (request.Dto.IsActive)
                entity.Activate();
            else
                entity.Deactivate();

            // Save
            await _repository.UpdateAsync(entity, cancellationToken);

            // Map to DTO
            var dto = _mapper.Map<ConfigurationItemDto>(entity);

            return Result<ConfigurationItemDto>.Success(
                dto,
                "Configuration updated successfully");
        }
        catch (Exception ex)
        {
            return Result<ConfigurationItemDto>.Failure(
                $"Error updating configuration: {ex.Message}");
        }
    }
}