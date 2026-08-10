using FluentValidation;
using Wefaaq.Bll.DTOs;

namespace Wefaaq.Bll.Validators;

/// <summary>
/// Validator for CostCreateDto
/// </summary>
public class CostCreateDtoValidator : AbstractValidator<CostCreateDto>
{
    public CostCreateDtoValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0 (المبلغ يجب أن يكون أكبر من صفر)");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters (الوصف لا يمكن أن يتجاوز 500 حرف)");
    }
}
