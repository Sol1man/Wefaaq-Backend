using Microsoft.EntityFrameworkCore;
using Wefaaq.Bll.Constants;
using Wefaaq.Bll.DTOs;
using Wefaaq.Bll.Interfaces;
using Wefaaq.Dal;
using Wefaaq.Dal.Entities;

namespace Wefaaq.Bll.Services;

/// <summary>
/// Admin management of per-user page permissions (صلاحيات المستخدمين).
/// Changes take effect on the user's very next API request: permissions are re-read from the
/// database on every request by the claims transformation, nothing is cached.
/// </summary>
public class UserPermissionService(WefaaqContext context) : IUserPermissionService
{
    public async Task<PermissionsOverviewDto> GetOverviewAsync()
    {
        var users = await context.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .Include(u => u.PagePermissions)
            .AsSplitQuery()
            .OrderBy(u => u.Name ?? u.Email)
            .ToListAsync();

        return new PermissionsOverviewDto
        {
            Pages = PermissionCatalog.Pages.Select(p => new PageDefinitionDto
            {
                Key = p.Key,
                SupportsAddEdit = p.SupportsAddEdit,
                SupportsDelete = p.SupportsDelete,
                DefaultView = p.DefaultView,
                DefaultAddEdit = p.DefaultAddEdit,
                DefaultDelete = p.DefaultDelete
            }).ToList(),
            Users = users.Select(ToDto).ToList()
        };
    }

    public async Task<UserPermissionsDto> UpdateAsync(int userId, UpdateUserPermissionsDto dto)
    {
        PermissionResolver.ValidateUpdate(dto);

        var user = await LoadEditableUserAsync(userId);

        foreach (var requested in dto.Permissions)
        {
            var page = PermissionCatalog.Find(requested.PageKey)!;
            var normalized = PermissionResolver.Normalize(
                page, requested.CanView, requested.CanAddEdit, requested.CanDelete);

            var row = user.PagePermissions.FirstOrDefault(r => r.PageKey == page.Key);
            if (row == null)
            {
                row = new UserPagePermission { UserId = user.Id, PageKey = page.Key };
                user.PagePermissions.Add(row);
            }

            row.CanView = normalized.CanView;
            row.CanAddEdit = normalized.CanAddEdit;
            row.CanDelete = normalized.CanDelete;
        }

        await context.SaveChangesAsync();
        return ToDto(user);
    }

    public async Task<UserPermissionsDto> ResetToDefaultsAsync(int userId)
    {
        var user = await LoadEditableUserAsync(userId);

        if (user.PagePermissions.Count > 0)
        {
            context.UserPagePermissions.RemoveRange(user.PagePermissions);
            await context.SaveChangesAsync();
        }

        user.PagePermissions.Clear();
        return ToDto(user);
    }

    /// <summary>
    /// Loads a user (tracked, with role + saved rows) and refuses admins, whose access is fixed.
    /// </summary>
    private async Task<User> LoadEditableUserAsync(int userId)
    {
        var user = await context.Users
            .Include(u => u.Role)
            .Include(u => u.PagePermissions)
            .AsSplitQuery()
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new KeyNotFoundException($"User with ID {userId} not found");

        if (IsAdmin(user))
        {
            throw new InvalidOperationException(
                "Admins always have full access; their permissions cannot be changed.");
        }

        return user;
    }

    private static bool IsAdmin(User user) => user.Role?.Name == Roles.Admin;

    private static UserPermissionsDto ToDto(User user)
    {
        var isAdmin = IsAdmin(user);
        return new UserPermissionsDto
        {
            UserId = user.Id,
            Email = user.Email,
            Name = user.Name,
            RoleName = user.Role?.Name ?? string.Empty,
            IsAdmin = isAdmin,
            IsActive = user.IsActive,
            LastLoginAt = user.LastLoginAt,
            UsesDefaults = !isAdmin && user.PagePermissions.Count == 0,
            Permissions = PermissionResolver.Resolve(user.PagePermissions, isAdmin)
        };
    }
}
