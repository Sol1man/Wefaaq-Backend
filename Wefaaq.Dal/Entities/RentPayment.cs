using System.ComponentModel.DataAnnotations;
using Wefaaq.Dal.Interfaces;

namespace Wefaaq.Dal.Entities;

/// <summary>
/// Money a renter paid against a <see cref="Rent"/> (دفعة إيجار).
///
/// The renter may settle a month in one go or in several parts — each part is its own
/// row, mirroring how a client payment works on the client-operations screen. A payment
/// is attributed to the month of <see cref="PaymentDate"/>, which is what the Rents page
/// filters on.
/// </summary>
public class RentPayment : ISoftDeletable
{
    /// <summary>Unique identifier</summary>
    public Guid Id { get; set; }

    /// <summary>The rent this payment settles</summary>
    public Guid RentId { get; set; }

    /// <summary>Navigation property to the rent</summary>
    public virtual Rent Rent { get; set; } = null!;

    /// <summary>Amount paid</summary>
    [Required]
    public decimal Amount { get; set; }

    /// <summary>Optional description / البيان</summary>
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The business date of the payment (editable by the admin; defaults to the moment of
    /// creation). The page groups by the month of this date, not CreatedAt.
    /// </summary>
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
