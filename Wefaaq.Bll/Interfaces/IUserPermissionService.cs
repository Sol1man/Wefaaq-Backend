using Wefaaq.Bll.DTOs;

namespace Wefaaq.Bll.Interfaces;

/// <summary>
/// Admin management of per-user page permissions (صلاحيات المستخدمين)
/// </summary>
public interface IUserPermissionService
{
    /// <summary>
    /// The page catalog plus every user with their effective permissions
    /// </summary>
    Task<PermissionsOverviewDto> GetOverviewAsync();

    /// <summary>
    /// Saves a user's permissions for the pages in the request.
    /// </summary>
    /// <exception cref="KeyNotFoundException">The user does not exist</exception>
    /// <exception cref="ArgumentException">The request names an unknown or duplicate page</exception>
    /// <exception cref="InvalidOperationException">The user is an admin (always full access)</exception>
    Task<UserPermissionsDto> UpdateAsync(int userId, UpdateUserPermissionsDto dto);

    /// <summary>
    /// Deletes a user's saved permissions so every page falls back to its default.
    /// </summary>
    /// <exception cref="KeyNotFoundException">The user does not exist</exception>
    /// <exception cref="InvalidOperationException">The user is an admin (always full access)</exception>
    Task<UserPermissionsDto> ResetToDefaultsAsync(int userId);
}
