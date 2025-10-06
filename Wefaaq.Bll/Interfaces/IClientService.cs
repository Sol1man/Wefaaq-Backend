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
}