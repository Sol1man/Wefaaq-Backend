using System.Linq.Expressions;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wefaaq.Api.Auth;
using Wefaaq.Api.Controllers;
using Wefaaq.Api.Extensions;
using Wefaaq.Bll.Constants;
using Wefaaq.Dal.Entities;
using Wefaaq.Dal.RepositoriesInterfaces;

namespace Wefaaq.Tests.Permissions;

/// <summary>
/// End-to-end check of the real authorization pipeline: the real controllers with their real
/// attributes, the real claims transformation and the real policies, over HTTP.
///
/// Only sign-in (a header instead of a Firebase token) and the user store are faked. Controller
/// dependencies are deliberately NOT registered: authorization runs before the controller is
/// created, so 401/403 means "blocked" and any other status means "let through".
/// </summary>
public sealed class PermissionPipelineTests : IAsyncLifetime
{
    private const string UidHeader = "X-Test-Uid";
    private static readonly string AnyId = Guid.NewGuid().ToString();

    private WebApplication _app = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing" });
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();

        builder.Services.AddControllers().AddApplicationPart(typeof(AuthController).Assembly);
        builder.Services
            .AddAuthentication(TestAuthHandler.Scheme)
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.Scheme, _ => { });
        builder.Services.AddSingleton<IUserRepository>(new FakeUserRepository());
        builder.Services.AddScoped<IClaimsTransformation, RoleClaimsTransformation>();
        builder.Services.AddAuthorizationPolicies();

        _app = builder.Build();
        _app.UseAuthentication();
        _app.UseAuthorization();
        _app.MapControllers();
        await _app.StartAsync();

        _client = new HttpClient { BaseAddress = new Uri(_app.Urls.First()) };
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _app.DisposeAsync();
    }

    public static IEnumerable<object[]> Cases()
    {
        // (user, method, path, allowed)
        var y = "?year=2026&month=11";

        // Admin: everything, including admin-only screens
        foreach (var (m, p) in new[]
        {
            ("DELETE", $"/api/clients/delete/{AnyId}"), ("DELETE", $"/api/costs/delete/{AnyId}"),
            ("GET", "/api/user-permissions/overview"), ("GET", "/api/user-payments/get-all"),
            ("GET", $"/api/employee-salaries/by-month{y}"), ("PUT", "/api/user-permissions/users/5"),
        })
            yield return new object[] { "admin", m, p, true };

        // A user nobody customized keeps exactly the legacy "User" role access
        yield return new object[] { "default", "GET", "/api/clients/get-all", true };
        yield return new object[] { "default", "POST", "/api/clients/add", true };
        yield return new object[] { "default", "DELETE", $"/api/clients/delete/{AnyId}", false };
        yield return new object[] { "default", "DELETE", $"/api/organizations/delete/{AnyId}", false };
        yield return new object[] { "default", "GET", "/api/costs/get-all", false };
        yield return new object[] { "default", "DELETE", $"/api/rents/delete/{AnyId}", true };
        yield return new object[] { "default", "DELETE", $"/api/client-operations/delete/{AnyId}", true };
        yield return new object[] { "default", "GET", "/api/client-operations/get-all", true };
        yield return new object[] { "default", "GET", "/api/user-payments/my-payments", true };
        yield return new object[] { "default", "GET", "/api/user-payments/get-all", false };
        yield return new object[] { "default", "GET", $"/api/employee-salaries/my-details{y}", true };
        yield return new object[] { "default", "GET", $"/api/employee-salaries/by-month{y}", false };
        yield return new object[] { "default", "GET", "/api/user-permissions/overview", false };
        yield return new object[] { "default", "GET", "/api/auth/me", true };

        // Customized: Clients view-only, Costs fully granted, Rents revoked
        yield return new object[] { "custom", "GET", "/api/clients/get-all", true };
        yield return new object[] { "custom", "POST", "/api/clients/add", false };
        yield return new object[] { "custom", "PUT", $"/api/clients/edit-with-details/{AnyId}", false };
        yield return new object[] { "custom", "GET", "/api/costs/get-all", true };
        yield return new object[] { "custom", "POST", "/api/costs/add", true };
        yield return new object[] { "custom", "DELETE", $"/api/costs/delete/{AnyId}", true };
        yield return new object[] { "custom", "GET", $"/api/rents/by-month{y}", false };
        yield return new object[] { "custom", "DELETE", $"/api/rents/delete/{AnyId}", false };

        // Rents only: the client/branch/organization lookups its forms need still work,
        // but the Clients and Organizations screens' own endpoints don't
        yield return new object[] { "rents-only", "GET", $"/api/rents/by-month{y}", true };
        yield return new object[] { "rents-only", "GET", "/api/clients/get-all", true };
        yield return new object[] { "rents-only", "GET", "/api/client-branches/get-all", true };
        yield return new object[] { "rents-only", "GET", "/api/organizations/get-all", true };
        yield return new object[] { "rents-only", "GET", $"/api/clients/get/{AnyId}", false };
        yield return new object[] { "rents-only", "GET", $"/api/organizations/details/{AnyId}", false };
        yield return new object[] { "rents-only", "GET", "/api/client-operations/get-all", false };
        yield return new object[] { "rents-only", "GET", "/api/user-payments/my-payments", false };

        // Deactivated account: still signed in, but every protected endpoint is closed
        yield return new object[] { "inactive", "GET", "/api/clients/get-all", false };
        yield return new object[] { "inactive", "GET", "/api/user-payments/my-payments", false };
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task Endpoint_IsAllowedOnlyWithPermission(string uid, string method, string path, bool allowed)
    {
        using var request = new HttpRequestMessage(new HttpMethod(method), path);
        request.Headers.Add(UidHeader, uid);
        if (method is "POST" or "PUT")
        {
            request.Content = new StringContent("{}", Encoding.UTF8, "application/json");
        }

        using var response = await _client.SendAsync(request);

        if (allowed)
        {
            // Let through → the controller can't be built (its services aren't registered) → 500.
            // Asserting 500 exactly (not just "not 403") also catches a mistyped route (404).
            Assert.True(
                response.StatusCode == HttpStatusCode.InternalServerError,
                $"{uid} {method} {path} expected to pass authorization, got {(int)response.StatusCode}");
        }
        else
        {
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }

    [Fact]
    public async Task Anonymous_IsUnauthorized()
    {
        using var response = await _client.GetAsync("/api/clients/get-all");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Signs the request in as the Firebase uid in the X-Test-Uid header (no header = anonymous).
    /// </summary>
    private sealed class TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        public const string Scheme = "Test";

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(UidHeader, out var uid) || string.IsNullOrEmpty(uid))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var identity = new ClaimsIdentity(new[] { new Claim("user_id", uid.ToString()) }, Scheme);
            return Task.FromResult(AuthenticateResult.Success(
                new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme)));
        }
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        private static readonly Role AdminRole = new() { Id = Roles.Ids.Admin, Name = Roles.Admin };
        private static readonly Role UserRole = new() { Id = Roles.Ids.User, Name = Roles.User };

        private static UserPagePermission Row(string page, bool view, bool addEdit = false, bool delete = false) =>
            new() { PageKey = page, CanView = view, CanAddEdit = addEdit, CanDelete = delete };

        private readonly Dictionary<string, User> _users = new()
        {
            ["admin"] = new User { Id = 1, Email = "admin@test", Role = AdminRole, IsActive = true },
            ["default"] = new User { Id = 2, Email = "default@test", Role = UserRole, IsActive = true },
            ["custom"] = new User
            {
                Id = 3, Email = "custom@test", Role = UserRole, IsActive = true,
                PagePermissions =
                {
                    Row(PageKeys.Clients, view: true),
                    Row(PageKeys.Costs, view: true, addEdit: true, delete: true),
                    Row(PageKeys.Rents, view: false),
                }
            },
            ["rents-only"] = new User
            {
                Id = 4, Email = "rents@test", Role = UserRole, IsActive = true,
                PagePermissions = PermissionCatalog.Pages
                    .Select(p => Row(p.Key, view: p.Key == PageKeys.Rents))
                    .ToList()
            },
            ["inactive"] = new User { Id = 5, Email = "inactive@test", Role = UserRole, IsActive = false },
        };

        public Task<User?> GetByFirebaseUidAsync(string firebaseUid) =>
            Task.FromResult(_users.GetValueOrDefault(firebaseUid));

        public Task<User?> GetByEmailAsync(string email) => throw new NotImplementedException();
        public Task UpdateLastLoginAsync(int userId) => throw new NotImplementedException();
        public Task<IEnumerable<User>> GetAllAsync() => throw new NotImplementedException();
        public Task<User?> GetByIdAsync(Guid id) => throw new NotImplementedException();
        public Task<IEnumerable<User>> FindAsync(Expression<Func<User, bool>> expression) => throw new NotImplementedException();
        public Task<User> AddAsync(User entity) => throw new NotImplementedException();
        public Task<User> UpdateAsync(User entity) => throw new NotImplementedException();
        public Task<bool> DeleteAsync(Guid id) => throw new NotImplementedException();
        public Task<bool> DeleteAsync(User entity) => throw new NotImplementedException();
        public Task<bool> HardDeleteAsync(Guid id) => throw new NotImplementedException();
        public Task<bool> HardDeleteAsync(User entity) => throw new NotImplementedException();
        public Task<bool> RestoreAsync(Guid id) => throw new NotImplementedException();
    }
}
