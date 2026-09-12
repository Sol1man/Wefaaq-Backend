using FluentValidation;
using Wefaaq.Bll.DTOs;

namespace Wefaaq.Bll.Validators;

/// <summary>
/// Validator for ExternalEmployeeCreateDto
/// </summary>
public class ExternalEmployeeCreateDtoValidator : AbstractValidator<ExternalEmployeeCreateDto>
{
    public ExternalEmployeeCreateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required (الاسم مطلوب)")
            .MaximumLength(255).WithMessage("Name cannot exceed 255 characters (الاسم لا يمكن أن يتجاوز 255 حرف)");

        RuleFor(x => x.Salary)
            .GreaterThanOrEqualTo(0).WithMessage("Salary cannot be negative (لا يمكن أن يكون الراتب سالباً)");
    }
}

/// <summary>
/// Validator for EmployeeDeductionCreateDto
/// </summary>
public class EmployeeDeductionCreateDtoValidator : AbstractValidator<EmployeeDeductionCreateDto>
{
    public EmployeeDeductionCreateDtoValidator()
    {
        RuleFor(x => x.Type)
            .Must(t => t == EmployeeRefType.User || t == EmployeeRefType.External)
            .WithMessage("Employee type must be 'user' or 'external' (نوع الموظف غير صالح)");

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Employee is required (الموظف مطلوب)");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0 (المبلغ يجب أن يكون أكبر من صفر)");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters (الوصف لا يمكن أن يتجاوز 500 حرف)");
    }
}

/// <summary>
/// Validator for EmployeeLoanCreateDto
/// </summary>
public class EmployeeLoanCreateDtoValidator : AbstractValidator<EmployeeLoanCreateDto>
{
    public EmployeeLoanCreateDtoValidator()
    {
        RuleFor(x => x.Type)
            .Must(t => t == EmployeeRefType.User || t == EmployeeRefType.External)
            .WithMessage("Employee type must be 'user' or 'external' (نوع الموظف غير صالح)");

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Employee is required (الموظف مطلوب)");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0 (المبلغ يجب أن يكون أكبر من صفر)");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters (الوصف لا يمكن أن يتجاوز 500 حرف)");
    }
}
