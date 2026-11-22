using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Wefaaq.Bll.Constants;
using Wefaaq.Bll.Services;
using Wefaaq.Dal.RepositoriesInterfaces;

namespace Wefaaq.Api.Auth;

/// <summary>
/// Transforms claims to include the user's role and page permissions from the database.
/// Runs on every authenticated request, so a role or permission change made by an admin
/// applies on the user's next request — no re-login needed.
/// </summary>
public class RoleClaimsTransformation : IClaimsTransformation
{
    private const string UserIdClaimType = "userId";

    private readonly IUserRepository _userRepository;
    private readonly ILogger<RoleClaimsTransformation> _logger;

    public RoleClaimsTransformation(
        IUserRepository userRepository,
        ILogger<RoleClaimsTransformation> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        try
        {
            // ASP.NET Core may call the transformation more than once per request;
            // the database claims only need to be added once.
            if (principal.HasClaim(c => c.Type == UserIdClaimType))
            {
                return principal;
            }

            // Get Firebase UID from claims (Firebase uses "user_id" claim)
            var firebaseUid = principal.FindFirst("user_id")?.Value;

            if (string.IsNullOrEmpty(firebaseUid))
            {
                _logger.LogDebug("No Firebase UID found in claims");
                return principal;
            }

            // Get user (with role + page permissions) from database
            var user = await _userRepository.GetByFirebaseUidAsync(firebaseUid);

            if (user?.Role == null)
            {
                _logger.LogDebug("User not found or has no role for FirebaseUid: {FirebaseUid}", firebaseUid);
                return principal;
            }

            // A deactivated account keeps a valid Firebase token until it expires; without
            // a role or permissions it can no longer reach anything that is protected.
            if (!user.IsActive)
            {
                _logger.LogInformation("Inactive user {UserId} denied role and permission claims", user.Id);
                return principal;
            }

            // Create a new identity with the role claim
            var claimsIdentity = new ClaimsIdentity();

            // Add role claim (used by [Authorize(Roles = "Admin")])
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, user.Role.Name));

            // Add custom role claim (used by policies)
            claimsIdentity.AddClaim(new Claim("role", user.Role.Name));

            // Add user ID claim for easy access
            claimsIdentity.AddClaim(new Claim(UserIdClaimType, user.Id.ToString()));

            // Add one "perm" claim per granted page action (used by [RequirePermission]).
            // Admins pass every permission check by role, so they don't need these.
            var isAdmin = user.Role.Name == Roles.Admin;
            if (!isAdmin)
            {
                var permissions = PermissionResolver.Resolve(user.PagePermissions, isAdmin: false);
                foreach (var value in PermissionResolver.ToClaimValues(permissions))
                {
                    claimsIdentity.AddClaim(new Claim(PermissionCatalog.ClaimType, value));
                }
            }

            principal.AddIdentity(claimsIdentity);

            _logger.LogDebug("Added role claim '{Role}' for user {UserId}", user.Role.Name, user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during claims transformation");
        }

        return principal;
    }
}
