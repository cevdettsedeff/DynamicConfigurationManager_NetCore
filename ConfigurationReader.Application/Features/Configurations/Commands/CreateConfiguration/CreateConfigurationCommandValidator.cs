using FluentValidation;

namespace ConfigurationReader.Application.Features.Configurations.Commands.CreateConfiguration;

public class CreateConfigurationCommandValidator : AbstractValidator<CreateConfigurationCommand>
{
    public CreateConfigurationCommandValidator()
    {
        RuleFor(x => x.Dto.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters")
            .Matches("^[a-zA-Z0-9_-]+$").WithMessage("Name can only contain letters, numbers, hyphens and underscores");

        RuleFor(x => x.Dto.ApplicationName)
            .NotEmpty().WithMessage("ApplicationName is required")
            .MaximumLength(100).WithMessage("ApplicationName must not exceed 100 characters")
            .Matches("^[a-zA-Z0-9_-]+$").WithMessage("ApplicationName can only contain letters, numbers, hyphens and underscores");

        RuleFor(x => x.Dto.Type)
            .NotEmpty().WithMessage("Type is required")
            .Must(BeValidType).WithMessage("Type must be one of: string, int, bool, double");

        RuleFor(x => x.Dto.Value)
            .NotEmpty().WithMessage("Value is required")
            .MaximumLength(5000).WithMessage("Value must not exceed 5000 characters");

        // Custom validation: Value must match the Type
        RuleFor(x => x.Dto)
            .Must(dto => ValidateValueForType(dto.Value, dto.Type))
            .WithMessage(dto => $"Value '{dto.Dto.Value}' is not valid for type '{dto.Dto.Type}'");
    }

    private bool BeValidType(string type)
    {
        var validTypes = new[] { "string", "int", "bool", "double" };
        return validTypes.Contains(type.ToLower());
    }

    private bool ValidateValueForType(string value, string type)
    {
        return type.ToLower() switch
        {
            "string" => true,
            "int" => int.TryParse(value, out _),
            "bool" => bool.TryParse(value, out _),
            "double" => double.TryParse(value, out _),
            _ => false
        };
    }
}