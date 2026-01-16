using DigitalVault.Application.Commands.Heir;
using FluentValidation;

namespace DigitalVault.Application.Validators.Heir;

public class AddHeirCommandValidator : AbstractValidator<AddHeirCommand>
{
    public AddHeirCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255);

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required")
            .MaximumLength(500);

        RuleFor(x => x.Relationship)
            .MaximumLength(100);

        RuleFor(x => x.PublicKey)
            .NotEmpty().WithMessage("Public key is required")
            .Must(BeValidBase64).WithMessage("Public key must be valid base64 encoded string");

        RuleFor(x => x.AccessLevel)
            .NotEmpty().WithMessage("Access level is required")
            .Must(x => x == "Full" || x == "Limited" || x == "ReadOnly")
            .WithMessage("Access level must be Full, Limited, or ReadOnly");

        RuleFor(x => x.CanAccessCategories)
            .NotNull()
            .When(x => x.AccessLevel == "Limited")
            .WithMessage("CanAccessCategories is required when AccessLevel is Limited");
    }

    private bool BeValidBase64(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        try
        {
            Convert.FromBase64String(value);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
