using Microsoft.AspNetCore.Mvc;
using Wefaaq.Bll.DTOs;
using Wefaaq.Bll.Interfaces;

namespace Wefaaq.Api.Controllers;

/// <summary>
/// Client management endpoints
/// </summary>
[ApiController]
[Route("api/clients")]
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

    #region Basic CRUD Operations

    /// <summary>
    /// Get all clients
    /// </summary>
    /// <returns>List of clients</returns>
    [HttpGet("get-all")]
    [ProducesResponseType(typeof(List<ClientDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var clients = await _clientService.GetAllAsync();
            return Ok(clients);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all clients");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get client by ID
    /// </summary>
    /// <param name="id">Client ID</param>
    /// <returns>Client details</returns>
    [HttpGet("get/{id}")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var client = await _clientService.GetByIdAsync(id);
            if (client == null)
            {
                return NotFound(new { message = $"Client with ID {id} not found" });
            }
            return Ok(client);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting client with ID {ClientId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Create new client
    /// </summary>
    /// <param name="clientCreateDto">Client creation data</param>
    /// <returns>Created client</returns>
    [HttpPost("add")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] ClientCreateDto clientCreateDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var client = await _clientService.CreateAsync(clientCreateDto);
            return CreatedAtAction(nameof(GetById), new { id = client.Id }, client);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating client");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update existing client
    /// </summary>
    /// <param name="clientUpdateDto">Client update data</param>
    /// <param name="id">Client ID</param>
    /// <returns>Updated client</returns>
    [HttpPut("edit/{id}")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] ClientUpdateDto clientUpdateDto)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating client with ID {ClientId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete client
    /// </summary>
    /// <param name="id">Client ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("delete/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _clientService.DeleteAsync(id);
            if (!result)
            {
                return NotFound(new { message = $"Client with ID {id} not found" });
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting client with ID {ClientId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    #endregion

    #region Client with Organizations

    /// <summary>
    /// Get client with organizations
    /// </summary>
    /// <param name="id">Client ID</param>
    /// <returns>Client with organizations</returns>
    [HttpGet("organizations/{id}")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWithOrganizations(Guid id)
    {
        try
        {
            var client = await _clientService.GetWithOrganizationsAsync(id);
            if (client == null)
            {
                return NotFound(new { message = $"Client with ID {id} not found" });
            }
            return Ok(client);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting client with organizations for ID {ClientId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Create new client with organizations in a single request
    /// </summary>
    /// <param name="dto">Client and organizations creation data</param>
    /// <returns>Created client with organizations</returns>
    [HttpPost("add-with-organizations")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateWithOrganizations([FromBody] ClientWithOrganizationsCreateDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var client = await _clientService.AddClientWithOrganizationsAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = client.Id }, client);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating client with organizations");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update existing client with organizations in a single request
    /// </summary>
    /// <param name="dto">Client and organizations update data</param>
    /// <param name="id">Client ID</param>
    /// <returns>Updated client with organizations</returns>
    [HttpPut("edit-with-organizations/{id}")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateWithOrganizations(Guid id, [FromBody] ClientWithOrganizationsUpdateDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var client = await _clientService.EditClientWithOrganizationsAsync(id, dto);
            if (client == null)
            {
                return NotFound(new { message = $"Client with ID {id} not found" });
            }
            return Ok(client);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating client with organizations for ID {ClientId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    #endregion

    #region Client Queries by Balance

    /// <summary>
    /// Get clients with positive balance (creditors - ????)
    /// </summary>
    /// <returns>List of creditor clients</returns>
    [HttpGet("creditors")]
    [ProducesResponseType(typeof(IEnumerable<ClientDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCreditors()
    {
        try
        {
            var clients = await _clientService.GetCreditorsAsync();
            return Ok(clients);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting creditor clients");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get clients with negative balance (debtors - ????)
    /// </summary>
    /// <returns>List of debtor clients</returns>
    [HttpGet("debtors")]
    [ProducesResponseType(typeof(IEnumerable<ClientDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetDebtors()
    {
        try
        {
            var clients = await _clientService.GetDebtorsAsync();
            return Ok(clients);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting debtor clients");
            return BadRequest(new { message = ex.Message });
        }
    }

    #endregion
}
