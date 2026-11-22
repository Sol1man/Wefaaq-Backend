using Wefaaq.Bll.Constants;
using Wefaaq.Bll.DTOs;
using Wefaaq.Bll.Services;
using Wefaaq.Dal.Entities;

namespace Wefaaq.Tests.Permissions;

public class PermissionResolverTests
{
    private static PagePermissionDto For(List<PagePermissionDto> perms, string page) =>
        perms.Single(p => p.PageKey == page);

    [Fact]
    public void UserWithNoRows_GetsExactlyTheLegacyUserRoleAccess()
    {
        var perms = PermissionResolver.Resolve(rows: null, isAdmin: false);

        // (view, addEdit, delete) — what the "User" role could do before per-user permissions.
        var expected = new Dictionary<string, (bool, bool, bool)>
        {
            [PageKeys.Clients] = (true, true, false),
            [PageKeys.Organizations] = (true, true, false),
            [PageKeys.ClientOperations] = (true, true, true),
            [PageKeys.Payments] = (true, false, false),
            [PageKeys.Costs] = (false, false, false),
            [PageKeys.EmployeeSalaries] = (true, false, false),
            [PageKeys.Rents] = (true, true, true),
        };

        Assert.Equal(expected.Count, perms.Count);
        foreach (var (page, (view, addEdit, delete)) in expected)
        {
            var p = For(perms, page);
            Assert.Equal((view, addEdit, delete), (p.CanView, p.CanAddEdit, p.CanDelete));
        }
    }

    [Fact]
    public void Admin_GetsEverythingRegardlessOfRows()
    {
        var rows = new[] { new UserPagePermission { PageKey = PageKeys.Clients } }; // all false

        var perms = PermissionResolver.Resolve(rows, isAdmin: true);

        Assert.Equal(PermissionCatalog.Pages.Count, perms.Count);
        Assert.All(perms, p => Assert.True(p.CanView && p.CanAddEdit && p.CanDelete));
    }

    [Fact]
    public void SavedRow_OverridesDefault_OnlyForItsPage()
    {
        var rows = new[]
        {
            new UserPagePermission { PageKey = PageKeys.Costs, CanView = true, CanAddEdit = true, CanDelete = true },
            new UserPagePermission { PageKey = PageKeys.Rents, CanView = false },
        };

        var perms = PermissionResolver.Resolve(rows, isAdmin: false);

        var costs = For(perms, PageKeys.Costs);
        Assert.True(costs.CanView && costs.CanAddEdit && costs.CanDelete);

        var rents = For(perms, PageKeys.Rents);
        Assert.False(rents.CanView || rents.CanAddEdit || rents.CanDelete);

        // Untouched page keeps its default
        var clients = For(perms, PageKeys.Clients);
        Assert.Equal((true, true, false), (clients.CanView, clients.CanAddEdit, clients.CanDelete));
    }

    [Fact]
    public void RowForUnknownPage_IsIgnored()
    {
        var rows = new[] { new UserPagePermission { PageKey = "Retired", CanView = true } };

        var perms = PermissionResolver.Resolve(rows, isAdmin: false);

        Assert.DoesNotContain(perms, p => p.PageKey == "Retired");
    }

    [Fact]
    public void Normalize_WithoutView_DropsAddEditAndDelete()
    {
        var page = PermissionCatalog.Find(PageKeys.Clients)!;

        var p = PermissionResolver.Normalize(page, canView: false, canAddEdit: true, canDelete: true);

        Assert.False(p.CanView || p.CanAddEdit || p.CanDelete);
    }

    [Theory]
    [InlineData(PageKeys.Payments)]
    [InlineData(PageKeys.EmployeeSalaries)]
    public void Normalize_UnsupportedActions_StayOff(string pageKey)
    {
        var page = PermissionCatalog.Find(pageKey)!;

        var p = PermissionResolver.Normalize(page, canView: true, canAddEdit: true, canDelete: true);

        Assert.True(p.CanView);
        Assert.False(p.CanAddEdit);
        Assert.False(p.CanDelete);
    }

    [Fact]
    public void ToClaimValues_EmitsOneClaimPerGrantedAction()
    {
        var perms = new[]
        {
            new PagePermissionDto { PageKey = "Clients", CanView = true, CanAddEdit = true },
            new PagePermissionDto { PageKey = "Costs" },
        };

        var claims = PermissionResolver.ToClaimValues(perms).ToList();

        Assert.Equal(new[] { "Clients:View", "Clients:AddEdit" }, claims);
    }

    [Fact]
    public void ValidateUpdate_AcceptsKnownDistinctPages()
    {
        var dto = new UpdateUserPermissionsDto
        {
            Permissions = PermissionCatalog.Pages.Select(p => new PagePermissionDto { PageKey = p.Key }).ToList()
        };

        PermissionResolver.ValidateUpdate(dto); // does not throw
    }

    [Fact]
    public void ValidateUpdate_RejectsUnknownPage()
    {
        var dto = new UpdateUserPermissionsDto
        {
            Permissions = { new PagePermissionDto { PageKey = "clients" } } // keys are case-sensitive
        };

        Assert.Throws<ArgumentException>(() => PermissionResolver.ValidateUpdate(dto));
    }

    [Fact]
    public void ValidateUpdate_RejectsDuplicatePage()
    {
        var dto = new UpdateUserPermissionsDto
        {
            Permissions =
            {
                new PagePermissionDto { PageKey = PageKeys.Rents },
                new PagePermissionDto { PageKey = PageKeys.Rents },
            }
        };

        Assert.Throws<ArgumentException>(() => PermissionResolver.ValidateUpdate(dto));
    }

    [Fact]
    public void ValidateUpdate_RejectsEmptyRequest()
    {
        Assert.Throws<ArgumentException>(() => PermissionResolver.ValidateUpdate(new UpdateUserPermissionsDto()));
        Assert.Throws<ArgumentException>(() => PermissionResolver.ValidateUpdate(null));
    }

    [Fact]
    public void Catalog_KeysAreUnique()
    {
        var keys = PermissionCatalog.Pages.Select(p => p.Key).ToList();
        Assert.Equal(keys.Count, keys.Distinct().Count());
    }
}
