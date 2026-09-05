using System.ComponentModel.DataAnnotations;
using Wefaaq.Dal.Interfaces;

namespace Wefaaq.Dal.Entities;

/// <summary>
/// Salary deduction entity (خصم). Belongs to exactly one employee — either a system
/// User or an ExternalEmployee (XOR, enforced by a check constraint). Deductions are
/// attributed to the month of <see cref="DeductionDate"/>.
/// </summary>
public class EmployeeDeduction : ISoftDeletable
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Deducted amount
    /// </summary>
    [Required]
    public decimal Amount { get; set; }

    /// <summary>
    /// Reason / description for the deduction
    /// </summary>
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The business date of the deduction (editable by the admin; defaults to the moment of
    /// creation). The salaries page groups by the month of this date, not CreatedAt.
    /// </summary>
    public DateTime DeductionDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// System user this deduction belongs to (null for external employees)
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// Navigation property to the system user
    /// </summary>
    public virtual User? User { get; set; }

    /// <summary>
    /// External employee this deduction belongs to (null for system users)
    /// </summary>
    public Guid? ExternalEmployeeId { get; set; }

    /// <summary>
    /// Navigation property to the external employee
    /// </summary>
    public virtual ExternalEmployee? ExternalEmployee { get; set; }

    /// <summary>
    /// Record creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indicates if the deduction is soft deleted
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Timestamp when the deduction was soft deleted
    /// </summary>
    public DateTime? DeletedAt { get; set; }
}
