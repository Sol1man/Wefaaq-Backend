using FluentValidation;
using Wefaaq.Bll.DTOs;

namespace Wefaaq.Bll.Validators;

/// <summary>
/// Validator for ExternalWorkerCreateDto
/// </summary>
public class ExternalWorkerCreateDtoValidator : AbstractValidator<ExternalWorkerCreateDto>
{
    public ExternalWorkerCreateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Worker name is required (اسم العامل مطلوب)")
            .MinimumLength(2).WithMessage("Worker name must be at least 2 characters")
            .MaximumLength(255).WithMessage("Worker name cannot exceed 255 characters");

        RuleFor(x => x.WorkerType)
            .IsInEnum().WithMessage("Invalid worker type (نوع العامل غير صحيح)");

        RuleFor(x => x.ResidenceNumber)
            .MaximumLength(50).WithMessage("Residence number cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.ResidenceNumber));

        RuleFor(x => x.ResidenceImagePath)
            .MaximumLength(500).WithMessage("Residence image path cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.ResidenceImagePath));

        RuleFor(x => x.ExpiryDate)
            .GreaterThan(DateTime.Today).WithMessage("Expiry date must be in the future")
            .When(x => x.ExpiryDate.HasValue);

        // Must have either ClientId or ClientBranchId, but not both
        RuleFor(x => x)
            .Must(x => x.ClientId.HasValue || x.ClientBranchId.HasValue)
            .WithMessage("Worker must belong to either a Client or a Client Branch")
            .Must(x => !(x.ClientId.HasValue && x.ClientBranchId.HasValue))
            .WithMessage("Worker cannot belong to both a Client and a Client Branch");
    }
}

/// <summary>
/// Validator for ExternalWorkerUpdateDto
/// </summary>
public class ExternalWorkerUpdateDtoValidator : AbstractValidator<ExternalWorkerUpdateDto>
{
    public ExternalWorkerUpdateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Worker name is required (اسم العامل مطلوب)")
            .MinimumLength(2).WithMessage("Worker name must be at least 2 characters")
            .MaximumLength(255).WithMessage("Worker name cannot exceed 255 characters");

        RuleFor(x => x.WorkerType)
            .IsInEnum().WithMessage("Invalid worker type (نوع العامل غير صحيح)");

        RuleFor(x => x.ResidenceNumber)
            .MaximumLength(50).WithMessage("Residence number cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.ResidenceNumber));

        RuleFor(x => x.ResidenceImagePath)
            .MaximumLength(500).WithMessage("Residence image path cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.ResidenceImagePath));

        RuleFor(x => x.ExpiryDate)
            .GreaterThan(DateTime.Today).WithMessage("Expiry date must be in the future")
            .When(x => x.ExpiryDate.HasValue);

        // Must have either ClientId or ClientBranchId, but not both
        RuleFor(x => x)
            .Must(x => x.ClientId.HasValue || x.ClientBranchId.HasValue)
            .WithMessage("Worker must belong to either a Client or a Client Branch")
            .Must(x => !(x.ClientId.HasValue && x.ClientBranchId.HasValue))
            .WithMessage("Worker cannot belong to both a Client and a Client Branch");
    }
}
