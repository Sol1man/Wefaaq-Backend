using Microsoft.AspNetCore.Mvc;
using Wefaaq.Bll.DTOs;
using Wefaaq.Bll.Interfaces;

namespace Wefaaq.Api.Controllers;

/// <summary>
/// Client management endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ClientController : ControllerBase
{
    private readonly IClientService _clientService;
    private readonly ILogger<ClientController> _logger;

    public ClientController(IClientService clientService, ILogger<ClientController> logger)
    {
        _clientService = clientService;
        _logger = logger;
    }

    /// <summary>
    /// Get all clients
    /// </summary>
    /// <returns>List of clients</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ClientDto>>> GetAll()
    {
        var clients = await _clientService.GetAllAsync();
        return Ok(clients);
    }

    /// <summary>
    /// Get client by ID
    /// </summary>
    /// <param name="id">Client ID</param>
    /// <returns>Client details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientDto>> GetById(Guid id)
    {
        var client = await _clientService.GetByIdAsync(id);
        if (client == null)
        {
            return NotFound(new { message = $"Client with ID {id} not found" });
        }
        return Ok(client);
    }

    /// <summary>
    /// Get client with organizations
    /// </summary>
    /// <param name="id">Client ID</param>
    /// <returns>Client with organizations</returns>
    [HttpGet("{id}/organizations")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientDto>> GetWithOrganizations(Guid id)
    {
        var client = await _clientService.GetWithOrganizationsAsync(id);
        if (client == null)
        {
            return NotFound(new { message = $"Client with ID {id} not found" });
        }
        return Ok(client);
    }

    /// <summary>
    /// Get clients with positive balance (creditors)
    /// </summary>
    /// <returns>List of creditor clients</returns>
    [HttpGet("creditors")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ClientDto>>> GetCreditors()
    {
        var clients = await _clientService.GetCreditorsAsync();
        return Ok(clients);
    }

    /// <summary>
    /// Get clients with negative balance (debtors)
    /// </summary>
    /// <returns>List of debtor clients</returns>
    [HttpGet("debtors")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ClientDto>>> GetDebtors()
    {
        var clients = await _clientService.GetDebtorsAsync();
        return Ok(clients);
    }

    /// <summary>
    /// Create new client
    /// </summary>
    /// <param name="clientCreateDto">Client creation data</param>
    /// <returns>Created client</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ClientDto>> Create([FromBody] ClientCreateDto clientCreateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var client = await _clientService.CreateAsync(clientCreateDto);
        return CreatedAtAction(nameof(GetById), new { id = client.Id }, client);
    }

    /// <summary>
    /// Update existing client
    /// </summary>
    /// <param name="id">Client ID</param>
    /// <param name="clientUpdateDto">Client update data</param>
    /// <returns>Updated client</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientDto>> Update(Guid id, [FromBody] ClientUpdateDto clientUpdateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var client = await _clientService.UpdateAsync(id, clientUpdateDto);
        if (client == null)
        {
            return NotFound(new { message = $"Client with ID {id} not found" });
        }
        return Ok(client);
    }

    /// <summary>
    /// Delete client
    /// </summary>
    /// <param name="id">Client ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _clientService.DeleteAsync(id);
        if (!result)
        {
            return NotFound(new { message = $"Client with ID {id} not found" });
        }
        return NoContent();
    }
}
