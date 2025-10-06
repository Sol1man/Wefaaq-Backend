using System.ComponentModel.DataAnnotations;

namespace Wefaaq.Dal.Entities;

/// <summary>
/// Organization record entity (سجل المؤسسة)
/// </summary>
public class OrganizationRecord
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Record number (رقم)
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Number { get; set; } = string.Empty;

    /// <summary>
    /// Record expiry date (تاريخ الانتهاء)
    /// </summary>
    public DateTime ExpiryDate { get; set; }

    /// <summary>
    /// Image file path (مسار الصورة)
    /// </summary>
    [MaxLength(500)]
    public string? ImagePath { get; set; }

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
}