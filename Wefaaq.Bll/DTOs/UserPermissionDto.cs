namespace Wefaaq.Bll.DTOs;

/// <summary>
/// Effective permissions of one user on one page
/// </summary>
public class PagePermissionDto
{
    public string PageKey { get; set; } = string.Empty;
    public bool CanView { get; set; }
    public bool CanAddEdit { get; set; }
    public bool CanDelete { get; set; }
}

/// <summary>
/// A grantable page as the admin screen needs it: which actions apply and what the default is
/// </summary>
public class PageDefinitionDto
{
    public string Key { get; set; } = string.Empty;
    public bool SupportsAddEdit { get; set; }
    public bool SupportsDelete { get; set; }
    public bool DefaultView { get; set; }
    public bool DefaultAddEdit { get; set; }
    public bool DefaultDelete { get; set; }
}

/// <summary>
/// One user on the permissions screen, with their effective permissions per page
/// </summary>
public class UserPermissionsDto
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string RoleName { get; set; } = string.Empty;

    /// <summary>
    /// Admins always have full access; their permissions cannot be edited
    /// </summary>
    public bool IsAdmin { get; set; }

    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// True when the admin never customized this user, so every page uses its defaults
    /// </summary>
    public bool UsesDefaults { get; set; }

    public List<PagePermissionDto> Permissions { get; set; } = new();
}

/// <summary>
/// Everything the permissions screen needs in one call
/// </summary>
public class PermissionsOverviewDto
{
    public List<PageDefinitionDto> Pages { get; set; } = new();
    public List<UserPermissionsDto> Users { get; set; } = new();
}

/// <summary>
/// Request body for saving a user's permissions. Pages left out keep their current value.
/// </summary>
public class UpdateUserPermissionsDto
{
    public List<PagePermissionDto> Permissions { get; set; } = new();
}
