namespace Wefaaq.Bll.Constants;

/// <summary>
/// Application role constants for authorization
/// </summary>
public static class Roles
{
    /// <summary>
    /// Administrator role - full system access
    /// </summary>
    public const string Admin = "Admin";

    /// <summary>
    /// Standard user role - limited access
    /// </summary>
    public const string User = "User";

    /// <summary>
    /// Role IDs for database operations
    /// </summary>
    public static class Ids
    {
        public const int Admin = 1;
        public const int User = 2;
    }

    /// <summary>
    /// Get all available role names
    /// </summary>
    public static readonly string[] AllRoles = { Admin, User };
}
