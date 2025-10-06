using System.ComponentModel.DataAnnotations;

namespace Wefaaq.Dal.Entities;

/// <summary>
/// Organization worker entity (عامل المؤسسة)
/// </summary>
public class OrganizationWorker
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Worker name (اسم العامل)
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Residence number (رقم إقامة)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string ResidenceNumber { get; set; } = string.Empty;

    /// <summary>
    /// Residence image file path (مسار صورة الإقامة)
    /// </summary>
    [MaxLength(500)]
    public string? ResidenceImagePath { get; set; }

    /// <summary>
    /// Residence expiry date (تاريخ انتهاء الإقامة)
    /// </summary>
    public DateTime ExpiryDate { get; set; }

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