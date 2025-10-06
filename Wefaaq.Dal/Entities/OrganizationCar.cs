using System.ComponentModel.DataAnnotations;

namespace Wefaaq.Dal.Entities;

/// <summary>
/// Organization car entity (سيارة المؤسسة)
/// </summary>
public class OrganizationCar
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Car plate number (رقم لوحة)
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string PlateNumber { get; set; } = string.Empty;

    /// <summary>
    /// Car color (لون السيارة)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Color { get; set; } = string.Empty;

    /// <summary>
    /// Car serial number (رقم تسلسلى)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string SerialNumber { get; set; } = string.Empty;

    /// <summary>
    /// Car image file path (مسار صورة السيارة)
    /// </summary>
    [MaxLength(500)]
    public string? ImagePath { get; set; }

    /// <summary>
    /// Operating card expiry date (تاريخ انتهاء كارت التشغيل)
    /// </summary>
    public DateTime OperatingCardExpiry { get; set; }

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