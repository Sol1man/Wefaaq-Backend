using System.ComponentModel.DataAnnotations;
using Wefaaq.Dal.Interfaces;

namespace Wefaaq.Dal.Entities;

/// <summary>
/// Client branch entity (فرع العميل)
/// Represents a branch or subsidiary of a main client
/// </summary>
public class ClientBranch : ISoftDeletable
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Branch name (أسم الفرع)
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Branch email (ايميل)
    /// </summary>
    [EmailAddress]
    [MaxLength(255)]
    public string? Email { get; set; }

    /// <summary>
    /// Branch phone number (رقم)
    /// </summary>
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Branch classification (تصنيف الفرع)
    /// </summary>
    public ClientClassification Classification { get; set; }

    /// <summary>
    /// Branch balance (رصيد) - negative = مدين (debtor), positive = دائن (creditor)
    /// </summary>
    [Range(-999999999.99, 999999999.99)]
    public decimal Balance { get; set; }

    /// <summary>
    /// Branch type descriptor (نوع الفرع)
    /// e.g., "Sister", "Subsidiary", "Sister Company", etc.
    /// </summary>
    [MaxLength(100)]
    public string? BranchType { get; set; }

    /// <summary>
    /// Parent client identifier (foreign key)
    /// </summary>
    public Guid ParentClientId { get; set; }

    /// <summary>
    /// Navigation property to parent Client
    /// </summary>
    public virtual Client ParentClient { get; set; } = null!;

    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indicates if the entity is soft deleted
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Timestamp when the entity was soft deleted
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// One-to-many relationship with organizations
    /// </summary>
    public virtual ICollection<Organization> Organizations { get; set; } = new List<Organization>();

    /// <summary>
    /// One-to-many relationship with external workers
    /// </summary>
    public virtual ICollection<ExternalWorker> ExternalWorkers { get; set; } = new List<ExternalWorker>();
}
