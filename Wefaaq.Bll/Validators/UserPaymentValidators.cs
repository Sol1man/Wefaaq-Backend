using FluentValidation;
using Wefaaq.Bll.DTOs;

namespace Wefaaq.Bll.Validators;

/// <summary>
/// Validator for UserPaymentCreateDto
/// </summary>
public class UserPaymentCreateDtoValidator : AbstractValidator<UserPaymentCreateDto>
{
    public UserPaymentCreateDtoValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0 (المبلغ يجب أن يكون أكبر من صفر)");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required (الوصف مطلوب)")
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters (الوصف لا يمكن أن يتجاوز 500 حرف)");
    }
}

/// <summary>
/// Validator for UserPaymentUpdateDto
/// </summary>
public class UserPaymentUpdateDtoValidator : AbstractValidator<UserPaymentUpdateDto>
{
    public UserPaymentUpdateDtoValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0 (المبلغ يجب أن يكون أكبر من صفر)");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required (الوصف مطلوب)")
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters (الوصف لا يمكن أن يتجاوز 500 حرف)");
    }
}
