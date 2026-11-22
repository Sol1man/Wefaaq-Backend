using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wefaaq.Bll.DTOs;
using Wefaaq.Bll.Interfaces;

namespace Wefaaq.Api.Controllers;

/// <summary>
/// Per-user page permissions (صلاحيات المستخدمين). Admin only.
/// </summary>
[ApiController]
[Route("api/user-permissions")]
[Produces("application/json")]
[Authorize(Policy = "AdminOnly")]
public class UserPermissionsController : ControllerBase
{
    private readonly IUserPermissionService _service;
    private readonly ILogger<UserPermissionsController> _logger;

    public UserPermissionsController(IUserPermissionService service, ILogger<UserPermissionsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// The grantable pages plus every user with their effective permissions
    /// </summary>
    [HttpGet("overview")]
    [ProducesResponseType(typeof(PermissionsOverviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetOverview()
    {
        try
        {
            return Ok(await _service.GetOverviewAsync());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while loading user permissions");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Save a user's page permissions. Pages left out of the request keep their current value.
    /// Takes effect on the user's next request.
    /// </summary>
    [HttpPut("users/{userId:int}")]
    [ProducesResponseType(typeof(UserPermissionsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int userId, [FromBody] UpdateUserPermissionsDto dto)
    {
        try
        {
            var result = await _service.UpdateAsync(userId, dto);

            _logger.LogInformation(
                "[UserPermissions] admin {AdminId} updated permissions of user {UserId}: {Permissions}",
                User.FindFirst("userId")?.Value, userId,
                string.Join(", ", result.Permissions.Select(p =>
                    $"{p.PageKey}={(p.CanView ? "V" : "-")}{(p.CanAddEdit ? "E" : "-")}{(p.CanDelete ? "D" : "-")}")));

            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating permissions of user {UserId}", userId);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Drop a user's custom permissions so every page goes back to its default
    /// </summary>
    [HttpPost("users/{userId:int}/reset")]
    [ProducesResponseType(typeof(UserPermissionsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reset(int userId)
    {
        try
        {
            var result = await _service.ResetToDefaultsAsync(userId);

            _logger.LogInformation(
                "[UserPermissions] admin {AdminId} reset permissions of user {UserId} to defaults",
                User.FindFirst("userId")?.Value, userId);

            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while resetting permissions of user {UserId}", userId);
            return BadRequest(new { message = ex.Message });
        }
    }
}
