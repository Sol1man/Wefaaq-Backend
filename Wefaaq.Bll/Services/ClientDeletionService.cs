using Microsoft.EntityFrameworkCore;
using Wefaaq.Bll.DTOs;
using Wefaaq.Bll.Interfaces;
using Wefaaq.Dal;
using Wefaaq.Dal.Entities;
using Wefaaq.Dal.Interfaces;

namespace Wefaaq.Bll.Services;

/// <summary>
/// Service for safely deleting clients with proper CASCADE behavior for soft deletes
/// </summary>
public class ClientDeletionService : IClientDeletionService
{
    private readonly WefaaqContext _context;

    public ClientDeletionService(WefaaqContext context)
    {
        _context = context;
    }

    public async Task<DeletionResultDto> ValidateDeletionAsync(Guid clientId)
    {
        var client = await _context.Clients
            .IgnoreQueryFilters()
            .Include(c => c.Organizations)
                .ThenInclude(o => o.Records)
            .Include(c => c.Organizations)
                .ThenInclude(o => o.Licenses)
            .Include(c => c.Organizations)
                .ThenInclude(o => o.Workers)
            .Include(c => c.Organizations)
                .ThenInclude(o => o.Cars)
            .Include(c => c.Organizations)
                .ThenInclude(o => o.Usernames)
            .Include(c => c.ExternalWorkers)
            .Include(c => c.ClientBranches)
                .ThenInclude(b => b.Organizations)
                    .ThenInclude(o => o.Records)
            .Include(c => c.ClientBranches)
                .ThenInclude(b => b.Organizations)
                    .ThenInclude(o => o.Licenses)
            .Include(c => c.ClientBranches)
                .ThenInclude(b => b.Organizations)
                    .ThenInclude(o => o.Workers)
            .Include(c => c.ClientBranches)
                .ThenInclude(b => b.Organizations)
                    .ThenInclude(o => o.Cars)
            .Include(c => c.ClientBranches)
                .ThenInclude(b => b.Organizations)
                    .ThenInclude(o => o.Usernames)
            .Include(c => c.ClientBranches)
                .ThenInclude(b => b.ExternalWorkers)
            .FirstOrDefaultAsync(c => c.Id == clientId);

        if (client == null)
        {
            return new DeletionResultDto
            {
                Success = false,
                ErrorMessage = "Client not found"
            };
        }

        var result = new DeletionResultDto
        {
            Success = true,
            DeletionType = "Validation",
            DeletedEntities = new Dictionary<string, int>()
        };

        // Count everything that would be deleted
        int count = 0;

        // Client itself
        count++;
        result.DeletedEntities["Clients"] = 1;

        // Direct organizations
        int orgCount = client.Organizations.Count;
        if (orgCount > 0)
        {
            count += orgCount;
            result.DeletedEntities["Organizations"] = orgCount;
            result.Warnings.Add($"This client has {orgCount} organization(s)");

            // Count organization children
            foreach (var org in client.Organizations)
            {
                count += CountOrganizationChildren(org, result.DeletedEntities);
            }
        }

        // External workers
        int extWorkerCount = client.ExternalWorkers.Count;
        if (extWorkerCount > 0)
        {
            count += extWorkerCount;
            result.DeletedEntities["ExternalWorkers"] = extWorkerCount;
        }

        // Client branches
        int branchCount = client.ClientBranches.Count;
        if (branchCount > 0)
        {
            count += branchCount;
            result.DeletedEntities["ClientBranches"] = branchCount;
            result.Warnings.Add($"This client has {branchCount} branch(es)");

            foreach (var branch in client.ClientBranches)
            {
                // Branch organizations
                int branchOrgCount = branch.Organizations.Count;
                if (branchOrgCount > 0)
                {
                    count += branchOrgCount;
                    result.DeletedEntities["Organizations"] = result.DeletedEntities.GetValueOrDefault("Organizations") + branchOrgCount;

                    foreach (var org in branch.Organizations)
                    {
                        count += CountOrganizationChildren(org, result.DeletedEntities);
                    }
                }

                // Branch external workers
                int branchExtWorkerCount = branch.ExternalWorkers.Count;
                if (branchExtWorkerCount > 0)
                {
                    count += branchExtWorkerCount;
                    result.DeletedEntities["ExternalWorkers"] = result.DeletedEntities.GetValueOrDefault("ExternalWorkers") + branchExtWorkerCount;
                }
            }
        }

        result.TotalEntitiesAffected = count;

        return result;
    }

    public async Task<DeletionResultDto> SoftDeleteClientAsync(Guid clientId)
    {
        // First validate
        var validation = await ValidateDeletionAsync(clientId);
        if (!validation.Success)
        {
            return validation;
        }

        // Load client with all dependencies
        var client = await LoadClientWithAllDependencies(clientId);
        if (client == null)
        {
            return new DeletionResultDto
            {
                Success = false,
                ErrorMessage = "Client not found"
            };
        }

        try
        {
            var result = new DeletionResultDto
            {
                Success = true,
                DeletionType = "Soft Delete",
                DeletedEntities = new Dictionary<string, int>()
            };

            // CASCADE SOFT DELETE: Mark everything as deleted
            await SoftDeleteClientAndChildren(client, result);

            await _context.SaveChangesAsync();

            return result;
        }
        catch (Exception ex)
        {
            return new DeletionResultDto
            {
                Success = false,
                ErrorMessage = $"Error during soft delete: {ex.Message}"
            };
        }
    }

    public async Task<DeletionResultDto> RestoreClientAsync(Guid clientId)
    {
        // Load client with all dependencies (using IgnoreQueryFilters to get soft-deleted)
        var client = await _context.Clients
            .IgnoreQueryFilters()
            .Include(c => c.Organizations)
                .ThenInclude(o => o.Records)
            .Include(c => c.Organizations)
                .ThenInclude(o => o.Licenses)
            .Include(c => c.Organizations)
                .ThenInclude(o => o.Workers)
            .Include(c => c.Organizations)
                .ThenInclude(o => o.Cars)
            .Include(c => c.Organizations)
                .ThenInclude(o => o.Usernames)
            .Include(c => c.ExternalWorkers)
            .Include(c => c.ClientBranches)
                .ThenInclude(b => b.Organizations)
                    .ThenInclude(o => o.Records)
            .Include(c => c.ClientBranches)
                .ThenInclude(b => b.Organizations)
                    .ThenInclude(o => o.Licenses)
            .Include(c => c.ClientBranches)
                .ThenInclude(b => b.Organizations)
                    .ThenInclude(o => o.Workers)
            .Include(c => c.ClientBranches)
                .ThenInclude(b => b.Organizations)
                    .ThenInclude(o => o.Cars)
            .Include(c => c.ClientBranches)
                .ThenInclude(b => b.Organizations)
                    .ThenInclude(o => o.Usernames)
            .Include(c => c.ClientBranches)
                .ThenInclude(b => b.ExternalWorkers)
            .FirstOrDefaultAsync(c => c.Id == clientId);

        if (client == null)
        {
            return new DeletionResultDto
            {
                Success = false,
                ErrorMessage = "Client not found"
            };
        }

        if (!client.IsDeleted)
        {
            return new DeletionResultDto
            {
                Success = false,
                ErrorMessage = "Client is not deleted"
            };
        }

        try
        {
            var result = new DeletionResultDto
            {
                Success = true,
                DeletionType = "Restore",
                DeletedEntities = new Dictionary<string, int>()
            };

            // CASCADE RESTORE: Restore everything
            await RestoreClientAndChildren(client, result);

            await _context.SaveChangesAsync();

            return result;
        }
        catch (Exception ex)
        {
            return new DeletionResultDto
            {
                Success = false,
                ErrorMessage = $"Error during restore: {ex.Message}"
            };
        }
    }

    public async Task<DeletionResultDto> HardDeleteClientAsync(Guid clientId)
    {
        var client = await LoadClientWithAllDependencies(clientId);
        if (client == null)
        {
            return new DeletionResultDto
            {
                Success = false,
                ErrorMessage = "Client not found"
            };
        }

        try
        {
            var result = new DeletionResultDto
            {
                Success = true,
                DeletionType = "Hard Delete (Permanent)",
                DeletedEntities = new Dictionary<string, int>()
            };

            // Count everything before deletion
            int count = 1; // Client itself

            // Delete branches first (with their dependencies)
            foreach (var branch in client.ClientBranches.ToList())
            {
                count += await HardDeleteBranchAndChildren(branch, result);
            }

            // Delete organizations (cascade handles children automatically)
            int orgCount = client.Organizations.Count;
            foreach (var org in client.Organizations.ToList())
            {
                count += CountOrganizationChildren(org, result.DeletedEntities);
                _context.Organizations.Remove(org);
            }
            count += orgCount;
            result.DeletedEntities["Organizations"] = orgCount;

            // Delete external workers
            int extWorkerCount = client.ExternalWorkers.Count;
            foreach (var worker in client.ExternalWorkers.ToList())
            {
                _context.ExternalWorkers.Remove(worker);
            }
            count += extWorkerCount;
            result.DeletedEntities["ExternalWorkers"] = extWorkerCount;

            // Finally delete client
            _context.Clients.Remove(client);
            result.DeletedEntities["Clients"] = 1;

            result.TotalEntitiesAffected = count;

            await _context.SaveChangesAsync();

            return result;
        }
        catch (Exception ex)
        {
            return new DeletionResultDto
            {
                Success = false,
                ErrorMessage = $"Error during hard delete: {ex.Message}"
            };
        }
    }

    public async Task<DeletionResultDto> SoftDeleteClientBranchAsync(Guid branchId)
    {
        var branch = await _context.ClientBranches
            .Include(b => b.Organizations)
                .ThenInclude(o => o.Records)
            .Include(b => b.Organizations)
                .ThenInclude(o => o.Licenses)
            .Include(b => b.Organizations)
                .ThenInclude(o => o.Workers)
            .Include(b => b.Organizations)
                .ThenInclude(o => o.Cars)
            .Include(b => b.Organizations)
                .ThenInclude(o => o.Usernames)
            .Include(b => b.ExternalWorkers)
            .FirstOrDefaultAsync(b => b.Id == branchId);

        if (branch == null)
        {
            return new DeletionResultDto
            {
                Success = false,
                ErrorMessage = "Client branch not found"
            };
        }

        try
        {
            var result = new DeletionResultDto
            {
                Success = true,
                DeletionType = "Soft Delete",
                DeletedEntities = new Dictionary<string, int>()
            };

            await SoftDeleteBranchAndChildren(branch, result);
            await _context.SaveChangesAsync();

            return result;
        }
        catch (Exception ex)
        {
            return new DeletionResultDto
            {
                Success = false,
                ErrorMessage = $"Error during soft delete: {ex.Message}"
            };
        }
    }

    public async Task<DeletionResultDto> HardDeleteClientBranchAsync(Guid branchId)
    {
        var branch = await _context.ClientBranches
            .IgnoreQueryFilters()
            .Include(b => b.Organizations)
                .ThenInclude(o => o.Records)
            .Include(b => b.Organizations)
                .ThenInclude(o => o.Licenses)
            .Include(b => b.Organizations)
                .ThenInclude(o => o.Workers)
            .Include(b => b.Organizations)
                .ThenInclude(o => o.Cars)
            .Include(b => b.Organizations)
                .ThenInclude(o => o.Usernames)
            .Include(b => b.ExternalWorkers)
            .FirstOrDefaultAsync(b => b.Id == branchId);

        if (branch == null)
        {
            return new DeletionResultDto
            {
                Success = false,
                ErrorMessage = "Client branch not found"
            };
        }

        try
        {
            var result = new DeletionResultDto
            {
                Success = true,
                DeletionType = "Hard Delete (Permanent)",
                DeletedEntities = new Dictionary<string, int>()
            };

            int count = await HardDeleteBranchAndChildren(branch, result);
            result.TotalEntitiesAffected = count;

            await _context.SaveChangesAsync();

            return result;
        }
        catch (Exception ex)
        {
            return new DeletionResultDto
            {
                Success = false,
                ErrorMessage = $"Error during hard delete: {ex.Message}"
            };
        }
    }

    public async Task<DeletionResultDto> RestoreClientBranchAsync(Guid branchId)
    {
        var branch = await _context.ClientBranches
            .IgnoreQueryFilters()
            .Include(b => b.Organizations)
                .ThenInclude(o => o.Records)
            .Include(b => b.Organizations)
                .ThenInclude(o => o.Licenses)
            .Include(b => b.Organizations)
                .ThenInclude(o => o.Workers)
            .Include(b => b.Organizations)
                .ThenInclude(o => o.Cars)
            .Include(b => b.Organizations)
                .ThenInclude(o => o.Usernames)
            .Include(b => b.ExternalWorkers)
            .FirstOrDefaultAsync(b => b.Id == branchId);

        if (branch == null || !branch.IsDeleted)
        {
            return new DeletionResultDto
            {
                Success = false,
                ErrorMessage = branch == null ? "Client branch not found" : "Client branch is not deleted"
            };
        }

        try
        {
            var result = new DeletionResultDto
            {
                Success = true,
                DeletionType = "Restore",
                DeletedEntities = new Dictionary<string, int>()
            };

            await RestoreBranchAndChildren(branch, result);
            await _context.SaveChangesAsync();

            return result;
        }
        catch (Exception ex)
        {
            return new DeletionResultDto
            {
                Success = false,
                ErrorMessage = $"Error during restore: {ex.Message}"
            };
        }
    }

    // Private helper methods

    private async Task<Client?> LoadClientWithAllDependencies(Guid clientId)
    {
        return await _context.Clients
            .IgnoreQueryFilters()
            .Include(c => c.Organizations)
                .ThenInclude(o => o.Records)
            .Include(c => c.Organizations)
                .ThenInclude(o => o.Licenses)
            .Include(c => c.Organizations)
                .ThenInclude(o => o.Workers)
            .Include(c => c.Organizations)
                .ThenInclude(o => o.Cars)
            .Include(c => c.Organizations)
                .ThenInclude(o => o.Usernames)
            .Include(c => c.ExternalWorkers)
            .Include(c => c.ClientBranches)
                .ThenInclude(b => b.Organizations)
                    .ThenInclude(o => o.Records)
            .Include(c => c.ClientBranches)
                .ThenInclude(b => b.Organizations)
                    .ThenInclude(o => o.Licenses)
            .Include(c => c.ClientBranches)
                .ThenInclude(b => b.Organizations)
                    .ThenInclude(o => o.Workers)
            .Include(c => c.ClientBranches)
                .ThenInclude(b => b.Organizations)
                    .ThenInclude(o => o.Cars)
            .Include(c => c.ClientBranches)
                .ThenInclude(b => b.Organizations)
                    .ThenInclude(o => o.Usernames)
            .Include(c => c.ClientBranches)
                .ThenInclude(b => b.ExternalWorkers)
            .FirstOrDefaultAsync(c => c.Id == clientId);
    }

    private int CountOrganizationChildren(Organization org, Dictionary<string, int> entities)
    {
        int count = 0;

        count += org.Records.Count;
        if (org.Records.Any())
            entities["OrganizationRecords"] = entities.GetValueOrDefault("OrganizationRecords") + org.Records.Count;

        count += org.Licenses.Count;
        if (org.Licenses.Any())
            entities["OrganizationLicenses"] = entities.GetValueOrDefault("OrganizationLicenses") + org.Licenses.Count;

        count += org.Workers.Count;
        if (org.Workers.Any())
            entities["OrganizationWorkers"] = entities.GetValueOrDefault("OrganizationWorkers") + org.Workers.Count;

        count += org.Cars.Count;
        if (org.Cars.Any())
            entities["OrganizationCars"] = entities.GetValueOrDefault("OrganizationCars") + org.Cars.Count;

        count += org.Usernames.Count;
        if (org.Usernames.Any())
            entities["OrganizationUsernames"] = entities.GetValueOrDefault("OrganizationUsernames") + org.Usernames.Count;

        return count;
    }

    private async Task SoftDeleteClientAndChildren(Client client, DeletionResultDto result)
    {
        int count = 0;

        // Soft delete client
        client.IsDeleted = true;
        client.DeletedAt = DateTime.UtcNow;
        count++;
        result.DeletedEntities["Clients"] = 1;

        // Soft delete all organizations and their children
        foreach (var org in client.Organizations)
        {
            count += await SoftDeleteOrganizationAndChildren(org, result);
        }

        // Soft delete external workers
        foreach (var worker in client.ExternalWorkers)
        {
            worker.IsDeleted = true;
            worker.DeletedAt = DateTime.UtcNow;
            count++;
        }
        if (client.ExternalWorkers.Any())
            result.DeletedEntities["ExternalWorkers"] = client.ExternalWorkers.Count;

        // Soft delete branches and their children
        foreach (var branch in client.ClientBranches)
        {
            count += await SoftDeleteBranchAndChildren(branch, result);
        }

        result.TotalEntitiesAffected = count;
    }

    private async Task<int> SoftDeleteOrganizationAndChildren(Organization org, DeletionResultDto result)
    {
        int count = 0;

        org.IsDeleted = true;
        org.DeletedAt = DateTime.UtcNow;
        count++;
        result.DeletedEntities["Organizations"] = result.DeletedEntities.GetValueOrDefault("Organizations") + 1;

        foreach (var record in org.Records)
        {
            record.IsDeleted = true;
            record.DeletedAt = DateTime.UtcNow;
            count++;
        }
        if (org.Records.Any())
            result.DeletedEntities["OrganizationRecords"] = result.DeletedEntities.GetValueOrDefault("OrganizationRecords") + org.Records.Count;

        foreach (var license in org.Licenses)
        {
            license.IsDeleted = true;
            license.DeletedAt = DateTime.UtcNow;
            count++;
        }
        if (org.Licenses.Any())
            result.DeletedEntities["OrganizationLicenses"] = result.DeletedEntities.GetValueOrDefault("OrganizationLicenses") + org.Licenses.Count;

        foreach (var worker in org.Workers)
        {
            worker.IsDeleted = true;
            worker.DeletedAt = DateTime.UtcNow;
            count++;
        }
        if (org.Workers.Any())
            result.DeletedEntities["OrganizationWorkers"] = result.DeletedEntities.GetValueOrDefault("OrganizationWorkers") + org.Workers.Count;

        foreach (var car in org.Cars)
        {
            car.IsDeleted = true;
            car.DeletedAt = DateTime.UtcNow;
            count++;
        }
        if (org.Cars.Any())
            result.DeletedEntities["OrganizationCars"] = result.DeletedEntities.GetValueOrDefault("OrganizationCars") + org.Cars.Count;

        foreach (var username in org.Usernames)
        {
            username.IsDeleted = true;
            username.DeletedAt = DateTime.UtcNow;
            count++;
        }
        if (org.Usernames.Any())
            result.DeletedEntities["OrganizationUsernames"] = result.DeletedEntities.GetValueOrDefault("OrganizationUsernames") + org.Usernames.Count;

        return count;
    }

    private async Task<int> SoftDeleteBranchAndChildren(ClientBranch branch, DeletionResultDto result)
    {
        int count = 0;

        branch.IsDeleted = true;
        branch.DeletedAt = DateTime.UtcNow;
        count++;
        result.DeletedEntities["ClientBranches"] = result.DeletedEntities.GetValueOrDefault("ClientBranches") + 1;

        foreach (var org in branch.Organizations)
        {
            count += await SoftDeleteOrganizationAndChildren(org, result);
        }

        foreach (var worker in branch.ExternalWorkers)
        {
            worker.IsDeleted = true;
            worker.DeletedAt = DateTime.UtcNow;
            count++;
        }
        if (branch.ExternalWorkers.Any())
            result.DeletedEntities["ExternalWorkers"] = result.DeletedEntities.GetValueOrDefault("ExternalWorkers") + branch.ExternalWorkers.Count;

        return count;
    }

    private async Task RestoreClientAndChildren(Client client, DeletionResultDto result)
    {
        int count = 0;

        client.IsDeleted = false;
        client.DeletedAt = null;
        count++;
        result.DeletedEntities["Clients"] = 1;

        foreach (var org in client.Organizations)
        {
            count += await RestoreOrganizationAndChildren(org, result);
        }

        foreach (var worker in client.ExternalWorkers)
        {
            worker.IsDeleted = false;
            worker.DeletedAt = null;
            count++;
        }
        if (client.ExternalWorkers.Any())
            result.DeletedEntities["ExternalWorkers"] = client.ExternalWorkers.Count;

        foreach (var branch in client.ClientBranches)
        {
            count += await RestoreBranchAndChildren(branch, result);
        }

        result.TotalEntitiesAffected = count;
    }

    private async Task<int> RestoreOrganizationAndChildren(Organization org, DeletionResultDto result)
    {
        int count = 0;

        org.IsDeleted = false;
        org.DeletedAt = null;
        count++;
        result.DeletedEntities["Organizations"] = result.DeletedEntities.GetValueOrDefault("Organizations") + 1;

        foreach (var record in org.Records)
        {
            record.IsDeleted = false;
            record.DeletedAt = null;
            count++;
        }
        if (org.Records.Any())
            result.DeletedEntities["OrganizationRecords"] = result.DeletedEntities.GetValueOrDefault("OrganizationRecords") + org.Records.Count;

        foreach (var license in org.Licenses)
        {
            license.IsDeleted = false;
            license.DeletedAt = null;
            count++;
        }
        if (org.Licenses.Any())
            result.DeletedEntities["OrganizationLicenses"] = result.DeletedEntities.GetValueOrDefault("OrganizationLicenses") + org.Licenses.Count;

        foreach (var worker in org.Workers)
        {
            worker.IsDeleted = false;
            worker.DeletedAt = null;
            count++;
        }
        if (org.Workers.Any())
            result.DeletedEntities["OrganizationWorkers"] = result.DeletedEntities.GetValueOrDefault("OrganizationWorkers") + org.Workers.Count;

        foreach (var car in org.Cars)
        {
            car.IsDeleted = false;
            car.DeletedAt = null;
            count++;
        }
        if (org.Cars.Any())
            result.DeletedEntities["OrganizationCars"] = result.DeletedEntities.GetValueOrDefault("OrganizationCars") + org.Cars.Count;

        foreach (var username in org.Usernames)
        {
            username.IsDeleted = false;
            username.DeletedAt = null;
            count++;
        }
        if (org.Usernames.Any())
            result.DeletedEntities["OrganizationUsernames"] = result.DeletedEntities.GetValueOrDefault("OrganizationUsernames") + org.Usernames.Count;

        return count;
    }

    private async Task<int> RestoreBranchAndChildren(ClientBranch branch, DeletionResultDto result)
    {
        int count = 0;

        branch.IsDeleted = false;
        branch.DeletedAt = null;
        count++;
        result.DeletedEntities["ClientBranches"] = result.DeletedEntities.GetValueOrDefault("ClientBranches") + 1;

        foreach (var org in branch.Organizations)
        {
            count += await RestoreOrganizationAndChildren(org, result);
        }

        foreach (var worker in branch.ExternalWorkers)
        {
            worker.IsDeleted = false;
            worker.DeletedAt = null;
            count++;
        }
        if (branch.ExternalWorkers.Any())
            result.DeletedEntities["ExternalWorkers"] = result.DeletedEntities.GetValueOrDefault("ExternalWorkers") + branch.ExternalWorkers.Count;

        return count;
    }

    private async Task<int> HardDeleteBranchAndChildren(ClientBranch branch, DeletionResultDto result)
    {
        int count = 1; // Branch itself

        // Delete organizations
        int orgCount = branch.Organizations.Count;
        foreach (var org in branch.Organizations.ToList())
        {
            count += CountOrganizationChildren(org, result.DeletedEntities);
            _context.Organizations.Remove(org);
        }
        count += orgCount;
        result.DeletedEntities["Organizations"] = result.DeletedEntities.GetValueOrDefault("Organizations") + orgCount;

        // Delete external workers
        int extWorkerCount = branch.ExternalWorkers.Count;
        foreach (var worker in branch.ExternalWorkers.ToList())
        {
            _context.ExternalWorkers.Remove(worker);
        }
        count += extWorkerCount;
        result.DeletedEntities["ExternalWorkers"] = result.DeletedEntities.GetValueOrDefault("ExternalWorkers") + extWorkerCount;

        // Delete branch
        _context.ClientBranches.Remove(branch);
        result.DeletedEntities["ClientBranches"] = result.DeletedEntities.GetValueOrDefault("ClientBranches") + 1;

        return count;
    }
}
