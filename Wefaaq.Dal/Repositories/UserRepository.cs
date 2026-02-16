using Microsoft.EntityFrameworkCore;
using Wefaaq.Dal.Entities;
using Wefaaq.Dal.RepositoriesInterfaces;

namespace Wefaaq.Dal.Repositories;

/// <summary>
/// User repository implementation
/// </summary>
public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(WefaaqContext context) : base(context)
    {
    }

    /// <summary>
    /// Get user by Firebase UID
    /// </summary>
    public async Task<User?> GetByFirebaseUidAsync(string firebaseUid)
    {
        return await Context.Users
            .Include(u => u.Organization)
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
    }

    /// <summary>
    /// Get user by email
    /// </summary>
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await Context.Users
            .Include(u => u.Organization)
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    /// <summary>
    /// Update last login timestamp
    /// </summary>
    public async Task UpdateLastLoginAsync(int userId)
    {
        var user = await Context.Users.FindAsync(userId);
        if (user != null)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await Context.SaveChangesAsync();
        }
    }
}
