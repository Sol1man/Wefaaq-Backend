using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Wefaaq.Bll.Constants;

namespace Wefaaq.Api.Auth;

/// <summary>
/// Requires the caller to hold <paramref name="action"/> on at least one of <paramref name="pages"/>.
/// Admins always pass.
///
/// Usage:
/// <code>
/// [RequirePermission(PermissionAction.Delete, PageKeys.Clients)]
/// // Lookup used by several screens — any one of them is enough:
/// [RequirePermission(PermissionAction.View, PageKeys.Clients, PageKeys.Rents)]
/// </code>
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class RequirePermissionAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "Permission:";

    public RequirePermissionAttribute(PermissionAction action, params string[] pages)
    {
        if (pages == null || pages.Length == 0)
        {
            throw new ArgumentException("At least one page is required.", nameof(pages));
        }

        Action = action;
        Pages = pages;
        Policy = $"{PolicyPrefix}{action}:{string.Join(',', pages)}";
    }

    public PermissionAction Action { get; }

    public IReadOnlyList<string> Pages { get; }
}

/// <summary>
/// Authorization requirement behind <see cref="RequirePermissionAttribute"/>.
/// </summary>
public sealed class PermissionRequirement(PermissionAction action, IReadOnlyList<string> pages)
    : IAuthorizationRequirement
{
    public PermissionAction Action { get; } = action;

    public IReadOnlyList<string> Pages { get; } = pages;

    /// <summary>
    /// Parses a policy name produced by <see cref="RequirePermissionAttribute"/>
    /// ("Permission:Delete:Clients,Rents"). Returns null for any other policy name.
    /// </summary>
    public static PermissionRequirement? TryParse(string policyName)
    {
        if (!policyName.StartsWith(RequirePermissionAttribute.PolicyPrefix, StringComparison.Ordinal))
        {
            return null;
        }

        var parts = policyName[RequirePermissionAttribute.PolicyPrefix.Length..].Split(':', 2);
        if (parts.Length != 2 || !Enum.TryParse<PermissionAction>(parts[0], out var action))
        {
            return null;
        }

        var pages = parts[1].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return pages.Length == 0 ? null : new PermissionRequirement(action, pages);
    }
}

/// <summary>
/// Builds "Permission:…" policies on demand so each endpoint can name its own page/action
/// without registering every combination up front. Every other policy name ("AdminOnly", …)
/// falls through to the normal options-based provider.
/// </summary>
public sealed class PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    : DefaultAuthorizationPolicyProvider(options)
{
    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var requirement = PermissionRequirement.TryParse(policyName);
        if (requirement == null)
        {
            return await base.GetPolicyAsync(policyName);
        }

        return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(requirement)
            .Build();
    }
}

/// <summary>
/// Grants the requirement to admins, or to anyone whose "perm" claims include the action on one
/// of the listed pages. The claims are added per request by <see cref="RoleClaimsTransformation"/>
/// from the database, so a permission the admin revokes stops working on the next request.
/// </summary>
public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var user = context.User;

        if (user.IsInRole(Roles.Admin))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var granted = requirement.Pages.Any(page =>
            user.HasClaim(PermissionCatalog.ClaimType, PermissionCatalog.ClaimValue(page, requirement.Action)));

        if (granted)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
