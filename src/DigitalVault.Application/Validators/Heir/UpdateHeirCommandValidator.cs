using DigitalVault.Application.Commands.Heir;
using FluentValidation;

namespace DigitalVault.Application.Validators.Heir;

public class UpdateHeirCommandValidator : AbstractValidator<UpdateHeirCommand>
{
    public UpdateHeirCommandValidator()
    {
        RuleFor(x => x.FullName)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.FullName));

        RuleFor(x => x.Relationship)
            .MaximumLength(100)
            .When(x => x.Relationship != null);

        RuleFor(x => x.AccessLevel)
            .Must(x => x == "Full" || x == "Limited" || x == "ReadOnly")
            .WithMessage("Access level must be Full, Limited, or ReadOnly")
            .When(x => !string.IsNullOrWhiteSpace(x.AccessLevel));

        RuleFor(x => x.CanAccessCategories)
            .NotNull()
            .When(x => x.AccessLevel == "Limited")
            .WithMessage("CanAccessCategories is required when AccessLevel is Limited");
    }
}
