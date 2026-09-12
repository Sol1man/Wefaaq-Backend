namespace Wefaaq.Bll.DTOs;

/// <summary>
/// One row of the employee salaries table for a given month (رواتب الموظفين).
/// Covers both system users and external employees — <see cref="Type"/> says which.
/// </summary>
public class EmployeeSalaryRowDto
{
    /// <summary>"user" or "external" — together with Id this addresses the employee</summary>
    public string Type { get; set; } = EmployeeRefType.User;

    /// <summary>User id (as a string) or external employee Guid (as a string)</summary>
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    /// <summary>Only populated for system users</summary>
    public string? Email { get; set; }

    /// <summary>Monthly salary</summary>
    public decimal Salary { get; set; }

    /// <summary>
    /// The employee's cut of the profit they generated in the selected month:
    /// ProfitPercentage% of their own Profit-type payment rows. Always 0 for external
    /// employees — they have no payment history.
    /// </summary>
    public decimal Profit { get; set; }

    /// <summary>Profit share percentage used to compute <see cref="Profit"/></summary>
    public decimal ProfitPercentage { get; set; }

    /// <summary>Raw sum of the employee's Profit-type rows for the month (before the percentage)</summary>
    public decimal ProfitBase { get; set; }

    /// <summary>Sum of the employee's deductions dated inside the selected month</summary>
    public decimal Deduction { get; set; }

    /// <summary>
    /// Sum of the employee's loans dated inside the selected month. A loan carries no
    /// profit or interest — it is subtracted from the month's pay just like a deduction.
    /// </summary>
    public decimal Loan { get; set; }

    /// <summary>Salary + Profit − Deduction − Loan</summary>
    public decimal Net { get; set; }
}

/// <summary>
/// Full detail view for one employee in one month: the salary row plus the month's
/// deductions and loans, each newest first and kept as separate lists.
/// </summary>
public class EmployeeSalaryDetailsDto : EmployeeSalaryRowDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public List<EmployeeDeductionDto> Deductions { get; set; } = new();
    public List<EmployeeLoanDto> Loans { get; set; } = new();
}

/// <summary>
/// Salary deduction response DTO (خصم)
/// </summary>
public class EmployeeDeductionDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime DeductionDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for adding a deduction to an employee. DeductionDate is optional — defaults to "now".
/// </summary>
public class EmployeeDeductionCreateDto
{
    /// <summary>"user" or "external"</summary>
    public string Type { get; set; } = EmployeeRefType.User;

    /// <summary>User id or external employee Guid, as a string</summary>
    public string Id { get; set; } = string.Empty;

    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public DateTime? DeductionDate { get; set; }
}

/// <summary>
/// Employee loan response DTO (سلفة)
/// </summary>
public class EmployeeLoanDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime LoanDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for recording a loan against an employee. LoanDate is optional — defaults to "now".
/// </summary>
public class EmployeeLoanCreateDto
{
    /// <summary>"user" or "external"</summary>
    public string Type { get; set; } = EmployeeRefType.User;

    /// <summary>User id or external employee Guid, as a string</summary>
    public string Id { get; set; } = string.Empty;

    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public DateTime? LoanDate { get; set; }
}

/// <summary>
/// External employee response DTO (موظف خارجي)
/// </summary>
public class ExternalEmployeeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for creating an external employee — just a name and a salary, no account.
/// </summary>
public class ExternalEmployeeCreateDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Salary { get; set; }
}

/// <summary>
/// DTO for setting an employee's monthly salary
/// </summary>
public class UpdateEmployeeSalaryDto
{
    public decimal Salary { get; set; }
}

/// <summary>
/// The two kinds of employee a salary row can point at.
/// </summary>
public static class EmployeeRefType
{
    public const string User = "user";
    public const string External = "external";
}
