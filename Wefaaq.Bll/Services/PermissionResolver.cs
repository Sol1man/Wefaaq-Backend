using Wefaaq.Bll.Constants;
using Wefaaq.Bll.DTOs;
using Wefaaq.Dal.Entities;

namespace Wefaaq.Bll.Services;

/// <summary>
/// Turns stored permission rows into the effective permissions a user actually has.
/// Pure functions only — shared by login, /auth/me, the claims transformation and the admin
/// screen, so all of them agree on what a user may do.
/// </summary>
public static class PermissionResolver
{
    /// <summary>
    /// Effective permissions for every page in the catalog.
    /// Admins get everything; for anyone else a saved row wins over the page default.
    /// </summary>
    public static List<PagePermissionDto> Resolve(IEnumerable<UserPagePermission>? rows, bool isAdmin)
    {
        var saved = (rows ?? Enumerable.Empty<UserPagePermission>())
            .GroupBy(r => r.PageKey)
            .ToDictionary(g => g.Key, g => g.First());

        return PermissionCatalog.Pages.Select(page =>
        {
            if (isAdmin)
            {
                return new PagePermissionDto
                {
                    PageKey = page.Key,
                    CanView = true,
                    CanAddEdit = true,
                    CanDelete = true
                };
            }

            return saved.TryGetValue(page.Key, out var row)
                ? Normalize(page, row.CanView, row.CanAddEdit, row.CanDelete)
                : Normalize(page, page.DefaultView, page.DefaultAddEdit, page.DefaultDelete);
        }).ToList();
    }

    /// <summary>
    /// Applies the matrix rules: an action the page does not support is always off, and
    /// adding/editing or deleting is meaningless without being able to open the page.
    /// </summary>
    public static PagePermissionDto Normalize(PageDefinition page, bool canView, bool canAddEdit, bool canDelete)
    {
        return new PagePermissionDto
        {
            PageKey = page.Key,
            CanView = canView,
            CanAddEdit = canView && page.SupportsAddEdit && canAddEdit,
            CanDelete = canView && page.SupportsDelete && canDelete
        };
    }

    /// <summary>
    /// Claim values ("Page:Action") for every granted permission.
    /// </summary>
    public static IEnumerable<string> ToClaimValues(IEnumerable<PagePermissionDto> permissions)
    {
        foreach (var p in permissions)
        {
            if (p.CanView) yield return PermissionCatalog.ClaimValue(p.PageKey, PermissionAction.View);
            if (p.CanAddEdit) yield return PermissionCatalog.ClaimValue(p.PageKey, PermissionAction.AddEdit);
            if (p.CanDelete) yield return PermissionCatalog.ClaimValue(p.PageKey, PermissionAction.Delete);
        }
    }

    /// <summary>
    /// Rejects a save request that names an unknown page or the same page twice.
    /// </summary>
    /// <exception cref="ArgumentException">The request is malformed</exception>
    public static void ValidateUpdate(UpdateUserPermissionsDto? dto)
    {
        if (dto?.Permissions == null || dto.Permissions.Count == 0)
        {
            throw new ArgumentException("At least one page permission is required.");
        }

        var seen = new HashSet<string>();
        foreach (var p in dto.Permissions)
        {
            if (p == null || PermissionCatalog.Find(p.PageKey) == null)
            {
                throw new ArgumentException($"Unknown page '{p?.PageKey}'.");
            }

            if (!seen.Add(p.PageKey))
            {
                throw new ArgumentException($"Page '{p.PageKey}' is listed more than once.");
            }
        }
    }
}
