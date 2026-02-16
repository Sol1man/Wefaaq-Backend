using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wefaaq.Bll.DTOs;
using Wefaaq.Bll.Interfaces;

namespace Wefaaq.Api.Controllers;

/// <summary>
/// External worker management endpoints
/// </summary>
[ApiController]
[Route("api/external-workers")]
[Produces("application/json")]
[Authorize] // Require authentication for all endpoints
public class ExternalWorkerController : ControllerBase
{
    private readonly IExternalWorkerService _workerService;
    private readonly ILogger<ExternalWorkerController> _logger;

    public ExternalWorkerController(IExternalWorkerService workerService, ILogger<ExternalWorkerController> logger)
    {
        _workerService = workerService;
        _logger = logger;
    }

    /// <summary>
    /// Get all external workers
    /// </summary>
    [HttpGet("get-all")]
    [ProducesResponseType(typeof(List<ExternalWorkerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var workers = await _workerService.GetAllAsync();
            return Ok(workers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all external workers");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get external worker by ID
    /// </summary>
    [HttpGet("get/{id}")]
    [ProducesResponseType(typeof(ExternalWorkerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var worker = await _workerService.GetByIdAsync(id);
            if (worker == null)
            {
                return NotFound(new { message = $"External worker with ID {id} not found" });
            }
            return Ok(worker);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting external worker with ID {WorkerId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get all external workers for a specific client
    /// </summary>
    [HttpGet("by-client/{clientId}")]
    [ProducesResponseType(typeof(IEnumerable<ExternalWorkerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByClientId(Guid clientId)
    {
        try
        {
            var workers = await _workerService.GetByClientIdAsync(clientId);
            return Ok(workers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting workers for client {ClientId}", clientId);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get all external workers for a specific client branch
    /// </summary>
    [HttpGet("by-branch/{branchId}")]
    [ProducesResponseType(typeof(IEnumerable<ExternalWorkerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByClientBranchId(Guid branchId)
    {
        try
        {
            var workers = await _workerService.GetByClientBranchIdAsync(branchId);
            return Ok(workers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting workers for branch {BranchId}", branchId);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Create new external worker
    /// </summary>
    [HttpPost("add")]
    [ProducesResponseType(typeof(ExternalWorkerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] ExternalWorkerCreateDto workerCreateDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var worker = await _workerService.CreateAsync(workerCreateDto);
            return CreatedAtAction(nameof(GetById), new { id = worker.Id }, worker);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating external worker");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update existing external worker
    /// </summary>
    [HttpPut("edit/{id}")]
    [ProducesResponseType(typeof(ExternalWorkerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] ExternalWorkerUpdateDto workerUpdateDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var worker = await _workerService.UpdateAsync(id, workerUpdateDto);
            if (worker == null)
            {
                return NotFound(new { message = $"External worker with ID {id} not found" });
            }
            return Ok(worker);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating external worker with ID {WorkerId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete external worker (Admin only, soft delete)
    /// </summary>
    [HttpDelete("delete/{id}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _workerService.DeleteAsync(id);
            if (!result)
            {
                return NotFound(new { message = $"External worker with ID {id} not found" });
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting external worker with ID {WorkerId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }
}
