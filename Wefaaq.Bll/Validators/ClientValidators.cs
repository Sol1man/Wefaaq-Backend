using FluentValidation;
using Wefaaq.Bll.DTOs;
using Wefaaq.Dal.Entities;

namespace Wefaaq.Bll.Validators;

/// <summary>
/// Validator for ClientCreateDto
/// </summary>
public class ClientCreateDtoValidator : AbstractValidator<ClientCreateDto>
{
    public ClientCreateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Client name is required (اسم العميل مطلوب)")
            .MinimumLength(2).WithMessage("Client name must be at least 2 characters (اسم العميل يجب أن يكون على الأقل حرفين)")
            .MaximumLength(255).WithMessage("Client name cannot exceed 255 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required (الإيميل مطلوب)")
            .EmailAddress().WithMessage("Email must be a valid email address (يجب أن يكون الإيميل صحيحاً)")
            .MaximumLength(255).WithMessage("Email cannot exceed 255 characters");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.Classification)
            .IsInEnum().WithMessage("Invalid client classification");

        RuleFor(x => x.ExternalWorkersCount)
            .GreaterThanOrEqualTo(0).WithMessage("External workers count cannot be negative");
    }
}

/// <summary>
/// Validator for ClientUpdateDto
/// </summary>
public class ClientUpdateDtoValidator : AbstractValidator<ClientUpdateDto>
{
    public ClientUpdateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Client name is required (اسم العميل مطلوب)")
            .MinimumLength(2).WithMessage("Client name must be at least 2 characters (اسم العميل يجب أن يكون على الأقل حرفين)")
            .MaximumLength(255).WithMessage("Client name cannot exceed 255 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required (الإيميل مطلوب)")
            .EmailAddress().WithMessage("Email must be a valid email address (يجب أن يكون الإيميل صحيحاً)")
            .MaximumLength(255).WithMessage("Email cannot exceed 255 characters");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.Classification)
            .IsInEnum().WithMessage("Invalid client classification");

        RuleFor(x => x.ExternalWorkersCount)
            .GreaterThanOrEqualTo(0).WithMessage("External workers count cannot be negative");
    }
}