using Wefaaq.Bll.DTOs;

namespace Wefaaq.Bll.Interfaces;

/// <summary>
/// Service interface for client branch operations
/// </summary>
public interface IClientBranchService
{
    /// <summary>
    /// Get all client branches
    /// </summary>
    Task<IEnumerable<ClientBranchDto>> GetAllAsync();

    /// <summary>
    /// Get client branch by ID
    /// </summary>
    Task<ClientBranchDto?> GetByIdAsync(Guid id);

    /// <summary>
    /// Get client branch with all details (organizations, external workers)
    /// </summary>
    Task<ClientBranchDto?> GetWithDetailsAsync(Guid id);

    /// <summary>
    /// Get all branches for a specific client
    /// </summary>
    Task<IEnumerable<ClientBranchDto>> GetByClientIdAsync(Guid clientId);

    /// <summary>
    /// Create new client branch
    /// </summary>
    Task<ClientBranchDto> CreateAsync(ClientBranchCreateDto branchCreateDto);

    /// <summary>
    /// Update existing client branch
    /// </summary>
    Task<ClientBranchDto?> UpdateAsync(Guid id, ClientBranchUpdateDto branchUpdateDto);

    /// <summary>
    /// Delete client branch (soft delete)
    /// </summary>
    Task<bool> DeleteAsync(Guid id);

    // ===== GRANULAR OPERATIONS (Add items to branch) =====

    /// <summary>
    /// Add organization to existing branch
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="organizationDto">Organization creation data</param>
    /// <returns>Created organization DTO</returns>
    Task<OrganizationDto> AddOrganizationToBranchAsync(Guid branchId, OrganizationCreateDto organizationDto);

    /// <summary>
    /// Add external worker to existing branch
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="workerDto">External worker creation data</param>
    /// <returns>Created external worker DTO</returns>
    Task<ExternalWorkerDto> AddExternalWorkerToBranchAsync(Guid branchId, ExternalWorkerCreateDto workerDto);
}
