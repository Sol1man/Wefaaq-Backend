using Wefaaq.Bll.DTOs;

namespace Wefaaq.Bll.Interfaces;

/// <summary>
/// User payment service interface
/// </summary>
public interface IUserPaymentService
{
    /// <summary>
    /// Get all payments (Admin only)
    /// </summary>
    /// <returns>List of all payment DTOs with user info</returns>
    Task<IEnumerable<UserPaymentDto>> GetAllAsync();

    /// <summary>
    /// Get payment by ID
    /// </summary>
    /// <param name="id">Payment ID</param>
    /// <returns>Payment DTO or null</returns>
    Task<UserPaymentDto?> GetByIdAsync(Guid id);

    /// <summary>
    /// Create a new payment for the current user
    /// </summary>
    /// <param name="userId">User ID creating the payment</param>
    /// <param name="dto">Payment creation data</param>
    /// <returns>Created payment DTO</returns>
    Task<UserPaymentDto> CreateAsync(int userId, UserPaymentCreateDto dto);

    /// <summary>
    /// Get payments for a specific user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>List of user's payment DTOs</returns>
    Task<IEnumerable<UserPaymentDto>> GetMyPaymentsAsync(int userId);

    /// <summary>
    /// Get payments within a date range (Admin only)
    /// </summary>
    /// <param name="from">Start date</param>
    /// <param name="to">End date</param>
    /// <returns>List of payment DTOs within the date range</returns>
    Task<IEnumerable<UserPaymentDto>> GetPaymentsByDateRangeAsync(DateTime from, DateTime to);

    /// <summary>
    /// Get payments by user ID (Admin only)
    /// </summary>
    /// <param name="userId">User ID to filter by</param>
    /// <returns>List of payment DTOs for the specified user</returns>
    Task<IEnumerable<UserPaymentDto>> GetPaymentsByUserAsync(int userId);

    /// <summary>
    /// Delete a payment (Admin only - soft delete)
    /// </summary>
    /// <param name="id">Payment ID</param>
    /// <returns>True if deleted, false if not found</returns>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    /// Get total payment amount for a date range
    /// </summary>
    /// <param name="from">Start date</param>
    /// <param name="to">End date</param>
    /// <returns>Total amount</returns>
    Task<decimal> GetTotalAmountByDateRangeAsync(DateTime from, DateTime to);

    /// <summary>
    /// Get total payment amount for a specific user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Total amount</returns>
    Task<decimal> GetTotalAmountByUserAsync(int userId);
}
