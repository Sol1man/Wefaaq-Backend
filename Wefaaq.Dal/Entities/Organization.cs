using System.ComponentModel.DataAnnotations;

namespace Wefaaq.Dal.Entities;

/// <summary>
/// Organization entity (المؤسسة)
/// </summary>
public class Organization
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
    /// Foreign key to Client (each organization belongs to one client)
    /// </summary>
    public Guid ClientId { get; set; }

    /// <summary>
    /// Navigation property to Client
    /// </summary>
    public virtual Client Client { get; set; } = null!;

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
}