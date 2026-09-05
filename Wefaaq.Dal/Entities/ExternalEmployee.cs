using System.ComponentModel.DataAnnotations;
using Wefaaq.Dal.Interfaces;

namespace Wefaaq.Dal.Entities;

/// <summary>
/// External employee entity (موظف خارجي). A person on the payroll who has no system
/// account — no email, no role, no login. Exists purely so they show up on the
/// employee salaries page alongside the real users.
/// </summary>
public class ExternalEmployee : ISoftDeletable
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Employee display name
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Monthly salary
    /// </summary>
    public decimal Salary { get; set; }

    /// <summary>
    /// Record creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indicates if the employee is soft deleted
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Timestamp when the employee was soft deleted
    /// </summary>
    public DateTime? DeletedAt { get; set; }
}
