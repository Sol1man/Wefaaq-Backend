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

    }
}

/// <summary>
/// Validator for ClientWithOrganizationsCreateDto
/// </summary>
public class ClientWithOrganizationsCreateDtoValidator : AbstractValidator<ClientWithOrganizationsCreateDto>
{
    public ClientWithOrganizationsCreateDtoValidator()
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


        RuleFor(x => x.Organizations)
            .NotNull().WithMessage("Organizations list cannot be null");

        RuleForEach(x => x.Organizations).ChildRules(org =>
        {
            org.RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Organization name is required (اسم المؤسسة مطلوب)")
                .MinimumLength(2).WithMessage("Organization name must be at least 2 characters")
                .MaximumLength(255).WithMessage("Organization name cannot exceed 255 characters");

            org.RuleFor(x => x.Records).NotNull();
            org.RuleForEach(x => x.Records).ChildRules(record =>
            {
                record.RuleFor(x => x.Number)
                    .NotEmpty().WithMessage("Record number is required (رقم السجل مطلوب)")
                    .MaximumLength(100).WithMessage("Record number cannot exceed 100 characters");
                record.RuleFor(x => x.ExpiryDate)
                    .GreaterThan(DateTime.Today).WithMessage("Expiry date must be in the future");
                record.RuleFor(x => x.ImagePath)
                    .MaximumLength(500).WithMessage("Image path cannot exceed 500 characters")
                    .When(x => !string.IsNullOrEmpty(x.ImagePath));
            });

            org.RuleFor(x => x.Licenses).NotNull();
            org.RuleForEach(x => x.Licenses).ChildRules(license =>
            {
                license.RuleFor(x => x.Number)
                    .NotEmpty().WithMessage("License number is required (رقم الترخيص مطلوب)")
                    .MaximumLength(100).WithMessage("License number cannot exceed 100 characters");
                license.RuleFor(x => x.ExpiryDate)
                    .GreaterThan(DateTime.Today).WithMessage("Expiry date must be in the future");
                license.RuleFor(x => x.ImagePath)
                    .MaximumLength(500).WithMessage("Image path cannot exceed 500 characters")
                    .When(x => !string.IsNullOrEmpty(x.ImagePath));
            });

            org.RuleFor(x => x.Workers).NotNull();
            org.RuleForEach(x => x.Workers).ChildRules(worker =>
            {
                worker.RuleFor(x => x.Name)
                    .NotEmpty().WithMessage("Worker name is required (اسم العامل مطلوب)")
                    .MaximumLength(255).WithMessage("Worker name cannot exceed 255 characters");
                worker.RuleFor(x => x.ResidenceNumber)
                    .NotEmpty().WithMessage("Residence number is required (رقم الإقامة مطلوب)")
                    .MaximumLength(50).WithMessage("Residence number cannot exceed 50 characters");
                worker.RuleFor(x => x.ExpiryDate)
                    .GreaterThan(DateTime.Today).WithMessage("Expiry date must be in the future");
                worker.RuleFor(x => x.ResidenceImagePath)
                    .MaximumLength(500).WithMessage("Residence image path cannot exceed 500 characters")
                    .When(x => !string.IsNullOrEmpty(x.ResidenceImagePath));
            });

            org.RuleFor(x => x.Cars).NotNull();
            org.RuleForEach(x => x.Cars).ChildRules(car =>
            {
                car.RuleFor(x => x.PlateNumber)
                    .NotEmpty().WithMessage("Plate number is required (رقم اللوحة مطلوب)")
                    .MaximumLength(20).WithMessage("Plate number cannot exceed 20 characters");
                car.RuleFor(x => x.Color)
                    .NotEmpty().WithMessage("Car color is required (لون السيارة مطلوب)")
                    .MaximumLength(50).WithMessage("Car color cannot exceed 50 characters");
                car.RuleFor(x => x.SerialNumber)
                    .NotEmpty().WithMessage("Serial number is required (الرقم التسلسلي مطلوب)")
                    .MaximumLength(50).WithMessage("Serial number cannot exceed 50 characters");
                car.RuleFor(x => x.OperatingCardExpiry)
                    .GreaterThan(DateTime.Today).WithMessage("Operating card expiry date must be in the future");
                car.RuleFor(x => x.ImagePath)
                    .MaximumLength(500).WithMessage("Image path cannot exceed 500 characters")
                    .When(x => !string.IsNullOrEmpty(x.ImagePath));
            });
        });
    }
}

/// <summary>
/// Validator for ClientWithOrganizationsUpdateDto
/// </summary>
public class ClientWithOrganizationsUpdateDtoValidator : AbstractValidator<ClientWithOrganizationsUpdateDto>
{
    public ClientWithOrganizationsUpdateDtoValidator()
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


        RuleFor(x => x.Organizations)
            .NotNull().WithMessage("Organizations list cannot be null");

        RuleForEach(x => x.Organizations).ChildRules(org =>
        {
            org.RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Organization name is required (اسم المؤسسة مطلوب)")
                .MinimumLength(2).WithMessage("Organization name must be at least 2 characters")
                .MaximumLength(255).WithMessage("Organization name cannot exceed 255 characters");

            org.RuleFor(x => x.Records).NotNull();
            org.RuleForEach(x => x.Records).ChildRules(record =>
            {
                record.RuleFor(x => x.Number)
                    .NotEmpty().WithMessage("Record number is required (رقم السجل مطلوب)")
                    .MaximumLength(100).WithMessage("Record number cannot exceed 100 characters");
                record.RuleFor(x => x.ExpiryDate)
                    .GreaterThan(DateTime.Today).WithMessage("Expiry date must be in the future");
                record.RuleFor(x => x.ImagePath)
                    .MaximumLength(500).WithMessage("Image path cannot exceed 500 characters")
                    .When(x => !string.IsNullOrEmpty(x.ImagePath));
            });

            org.RuleFor(x => x.Licenses).NotNull();
            org.RuleForEach(x => x.Licenses).ChildRules(license =>
            {
                license.RuleFor(x => x.Number)
                    .NotEmpty().WithMessage("License number is required (رقم الترخيص مطلوب)")
                    .MaximumLength(100).WithMessage("License number cannot exceed 100 characters");
                license.RuleFor(x => x.ExpiryDate)
                    .GreaterThan(DateTime.Today).WithMessage("Expiry date must be in the future");
                license.RuleFor(x => x.ImagePath)
                    .MaximumLength(500).WithMessage("Image path cannot exceed 500 characters")
                    .When(x => !string.IsNullOrEmpty(x.ImagePath));
            });

            org.RuleFor(x => x.Workers).NotNull();
            org.RuleForEach(x => x.Workers).ChildRules(worker =>
            {
                worker.RuleFor(x => x.Name)
                    .NotEmpty().WithMessage("Worker name is required (اسم العامل مطلوب)")
                    .MaximumLength(255).WithMessage("Worker name cannot exceed 255 characters");
                worker.RuleFor(x => x.ResidenceNumber)
                    .NotEmpty().WithMessage("Residence number is required (رقم الإقامة مطلوب)")
                    .MaximumLength(50).WithMessage("Residence number cannot exceed 50 characters");
                worker.RuleFor(x => x.ExpiryDate)
                    .GreaterThan(DateTime.Today).WithMessage("Expiry date must be in the future");
                worker.RuleFor(x => x.ResidenceImagePath)
                    .MaximumLength(500).WithMessage("Residence image path cannot exceed 500 characters")
                    .When(x => !string.IsNullOrEmpty(x.ResidenceImagePath));
            });

            org.RuleFor(x => x.Cars).NotNull();
            org.RuleForEach(x => x.Cars).ChildRules(car =>
            {
                car.RuleFor(x => x.PlateNumber)
                    .NotEmpty().WithMessage("Plate number is required (رقم اللوحة مطلوب)")
                    .MaximumLength(20).WithMessage("Plate number cannot exceed 20 characters");
                car.RuleFor(x => x.Color)
                    .NotEmpty().WithMessage("Car color is required (لون السيارة مطلوب)")
                    .MaximumLength(50).WithMessage("Car color cannot exceed 50 characters");
                car.RuleFor(x => x.SerialNumber)
                    .NotEmpty().WithMessage("Serial number is required (الرقم التسلسلي مطلوب)")
                    .MaximumLength(50).WithMessage("Serial number cannot exceed 50 characters");
                car.RuleFor(x => x.OperatingCardExpiry)
                    .GreaterThan(DateTime.Today).WithMessage("Operating card expiry date must be in the future");
                car.RuleFor(x => x.ImagePath)
                    .MaximumLength(500).WithMessage("Image path cannot exceed 500 characters")
                    .When(x => !string.IsNullOrEmpty(x.ImagePath));
            });
        });
    }
}