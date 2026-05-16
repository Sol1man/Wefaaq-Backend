using FluentValidation;
using Wefaaq.Bll.DTOs;
using Wefaaq.Dal.Entities;

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
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters (الوصف لا يمكن أن يتجاوز 500 حرف)");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid payment type (نوع الدفع غير صالح)");

        // Only Profit rows may carry a RelatedPaymentId — a Payment cannot link to another Payment.
        RuleFor(x => x.RelatedPaymentId)
            .Null().When(x => x.Type == UserPaymentType.Payment)
            .WithMessage("Payment entries cannot reference another payment (لا يمكن ربط الدفع بدفع آخر)");
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

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid payment type (نوع الدفع غير صالح)");

        RuleFor(x => x.RelatedPaymentId)
            .Null().When(x => x.Type == UserPaymentType.Payment)
            .WithMessage("Payment entries cannot reference another payment (لا يمكن ربط الدفع بدفع آخر)");
    }
}
