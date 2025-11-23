using Wefaaq.Dal.Entities;

namespace Wefaaq.Dal.RepositoriesInterfaces;

/// <summary>
/// User repository interface
/// </summary>
public interface IUserRepository : IGenericRepository<User>
{
    /// <summary>
    /// Get user by Firebase UID
    /// </summary>
    Task<User?> GetByFirebaseUidAsync(string firebaseUid);

    /// <summary>
    /// Get user by email
    /// </summary>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>
    /// Update last login timestamp
    /// </summary>
    Task UpdateLastLoginAsync(int userId);
}
