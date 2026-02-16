using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wefaaq.Bll.DTOs;
using Wefaaq.Bll.Interfaces;

namespace Wefaaq.Api.Controllers;

/// <summary>
/// Organization management endpoints
/// </summary>
[ApiController]
[Route("api/organizations")]
[Produces("application/json")]
[Authorize] // Require authentication for all endpoints
public class OrganizationController : ControllerBase
{
	private readonly IOrganizationService _organizationService;
	private readonly ILogger<OrganizationController> _logger;

	public OrganizationController(IOrganizationService organizationService, ILogger<OrganizationController> logger)
	{
		_organizationService = organizationService;
		_logger = logger;
	}

	#region Organization CRUD

	/// <summary>
	/// Get all organizations
	/// </summary>
	/// <returns>List of organizations</returns>
	[HttpGet("get-all")]
	[ProducesResponseType(typeof(List<OrganizationDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<IActionResult> GetAll()
	{
		try
		{
			var organizations = await _organizationService.GetAllAsync();
			return Ok(organizations);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while getting all organizations");
			return BadRequest(new { message = ex.Message });
		}
	}

	/// <summary>
	/// Get organization by ID
	/// </summary>
	/// <param name="id">Organization ID</param>
	/// <returns>Organization details</returns>
	[HttpGet("get/{id}")]
	[ProducesResponseType(typeof(OrganizationDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> GetById(Guid id)
	{
		try
		{
			var organization = await _organizationService.GetByIdAsync(id);
			if (organization == null)
			{
				return NotFound(new { message = $"Organization with ID {id} not found" });
			}
			return Ok(organization);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while getting organization with ID {OrganizationId}", id);
			return BadRequest(new { message = ex.Message });
		}
	}

	/// <summary>
	/// Get organization with all details
	/// </summary>
	/// <param name="id">Organization ID</param>
	/// <returns>Organization with all details</returns>
	[HttpGet("details/{id}")]
	[ProducesResponseType(typeof(OrganizationDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> GetWithDetails(Guid id)
	{
		try
		{
			var organization = await _organizationService.GetWithDetailsAsync(id);
			if (organization == null)
			{
				return NotFound(new { message = $"Organization with ID {id} not found" });
			}
			return Ok(organization);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while getting organization details for ID {OrganizationId}", id);
			return BadRequest(new { message = ex.Message });
		}
	}

	/// <summary>
	/// Get organizations with expiring cards
	/// </summary>
	/// <returns>List of organizations with expiring cards</returns>
	[HttpGet("expiring-cards")]
	[ProducesResponseType(typeof(IEnumerable<OrganizationDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<IActionResult> GetWithExpiringCards()
	{
		try
		{
			var organizations = await _organizationService.GetWithExpiringCardsAsync();
			return Ok(organizations);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while getting organizations with expiring cards");
			return BadRequest(new { message = ex.Message });
		}
	}

	/// <summary>
	/// Create new organization
	/// </summary>
	/// <param name="organizationCreateDto">Organization creation data</param>
	/// <returns>Created organization</returns>
	[HttpPost("add")]
	[ProducesResponseType(typeof(OrganizationDto), StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<IActionResult> Create([FromBody] OrganizationCreateDto organizationCreateDto)
	{
		try
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var organization = await _organizationService.CreateAsync(organizationCreateDto);
			return CreatedAtAction(nameof(GetById), new { id = organization.Id }, organization);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while creating organization");
			return BadRequest(new { message = ex.Message });
		}
	}

	/// <summary>
	/// Update existing organization
	/// </summary>
	/// <param name="id">Organization ID</param>
	/// <param name="organizationUpdateDto">Organization update data</param>
	/// <returns>Updated organization</returns>
	[HttpPut("edit/{id}")]
	[ProducesResponseType(typeof(OrganizationDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> Update(Guid id, [FromBody] OrganizationUpdateDto organizationUpdateDto)
	{
		try
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var organization = await _organizationService.UpdateAsync(id, organizationUpdateDto);
			if (organization == null)
			{
				return NotFound(new { message = $"Organization with ID {id} not found" });
			}
			return Ok(organization);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while updating organization with ID {OrganizationId}", id);
			return BadRequest(new { message = ex.Message });
		}
	}

	/// <summary>
	/// Delete organization (Admin only)
	/// </summary>
	/// <param name="id">Organization ID</param>
	/// <returns>No content on success</returns>
	[HttpDelete("delete/{id}")]
	[Authorize(Policy = "AdminOnly")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	public async Task<IActionResult> Delete(Guid id)
	{
		try
		{
			var result = await _organizationService.DeleteAsync(id);
			if (!result)
			{
				return NotFound(new { message = $"Organization with ID {id} not found" });
			}
			return NoContent();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while deleting organization with ID {OrganizationId}", id);
			return BadRequest(new { message = ex.Message });
		}
	}

	#endregion

	#region Organization Records

	/// <summary>
	/// Add record to organization
	/// </summary>
	/// <param name="organizationId">Organization ID</param>
	/// <param name="recordCreateDto">Record creation data</param>
	/// <returns>Created record</returns>
	[HttpPost("records/add/{organizationId}")]
	[ProducesResponseType(typeof(OrganizationRecordDto), StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> AddRecord(Guid organizationId, [FromBody] OrganizationRecordCreateDto recordCreateDto)
	{
		try
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var record = await _organizationService.AddRecordAsync(organizationId, recordCreateDto);
			return CreatedAtAction(nameof(GetById), new { id = organizationId }, record);
		}
		catch (KeyNotFoundException)
		{
			return NotFound(new { message = $"Organization with ID {organizationId} not found" });
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while adding record to organization with ID {OrganizationId}", organizationId);
			return BadRequest(new { message = ex.Message });
		}
	}

	/// <summary>
	/// Update organization record
	/// </summary>
	/// <param name="organizationId">Organization ID</param>
	/// <param name="recordId">Record ID</param>
	/// <param name="recordUpdateDto">Record update data</param>
	/// <returns>Updated record</returns>
	[HttpPut("records/edit/{organizationId}/{recordId}")]
	[ProducesResponseType(typeof(OrganizationRecordDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> UpdateRecord(Guid organizationId, Guid recordId, [FromBody] OrganizationRecordUpdateDto recordUpdateDto)
	{
		try
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var record = await _organizationService.UpdateRecordAsync(organizationId, recordId, recordUpdateDto);
			if (record == null)
			{
				return NotFound(new { message = $"Record with ID {recordId} not found for organization {organizationId}" });
			}
			return Ok(record);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while updating record {RecordId} for organization {OrganizationId}", recordId, organizationId);
			return BadRequest(new { message = ex.Message });
		}
	}

	/// <summary>
	/// Delete organization record (Admin only)
	/// </summary>
	/// <param name="organizationId">Organization ID</param>
	/// <param name="recordId">Record ID</param>
	/// <returns>No content on success</returns>
	[HttpDelete("records/delete/{organizationId}/{recordId}")]
	[Authorize(Policy = "AdminOnly")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	public async Task<IActionResult> DeleteRecord(Guid organizationId, Guid recordId)
	{
		try
		{
			var result = await _organizationService.DeleteRecordAsync(organizationId, recordId);
			if (!result)
			{
				return NotFound(new { message = $"Record with ID {recordId} not found for organization {organizationId}" });
			}
			return NoContent();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while deleting record {RecordId} from organization {OrganizationId}", recordId, organizationId);
			return BadRequest(new { message = ex.Message });
		}
	}

	#endregion

	#region Organization Workers

	/// <summary>
	/// Add worker to organization
	/// </summary>
	/// <param name="organizationId">Organization ID</param>
	/// <param name="workerCreateDto">Worker creation data</param>
	/// <returns>Created worker</returns>
	[HttpPost("workers/add/{organizationId}")]
	[ProducesResponseType(typeof(OrganizationWorkerDto), StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> AddWorker(Guid organizationId, [FromBody] OrganizationWorkerCreateDto workerCreateDto)
	{
		try
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var worker = await _organizationService.AddWorkerAsync(organizationId, workerCreateDto);
			return CreatedAtAction(nameof(GetById), new { id = organizationId }, worker);
		}
		catch (KeyNotFoundException)
		{
			return NotFound(new { message = $"Organization with ID {organizationId} not found" });
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while adding worker to organization with ID {OrganizationId}", organizationId);
			return BadRequest(new { message = ex.Message });
		}
	}

	/// <summary>
	/// Update organization worker
	/// </summary>
	/// <param name="organizationId">Organization ID</param>
	/// <param name="workerId">Worker ID</param>
	/// <param name="workerUpdateDto">Worker update data</param>
	/// <returns>Updated worker</returns>
	[HttpPut("workers/edit/{organizationId}/{workerId}")]
	[ProducesResponseType(typeof(OrganizationWorkerDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> UpdateWorker(Guid organizationId, Guid workerId, [FromBody] OrganizationWorkerUpdateDto workerUpdateDto)
	{
		try
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var worker = await _organizationService.UpdateWorkerAsync(organizationId, workerId, workerUpdateDto);
			if (worker == null)
			{
				return NotFound(new { message = $"Worker with ID {workerId} not found for organization {organizationId}" });
			}
			return Ok(worker);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while updating worker {WorkerId} for organization {OrganizationId}", workerId, organizationId);
			return BadRequest(new { message = ex.Message });
		}
	}

	/// <summary>
	/// Delete organization worker (Admin only)
	/// </summary>
	/// <param name="organizationId">Organization ID</param>
	/// <param name="workerId">Worker ID</param>
	/// <returns>No content on success</returns>
	[HttpDelete("workers/delete/{organizationId}/{workerId}")]
	[Authorize(Policy = "AdminOnly")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	public async Task<IActionResult> DeleteWorker(Guid organizationId, Guid workerId)
	{
		try
		{
			var result = await _organizationService.DeleteWorkerAsync(organizationId, workerId);
			if (!result)
			{
				return NotFound(new { message = $"Worker with ID {workerId} not found for organization {organizationId}" });
			}
			return NoContent();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while deleting worker {WorkerId} from organization {OrganizationId}", workerId, organizationId);
			return BadRequest(new { message = ex.Message });
		}
	}

	#endregion

	#region Organization Usernames

	/// <summary>
	/// Add username to organization
	/// </summary>
	/// <param name="organizationId">Organization ID</param>
	/// <param name="usernameCreateDto">Username creation data</param>
	/// <returns>Created username</returns>
	[HttpPost("usernames/add/{organizationId}")]
	[ProducesResponseType(typeof(OrganizationUsernameDto), StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> AddUsername(Guid organizationId, [FromBody] OrganizationUsernameCreateDto usernameCreateDto)
	{
		try
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var username = await _organizationService.AddUsernameAsync(organizationId, usernameCreateDto);
			return CreatedAtAction(nameof(GetById), new { id = organizationId }, username);
		}
		catch (KeyNotFoundException)
		{
			return NotFound(new { message = $"Organization with ID {organizationId} not found" });
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while adding username to organization with ID {OrganizationId}", organizationId);
			return BadRequest(new { message = ex.Message });
		}
	}

	/// <summary>
	/// Update organization username
	/// </summary>
	/// <param name="organizationId">Organization ID</param>
	/// <param name="usernameId">Username ID</param>
	/// <param name="usernameUpdateDto">Username update data</param>
	/// <returns>Updated username</returns>
	[HttpPut("usernames/edit/{organizationId}/{usernameId}")]
	[ProducesResponseType(typeof(OrganizationUsernameDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> UpdateUsername(Guid organizationId, Guid usernameId, [FromBody] OrganizationUsernameUpdateDto usernameUpdateDto)
	{
		try
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var username = await _organizationService.UpdateUsernameAsync(organizationId, usernameId, usernameUpdateDto);
			if (username == null)
			{
				return NotFound(new { message = $"Username with ID {usernameId} not found for organization {organizationId}" });
			}
			return Ok(username);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while updating username {UsernameId} for organization {OrganizationId}", usernameId, organizationId);
			return BadRequest(new { message = ex.Message });
		}
	}

	/// <summary>
	/// Delete organization username (Admin only)
	/// </summary>
	/// <param name="organizationId">Organization ID</param>
	/// <param name="usernameId">Username ID</param>
	/// <returns>No content on success</returns>
	[HttpDelete("usernames/delete/{organizationId}/{usernameId}")]
	[Authorize(Policy = "AdminOnly")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	public async Task<IActionResult> DeleteUsername(Guid organizationId, Guid usernameId)
	{
		try
		{
			var result = await _organizationService.DeleteUsernameAsync(organizationId, usernameId);
			if (!result)
			{
				return NotFound(new { message = $"Username with ID {usernameId} not found for organization {organizationId}" });
			}
			return NoContent();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while deleting username {UsernameId} from organization {OrganizationId}", usernameId, organizationId);
			return BadRequest(new { message = ex.Message });
		}
	}

	#endregion
}
