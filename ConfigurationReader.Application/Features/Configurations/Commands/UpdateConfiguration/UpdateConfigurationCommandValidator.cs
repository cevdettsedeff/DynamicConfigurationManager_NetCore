using FluentValidation;

namespace ConfigurationReader.Application.Features.Configurations.Commands.UpdateConfiguration;

public class UpdateConfigurationCommandValidator : AbstractValidator<UpdateConfigurationCommand>
{
    public UpdateConfigurationCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id must be greater than 0");

        RuleFor(x => x.Dto.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.Dto.ApplicationName)
            .NotEmpty().WithMessage("ApplicationName is required")
            .MaximumLength(100).WithMessage("ApplicationName must not exceed 100 characters");

        RuleFor(x => x.Dto.Value)
            .NotEmpty().WithMessage("Value is required")
            .MaximumLength(5000).WithMessage("Value must not exceed 5000 characters");

    }
}