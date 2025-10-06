using Microsoft.EntityFrameworkCore;
using Wefaaq.Dal.Entities;
using Wefaaq.Dal.RepositoriesInterfaces;

namespace Wefaaq.Dal.Repositories;

/// <summary>
/// Organization repository implementation with specific organization operations
/// </summary>
public class OrganizationRepository : GenericRepository<Organization>, IOrganizationRepository
{
    public OrganizationRepository(WefaaqContext context) : base(context)
    {
    }

    public async Task<Organization?> GetWithDetailsAsync(Guid id)
    {
        return await _dbSet
            .Include(o => o.Records)
            .Include(o => o.Licenses)
            .Include(o => o.Workers)
            .Include(o => o.Cars)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<IEnumerable<Organization>> GetWithExpiringCardsAsync()
    {
        return await _dbSet
            .Where(o => o.CardExpiringSoon)
            .ToListAsync();
    }

    public async Task<Organization?> GetWithClientsAsync(Guid id)
    {
        return await _dbSet
            .Include(o => o.Clients)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<IEnumerable<Organization>> GetByClientIdAsync(Guid clientId)
    {
        return await _dbSet
            .Where(o => o.Clients.Any(c => c.Id == clientId))
            .ToListAsync();
    }

    public override async Task<IEnumerable<Organization>> GetAllAsync()
    {
        return await _dbSet
            .Include(o => o.Records)
            .Include(o => o.Licenses)
            .Include(o => o.Workers)
            .Include(o => o.Cars)
            .Include(o => o.Clients)
            .ToListAsync();
    }

    public override async Task<Organization?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(o => o.Records)
            .Include(o => o.Licenses)
            .Include(o => o.Workers)
            .Include(o => o.Cars)
            .Include(o => o.Clients)
            .FirstOrDefaultAsync(o => o.Id == id);
    }
}