using DigitalVault.Application.Commands.Vault;
using DigitalVault.Domain.Enums;
using FluentValidation;

namespace DigitalVault.Application.Validators.Vault;

public class CreateVaultEntryCommandValidator : AbstractValidator<CreateVaultEntryCommand>
{
    public CreateVaultEntryCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(500).WithMessage("Title must not exceed 500 characters");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required")
            .Must(BeValidCategory).WithMessage("Invalid category");

        RuleFor(x => x.EncryptedDataKey)
            .NotEmpty().WithMessage("Encrypted data key is required");

        RuleFor(x => x.IV)
            .NotEmpty().WithMessage("Initialization vector is required");
    }

    private bool BeValidCategory(string category)
    {
        return Enum.TryParse<VaultCategory>(category, out _);
    }
}
