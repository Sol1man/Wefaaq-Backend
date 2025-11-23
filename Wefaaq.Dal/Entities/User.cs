using System.ComponentModel.DataAnnotations;

namespace Wefaaq.Dal.Entities;

/// <summary>
/// User entity for authentication
/// </summary>
public class User
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Firebase UID - unique identifier from Firebase Authentication
    /// </summary>
    [Required]
    [MaxLength(128)]
    public string FirebaseUid { get; set; } = string.Empty;

    /// <summary>
    /// User email address
    /// </summary>
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User display name
    /// </summary>
    [MaxLength(255)]
    public string? Name { get; set; }

    /// <summary>
    /// User role (Admin, User, etc.)
    /// </summary>
    [MaxLength(50)]
    public string? Role { get; set; }

    /// <summary>
    /// Organization ID (foreign key - optional)
    /// </summary>
    public Guid? OrganizationId { get; set; }

    /// <summary>
    /// Navigation property to Organization (optional)
    /// </summary>
    public virtual Organization? Organization { get; set; }

    /// <summary>
    /// Indicates if the user account is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Account creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last login timestamp
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
