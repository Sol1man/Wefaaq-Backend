using Wefaaq.Dal.Entities;

namespace Wefaaq.Bll.DTOs;

/// <summary>
/// One row of the Rents table for a given month (الإيجارات).
/// </summary>
public class RentRowDto
{
    public Guid Id { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public string? Notes { get; set; }

    /// <summary>The currently agreed monthly rent</summary>
    public decimal MonthlyAmount { get; set; }
    public bool IsActive { get; set; }
    public DateTime StartDate { get; set; }

    // Renter (resolved for display)
    public OperationTargetType TargetType { get; set; }
    public Guid? ClientId { get; set; }
    public string? ClientName { get; set; }
    public Guid? ClientBranchId { get; set; }
    public string? ClientBranchName { get; set; }
    public Guid? OrganizationId { get; set; }
    public string? OrganizationName { get; set; }
    public string? ExternalPersonName { get; set; }
    public string? ExternalPersonIdNumber { get; set; }

    /// <summary>Whichever of the four renter names applies — ready to print</summary>
    public string RenterDisplay { get; set; } = string.Empty;

    // ── Figures for the selected month ──────────────────────────────────────

    /// <summary>Amount owed for the selected month (0 when the rent wasn't running yet)</summary>
    public decimal MonthDue { get; set; }

    /// <summary>Payments dated inside the selected month</summary>
    public decimal MonthPaid { get; set; }

    /// <summary>MonthDue − MonthPaid. Negative means the renter overpaid this month.</summary>
    public decimal MonthRemaining { get; set; }

    /// <summary>True when this month is fully settled (and something was actually due)</summary>
    public bool IsSettled { get; set; }

    // ── Lifetime figures, across every month ────────────────────────────────

    public decimal TotalDue { get; set; }
    public decimal TotalPaid { get; set; }

    /// <summary>TotalDue − TotalPaid — what the renter still owes overall</summary>
    public decimal TotalRemaining { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Full detail view for one rent: the row figures plus the complete month-by-month
/// dues and the payment history, each newest first.
/// </summary>
public class RentDetailsDto : RentRowDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public List<RentChargeDto> Charges { get; set; } = new();
    public List<RentPaymentDto> Payments { get; set; } = new();
}

/// <summary>
/// One month's rent due (قيمة إيجار الشهر)
/// </summary>
public class RentChargeDto
{
    public Guid Id { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Amount { get; set; }

    /// <summary>Payments dated inside this charge's month</summary>
    public decimal Paid { get; set; }

    /// <summary>Amount − Paid</summary>
    public decimal Remaining { get; set; }

    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// A rent payment (دفعة إيجار)
/// </summary>
public class RentPaymentDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for creating a rent. Only the FK matching TargetType need be supplied.
/// </summary>
public class RentCreateDto
{
    public string AssetName { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public decimal MonthlyAmount { get; set; }
    public OperationTargetType TargetType { get; set; }

    public Guid? ClientId { get; set; }
    public Guid? ClientBranchId { get; set; }
    public Guid? OrganizationId { get; set; }

    public string? ExternalPersonName { get; set; }
    public string? ExternalPersonIdNumber { get; set; }

    /// <summary>First month the rent is owed for. Defaults to today when omitted.</summary>
    public DateTime? StartDate { get; set; }
}

/// <summary>
/// DTO for updating a rent. The renter is fixed once created, so it isn't included —
/// changing MonthlyAmount affects future months only.
/// </summary>
public class RentUpdateDto
{
    public string AssetName { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public decimal MonthlyAmount { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// DTO for overriding a single month's rent amount.
/// </summary>
public class RentChargeSetDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Amount { get; set; }
}

/// <summary>
/// DTO for recording a payment against a rent — the whole month or part of it.
/// </summary>
public class RentPaymentCreateDto
{
    public Guid RentId { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }

    /// <summary>Business date of the payment. Defaults to "now" when omitted.</summary>
    public DateTime? PaymentDate { get; set; }
}
