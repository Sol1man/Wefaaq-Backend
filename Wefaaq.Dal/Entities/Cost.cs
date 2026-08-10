using System.ComponentModel.DataAnnotations;
using Wefaaq.Dal.Interfaces;

namespace Wefaaq.Dal.Entities;

/// <summary>
/// Business cost / expense entity (المصروفات). A global list of company expenses,
/// not tied to any user — mirrors the payments concept but for money going out.
/// </summary>
public class Cost : ISoftDeletable
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Cost amount
    /// </summary>
    [Required]
    public decimal Amount { get; set; }

    /// <summary>
    /// Cost description
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The business date of the cost (editable by the admin; defaults to the moment of creation).
    /// Filtering and the period cards run against this, not CreatedAt.
    /// </summary>
    public DateTime CostDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Record creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indicates if the cost is soft deleted
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Timestamp when the cost was soft deleted
    /// </summary>
    public DateTime? DeletedAt { get; set; }
}
