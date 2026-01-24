using Wefaaq.Dal.Entities;

namespace Wefaaq.Bll.DTOs;

/// <summary>
/// DTO for updating client with all details (organizations, branches, external workers)
/// </summary>
public class ClientWithDetailsUpdateDto
{
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    public ClientClassification Classification { get; set; }
    public decimal Balance { get; set; }

    // Direct organizations
    public List<OrganizationUpdateDto> Organizations { get; set; } = new();

    // Client branches (with their organizations and external workers)
    public List<ClientBranchWithDetailsUpdateDto> Branches { get; set; } = new();

    // Direct external workers
    public List<ExternalWorkerUpdateDto> ExternalWorkers { get; set; } = new();
}

/// <summary>
/// DTO for updating branch with organizations and external workers
/// </summary>
public class ClientBranchWithDetailsUpdateDto
{
    public Guid? Id { get; set; } // Null for new branches, populated for existing
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    public ClientClassification Classification { get; set; }
    public decimal Balance { get; set; }
    public string? BranchType { get; set; }

    // Branch organizations
    public List<OrganizationUpdateDto> Organizations { get; set; } = new();

    // Branch external workers
    public List<ExternalWorkerUpdateDto> ExternalWorkers { get; set; } = new();
}
