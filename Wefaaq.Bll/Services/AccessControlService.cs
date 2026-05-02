using Microsoft.EntityFrameworkCore;
using Wefaaq.Bll.Interfaces;
using Wefaaq.Dal;
using Wefaaq.Dal.Entities;

namespace Wefaaq.Bll.Services;

/// <inheritdoc />
public class AccessControlService : IAccessControlService
{
    private readonly ICurrentUserService _currentUser;
    private readonly WefaaqContext _context;

    public AccessControlService(ICurrentUserService currentUser, WefaaqContext context)
    {
        _currentUser = currentUser;
        _context = context;
    }

    public bool IsAdmin => _currentUser.IsAdmin;
    public int? CurrentUserId => _currentUser.UserId;

    public async Task<bool> CanAccessClientAsync(Guid clientId)
    {
        if (IsAdmin) return true;
        if (CurrentUserId is not int uid) return false;

        return await _context.Clients
            .AnyAsync(c => c.Id == clientId && c.AssignedUserId == uid);
    }

    public async Task<bool> CanAccessBranchAsync(Guid branchId)
    {
        if (IsAdmin) return true;
        if (CurrentUserId is not int uid) return false;

        return await _context.ClientBranches
            .AnyAsync(b => b.Id == branchId && b.ParentClient.AssignedUserId == uid);
    }

    public async Task<bool> CanAccessOrganizationAsync(Guid organizationId)
    {
        if (IsAdmin) return true;
        if (CurrentUserId is not int uid) return false;

        return await _context.Organizations
            .AnyAsync(o => o.Id == organizationId &&
                ((o.ClientId != null && o.Client!.AssignedUserId == uid) ||
                 (o.ClientBranchId != null && o.ClientBranch!.ParentClient.AssignedUserId == uid)));
    }

    public IQueryable<Client> FilterClients(IQueryable<Client> query)
    {
        if (IsAdmin) return query;
        if (CurrentUserId is not int uid) return query.Where(_ => false);
        return query.Where(c => c.AssignedUserId == uid);
    }

    public IQueryable<ClientBranch> FilterBranches(IQueryable<ClientBranch> query)
    {
        if (IsAdmin) return query;
        if (CurrentUserId is not int uid) return query.Where(_ => false);
        return query.Where(b => b.ParentClient.AssignedUserId == uid);
    }

    public IQueryable<Organization> FilterOrganizations(IQueryable<Organization> query)
    {
        if (IsAdmin) return query;
        if (CurrentUserId is not int uid) return query.Where(_ => false);
        return query.Where(o =>
            (o.ClientId != null && o.Client!.AssignedUserId == uid) ||
            (o.ClientBranchId != null && o.ClientBranch!.ParentClient.AssignedUserId == uid));
    }

    public IQueryable<ExternalWorker> FilterExternalWorkers(IQueryable<ExternalWorker> query)
    {
        if (IsAdmin) return query;
        if (CurrentUserId is not int uid) return query.Where(_ => false);
        return query.Where(w =>
            (w.ClientId != null && w.Client!.AssignedUserId == uid) ||
            (w.ClientBranchId != null && w.ClientBranch!.ParentClient.AssignedUserId == uid));
    }

    public IQueryable<ClientOperation> FilterOperations(IQueryable<ClientOperation> query)
    {
        if (IsAdmin) return query;
        if (CurrentUserId is not int uid) return query.Where(_ => false);
        return query.Where(o =>
            (o.ClientId != null && o.Client!.AssignedUserId == uid) ||
            (o.ClientBranchId != null && o.ClientBranch!.ParentClient.AssignedUserId == uid) ||
            (o.OrganizationId != null &&
                ((o.Organization!.ClientId != null && o.Organization.Client!.AssignedUserId == uid) ||
                 (o.Organization.ClientBranchId != null && o.Organization.ClientBranch!.ParentClient.AssignedUserId == uid))));
    }
}
