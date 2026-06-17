using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wefaaq.Bll.DTOs;
using Wefaaq.Bll.Interfaces;

namespace Wefaaq.Api.Controllers;

/// <summary>
/// Client operations management (عمليات العملاء)
/// </summary>
[ApiController]
[Route("api/client-operations")]
[Produces("application/json")]
[Authorize]
public class ClientOperationController : ControllerBase
{
    private readonly IClientOperationService _service;
    private readonly ILogger<ClientOperationController> _logger;

    public ClientOperationController(IClientOperationService service, ILogger<ClientOperationController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>Get all operations (Admin only)</summary>
    [HttpGet("get-all")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all client operations");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Get single operation by ID</summary>
    [HttpGet("get/{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound(new { message = $"Operation {id} not found" });
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting operation {Id}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get all operations related to a client (direct + branches + organizations)
    /// </summary>
    [HttpGet("by-client/{clientId}")]
    public async Task<IActionResult> GetByClient(Guid clientId)
    {
        try
        {
            var result = await _service.GetByClientAsync(clientId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting operations for client {ClientId}", clientId);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Get operations for a branch and its organizations</summary>
    [HttpGet("by-branch/{branchId}")]
    public async Task<IActionResult> GetByBranch(Guid branchId)
    {
        try
        {
            var result = await _service.GetByBranchAsync(branchId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting operations for branch {BranchId}", branchId);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Get operations for an organization</summary>
    [HttpGet("by-organization/{organizationId}")]
    public async Task<IActionResult> GetByOrganization(Guid organizationId)
    {
        try
        {
            var result = await _service.GetByOrganizationAsync(organizationId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting operations for organization {OrganizationId}", organizationId);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Create a new operation</summary>
    [HttpPost("add")]
    public async Task<IActionResult> Create([FromBody] ClientOperationCreateDto dto)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized(new { message = "Cannot determine current user" });

            var result = await _service.CreateAsync(dto, userId.Value);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating client operation");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Record a client payment (cash paid against fees) — credits the target balance</summary>
    [HttpPost("add-payment")]
    public async Task<IActionResult> CreatePayment([FromBody] ClientOperationPaymentCreateDto dto)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized(new { message = "Cannot determine current user" });

            var result = await _service.CreatePaymentAsync(dto, userId.Value);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording client payment");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Update an operation (type, price, notes)</summary>
    [HttpPut("edit/{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ClientOperationUpdateDto dto)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _service.UpdateAsync(id, dto);
            if (result == null) return NotFound(new { message = $"Operation {id} not found" });
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating operation {Id}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Delete an operation</summary>
    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _service.DeleteAsync(id);
            if (!result) return NotFound(new { message = $"Operation {id} not found" });
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting operation {Id}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
