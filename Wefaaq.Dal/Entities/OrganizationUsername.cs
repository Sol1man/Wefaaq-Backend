using System.ComponentModel.DataAnnotations;
using Wefaaq.Dal.Interfaces;

namespace Wefaaq.Dal.Entities;

/// <summary>
/// Organization username/credential entity (اسماء المستخدمين للمؤسسة)
/// Stores website login credentials for organizations
/// </summary>
public class OrganizationUsername : ISoftDeletable
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Website or service name (اسم الموقع)
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string SiteName { get; set; } = string.Empty;

    /// <summary>
    /// Login username (اسم المستخدم)
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Password (كلمة المرور)
    /// WARNING: Should be encrypted/hashed before storage
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Organization identifier (foreign key)
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Navigation property to Organization
    /// </summary>
    public virtual Organization Organization { get; set; } = null!;

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
}
