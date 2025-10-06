using Wefaaq.Dal.Entities;

namespace Wefaaq.Dal.RepositoriesInterfaces;

/// <summary>
/// Client repository interface with specific client operations
/// </summary>
public interface IClientRepository : IGenericRepository<Client>
{
    /// <summary>
    /// Get client with their organizations
    /// </summary>
    /// <param name="id">Client ID</param>
    /// <returns>Client with organizations or null</returns>
    Task<Client?> GetWithOrganizationsAsync(Guid id);

    /// <summary>
    /// Get clients by classification
    /// </summary>
    /// <param name="classification">Client classification</param>
    /// <returns>Clients with the specified classification</returns>
    Task<IEnumerable<Client>> GetByClassificationAsync(ClientClassification classification);

    /// <summary>
    /// Get clients with positive balance (creditors - دائن)
    /// </summary>
    /// <returns>Clients with positive balance</returns>
    Task<IEnumerable<Client>> GetCreditorsAsync();

    /// <summary>
    /// Get clients with negative balance (debtors - مدين)
    /// </summary>
    /// <returns>Clients with negative balance</returns>
    Task<IEnumerable<Client>> GetDebtorsAsync();

    /// <summary>
    /// Check if email exists for another client
    /// </summary>
    /// <param name="email">Email to check</param>
    /// <param name="excludeClientId">Client ID to exclude from check</param>
    /// <returns>True if email exists</returns>
    Task<bool> EmailExistsAsync(string email, Guid? excludeClientId = null);
}