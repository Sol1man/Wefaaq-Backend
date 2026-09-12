using System.ComponentModel.DataAnnotations;
using Wefaaq.Dal.Interfaces;

namespace Wefaaq.Dal.Entities;

/// <summary>
/// Employee loan entity (سلفة). Money the company advances to an employee. It carries no
/// profit and no interest — the amount is simply subtracted from that month's salary,
/// exactly like an <see cref="EmployeeDeduction"/>, but tracked separately so the two
/// histories stay distinct.
///
/// Belongs to exactly one employee — either a system User or an ExternalEmployee (XOR,
/// enforced by a check constraint). Loans are attributed to the month of <see cref="LoanDate"/>.
/// </summary>
public class EmployeeLoan : ISoftDeletable
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Loan amount advanced to the employee
    /// </summary>
    [Required]
    public decimal Amount { get; set; }

    /// <summary>
    /// Reason / description for the loan
    /// </summary>
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The business date of the loan (editable by the admin; defaults to the moment of
    /// creation). The salaries page groups by the month of this date, not CreatedAt.
    /// </summary>
    public DateTime LoanDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// System user this loan belongs to (null for external employees)
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// Navigation property to the system user
    /// </summary>
    public virtual User? User { get; set; }

    /// <summary>
    /// External employee this loan belongs to (null for system users)
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
    /// Indicates if the loan is soft deleted
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Timestamp when the loan was soft deleted
    /// </summary>
    public DateTime? DeletedAt { get; set; }
}
