using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wefaaq.Bll.DTOs;
using Wefaaq.Bll.Interfaces;

namespace Wefaaq.Api.Controllers;

/// <summary>
/// User payment management endpoints
/// </summary>
[ApiController]
[Route("api/user-payments")]
[Produces("application/json")]
[Authorize]
public class UserPaymentController : ControllerBase
{
    private readonly IUserPaymentService _userPaymentService;
    private readonly ILogger<UserPaymentController> _logger;

    public UserPaymentController(IUserPaymentService userPaymentService, ILogger<UserPaymentController> logger)
    {
        _userPaymentService = userPaymentService;
        _logger = logger;
    }

    /// <summary>
    /// Get all payments (Admin only)
    /// </summary>
    /// <returns>List of all payments with user info</returns>
    [HttpGet("get-all")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(List<UserPaymentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var payments = await _userPaymentService.GetAllAsync();
            return Ok(payments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all payments");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get current user's payments
    /// </summary>
    /// <returns>List of current user's payments</returns>
    [HttpGet("my-payments")]
    [ProducesResponseType(typeof(List<UserPaymentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMyPayments()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { message = "User ID not found in claims" });
            }

            var payments = await _userPaymentService.GetMyPaymentsAsync(userId.Value);
            return Ok(payments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting user's payments");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Create a new payment
    /// </summary>
    /// <param name="dto">Payment creation data</param>
    /// <returns>Created payment</returns>
    [HttpPost("add")]
    [ProducesResponseType(typeof(UserPaymentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] UserPaymentCreateDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            // traceId == HttpContext.TraceIdentifier — unique per HTTP request, lets us see at a
            // glance whether two backend creates came from one POST (replay) or two POSTs (double-click).
            _logger.LogInformation(
                "[UserPayments] HTTP POST /add traceId={TraceId} userId={UserId} amount={Amount} ip={Ip}",
                HttpContext.TraceIdentifier, userId, dto?.Amount, HttpContext.Connection.RemoteIpAddress?.ToString());

            if (userId == null)
            {
                return Unauthorized(new { message = "User ID not found in claims" });
            }

            var payment = await _userPaymentService.CreateAsync(userId.Value, dto);
            return CreatedAtAction(nameof(GetMyPayments), payment);
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(new { message = "Validation failed", errors = ex.Errors.Select(e => e.ErrorMessage) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating payment");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Create a combined operation entry (Payment, Profit, or both linked).
    /// </summary>
    [HttpPost("add-operation")]
    [ProducesResponseType(typeof(IEnumerable<UserPaymentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOperation([FromBody] UserPaymentOperationCreateDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { message = "User ID not found in claims" });
            }

            var rows = await _userPaymentService.CreateOperationAsync(userId.Value, dto);
            return CreatedAtAction(nameof(GetMyPayments), rows);
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(new { message = "Validation failed", errors = ex.Errors.Select(e => e.ErrorMessage) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating operation");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get payments by date range (Admin only)
    /// </summary>
    /// <param name="from">Start date</param>
    /// <param name="to">End date</param>
    /// <returns>List of payments within the date range</returns>
    [HttpGet("by-date-range")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(List<UserPaymentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByDateRange([FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        try
        {
            var payments = await _userPaymentService.GetPaymentsByDateRangeAsync(from, to);
            return Ok(payments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting payments by date range");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get payments by user ID (Admin only). Optionally filter by date range.
    /// </summary>
    /// <param name="userId">User ID to filter by</param>
    /// <param name="from">Optional start date</param>
    /// <param name="to">Optional end date</param>
    /// <returns>List of payments for the specified user</returns>
    [HttpGet("by-user/{userId}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(List<UserPaymentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByUser(int userId, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
    {
        try
        {
            var payments = from.HasValue && to.HasValue
                ? await _userPaymentService.GetPaymentsByUserAndDateRangeAsync(userId, from.Value, to.Value)
                : await _userPaymentService.GetPaymentsByUserAsync(userId);
            return Ok(payments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting payments by user {UserId}", userId);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get aggregated per-user summaries (Admin only). One row per user with account amounts and today/month payment totals.
    /// </summary>
    [HttpGet("user-summaries")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(List<UserPaymentSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUserSummaries()
    {
        try
        {
            var summaries = await _userPaymentService.GetUserSummariesAsync();
            return Ok(summaries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting user payment summaries");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Top up a user's account (Admin only). The amount is ADDED to both Initial and Current
    /// balances (cumulative) and a UserPayment row of Type=Initial is logged for traceability.
    /// </summary>
    [HttpPut("set-initial-amount/{userId}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetInitialAccountAmount(int userId, [FromBody] UpdateUserAccountAmountDto dto)
    {
        try
        {
            if (dto.InitialAccountAmount <= 0)
            {
                return BadRequest(new { message = "Top-up amount must be greater than zero" });
            }

            var updated = await _userPaymentService.SetInitialAccountAmountAsync(userId, dto.InitialAccountAmount, dto.Description);
            if (updated == null)
            {
                return NotFound(new { message = $"User with ID {userId} not found" });
            }
            return Ok(updated);
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(new { message = "Validation failed", errors = ex.Errors.Select(e => e.ErrorMessage) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while topping up user account for user {UserId}", userId);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get total amount by date range (Admin only)
    /// </summary>
    /// <param name="from">Start date</param>
    /// <param name="to">End date</param>
    /// <returns>Total amount within the date range</returns>
    [HttpGet("total-by-date-range")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTotalByDateRange([FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        try
        {
            var total = await _userPaymentService.GetTotalAmountByDateRangeAsync(from, to);
            return Ok(new { totalAmount = total, from, to });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while calculating total by date range");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get total amount by user (Admin only)
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Total amount for the specified user</returns>
    [HttpGet("total-by-user/{userId}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTotalByUser(int userId)
    {
        try
        {
            var total = await _userPaymentService.GetTotalAmountByUserAsync(userId);
            return Ok(new { totalAmount = total, userId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while calculating total for user {UserId}", userId);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete a payment (Admin only - soft delete)
    /// </summary>
    /// <param name="id">Payment ID</param>
    /// <returns>Success or not found</returns>
    [HttpDelete("delete/{id}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var deleted = await _userPaymentService.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = $"Payment with ID {id} not found" });
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting payment with ID {PaymentId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get the current user's ID from claims
    /// </summary>
    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (int.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        return null;
    }
}
