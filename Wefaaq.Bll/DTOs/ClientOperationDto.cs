using Wefaaq.Dal.Entities;

namespace Wefaaq.Bll.DTOs;

/// <summary>
/// Full response DTO for a client operation
/// </summary>
public class ClientOperationDto
{
    public Guid Id { get; set; }
    public OperationType Type { get; set; }
    public string? CustomType { get; set; }
    public string TypeDisplay { get; set; } = string.Empty;
    public OperationTargetType TargetType { get; set; }
    public OperationStatus Status { get; set; }
    public decimal? Price { get; set; }
    public string? Notes { get; set; }

    // Target info (resolved for display)
    public Guid? ClientId { get; set; }
    public string? ClientName { get; set; }
    public Guid? ClientBranchId { get; set; }
    public string? ClientBranchName { get; set; }
    public Guid? OrganizationId { get; set; }
    public string? OrganizationName { get; set; }
    public string? ExternalPersonName { get; set; }
    public string? ExternalPersonIdNumber { get; set; }

    // Audit
    public int PerformedByUserId { get; set; }
    public string PerformedByUserName { get; set; } = string.Empty;
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating a new operation
/// </summary>
public class ClientOperationCreateDto
{
    public OperationType Type { get; set; }
    /// <summary>Free-text label, required only when Type = Other (نوع مخصص)</summary>
    public string? CustomType { get; set; }
    public OperationTargetType TargetType { get; set; }
    public decimal? Price { get; set; }
    public string? Notes { get; set; }

    // Target — only the relevant FK is provided based on TargetType
    public Guid? ClientId { get; set; }
    public Guid? ClientBranchId { get; set; }
    public Guid? OrganizationId { get; set; }

    // External person
    public string? ExternalPersonName { get; set; }
    public string? ExternalPersonIdNumber { get; set; }
}

/// <summary>
/// DTO for updating an existing operation (status, price, notes)
/// </summary>
public class ClientOperationUpdateDto
{
    public OperationType Type { get; set; }
    /// <summary>Free-text label, required only when Type = Other (نوع مخصص)</summary>
    public string? CustomType { get; set; }
    public OperationStatus Status { get; set; }
    public decimal? Price { get; set; }
    public string? Notes { get; set; }
}
