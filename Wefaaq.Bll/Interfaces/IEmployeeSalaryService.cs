using Wefaaq.Bll.DTOs;

namespace Wefaaq.Bll.Interfaces;

/// <summary>
/// Employee salaries service interface (رواتب الموظفين)
/// </summary>
public interface IEmployeeSalaryService
{
    /// <summary>All employees (system users + external) with their figures for the given month</summary>
    Task<IEnumerable<EmployeeSalaryRowDto>> GetMonthlyAsync(int year, int month);

    /// <summary>One employee's figures for the given month, plus that month's deductions</summary>
    Task<EmployeeSalaryDetailsDto?> GetDetailsAsync(string type, string id, int year, int month);

    /// <summary>Add an external employee (payroll-only, no system account)</summary>
    Task<ExternalEmployeeDto> CreateExternalAsync(ExternalEmployeeCreateDto dto);

    /// <summary>Delete an external employee (soft delete). System users are never deleted here.</summary>
    Task<bool> DeleteExternalAsync(Guid id);

    /// <summary>Set an employee's monthly salary</summary>
    Task<bool> UpdateSalaryAsync(string type, string id, decimal salary);

    /// <summary>Record a deduction against an employee</summary>
    Task<EmployeeDeductionDto> AddDeductionAsync(EmployeeDeductionCreateDto dto);

    /// <summary>Delete a deduction (soft delete)</summary>
    Task<bool> DeleteDeductionAsync(Guid id);
}
