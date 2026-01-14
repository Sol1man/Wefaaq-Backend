using FluentValidation;
using Wefaaq.Bll.DTOs;

namespace Wefaaq.Bll.Validators;

/// <summary>
/// Validator for ClientBranchCreateDto
/// </summary>
public class ClientBranchCreateDtoValidator : AbstractValidator<ClientBranchCreateDto>
{
    public ClientBranchCreateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Branch name is required (اسم الفرع مطلوب)")
            .MinimumLength(2).WithMessage("Branch name must be at least 2 characters")
            .MaximumLength(255).WithMessage("Branch name cannot exceed 255 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required (الإيميل مطلوب)")
            .EmailAddress().WithMessage("Email must be a valid email address (يجب أن يكون الإيميل صحيحاً)")
            .MaximumLength(255).WithMessage("Email cannot exceed 255 characters");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.Classification)
            .IsInEnum().WithMessage("Invalid classification");

        RuleFor(x => x.ParentClientId)
            .NotEmpty().WithMessage("Parent client is required (العميل الأساسي مطلوب)");

        RuleFor(x => x.BranchType)
            .MaximumLength(100).WithMessage("Branch type cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.BranchType));
    }
}

/// <summary>
/// Validator for ClientBranchUpdateDto
/// </summary>
public class ClientBranchUpdateDtoValidator : AbstractValidator<ClientBranchUpdateDto>
{
    public ClientBranchUpdateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Branch name is required (اسم الفرع مطلوب)")
            .MinimumLength(2).WithMessage("Branch name must be at least 2 characters")
            .MaximumLength(255).WithMessage("Branch name cannot exceed 255 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required (الإيميل مطلوب)")
            .EmailAddress().WithMessage("Email must be a valid email address (يجب أن يكون الإيميل صحيحاً)")
            .MaximumLength(255).WithMessage("Email cannot exceed 255 characters");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.Classification)
            .IsInEnum().WithMessage("Invalid classification");

        RuleFor(x => x.ParentClientId)
            .NotEmpty().WithMessage("Parent client is required (العميل الأساسي مطلوب)");

        RuleFor(x => x.BranchType)
            .MaximumLength(100).WithMessage("Branch type cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.BranchType));
    }
}
