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

    /// <summary>
    /// Combined client-operation entry: creates 1 or 2 linked rows (Payment, Profit, or both).
    /// </summary>
    Task<IEnumerable<UserPaymentDto>> CreateOperationAsync(int userId, UserPaymentOperationCreateDto dto);

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
    /// Aggregated summary for a single user — used by the user-facing payments page (self view).
    /// Returns null if the user does not exist / is inactive.
    /// </summary>
    Task<UserPaymentSummaryDto?> GetUserSummaryAsync(int userId);

    /// <summary>
    /// Admin account update. When <paramref name="amountToAdd"/> &gt; 0 it is added to both Initial
    /// and Current balances (cumulative) and logged as a UserPayment row of Type=Initial. When
    /// <paramref name="profitPercentage"/> is supplied it replaces the user's profit-share percentage.
    /// Returns the updated user, or null if not found.
    /// </summary>
    Task<UserDto?> SetInitialAccountAmountAsync(int userId, decimal amountToAdd, decimal? profitPercentage = null, string? description = null);
}
