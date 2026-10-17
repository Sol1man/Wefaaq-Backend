using System.ComponentModel.DataAnnotations;
using Wefaaq.Dal.Interfaces;

namespace Wefaaq.Dal.Entities;

/// <summary>
/// Rent agreement entity (إيجار) — an asset the company rents out to a client
/// (a car, equipment, anything described in <see cref="AssetName"/>).
///
/// A rent is a monthly arrangement: every month from <see cref="StartDate"/> onward it
/// owes an amount, materialized as a <see cref="RentCharge"/> row so each month keeps its
/// own figure even when the agreed rent changes. Money the renter pays arrives as
/// <see cref="RentPayment"/> rows (دفعة), in full or in parts.
///
/// Rents keep their own ledger — they never touch Client.Balance, which belongs to
/// client operations.
/// </summary>
public class Rent : ISoftDeletable
{
    /// <summary>Unique identifier</summary>
    public Guid Id { get; set; }

    /// <summary>Free-text description of the rented asset (الأصل المستأجر)</summary>
    [Required]
    [MaxLength(255)]
    public string AssetName { get; set; } = string.Empty;

    /// <summary>Optional notes (ملاحظات)</summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }

    /// <summary>
    /// The currently agreed monthly rent (قيمة الإيجار الشهري). Seeds each new month's
    /// charge; changing it never rewrites months already materialized.
    /// </summary>
    public decimal MonthlyAmount { get; set; }

    /// <summary>Who is renting (نوع المستأجر) — same targeting as client operations</summary>
    public OperationTargetType TargetType { get; set; }

    // ── Polymorphic target — at most ONE of the following three FKs is non-null
    //    (none of them, when TargetType = ExternalPerson)

    /// <summary>Client FK — set when TargetType = Client</summary>
    public Guid? ClientId { get; set; }
    public virtual Client? Client { get; set; }

    /// <summary>ClientBranch FK — set when TargetType = ClientBranch</summary>
    public Guid? ClientBranchId { get; set; }
    public virtual ClientBranch? ClientBranch { get; set; }

    /// <summary>Organization FK — set when TargetType = Organization</summary>
    public Guid? OrganizationId { get; set; }
    public virtual Organization? Organization { get; set; }

    // ── External person fields (when TargetType = ExternalPerson)

    /// <summary>External renter full name (اسم المستأجر الخارجي)</summary>
    [MaxLength(255)]
    public string? ExternalPersonName { get; set; }

    /// <summary>External renter national/residence ID (رقم الهوية)</summary>
    [MaxLength(50)]
    public string? ExternalPersonIdNumber { get; set; }

    /// <summary>First month the rent is owed for (تاريخ بداية الإيجار)</summary>
    public DateTime StartDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether the rent is still running. An inactive rent stops accruing new monthly
    /// charges but keeps every month and payment already recorded.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Monthly dues recorded for this rent</summary>
    public virtual ICollection<RentCharge> Charges { get; set; } = new List<RentCharge>();

    /// <summary>Payments the renter has made against this rent</summary>
    public virtual ICollection<RentPayment> Payments { get; set; } = new List<RentPayment>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
