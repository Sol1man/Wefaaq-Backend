using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Wefaaq.Dal.RepositoriesInterfaces;

namespace Wefaaq.Api.Auth;

/// <summary>
/// Transforms claims to include user role from database
/// </summary>
public class RoleClaimsTransformation : IClaimsTransformation
{
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
        // Get Firebase UID from claims (Firebase uses "user_id" claim)
        var firebaseUid = principal.FindFirst("user_id")?.Value;

        if (string.IsNullOrEmpty(firebaseUid))
        {
            _logger.LogDebug("No Firebase UID found in claims");
            return principal;
        }

        // Get user from database
        var user = await _userRepository.GetByFirebaseUidAsync(firebaseUid);

        if (user?.Role == null)
        {
            _logger.LogDebug("User not found or has no role for FirebaseUid: {FirebaseUid}", firebaseUid);
            return principal;
        }

        // Create a new identity with the role claim
        var claimsIdentity = new ClaimsIdentity();

        // Add role claim (used by [Authorize(Roles = "Admin")])
        claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, user.Role.Name));

        // Add custom role claim (used by policies)
        claimsIdentity.AddClaim(new Claim("role", user.Role.Name));

        // Add user ID claim for easy access
        claimsIdentity.AddClaim(new Claim("userId", user.Id.ToString()));

        principal.AddIdentity(claimsIdentity);

        _logger.LogDebug("Added role claim '{Role}' for user {UserId}", user.Role.Name, user.Id);

        return principal;
    }
}
