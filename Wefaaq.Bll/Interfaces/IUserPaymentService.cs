using Wefaaq.Bll.DTOs;

namespace Wefaaq.Bll.Interfaces;

/// <summary>
/// User payment service interface
/// </summary>
public interface IUserPaymentService
{
    Task<IEnumerable<UserPaymentDto>> GetAllAsync();

    Task<UserPaymentDto?> GetByIdAsync(Guid id);

    Task<UserPaymentDto> CreateAsync(int userId, UserPaymentCreateDto dto);

    Task<IEnumerable<UserPaymentDto>> GetMyPaymentsAsync(int userId);

    Task<IEnumerable<UserPaymentDto>> GetPaymentsByDateRangeAsync(DateTime from, DateTime to);

    Task<IEnumerable<UserPaymentDto>> GetPaymentsByUserAsync(int userId);

    /// <summary>
    /// Get payments for a specific user within a date range — used by the user details page.
    /// </summary>
    Task<IEnumerable<UserPaymentDto>> GetPaymentsByUserAndDateRangeAsync(int userId, DateTime from, DateTime to);

    Task<bool> DeleteAsync(Guid id);

    Task<decimal> GetTotalAmountByDateRangeAsync(DateTime from, DateTime to);

    Task<decimal> GetTotalAmountByUserAsync(int userId);

    /// <summary>
    /// Aggregated per-user view (one row per user) for the payments management page.
    /// </summary>
    Task<IEnumerable<UserPaymentSummaryDto>> GetUserSummariesAsync();

    /// <summary>
    /// Admin sets/resets a user's daily seed amount. Resets CurrentAccountAmount to the same value.
    /// Returns the updated user, or null if not found.
    /// </summary>
    Task<UserDto?> SetInitialAccountAmountAsync(int userId, decimal newInitialAmount);
}
