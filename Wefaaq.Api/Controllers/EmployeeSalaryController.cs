using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wefaaq.Bll.DTOs;
using Wefaaq.Bll.Interfaces;

namespace Wefaaq.Api.Controllers;

/// <summary>
/// Employee salaries management (رواتب الموظفين). Admin-only except for
/// <see cref="GetMyDetails"/>, which lets a plain user read their own row.
/// </summary>
[ApiController]
[Route("api/employee-salaries")]
[Produces("application/json")]
[Authorize]
public class EmployeeSalaryController : ControllerBase
{
    private readonly IEmployeeSalaryService _service;
    private readonly ILogger<EmployeeSalaryController> _logger;

    public EmployeeSalaryController(IEmployeeSalaryService service, ILogger<EmployeeSalaryController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>Get every employee's salary figures for one month</summary>
    [HttpGet("by-month")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(List<EmployeeSalaryRowDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByMonth([FromQuery] int year, [FromQuery] int month)
    {
        try
        {
            var rows = await _service.GetMonthlyAsync(year, month);
            return Ok(rows);
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting employee salaries for {Year}-{Month}", year, month);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Get one employee's figures for a month, plus that month's deductions (latest first)</summary>
    [HttpGet("details/{type}/{id}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(EmployeeSalaryDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDetails(string type, string id, [FromQuery] int year, [FromQuery] int month)
    {
        try
        {
            var details = await _service.GetDetailsAsync(type, id, year, month);
            if (details == null)
            {
                return NotFound(new { message = $"Employee {type}/{id} not found" });
            }
            return Ok(details);
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting employee salary details for {Type}/{Id}", type, id);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get the signed-in user's own salary row for a month, plus that month's deductions.
    /// The only endpoint here a non-admin may call — the employee id comes from the token,
    /// never from the caller, so nobody can read someone else's payroll.
    /// </summary>
    [HttpGet("my-details")]
    [ProducesResponseType(typeof(EmployeeSalaryDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyDetails([FromQuery] int year, [FromQuery] int month)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { message = "User ID not found in claims" });
            }

            var details = await _service.GetDetailsAsync(
                EmployeeRefType.User, userId.Value.ToString(), year, month);

            if (details == null)
            {
                return NotFound(new { message = "Employee record not found" });
            }
            return Ok(details);
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting the current user's salary details");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Add an external employee — payroll only, no system account</summary>
    [HttpPost("external/add")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ExternalEmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateExternal([FromBody] ExternalEmployeeCreateDto dto)
    {
        try
        {
            var employee = await _service.CreateExternalAsync(dto);
            // GetByMonth is addressed by query params, so there is no meaningful location
            // header to hand back — return the created row directly.
            return Ok(employee);
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(new { message = "Validation failed", errors = ex.Errors.Select(e => e.ErrorMessage) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating external employee");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Delete an external employee (soft delete)</summary>
    [HttpDelete("external/delete/{id}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteExternal(Guid id)
    {
        try
        {
            var deleted = await _service.DeleteExternalAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = $"External employee with ID {id} not found" });
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting external employee {EmployeeId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Set an employee's monthly salary</summary>
    [HttpPut("salary/{type}/{id}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSalary(string type, string id, [FromBody] UpdateEmployeeSalaryDto dto)
    {
        try
        {
            var updated = await _service.UpdateSalaryAsync(type, id, dto.Salary);
            if (!updated)
            {
                return NotFound(new { message = $"Employee {type}/{id} not found" });
            }
            return NoContent();
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating salary for {Type}/{Id}", type, id);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Record a deduction against an employee</summary>
    [HttpPost("deductions/add")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(EmployeeDeductionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddDeduction([FromBody] EmployeeDeductionCreateDto dto)
    {
        try
        {
            var deduction = await _service.AddDeductionAsync(dto);
            return Ok(deduction);
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(new { message = "Validation failed", errors = ex.Errors.Select(e => e.ErrorMessage) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding an employee deduction");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Delete a deduction (soft delete)</summary>
    [HttpDelete("deductions/delete/{id}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDeduction(Guid id)
    {
        try
        {
            var deleted = await _service.DeleteDeductionAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = $"Deduction with ID {id} not found" });
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting deduction {DeductionId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Record a loan advanced to an employee</summary>
    [HttpPost("loans/add")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(EmployeeLoanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddLoan([FromBody] EmployeeLoanCreateDto dto)
    {
        try
        {
            var loan = await _service.AddLoanAsync(dto);
            return Ok(loan);
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(new { message = "Validation failed", errors = ex.Errors.Select(e => e.ErrorMessage) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding an employee loan");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Delete a loan (soft delete)</summary>
    [HttpDelete("loans/delete/{id}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteLoan(Guid id)
    {
        try
        {
            var deleted = await _service.DeleteLoanAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = $"Loan with ID {id} not found" });
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting loan {LoanId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get the current user's ID from claims
    /// </summary>
    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (int.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        return null;
    }
}
