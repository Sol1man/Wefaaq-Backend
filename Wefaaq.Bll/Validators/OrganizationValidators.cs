using FluentValidation;
using Wefaaq.Bll.DTOs;

namespace Wefaaq.Bll.Validators;

/// <summary>
/// Validator for OrganizationCreateDto
/// </summary>
public class OrganizationCreateDtoValidator : AbstractValidator<OrganizationCreateDto>
{
    public OrganizationCreateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Organization name is required (اسم المؤسسة مطلوب)")
            .MinimumLength(2).WithMessage("Organization name must be at least 2 characters")
            .MaximumLength(255).WithMessage("Organization name cannot exceed 255 characters");
    }
}

/// <summary>
/// Validator for OrganizationUpdateDto
/// </summary>
public class OrganizationUpdateDtoValidator : AbstractValidator<OrganizationUpdateDto>
{
    public OrganizationUpdateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Organization name is required (اسم المؤسسة مطلوب)")
            .MinimumLength(2).WithMessage("Organization name must be at least 2 characters")
            .MaximumLength(255).WithMessage("Organization name cannot exceed 255 characters");
    }
}

/// <summary>
/// Validator for OrganizationRecordCreateDto
/// </summary>
public class OrganizationRecordCreateDtoValidator : AbstractValidator<OrganizationRecordCreateDto>
{
    public OrganizationRecordCreateDtoValidator()
    {
        RuleFor(x => x.Number)
            .NotEmpty().WithMessage("Record number is required (رقم السجل مطلوب)")
            .MaximumLength(100).WithMessage("Record number cannot exceed 100 characters");

        RuleFor(x => x.ExpiryDate)
            .GreaterThan(DateTime.Today).WithMessage("Expiry date must be in the future");

        RuleFor(x => x.ImagePath)
            .MaximumLength(500).WithMessage("Image path cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.ImagePath));
    }
}

/// <summary>
/// Validator for OrganizationRecordUpdateDto
/// </summary>
public class OrganizationRecordUpdateDtoValidator : AbstractValidator<OrganizationRecordUpdateDto>
{
    public OrganizationRecordUpdateDtoValidator()
    {
        RuleFor(x => x.Number)
            .NotEmpty().WithMessage("Record number is required (رقم السجل مطلوب)")
            .MaximumLength(100).WithMessage("Record number cannot exceed 100 characters");

        RuleFor(x => x.ExpiryDate)
            .GreaterThan(DateTime.Today).WithMessage("Expiry date must be in the future");

        RuleFor(x => x.ImagePath)
            .MaximumLength(500).WithMessage("Image path cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.ImagePath));
    }
}

/// <summary>
/// Validator for OrganizationLicenseCreateDto
/// </summary>
public class OrganizationLicenseCreateDtoValidator : AbstractValidator<OrganizationLicenseCreateDto>
{
    public OrganizationLicenseCreateDtoValidator()
    {
        RuleFor(x => x.Number)
            .NotEmpty().WithMessage("License number is required (رقم الترخيص مطلوب)")
            .MaximumLength(100).WithMessage("License number cannot exceed 100 characters");

        RuleFor(x => x.ExpiryDate)
            .GreaterThan(DateTime.Today).WithMessage("Expiry date must be in the future");

        RuleFor(x => x.ImagePath)
            .MaximumLength(500).WithMessage("Image path cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.ImagePath));
    }
}

/// <summary>
/// Validator for OrganizationLicenseUpdateDto
/// </summary>
public class OrganizationLicenseUpdateDtoValidator : AbstractValidator<OrganizationLicenseUpdateDto>
{
    public OrganizationLicenseUpdateDtoValidator()
    {
        RuleFor(x => x.Number)
            .NotEmpty().WithMessage("License number is required (رقم الترخيص مطلوب)")
            .MaximumLength(100).WithMessage("License number cannot exceed 100 characters");

        RuleFor(x => x.ExpiryDate)
            .GreaterThan(DateTime.Today).WithMessage("Expiry date must be in the future");

        RuleFor(x => x.ImagePath)
            .MaximumLength(500).WithMessage("Image path cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.ImagePath));
    }
}

/// <summary>
/// Validator for OrganizationWorkerCreateDto
/// </summary>
public class OrganizationWorkerCreateDtoValidator : AbstractValidator<OrganizationWorkerCreateDto>
{
    public OrganizationWorkerCreateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Worker name is required (اسم العامل مطلوب)")
            .MaximumLength(255).WithMessage("Worker name cannot exceed 255 characters");

        RuleFor(x => x.ResidenceNumber)
            .NotEmpty().WithMessage("Residence number is required (رقم الإقامة مطلوب)")
            .MaximumLength(50).WithMessage("Residence number cannot exceed 50 characters");

        RuleFor(x => x.ExpiryDate)
            .GreaterThan(DateTime.Today).WithMessage("Expiry date must be in the future");

        RuleFor(x => x.ResidenceImagePath)
            .MaximumLength(500).WithMessage("Residence image path cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.ResidenceImagePath));
    }
}

/// <summary>
/// Validator for OrganizationWorkerUpdateDto
/// </summary>
public class OrganizationWorkerUpdateDtoValidator : AbstractValidator<OrganizationWorkerUpdateDto>
{
    public OrganizationWorkerUpdateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Worker name is required (اسم العامل مطلوب)")
            .MaximumLength(255).WithMessage("Worker name cannot exceed 255 characters");

        RuleFor(x => x.ResidenceNumber)
            .NotEmpty().WithMessage("Residence number is required (رقم الإقامة مطلوب)")
            .MaximumLength(50).WithMessage("Residence number cannot exceed 50 characters");

        RuleFor(x => x.ExpiryDate)
            .GreaterThan(DateTime.Today).WithMessage("Expiry date must be in the future");

        RuleFor(x => x.ResidenceImagePath)
            .MaximumLength(500).WithMessage("Residence image path cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.ResidenceImagePath));
    }
}

/// <summary>
/// Validator for OrganizationCarCreateDto
/// </summary>
public class OrganizationCarCreateDtoValidator : AbstractValidator<OrganizationCarCreateDto>
{
    public OrganizationCarCreateDtoValidator()
    {
        RuleFor(x => x.PlateNumber)
            .NotEmpty().WithMessage("Plate number is required (رقم اللوحة مطلوب)")
            .MaximumLength(20).WithMessage("Plate number cannot exceed 20 characters");

        RuleFor(x => x.Color)
            .NotEmpty().WithMessage("Car color is required (لون السيارة مطلوب)")
            .MaximumLength(50).WithMessage("Car color cannot exceed 50 characters");

        RuleFor(x => x.SerialNumber)
            .NotEmpty().WithMessage("Serial number is required (الرقم التسلسلي مطلوب)")
            .MaximumLength(50).WithMessage("Serial number cannot exceed 50 characters");

        RuleFor(x => x.OperatingCardExpiry)
            .GreaterThan(DateTime.Today).WithMessage("Operating card expiry date must be in the future");

        RuleFor(x => x.ImagePath)
            .MaximumLength(500).WithMessage("Image path cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.ImagePath));
    }
}

/// <summary>
/// Validator for OrganizationCarUpdateDto
/// </summary>
public class OrganizationCarUpdateDtoValidator : AbstractValidator<OrganizationCarUpdateDto>
{
    public OrganizationCarUpdateDtoValidator()
    {
        RuleFor(x => x.PlateNumber)
            .NotEmpty().WithMessage("Plate number is required (رقم اللوحة مطلوب)")
            .MaximumLength(20).WithMessage("Plate number cannot exceed 20 characters");

        RuleFor(x => x.Color)
            .NotEmpty().WithMessage("Car color is required (لون السيارة مطلوب)")
            .MaximumLength(50).WithMessage("Car color cannot exceed 50 characters");

        RuleFor(x => x.SerialNumber)
            .NotEmpty().WithMessage("Serial number is required (الرقم التسلسلي مطلوب)")
            .MaximumLength(50).WithMessage("Serial number cannot exceed 50 characters");

        RuleFor(x => x.OperatingCardExpiry)
            .GreaterThan(DateTime.Today).WithMessage("Operating card expiry date must be in the future");

        RuleFor(x => x.ImagePath)
            .MaximumLength(500).WithMessage("Image path cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.ImagePath));
    }
}