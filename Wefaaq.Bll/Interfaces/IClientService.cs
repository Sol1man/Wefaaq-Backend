using Wefaaq.Bll.DTOs;

namespace Wefaaq.Bll.Interfaces;

/// <summary>
/// Client service interface
/// </summary>
public interface IClientService
{
    /// <summary>
    /// Get all clients
    /// </summary>
    /// <returns>List of client DTOs</returns>
    Task<IEnumerable<ClientDto>> GetAllAsync();

    /// <summary>
    /// Get client by ID
    /// </summary>
    /// <param name="id">Client ID</param>
    /// <returns>Client DTO or null</returns>
    Task<ClientDto?> GetByIdAsync(Guid id);

    /// <summary>
    /// Create new client
    /// </summary>
    /// <param name="clientCreateDto">Client creation data</param>
    /// <returns>Created client DTO</returns>
    Task<ClientDto> CreateAsync(ClientCreateDto clientCreateDto);

    /// <summary>
    /// Update existing client
    /// </summary>
    /// <param name="id">Client ID</param>
    /// <param name="clientUpdateDto">Client update data</param>
    /// <returns>Updated client DTO or null if not found</returns>
    Task<ClientDto?> UpdateAsync(Guid id, ClientUpdateDto clientUpdateDto);

    /// <summary>
    /// Delete client
    /// </summary>
    /// <param name="id">Client ID</param>
    /// <returns>True if deleted, false if not found</returns>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    /// Get client with organizations
    /// </summary>
    /// <param name="id">Client ID</param>
    /// <returns>Client DTO with organizations or null</returns>
    Task<ClientDto?> GetWithOrganizationsAsync(Guid id);

    /// <summary>
    /// Get clients with positive balance (creditors - دائن)
    /// </summary>
    /// <returns>List of creditor client DTOs</returns>
    Task<IEnumerable<ClientDto>> GetCreditorsAsync();

    /// <summary>
    /// Get clients with negative balance (debtors - مدين)
    /// </summary>
    /// <returns>List of debtor client DTOs</returns>
    Task<IEnumerable<ClientDto>> GetDebtorsAsync();

    /// <summary>
    /// Create new client with organizations in a single request
    /// </summary>
    /// <param name="dto">Client and organizations creation data</param>
    /// <returns>Created client DTO with organizations</returns>
    Task<ClientDto> AddClientWithOrganizationsAsync(ClientWithOrganizationsCreateDto dto);

    /// <summary>
    /// Update existing client with organizations in a single request
    /// </summary>
    /// <param name="id">Client ID</param>
    /// <param name="dto">Client and organizations update data</param>
    /// <returns>Updated client DTO with organizations or null if not found</returns>
    Task<ClientDto?> EditClientWithOrganizationsAsync(Guid id, ClientWithOrganizationsUpdateDto dto);

    // ===== BULK OPERATIONS (Create/Edit with all details) =====

    /// <summary>
    /// Create new client with all details (organizations, branches, external workers)
    /// </summary>
    /// <param name="dto">Client with all details creation data</param>
    /// <returns>Created client DTO with all details</returns>
    Task<ClientDto> AddClientWithDetailsAsync(ClientWithDetailsCreateDto dto);

    /// <summary>
    /// Update existing client with all details (organizations, branches, external workers)
    /// </summary>
    /// <param name="id">Client ID</param>
    /// <param name="dto">Client with all details update data</param>
    /// <returns>Updated client DTO with all details or null if not found</returns>
    Task<ClientDto?> EditClientWithDetailsAsync(Guid id, ClientWithDetailsUpdateDto dto);

    // ===== GRANULAR OPERATIONS (Add individual items to existing client) =====

    /// <summary>
    /// Add organization to existing client
    /// </summary>
    /// <param name="clientId">Client ID</param>
    /// <param name="organizationDto">Organization creation data</param>
    /// <returns>Created organization DTO</returns>
    Task<OrganizationDto> AddOrganizationToClientAsync(Guid clientId, OrganizationCreateDto organizationDto);

    /// <summary>
    /// Add branch to existing client
    /// </summary>
    /// <param name="clientId">Client ID</param>
    /// <param name="branchDto">Branch creation data</param>
    /// <returns>Created branch DTO</returns>
    Task<ClientBranchDto> AddBranchToClientAsync(Guid clientId, ClientBranchCreateDto branchDto);

    /// <summary>
    /// Add external worker to existing client
    /// </summary>
    /// <param name="clientId">Client ID</param>
    /// <param name="workerDto">External worker creation data</param>
    /// <returns>Created external worker DTO</returns>
    Task<ExternalWorkerDto> AddExternalWorkerToClientAsync(Guid clientId, ExternalWorkerCreateDto workerDto);
}