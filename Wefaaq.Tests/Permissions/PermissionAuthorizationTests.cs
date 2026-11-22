using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Wefaaq.Api.Auth;
using Wefaaq.Bll.Constants;

namespace Wefaaq.Tests.Permissions;

public class PermissionAuthorizationTests
{
    private static ClaimsPrincipal Principal(string? role, params string[] perms)
    {
        var identity = new ClaimsIdentity("Test");
        if (role != null)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, role));
        }
        foreach (var p in perms)
        {
            identity.AddClaim(new Claim(PermissionCatalog.ClaimType, p));
        }
        return new ClaimsPrincipal(identity);
    }

    private static async Task<bool> Authorize(ClaimsPrincipal user, PermissionAction action, params string[] pages)
    {
        var requirement = new PermissionRequirement(action, pages);
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, resource: null);
        await new PermissionAuthorizationHandler().HandleAsync(context);
        return context.HasSucceeded;
    }

    [Fact]
    public async Task Admin_PassesWithoutAnyPermissionClaim()
    {
        Assert.True(await Authorize(Principal(Roles.Admin), PermissionAction.Delete, PageKeys.Costs));
    }

    [Fact]
    public async Task User_WithMatchingClaim_Passes()
    {
        var user = Principal(Roles.User, "Clients:View", "Clients:AddEdit");

        Assert.True(await Authorize(user, PermissionAction.AddEdit, PageKeys.Clients));
    }

    [Fact]
    public async Task User_WithoutClaim_Fails()
    {
        var user = Principal(Roles.User, "Clients:View");

        Assert.False(await Authorize(user, PermissionAction.Delete, PageKeys.Clients));
        Assert.False(await Authorize(user, PermissionAction.View, PageKeys.Costs));
    }

    [Fact]
    public async Task AnyOfSeveralPages_IsEnough()
    {
        var user = Principal(Roles.User, "Rents:View");

        Assert.True(await Authorize(user, PermissionAction.View, PageKeys.Clients, PageKeys.Rents));
    }

    [Fact]
    public async Task NoRole_NoClaims_Fails()
    {
        Assert.False(await Authorize(Principal(role: null), PermissionAction.View, PageKeys.Clients));
    }

    [Fact]
    public void Attribute_PolicyName_RoundTripsThroughParser()
    {
        var attribute = new RequirePermissionAttribute(PermissionAction.View, PageKeys.Clients, PageKeys.Rents);

        var requirement = PermissionRequirement.TryParse(attribute.Policy!);

        Assert.NotNull(requirement);
        Assert.Equal(PermissionAction.View, requirement!.Action);
        Assert.Equal(new[] { PageKeys.Clients, PageKeys.Rents }, requirement.Pages);
    }

    [Theory]
    [InlineData("AdminOnly")]
    [InlineData("Permission:")]
    [InlineData("Permission:Fly:Clients")]
    [InlineData("Permission:View:")]
    public void Parser_RejectsOtherPolicyNames(string policyName)
    {
        Assert.Null(PermissionRequirement.TryParse(policyName));
    }

    [Fact]
    public async Task PolicyProvider_BuildsPermissionPolicies_AndKeepsNamedOnes()
    {
        var options = new AuthorizationOptions();
        options.AddPolicy("AdminOnly", p => p.RequireRole(Roles.Admin));
        var provider = new PermissionPolicyProvider(Options.Create(options));

        var permissionPolicy = await provider.GetPolicyAsync("Permission:Delete:Clients");
        Assert.NotNull(permissionPolicy);
        Assert.Contains(permissionPolicy!.Requirements, r => r is PermissionRequirement);

        var adminPolicy = await provider.GetPolicyAsync("AdminOnly");
        Assert.NotNull(adminPolicy);
        Assert.DoesNotContain(adminPolicy!.Requirements, r => r is PermissionRequirement);
    }
}
