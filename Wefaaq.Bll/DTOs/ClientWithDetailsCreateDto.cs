using Wefaaq.Dal.Entities;

namespace Wefaaq.Bll.DTOs;

/// <summary>
/// DTO for creating client with all details (organizations, branches, external workers)
/// </summary>
public class ClientWithDetailsCreateDto
{
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    public ClientClassification Classification { get; set; }
    public decimal Balance { get; set; }

    // Direct organizations
    public List<OrganizationCreateDto> Organizations { get; set; } = new();

    // Client branches (with their organizations and external workers)
    public List<ClientBranchWithDetailsCreateDto> Branches { get; set; } = new();

    // Direct external workers
    public List<ExternalWorkerCreateDto> ExternalWorkers { get; set; } = new();
}

/// <summary>
/// DTO for creating branch with organizations and external workers
/// </summary>
public class ClientBranchWithDetailsCreateDto
{
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    public ClientClassification Classification { get; set; }
    public decimal Balance { get; set; }
    public string? BranchType { get; set; }

    // Branch organizations
    public List<OrganizationCreateDto> Organizations { get; set; } = new();

    // Branch external workers
    public List<ExternalWorkerCreateDto> ExternalWorkers { get; set; } = new();
}
