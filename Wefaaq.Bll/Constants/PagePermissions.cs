namespace Wefaaq.Bll.Constants;

/// <summary>
/// Pages (screens) whose access the admin controls per user. The string values are stored
/// in UserPagePermissions.PageKey and mirrored by the frontend's PageKey enum — never rename one.
/// </summary>
public static class PageKeys
{
    public const string Clients = "Clients";
    public const string Organizations = "Organizations";
    public const string ClientOperations = "ClientOperations";
    public const string Payments = "Payments";
    public const string Costs = "Costs";
    public const string EmployeeSalaries = "EmployeeSalaries";
    public const string Rents = "Rents";
}

/// <summary>
/// What a user may do on a page.
/// </summary>
public enum PermissionAction
{
    /// <summary>Open the page and read its data</summary>
    View = 0,

    /// <summary>Add new records and edit existing ones</summary>
    AddEdit = 1,

    /// <summary>Delete records</summary>
    Delete = 2
}

/// <summary>
/// A page the admin can grant, which actions it supports for non-admin users, and the
/// permissions a user gets on it until the admin sets them explicitly.
/// </summary>
/// <param name="Key">Page key (see <see cref="PageKeys"/>)</param>
/// <param name="SupportsAddEdit">False when adding/editing on this page stays admin-only</param>
/// <param name="SupportsDelete">False when deleting on this page stays admin-only</param>
/// <param name="DefaultView">View permission for a user with no saved row for this page</param>
/// <param name="DefaultAddEdit">Add/Edit permission for a user with no saved row for this page</param>
/// <param name="DefaultDelete">Delete permission for a user with no saved row for this page</param>
public sealed record PageDefinition(
    string Key,
    bool SupportsAddEdit,
    bool SupportsDelete,
    bool DefaultView,
    bool DefaultAddEdit,
    bool DefaultDelete);

/// <summary>
/// The single source of truth for the permission matrix.
/// </summary>
public static class PermissionCatalog
{
    /// <summary>
    /// Claim type carrying one granted permission, e.g. "Clients:AddEdit".
    /// </summary>
    public const string ClaimType = "perm";

    /// <summary>
    /// Every grantable page, in the order the admin screen lists them.
    ///
    /// The defaults reproduce exactly what the legacy "User" role could do before per-user
    /// permissions existed, so existing users see no change until an admin edits them:
    /// - Clients / Organizations: view + add/edit, delete was admin-only.
    /// - Client operations / Rents: view + add/edit + delete.
    /// - Payments / Employee salaries: the user's OWN account / salary, read-only. Managing other
    ///   users' payments and salaries stays admin-only (it needs data scoping, a later phase).
    /// - Costs: admin-only.
    /// </summary>
    public static readonly IReadOnlyList<PageDefinition> Pages = new[]
    {
        new PageDefinition(PageKeys.Clients,          SupportsAddEdit: true,  SupportsDelete: true,  DefaultView: true,  DefaultAddEdit: true,  DefaultDelete: false),
        new PageDefinition(PageKeys.Organizations,    SupportsAddEdit: true,  SupportsDelete: true,  DefaultView: true,  DefaultAddEdit: true,  DefaultDelete: false),
        new PageDefinition(PageKeys.ClientOperations, SupportsAddEdit: true,  SupportsDelete: true,  DefaultView: true,  DefaultAddEdit: true,  DefaultDelete: true),
        new PageDefinition(PageKeys.Payments,         SupportsAddEdit: false, SupportsDelete: false, DefaultView: true,  DefaultAddEdit: false, DefaultDelete: false),
        new PageDefinition(PageKeys.Costs,            SupportsAddEdit: true,  SupportsDelete: true,  DefaultView: false, DefaultAddEdit: false, DefaultDelete: false),
        new PageDefinition(PageKeys.EmployeeSalaries, SupportsAddEdit: false, SupportsDelete: false, DefaultView: true,  DefaultAddEdit: false, DefaultDelete: false),
        new PageDefinition(PageKeys.Rents,            SupportsAddEdit: true,  SupportsDelete: true,  DefaultView: true,  DefaultAddEdit: true,  DefaultDelete: true),
    };

    /// <summary>
    /// Finds a page by key (case-sensitive, keys are stored verbatim).
    /// </summary>
    public static PageDefinition? Find(string? key) =>
        key == null ? null : Pages.FirstOrDefault(p => p.Key == key);

    /// <summary>
    /// Claim value for one permission, e.g. ClaimValue("Clients", AddEdit) = "Clients:AddEdit".
    /// </summary>
    public static string ClaimValue(string pageKey, PermissionAction action) => $"{pageKey}:{action}";
}
