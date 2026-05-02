using Wefaaq.Dal.Entities;

namespace Wefaaq.Bll.Interfaces;

/// <summary>
/// Centralizes "can the current user see/touch this entity?" decisions
/// that flow from <c>Client.AssignedUserId</c>.
///
/// Admins always pass. Normal users only see clients assigned to them, and
/// transitively the branches/organizations/workers/operations underneath.
/// </summary>
public interface IAccessControlService
{
    bool IsAdmin { get; }
    int? CurrentUserId { get; }

    Task<bool> CanAccessClientAsync(Guid clientId);
    Task<bool> CanAccessBranchAsync(Guid branchId);
    Task<bool> CanAccessOrganizationAsync(Guid organizationId);

    /// <summary>Returns the input query unchanged for admins; otherwise restricts to assigned clients.</summary>
    IQueryable<Client> FilterClients(IQueryable<Client> query);
    IQueryable<ClientBranch> FilterBranches(IQueryable<ClientBranch> query);
    IQueryable<Organization> FilterOrganizations(IQueryable<Organization> query);
    IQueryable<ExternalWorker> FilterExternalWorkers(IQueryable<ExternalWorker> query);
    IQueryable<ClientOperation> FilterOperations(IQueryable<ClientOperation> query);
}
