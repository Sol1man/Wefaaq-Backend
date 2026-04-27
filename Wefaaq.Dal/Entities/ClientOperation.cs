using System.ComponentModel.DataAnnotations;
using Wefaaq.Dal.Interfaces;

namespace Wefaaq.Dal.Entities;

/// <summary>
/// Client operation entity — a service performed on/for a client, branch, organization, or external person
/// (عملية العميل)
/// </summary>
public class ClientOperation : ISoftDeletable
{
    /// <summary>Unique identifier</summary>
    public Guid Id { get; set; }

    /// <summary>Type of operation (نوع العملية)</summary>
    public OperationType Type { get; set; }

    /// <summary>Target entity type (نوع المستهدف)</summary>
    public OperationTargetType TargetType { get; set; }

    /// <summary>Current status (الحالة)</summary>
    public OperationStatus Status { get; set; } = OperationStatus.Pending;

    /// <summary>Cost/price of the operation (التكلفة) — debited from target balance on completion</summary>
    public decimal? Price { get; set; }

    /// <summary>Optional notes (ملاحظات)</summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }

    // ── Polymorphic target — exactly ONE of the following three FKs is non-null
    //    (or none, when TargetType = ExternalPerson)

    /// <summary>Client FK — set when TargetType = Client</summary>
    public Guid? ClientId { get; set; }
    public virtual Client? Client { get; set; }

    /// <summary>ClientBranch FK — set when TargetType = ClientBranch</summary>
    public Guid? ClientBranchId { get; set; }
    public virtual ClientBranch? ClientBranch { get; set; }

    /// <summary>Organization FK — set when TargetType = Organization</summary>
    public Guid? OrganizationId { get; set; }
    public virtual Organization? Organization { get; set; }

    // ── External person fields (when TargetType = ExternalPerson)

    /// <summary>External person full name (اسم العميل الخارجي)</summary>
    [MaxLength(255)]
    public string? ExternalPersonName { get; set; }

    /// <summary>External person national/residence ID (رقم الهوية)</summary>
    [MaxLength(50)]
    public string? ExternalPersonIdNumber { get; set; }

    // ── Audit fields

    /// <summary>User who performed / created this operation</summary>
    public int PerformedByUserId { get; set; }
    public virtual User? PerformedByUser { get; set; }

    /// <summary>Timestamp when the operation was completed</summary>
    public DateTime? CompletedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
