using System.ComponentModel.DataAnnotations;

namespace Wefaaq.Dal.Entities;

/// <summary>
/// What a single (non-admin) user may do on one page of the app (صلاحيات المستخدم).
/// One row per user per page. A page with no row falls back to that page's default
/// permissions, so a user with no rows at all behaves exactly like the legacy "User" role.
/// Admins ignore these rows entirely — they always have full access.
/// </summary>
public class UserPagePermission
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The user these permissions belong to
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Navigation property to the user
    /// </summary>
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// Page key (e.g. "Clients", "Rents") — see PageKeys in the business layer
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string PageKey { get; set; } = string.Empty;

    /// <summary>
    /// May open the page and read its data
    /// </summary>
    public bool CanView { get; set; }

    /// <summary>
    /// May add new records and edit existing ones on the page
    /// </summary>
    public bool CanAddEdit { get; set; }

    /// <summary>
    /// May delete records on the page
    /// </summary>
    public bool CanDelete { get; set; }

    /// <summary>
    /// Last time an admin changed this row
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
