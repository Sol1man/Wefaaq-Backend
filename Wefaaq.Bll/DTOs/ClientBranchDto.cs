using Wefaaq.Dal.Entities;

namespace Wefaaq.Bll.DTOs;

/// <summary>
/// Client Branch DTO for responses
/// </summary>
public class ClientBranchDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public ClientClassification Classification { get; set; }
    public decimal Balance { get; set; }
    public Guid ParentClientId { get; set; }
    public string ParentClient { get; set; } = string.Empty;
    public string? BranchType { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<OrganizationDto> Organizations { get; set; } = new();
    public List<ExternalWorkerDto> ExternalWorkers { get; set; } = new();
}

/// <summary>
/// Client Branch DTO for creating new branches
/// </summary>
public class ClientBranchCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public ClientClassification Classification { get; set; }
    public decimal Balance { get; set; }
    public Guid ParentClientId { get; set; }
    public string? BranchType { get; set; }
}

/// <summary>
/// Client Branch DTO for updating existing branches
/// </summary>
public class ClientBranchUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public ClientClassification Classification { get; set; }
    public decimal Balance { get; set; }
    public Guid ParentClientId { get; set; }
    public string? BranchType { get; set; }
}
