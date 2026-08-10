using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wefaaq.Bll.DTOs;
using Wefaaq.Bll.Interfaces;

namespace Wefaaq.Api.Controllers;

/// <summary>
/// Business cost / expense management (المصروفات). Admin only.
/// </summary>
[ApiController]
[Route("api/costs")]
[Produces("application/json")]
[Authorize(Policy = "AdminOnly")]
public class CostController : ControllerBase
{
    private readonly ICostService _costService;
    private readonly ILogger<CostController> _logger;

    public CostController(ICostService costService, ILogger<CostController> logger)
    {
        _costService = costService;
        _logger = logger;
    }

    /// <summary>Get all costs</summary>
    [HttpGet("get-all")]
    [ProducesResponseType(typeof(List<CostDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var costs = await _costService.GetAllAsync();
            return Ok(costs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all costs");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Get costs within a date range</summary>
    [HttpGet("by-date-range")]
    [ProducesResponseType(typeof(List<CostDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByDateRange([FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        try
        {
            var costs = await _costService.GetByDateRangeAsync(from, to);
            return Ok(costs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting costs by date range");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Create a new cost</summary>
    [HttpPost("add")]
    [ProducesResponseType(typeof(CostDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CostCreateDto dto)
    {
        try
        {
            var cost = await _costService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetAll), cost);
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(new { message = "Validation failed", errors = ex.Errors.Select(e => e.ErrorMessage) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating cost");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Delete a cost (soft delete)</summary>
    [HttpDelete("delete/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var deleted = await _costService.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = $"Cost with ID {id} not found" });
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting cost with ID {CostId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }
}
