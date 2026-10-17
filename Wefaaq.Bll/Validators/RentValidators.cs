using FluentValidation;
using Wefaaq.Bll.DTOs;

namespace Wefaaq.Bll.Validators;

/// <summary>
/// Validator for RentCreateDto. The renter itself is checked against the database in
/// the service, which also enforces that the FK matches the chosen target type.
/// </summary>
public class RentCreateDtoValidator : AbstractValidator<RentCreateDto>
{
    public RentCreateDtoValidator()
    {
        RuleFor(x => x.AssetName)
            .NotEmpty().WithMessage("Asset name is required (اسم الأصل مطلوب)")
            .MaximumLength(255).WithMessage("Asset name cannot exceed 255 characters (اسم الأصل لا يمكن أن يتجاوز 255 حرف)");

        RuleFor(x => x.MonthlyAmount)
            .GreaterThan(0).WithMessage("Monthly amount must be greater than 0 (قيمة الإيجار يجب أن تكون أكبر من صفر)");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters (الملاحظات لا يمكن أن تتجاوز 1000 حرف)");

        RuleFor(x => x.ExternalPersonIdNumber)
            .MaximumLength(50).WithMessage("ID number cannot exceed 50 characters (رقم الهوية لا يمكن أن يتجاوز 50 حرف)");
    }
}

/// <summary>
/// Validator for RentPaymentCreateDto
/// </summary>
public class RentPaymentCreateDtoValidator : AbstractValidator<RentPaymentCreateDto>
{
    public RentPaymentCreateDtoValidator()
    {
        RuleFor(x => x.RentId)
            .NotEmpty().WithMessage("Rent is required (الإيجار مطلوب)");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0 (المبلغ يجب أن يكون أكبر من صفر)");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters (الوصف لا يمكن أن يتجاوز 500 حرف)");
    }
}
