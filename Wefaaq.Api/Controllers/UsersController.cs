using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wefaaq.Bll.DTOs;
using Wefaaq.Dal.RepositoriesInterfaces;

namespace Wefaaq.Api.Controllers;

/// <summary>
/// User listing endpoints. Admin-only — used by the client-assignment UI.
/// </summary>
[ApiController]
[Route("api/users")]
[Produces("application/json")]
[Authorize(Policy = "AdminOnly")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IUserRepository userRepository,
        IMapper mapper,
        ILogger<UsersController> logger)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Get all users (admins + normal users).
    /// </summary>
    [HttpGet("get-all")]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var users = await _userRepository.GetAllWithRolesAsync();
            return Ok(_mapper.Map<IEnumerable<UserDto>>(users));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while listing users");
            return BadRequest(new { message = ex.Message });
        }
    }
}
