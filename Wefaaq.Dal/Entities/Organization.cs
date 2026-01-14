using System.ComponentModel.DataAnnotations;
using Wefaaq.Dal.Interfaces;

namespace Wefaaq.Dal.Entities;

/// <summary>
/// Organization entity (المؤسسة)
/// </summary>
public class Organization : ISoftDeletable
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Organization name (اسم)
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if card is expiring soon (الكارت شارف على الانتهاء)
    /// </summary>
    public bool CardExpiringSoon { get; set; }

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
    /// Foreign key to Client (each organization belongs to one client)
    /// </summary>
    public Guid? ClientId { get; set; }

    /// <summary>
    /// Navigation property to Client
    /// </summary>
    public virtual Client? Client { get; set; }

    /// <summary>
    /// Foreign key to ClientBranch (each organization can belong to a client branch)
    /// </summary>
    public Guid? ClientBranchId { get; set; }

    /// <summary>
    /// Navigation property to ClientBranch
    /// </summary>
    public virtual ClientBranch? ClientBranch { get; set; }

    /// <summary>
    /// Organization records collection (سجلات المؤسسة)
    /// </summary>
    public virtual ICollection<OrganizationRecord> Records { get; set; } = new List<OrganizationRecord>();

    /// <summary>
    /// Organization licenses collection (تراخيص المؤسسة)
    /// </summary>
    public virtual ICollection<OrganizationLicense> Licenses { get; set; } = new List<OrganizationLicense>();

    /// <summary>
    /// Organization workers collection (عمال المؤسسة)
    /// </summary>
    public virtual ICollection<OrganizationWorker> Workers { get; set; } = new List<OrganizationWorker>();

    /// <summary>
    /// Organization cars collection (سيارات المؤسسة)
    /// </summary>
    public virtual ICollection<OrganizationCar> Cars { get; set; } = new List<OrganizationCar>();

    /// <summary>
    /// Organization usernames/credentials collection (اسماء المستخدمين)
    /// </summary>
    public virtual ICollection<OrganizationUsername> Usernames { get; set; } = new List<OrganizationUsername>();
}