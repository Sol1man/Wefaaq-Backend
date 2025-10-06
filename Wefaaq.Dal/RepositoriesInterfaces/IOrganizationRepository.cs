using Wefaaq.Dal.Entities;

namespace Wefaaq.Dal.RepositoriesInterfaces;

/// <summary>
/// Organization repository interface with specific organization operations
/// </summary>
public interface IOrganizationRepository : IGenericRepository<Organization>
{
    /// <summary>
    /// Get organization with all details (records, licenses, workers, cars)
    /// </summary>
    /// <param name="id">Organization ID</param>
    /// <returns>Organization with details or null</returns>
    Task<Organization?> GetWithDetailsAsync(Guid id);

    /// <summary>
    /// Get organizations with expiring cards
    /// </summary>
    /// <returns>Organizations with cards expiring soon</returns>
    Task<IEnumerable<Organization>> GetWithExpiringCardsAsync();

    /// <summary>
    /// Get organizations with their clients
    /// </summary>
    /// <param name="id">Organization ID</param>
    /// <returns>Organization with clients or null</returns>
    Task<Organization?> GetWithClientsAsync(Guid id);

    /// <summary>
    /// Get organizations by client ID
    /// </summary>
    /// <param name="clientId">Client ID</param>
    /// <returns>Organizations associated with the client</returns>
    Task<IEnumerable<Organization>> GetByClientIdAsync(Guid clientId);
}