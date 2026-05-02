using Microsoft.EntityFrameworkCore;
using Wefaaq.Dal.Entities;
using Wefaaq.Dal.RepositoriesInterfaces;

namespace Wefaaq.Dal.Repositories;

/// <summary>
/// Client repository implementation with specific client operations
/// </summary>
public class ClientRepository : GenericRepository<Client>, IClientRepository
{
    public ClientRepository(WefaaqContext context) : base(context)
    {
    }

    public async Task<Client?> GetWithOrganizationsAsync(Guid id)
    {
        return await FullDetailsQuery()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Client?> GetWithOrganizationsReadOnlyAsync(Guid id)
    {
        return await FullDetailsQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    private IQueryable<Client> FullDetailsQuery()
    {
        return DbSet
            .AsSplitQuery()
            .Include(c => c.AssignedUser)
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
            .Include(c => c.ClientBranches)
                .ThenInclude(cb => cb.Organizations)
                    .ThenInclude(o => o.Records)
            .Include(c => c.ClientBranches)
                .ThenInclude(cb => cb.Organizations)
                    .ThenInclude(o => o.Licenses)
            .Include(c => c.ClientBranches)
                .ThenInclude(cb => cb.Organizations)
                    .ThenInclude(o => o.Workers)
            .Include(c => c.ClientBranches)
                .ThenInclude(cb => cb.Organizations)
                    .ThenInclude(o => o.Cars)
            .Include(c => c.ClientBranches)
                .ThenInclude(cb => cb.Organizations)
                    .ThenInclude(o => o.Usernames)
            .Include(c => c.ClientBranches)
                .ThenInclude(cb => cb.ExternalWorkers)
            .Include(c => c.ExternalWorkers);
    }

    public async Task<IEnumerable<Client>> GetByClassificationAsync(ClientClassification classification)
    {
        return await DbSet
            .AsNoTracking()
            .Where(c => c.Classification == classification)
            .ToListAsync();
    }

    public async Task<IEnumerable<Client>> GetCreditorsAsync()
    {
        return await DbSet
            .AsNoTracking()
            .Where(c => c.Balance > 0)
            .ToListAsync();
    }

    public async Task<IEnumerable<Client>> GetDebtorsAsync()
    {
        return await DbSet
            .AsNoTracking()
            .Where(c => c.Balance < 0)
            .ToListAsync();
    }

    public async Task<bool> EmailExistsAsync(string email, Guid? excludeClientId = null)
    {
        var query = DbSet.Where(c => c.Email.ToLower() == email.ToLower());

        if (excludeClientId.HasValue)
        {
            query = query.Where(c => c.Id != excludeClientId.Value);
        }

        return await query.AnyAsync();
    }

    public override async Task<IEnumerable<Client>> GetAllAsync()
    {
        // Do NOT include organizations - use GetWithOrganizationsAsync for that
        return await DbSet.AsNoTracking().ToListAsync();
    }

    public override async Task<Client?> GetByIdAsync(Guid id)
    {
        // Do NOT include organizations - use GetWithOrganizationsAsync for that
        return await DbSet.FirstOrDefaultAsync(c => c.Id == id);
    }
}