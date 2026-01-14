using System.ComponentModel.DataAnnotations;
using Wefaaq.Dal.Interfaces;

namespace Wefaaq.Dal.Entities;

/// <summary>
/// External worker entity (عامل خارجي)
/// Workers employed directly by clients, not through organizations
/// </summary>
public class ExternalWorker : ISoftDeletable
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
    /// Worker type classification (نوع العامل)
    /// </summary>
    public WorkerType WorkerType { get; set; }

    /// <summary>
    /// Client identifier (foreign key to main client)
    /// </summary>
    public Guid? ClientId { get; set; }

    /// <summary>
    /// Navigation property to Client
    /// </summary>
    public virtual Client? Client { get; set; }

    /// <summary>
    /// Client branch identifier (foreign key to client branch)
    /// </summary>
    public Guid? ClientBranchId { get; set; }

    /// <summary>
    /// Navigation property to ClientBranch
    /// </summary>
    public virtual ClientBranch? ClientBranch { get; set; }

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
