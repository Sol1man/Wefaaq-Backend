using FluentValidation;
using Wefaaq.Bll.DTOs;

namespace Wefaaq.Bll.Validators;

/// <summary>
/// Validator for OrganizationUsernameCreateDto
/// </summary>
public class OrganizationUsernameCreateDtoValidator : AbstractValidator<OrganizationUsernameCreateDto>
{
    public OrganizationUsernameCreateDtoValidator()
    {
        RuleFor(x => x.SiteName)
            .NotEmpty().WithMessage("Site name is required (اسم الموقع مطلوب)")
            .MaximumLength(255).WithMessage("Site name cannot exceed 255 characters");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required (اسم المستخدم مطلوب)")
            .MaximumLength(255).WithMessage("Username cannot exceed 255 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required (كلمة المرور مطلوبة)")
            .MaximumLength(500).WithMessage("Password cannot exceed 500 characters");
    }
}

/// <summary>
/// Validator for OrganizationUsernameUpdateDto
/// </summary>
public class OrganizationUsernameUpdateDtoValidator : AbstractValidator<OrganizationUsernameUpdateDto>
{
    public OrganizationUsernameUpdateDtoValidator()
    {
        RuleFor(x => x.SiteName)
            .NotEmpty().WithMessage("Site name is required (اسم الموقع مطلوب)")
            .MaximumLength(255).WithMessage("Site name cannot exceed 255 characters");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required (اسم المستخدم مطلوب)")
            .MaximumLength(255).WithMessage("Username cannot exceed 255 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required (كلمة المرور مطلوبة)")
            .MaximumLength(500).WithMessage("Password cannot exceed 500 characters");
    }
}
