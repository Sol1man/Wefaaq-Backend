using FluentValidation;
using Wefaaq.Bll.DTOs;

namespace Wefaaq.Bll.Validators;

/// <summary>
/// Validator for ClientWithDetailsCreateDto
/// </summary>
public class ClientWithDetailsCreateDtoValidator : AbstractValidator<ClientWithDetailsCreateDto>
{
    public ClientWithDetailsCreateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Client name is required")
            .MaximumLength(200).WithMessage("Client name cannot exceed 200 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required")
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters");

        RuleFor(x => x.Classification)
            .IsInEnum().WithMessage("Invalid client classification");

        RuleFor(x => x.Balance)
            .NotNull().WithMessage("Balance is required");
    }
}

/// <summary>
/// Validator for ClientWithDetailsUpdateDto
/// </summary>
public class ClientWithDetailsUpdateDtoValidator : AbstractValidator<ClientWithDetailsUpdateDto>
{
    public ClientWithDetailsUpdateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Client name is required")
            .MaximumLength(200).WithMessage("Client name cannot exceed 200 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required")
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters");

        RuleFor(x => x.Classification)
            .IsInEnum().WithMessage("Invalid client classification");

        RuleFor(x => x.Balance)
            .NotNull().WithMessage("Balance is required");
    }
}

/// <summary>
/// Validator for ClientBranchWithDetailsCreateDto
/// </summary>
public class ClientBranchWithDetailsCreateDtoValidator : AbstractValidator<ClientBranchWithDetailsCreateDto>
{
    public ClientBranchWithDetailsCreateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Branch name is required")
            .MaximumLength(200).WithMessage("Branch name cannot exceed 200 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required")
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters");

        RuleFor(x => x.Classification)
            .IsInEnum().WithMessage("Invalid classification");

        RuleFor(x => x.Balance)
            .NotNull().WithMessage("Balance is required");
    }
}

/// <summary>
/// Validator for ClientBranchWithDetailsUpdateDto
/// </summary>
public class ClientBranchWithDetailsUpdateDtoValidator : AbstractValidator<ClientBranchWithDetailsUpdateDto>
{
    public ClientBranchWithDetailsUpdateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Branch name is required")
            .MaximumLength(200).WithMessage("Branch name cannot exceed 200 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required")
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters");

        RuleFor(x => x.Classification)
            .IsInEnum().WithMessage("Invalid classification");

        RuleFor(x => x.Balance)
            .NotNull().WithMessage("Balance is required");
    }
}
