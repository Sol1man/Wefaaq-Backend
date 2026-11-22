using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Wefaaq.Api.Auth;
using Wefaaq.Api.Controllers;
using Wefaaq.Bll.Constants;

namespace Wefaaq.Tests.Permissions;

/// <summary>
/// Guards against an endpoint shipping without access control. Every action must be
/// anonymous, AdminOnly, or protected by a page permission — or be listed below on purpose.
/// </summary>
public class EndpointAuthorizationCoverageTests
{
    /// <summary>
    /// Endpoints any signed-in user may call without a page permission.
    /// </summary>
    private static readonly HashSet<string> AuthenticatedOnly = new()
    {
        "AuthController.GetCurrentUser", // the frontend needs its own profile + permissions
    };

    /// <summary>
    /// Writes that act on the caller's OWN account and therefore only need page access (View).
    /// </summary>
    private static readonly HashSet<string> SelfServiceWrites = new()
    {
        "UserPaymentController.Create",
        "UserPaymentController.CreateOperation",
    };

    private static IEnumerable<(Type Controller, MethodInfo Action)> Endpoints() =>
        typeof(AuthController).Assembly.GetTypes()
            .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract)
            .SelectMany(t => t
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(m => m.GetCustomAttributes<HttpMethodAttribute>().Any())
                .Select(m => (t, m)));

    private static string Name((Type Controller, MethodInfo Action) e) => $"{e.Controller.Name}.{e.Action.Name}";

    private static List<object> Attributes((Type Controller, MethodInfo Action) e) =>
        e.Action.GetCustomAttributes(true).Concat(e.Controller.GetCustomAttributes(true)).ToList();

    [Fact]
    public void Endpoints_AreDiscovered()
    {
        // Sanity check so the coverage test below can't pass vacuously.
        Assert.True(Endpoints().Count() > 50);
    }

    [Fact]
    public void EveryEndpoint_IsAnonymous_AdminOnly_OrPermissionProtected()
    {
        var unprotected = Endpoints()
            .Where(e =>
            {
                var attrs = Attributes(e);
                return !attrs.OfType<AllowAnonymousAttribute>().Any()
                    && !attrs.OfType<RequirePermissionAttribute>().Any()
                    && !attrs.OfType<AuthorizeAttribute>().Any(a => a.Policy == "AdminOnly")
                    && !AuthenticatedOnly.Contains(Name(e));
            })
            .Select(Name)
            .ToList();

        Assert.True(unprotected.Count == 0,
            "Endpoints with no AdminOnly policy and no [RequirePermission]: " + string.Join(", ", unprotected));
    }

    [Fact]
    public void RequirePermission_OnlyNamesCatalogPages()
    {
        var unknown = Endpoints()
            .SelectMany(e => Attributes(e).OfType<RequirePermissionAttribute>()
                .SelectMany(a => a.Pages)
                .Where(page => PermissionCatalog.Find(page) == null)
                .Select(page => $"{Name(e)} -> {page}"))
            .ToList();

        Assert.True(unknown.Count == 0, "Unknown page keys: " + string.Join(", ", unknown));
    }

    [Fact]
    public void DeleteEndpoints_RequireDeletePermission()
    {
        var wrong = Endpoints()
            .Where(e => e.Action.GetCustomAttributes<HttpDeleteAttribute>().Any())
            .SelectMany(e => Attributes(e).OfType<RequirePermissionAttribute>()
                .Where(a => a.Action != PermissionAction.Delete)
                .Select(_ => Name(e)))
            .ToList();

        Assert.True(wrong.Count == 0, "DELETE endpoints not requiring Delete: " + string.Join(", ", wrong));
    }

    [Fact]
    public void WriteEndpoints_RequireAddEditPermission()
    {
        var wrong = Endpoints()
            .Where(e => e.Action.GetCustomAttributes<HttpPostAttribute>().Any()
                     || e.Action.GetCustomAttributes<HttpPutAttribute>().Any())
            .Where(e => !SelfServiceWrites.Contains(Name(e)))
            .SelectMany(e => Attributes(e).OfType<RequirePermissionAttribute>()
                .Where(a => a.Action != PermissionAction.AddEdit)
                .Select(_ => Name(e)))
            .ToList();

        Assert.True(wrong.Count == 0, "POST/PUT endpoints not requiring AddEdit: " + string.Join(", ", wrong));
    }

    [Fact]
    public void PermissionsScreen_IsAdminOnly()
    {
        var attrs = typeof(UserPermissionsController).GetCustomAttributes<AuthorizeAttribute>(true);
        Assert.Contains(attrs, a => a.Policy == "AdminOnly");
    }
}
