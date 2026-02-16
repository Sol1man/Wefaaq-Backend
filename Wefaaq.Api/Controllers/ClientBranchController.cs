using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wefaaq.Bll.DTOs;
using Wefaaq.Bll.Interfaces;

namespace Wefaaq.Api.Controllers;

/// <summary>
/// Client branch management endpoints
/// </summary>
[ApiController]
[Route("api/client-branches")]
[Produces("application/json")]
[Authorize] // Require authentication for all endpoints
public class ClientBranchController : ControllerBase
{
    private readonly IClientBranchService _branchService;
    private readonly ILogger<ClientBranchController> _logger;

    public ClientBranchController(IClientBranchService branchService, ILogger<ClientBranchController> logger)
    {
        _branchService = branchService;
        _logger = logger;
    }

    /// <summary>
    /// Get all client branches
    /// </summary>
    [HttpGet("get-all")]
    [ProducesResponseType(typeof(List<ClientBranchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var branches = await _branchService.GetAllAsync();
            return Ok(branches);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all client branches");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get client branch by ID
    /// </summary>
    [HttpGet("get/{id}")]
    [ProducesResponseType(typeof(ClientBranchDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var branch = await _branchService.GetByIdAsync(id);
            if (branch == null)
            {
                return NotFound(new { message = $"Client branch with ID {id} not found" });
            }
            return Ok(branch);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting client branch with ID {BranchId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get client branch with full details (organizations, external workers)
    /// </summary>
    [HttpGet("details/{id}")]
    [ProducesResponseType(typeof(ClientBranchDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetWithDetails(Guid id)
    {
        try
        {
            var branch = await _branchService.GetWithDetailsAsync(id);
            if (branch == null)
            {
                return NotFound(new { message = $"Client branch with ID {id} not found" });
            }
            return Ok(branch);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting client branch details for ID {BranchId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get all branches for a specific client
    /// </summary>
    [HttpGet("by-client/{clientId}")]
    [ProducesResponseType(typeof(IEnumerable<ClientBranchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByClientId(Guid clientId)
    {
        try
        {
            var branches = await _branchService.GetByClientIdAsync(clientId);
            return Ok(branches);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting branches for client {ClientId}", clientId);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Create new client branch
    /// </summary>
    [HttpPost("add")]
    [ProducesResponseType(typeof(ClientBranchDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] ClientBranchCreateDto branchCreateDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var branch = await _branchService.CreateAsync(branchCreateDto);
            return CreatedAtAction(nameof(GetById), new { id = branch.Id }, branch);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating client branch");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update existing client branch
    /// </summary>
    [HttpPut("edit/{id}")]
    [ProducesResponseType(typeof(ClientBranchDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] ClientBranchUpdateDto branchUpdateDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var branch = await _branchService.UpdateAsync(id, branchUpdateDto);
            if (branch == null)
            {
                return NotFound(new { message = $"Client branch with ID {id} not found" });
            }
            return Ok(branch);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating client branch with ID {BranchId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete client branch (Admin only, soft delete)
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
            var result = await _branchService.DeleteAsync(id);
            if (!result)
            {
                return NotFound(new { message = $"Client branch with ID {id} not found" });
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting client branch with ID {BranchId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Add organization to existing branch
    /// </summary>
    [HttpPost("{branchId}/organizations")]
    [ProducesResponseType(typeof(OrganizationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddOrganizationToBranch(Guid branchId, [FromBody] OrganizationCreateDto organizationDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var organization = await _branchService.AddOrganizationToBranchAsync(branchId, organizationDto);
            return CreatedAtRoute(null, organization);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error occurred while adding organization to branch {BranchId}", branchId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding organization to branch {BranchId}", branchId);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Add external worker to existing branch
    /// </summary>
    [HttpPost("{branchId}/external-workers")]
    [ProducesResponseType(typeof(ExternalWorkerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddExternalWorkerToBranch(Guid branchId, [FromBody] ExternalWorkerCreateDto workerDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var worker = await _branchService.AddExternalWorkerToBranchAsync(branchId, workerDto);
            return CreatedAtRoute(null, worker);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error occurred while adding external worker to branch {BranchId}", branchId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding external worker to branch {BranchId}", branchId);
            return BadRequest(new { message = ex.Message });
        }
    }
}
