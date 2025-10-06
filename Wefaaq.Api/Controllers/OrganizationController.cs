using Microsoft.AspNetCore.Mvc;
using Wefaaq.Bll.DTOs;
using Wefaaq.Bll.Interfaces;

namespace Wefaaq.Api.Controllers;

/// <summary>
/// Organization management endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
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
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrganizationDto>>> GetAll()
    {
        var organizations = await _organizationService.GetAllAsync();
        return Ok(organizations);
    }

    /// <summary>
    /// Get organization by ID
    /// </summary>
    /// <param name="id">Organization ID</param>
    /// <returns>Organization details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganizationDto>> GetById(Guid id)
    {
        var organization = await _organizationService.GetByIdAsync(id);
        if (organization == null)
        {
            return NotFound(new { message = $"Organization with ID {id} not found" });
        }
        return Ok(organization);
    }

    /// <summary>
    /// Get organization with all details
    /// </summary>
    /// <param name="id">Organization ID</param>
    /// <returns>Organization with all details</returns>
    [HttpGet("{id}/details")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganizationDto>> GetWithDetails(Guid id)
    {
        var organization = await _organizationService.GetWithDetailsAsync(id);
        if (organization == null)
        {
            return NotFound(new { message = $"Organization with ID {id} not found" });
        }
        return Ok(organization);
    }

    /// <summary>
    /// Get organizations with expiring cards
    /// </summary>
    /// <returns>List of organizations with expiring cards</returns>
    [HttpGet("expiring-cards")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrganizationDto>>> GetWithExpiringCards()
    {
        var organizations = await _organizationService.GetWithExpiringCardsAsync();
        return Ok(organizations);
    }

    /// <summary>
    /// Create new organization
    /// </summary>
    /// <param name="organizationCreateDto">Organization creation data</param>
    /// <returns>Created organization</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrganizationDto>> Create([FromBody] OrganizationCreateDto organizationCreateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var organization = await _organizationService.CreateAsync(organizationCreateDto);
        return CreatedAtAction(nameof(GetById), new { id = organization.Id }, organization);
    }

    /// <summary>
    /// Update existing organization
    /// </summary>
    /// <param name="id">Organization ID</param>
    /// <param name="organizationUpdateDto">Organization update data</param>
    /// <returns>Updated organization</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganizationDto>> Update(Guid id, [FromBody] OrganizationUpdateDto organizationUpdateDto)
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

    /// <summary>
    /// Delete organization
    /// </summary>
    /// <param name="id">Organization ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _organizationService.DeleteAsync(id);
        if (!result)
        {
            return NotFound(new { message = $"Organization with ID {id} not found" });
        }
        return NoContent();
    }

    #endregion

    #region Organization Records

    /// <summary>
    /// Add record to organization
    /// </summary>
    /// <param name="organizationId">Organization ID</param>
    /// <param name="recordCreateDto">Record creation data</param>
    /// <returns>Created record</returns>
    [HttpPost("{organizationId}/records")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganizationRecordDto>> AddRecord(Guid organizationId, [FromBody] OrganizationRecordCreateDto recordCreateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var record = await _organizationService.AddRecordAsync(organizationId, recordCreateDto);
            return CreatedAtAction(nameof(GetById), new { id = organizationId }, record);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Organization with ID {organizationId} not found" });
        }
    }

    /// <summary>
    /// Update organization record
    /// </summary>
    /// <param name="organizationId">Organization ID</param>
    /// <param name="recordId">Record ID</param>
    /// <param name="recordUpdateDto">Record update data</param>
    /// <returns>Updated record</returns>
    [HttpPut("{organizationId}/records/{recordId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganizationRecordDto>> UpdateRecord(Guid organizationId, Guid recordId, [FromBody] OrganizationRecordUpdateDto recordUpdateDto)
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

    /// <summary>
    /// Delete organization record
    /// </summary>
    /// <param name="organizationId">Organization ID</param>
    /// <param name="recordId">Record ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{organizationId}/records/{recordId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRecord(Guid organizationId, Guid recordId)
    {
        var result = await _organizationService.DeleteRecordAsync(organizationId, recordId);
        if (!result)
        {
            return NotFound(new { message = $"Record with ID {recordId} not found for organization {organizationId}" });
        }
        return NoContent();
    }

    #endregion

    #region Organization Workers

    /// <summary>
    /// Add worker to organization
    /// </summary>
    /// <param name="organizationId">Organization ID</param>
    /// <param name="workerCreateDto">Worker creation data</param>
    /// <returns>Created worker</returns>
    [HttpPost("{organizationId}/workers")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganizationWorkerDto>> AddWorker(Guid organizationId, [FromBody] OrganizationWorkerCreateDto workerCreateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var worker = await _organizationService.AddWorkerAsync(organizationId, workerCreateDto);
            return CreatedAtAction(nameof(GetById), new { id = organizationId }, worker);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Organization with ID {organizationId} not found" });
        }
    }

    /// <summary>
    /// Update organization worker
    /// </summary>
    /// <param name="organizationId">Organization ID</param>
    /// <param name="workerId">Worker ID</param>
    /// <param name="workerUpdateDto">Worker update data</param>
    /// <returns>Updated worker</returns>
    [HttpPut("{organizationId}/workers/{workerId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganizationWorkerDto>> UpdateWorker(Guid organizationId, Guid workerId, [FromBody] OrganizationWorkerUpdateDto workerUpdateDto)
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

    /// <summary>
    /// Delete organization worker
    /// </summary>
    /// <param name="organizationId">Organization ID</param>
    /// <param name="workerId">Worker ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{organizationId}/workers/{workerId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteWorker(Guid organizationId, Guid workerId)
    {
        var result = await _organizationService.DeleteWorkerAsync(organizationId, workerId);
        if (!result)
        {
            return NotFound(new { message = $"Worker with ID {workerId} not found for organization {organizationId}" });
        }
        return NoContent();
    }

    #endregion
}
