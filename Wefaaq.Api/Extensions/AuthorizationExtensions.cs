using Microsoft.AspNetCore.Authorization;
using Wefaaq.Api.Auth;
using Wefaaq.Bll.Constants;

namespace Wefaaq.Api.Extensions;

/// <summary>
/// Extension methods for configuring authorization policies
/// </summary>
public static class AuthorizationExtensions
{
    /// <summary>
    /// Adds authorization policies to the service collection
    /// </summary>
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // Admin-only policy - requires Admin role
            options.AddPolicy("AdminOnly", policy =>
                policy.RequireRole(Roles.Admin));

            // Any authenticated user policy
            options.AddPolicy("Authenticated", policy =>
                policy.RequireAuthenticatedUser());

            // Admin or User policy (for explicit multi-role endpoints)
            options.AddPolicy("AdminOrUser", policy =>
                policy.RequireRole(Roles.Admin, Roles.User));
        });

        // Per-user page permissions: [RequirePermission(PermissionAction.X, PageKeys.Y)]
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

        return services;
    }
}
