using Wefaaq.Bll.DTOs;

namespace Wefaaq.Bll.Interfaces;

/// <summary>
/// Rent management service interface (الإيجارات)
/// </summary>
public interface IRentService
{
    /// <summary>
    /// Every rent with its figures for the given month. Materializes any missing monthly
    /// dues for active rents first, so a freshly opened month is already billed.
    /// </summary>
    Task<IEnumerable<RentRowDto>> GetByMonthAsync(int year, int month);

    /// <summary>One rent's figures for a month, plus its full dues and payment history</summary>
    Task<RentDetailsDto?> GetByIdAsync(Guid id, int year, int month);

    /// <summary>Create a rent agreement</summary>
    Task<RentRowDto> CreateAsync(RentCreateDto dto);

    /// <summary>Update a rent's asset, notes, agreed monthly amount or active flag</summary>
    Task<RentRowDto?> UpdateAsync(Guid id, RentUpdateDto dto);

    /// <summary>Delete a rent (soft delete), along with its dues and payments</summary>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>Override a single month's rent amount</summary>
    Task<RentChargeDto?> SetChargeAsync(Guid rentId, RentChargeSetDto dto);

    /// <summary>Record a payment against a rent (دفعة) — the whole month or part of it</summary>
    Task<RentPaymentDto> AddPaymentAsync(RentPaymentCreateDto dto);

    /// <summary>Delete a rent payment (soft delete)</summary>
    Task<bool> DeletePaymentAsync(Guid id);
}
