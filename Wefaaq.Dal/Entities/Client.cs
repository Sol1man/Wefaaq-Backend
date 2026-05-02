using System.ComponentModel.DataAnnotations;
using Wefaaq.Dal.Interfaces;

namespace Wefaaq.Dal.Entities;

/// <summary>
/// Client entity (العميل)
/// </summary>
public class Client : ISoftDeletable
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Client name (أسم)
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Client email (ايميل)
    /// </summary>
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Client phone number (رقم)
    /// </summary>
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Client classification (تصنيف العميل)
    /// </summary>
    public ClientClassification Classification { get; set; }

    /// <summary>
    /// Client balance (رصيد) - negative = مدين (debtor), positive = دائن (creditor)
    /// </summary>
    [Range(-999999999.99, 999999999.99)]
    public decimal Balance { get; set; }

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
    /// Foreign key to the user this client is assigned to.
    /// Null = unassigned (visible to admins only). Branches and organizations
    /// inherit ownership from this assignment via their parent client.
    /// </summary>
    public int? AssignedUserId { get; set; }

    /// <summary>
    /// Navigation property to the assigned user.
    /// </summary>
    public virtual User? AssignedUser { get; set; }

    /// <summary>
    /// One-to-many relationship with organizations
    /// </summary>
    public virtual ICollection<Organization> Organizations { get; set; } = new List<Organization>();

    /// <summary>
    /// One-to-many relationship with external workers
    /// </summary>
    public virtual ICollection<ExternalWorker> ExternalWorkers { get; set; } = new List<ExternalWorker>();

    /// <summary>
    /// One-to-many relationship with client branches
    /// </summary>
    public virtual ICollection<ClientBranch> ClientBranches { get; set; } = new List<ClientBranch>();
}