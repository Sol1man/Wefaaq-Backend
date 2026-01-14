using Wefaaq.Dal.Entities;

namespace Wefaaq.Bll.DTOs;

/// <summary>
/// External Worker DTO for responses
/// </summary>
public class ExternalWorkerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public WorkerType WorkerType { get; set; }
    public string WorkerTypeDisplay => WorkerType.ToString();
    public string? ResidenceNumber { get; set; }
    public string? ResidenceImagePath { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public Guid? ClientId { get; set; }
    public string? Client { get; set; }
    public Guid? ClientBranchId { get; set; }
    public string? ClientBranch { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// External Worker DTO for creating new workers
/// </summary>
public class ExternalWorkerCreateDto
{
    public string Name { get; set; } = string.Empty;
    public WorkerType WorkerType { get; set; }
    public string? ResidenceNumber { get; set; }
    public string? ResidenceImagePath { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public Guid? ClientId { get; set; }
    public Guid? ClientBranchId { get; set; }
}

/// <summary>
/// External Worker DTO for updating existing workers
/// </summary>
public class ExternalWorkerUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public WorkerType WorkerType { get; set; }
    public string? ResidenceNumber { get; set; }
    public string? ResidenceImagePath { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public Guid? ClientId { get; set; }
    public Guid? ClientBranchId { get; set; }
}
