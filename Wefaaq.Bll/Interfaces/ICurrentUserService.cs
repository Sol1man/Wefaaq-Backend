namespace Wefaaq.Bll.Interfaces;

/// <summary>
/// Resolves identity and role of the caller from the current request context.
/// Returns null/false when called outside an authenticated request.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Database User.Id of the authenticated caller, or null if anonymous /
    /// no matching user record exists.
    /// </summary>
    int? UserId { get; }

    /// <summary>
    /// True when the caller has the Admin role.
    /// </summary>
    bool IsAdmin { get; }
}
