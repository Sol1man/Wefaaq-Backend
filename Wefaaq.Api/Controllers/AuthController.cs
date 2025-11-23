using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wefaaq.Bll.DTOs;
using Wefaaq.Bll.Interfaces;

namespace Wefaaq.Api.Controllers;

/// <summary>
/// Authentication endpoints
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    #region Authentication Operations

    /// <summary>
    /// Login with Firebase ID token
    /// </summary>
    /// <param name="request">Login request containing Firebase ID token</param>
    /// <returns>Login response with user data</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.IdToken))
            {
                return BadRequest(new { message = "ID token is required" });
            }

            var result = await _authService.LoginAsync(request);

            if (!result.Success)
            {
                if (result.Message == "You Are Not Authorized")
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new { message = result.Message });
                }

                return Unauthorized(new { message = result.Message });
            }

            return Ok(result.User);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during login");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Logout user
    /// </summary>
    /// <param name="request">Logout request containing Firebase ID token</param>
    /// <returns>Logout response</returns>
    [HttpPost("logout")]
    [ProducesResponseType(typeof(LogoutResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequestDto request)
    {
        try
        {
            var result = await _authService.LogoutAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during logout");
            // Always allow logout to succeed
            return Ok(new LogoutResponseDto
            {
                Success = true,
                Message = "Logged out"
            });
        }
    }

    /// <summary>
    /// Get current user (for testing - requires authentication)
    /// </summary>
    /// <returns>Current user data</returns>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            // Get Firebase UID from claims
            string? firebaseUid = User.FindFirst("user_id")?.Value;

            if (string.IsNullOrEmpty(firebaseUid))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var user = await _authService.GetUserByFirebaseUidAsync(firebaseUid);

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting current user");
            return BadRequest(new { message = ex.Message });
        }
    }

    #endregion
}
