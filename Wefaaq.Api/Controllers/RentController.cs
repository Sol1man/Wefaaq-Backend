using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wefaaq.Bll.DTOs;
using Wefaaq.Bll.Interfaces;

namespace Wefaaq.Api.Controllers;

/// <summary>
/// Rent management (الإيجارات) — assets the company rents out to its clients, the monthly
/// amount owed for each, and the payments received against them.
/// Available to Admin and User, matching the client-operations screen.
/// </summary>
[ApiController]
[Route("api/rents")]
[Produces("application/json")]
[Authorize]
public class RentController : ControllerBase
{
    private readonly IRentService _rentService;
    private readonly ILogger<RentController> _logger;

    public RentController(IRentService rentService, ILogger<RentController> logger)
    {
        _rentService = rentService;
        _logger = logger;
    }

    /// <summary>Get every rent with its figures for one month</summary>
    [HttpGet("by-month")]
    [ProducesResponseType(typeof(List<RentRowDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByMonth([FromQuery] int year, [FromQuery] int month)
    {
        try
        {
            var rents = await _rentService.GetByMonthAsync(year, month);
            return Ok(rents);
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting rents for {Year}-{Month}", year, month);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Get one rent with its month-by-month dues and payment history</summary>
    [HttpGet("get/{id}")]
    [ProducesResponseType(typeof(RentDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] int year, [FromQuery] int month)
    {
        try
        {
            var rent = await _rentService.GetByIdAsync(id, year, month);
            if (rent == null)
            {
                return NotFound(new { message = $"Rent with ID {id} not found" });
            }
            return Ok(rent);
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting rent {RentId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Create a rent agreement</summary>
    [HttpPost("add")]
    [ProducesResponseType(typeof(RentRowDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] RentCreateDto dto)
    {
        try
        {
            var rent = await _rentService.CreateAsync(dto);
            return Ok(rent);
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(new { message = "Validation failed", errors = ex.Errors.Select(e => e.ErrorMessage) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating a rent");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Update a rent's asset, notes, agreed monthly amount or active flag</summary>
    [HttpPut("edit/{id}")]
    [ProducesResponseType(typeof(RentRowDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] RentUpdateDto dto)
    {
        try
        {
            var rent = await _rentService.UpdateAsync(id, dto);
            if (rent == null)
            {
                return NotFound(new { message = $"Rent with ID {id} not found" });
            }
            return Ok(rent);
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating rent {RentId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Delete a rent, its monthly dues and its payments (soft delete)</summary>
    [HttpDelete("delete/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var deleted = await _rentService.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = $"Rent with ID {id} not found" });
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting rent {RentId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Override a single month's rent amount</summary>
    [HttpPut("charges/set/{rentId}")]
    [ProducesResponseType(typeof(RentChargeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetCharge(Guid rentId, [FromBody] RentChargeSetDto dto)
    {
        try
        {
            var charge = await _rentService.SetChargeAsync(rentId, dto);
            if (charge == null)
            {
                return NotFound(new { message = $"Rent with ID {rentId} not found" });
            }
            return Ok(charge);
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while setting the monthly amount for rent {RentId}", rentId);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Record a payment against a rent (دفعة) — the whole month or part of it</summary>
    [HttpPost("payments/add")]
    [ProducesResponseType(typeof(RentPaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddPayment([FromBody] RentPaymentCreateDto dto)
    {
        try
        {
            var payment = await _rentService.AddPaymentAsync(dto);
            return Ok(payment);
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(new { message = "Validation failed", errors = ex.Errors.Select(e => e.ErrorMessage) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding a rent payment");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Delete a rent payment (soft delete)</summary>
    [HttpDelete("payments/delete/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePayment(Guid id)
    {
        try
        {
            var deleted = await _rentService.DeletePaymentAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = $"Rent payment with ID {id} not found" });
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting rent payment {PaymentId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }
}
