using System.ComponentModel.DataAnnotations;
using Wefaaq.Dal.Interfaces;

namespace Wefaaq.Dal.Entities;

/// <summary>
/// One month's rent due for a <see cref="Rent"/> (قيمة إيجار الشهر).
///
/// Materialized once per rent per month, seeded from the rent's current
/// <see cref="Rent.MonthlyAmount"/>, then frozen — so raising the agreed rent affects
/// only months not yet recorded, and every past month keeps the figure it was billed at.
/// The admin can still override a single month's amount.
///
/// The period is stored as plain Year/Month integers rather than a date, because a
/// monthly due has no meaningful day and this keeps the uniqueness rule simple.
/// </summary>
public class RentCharge : ISoftDeletable
{
    /// <summary>Unique identifier</summary>
    public Guid Id { get; set; }

    /// <summary>The rent this month's due belongs to</summary>
    public Guid RentId { get; set; }

    /// <summary>Navigation property to the rent</summary>
    public virtual Rent Rent { get; set; } = null!;

    /// <summary>Calendar year of the billed month</summary>
    public int Year { get; set; }

    /// <summary>Calendar month of the billed month (1-12)</summary>
    public int Month { get; set; }

    /// <summary>Amount owed for this month</summary>
    [Required]
    public decimal Amount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
