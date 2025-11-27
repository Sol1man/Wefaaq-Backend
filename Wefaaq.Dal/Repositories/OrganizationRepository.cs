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
        return await DbSet
            .Include(o => o.Records)
            .Include(o => o.Licenses)
            .Include(o => o.Workers)
            .Include(o => o.Cars)
            .Include(o => o.Client)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<IEnumerable<Organization>> GetWithExpiringCardsAsync()
    {
        return await DbSet
            .Where(o => o.CardExpiringSoon)
            .Include(o => o.Client)
            .ToListAsync();
    }

    public async Task<IEnumerable<Organization>> GetByClientIdAsync(Guid clientId)
    {
        return await DbSet
            .Where(o => o.ClientId == clientId)
            .Include(o => o.Records)
            .Include(o => o.Licenses)
            .Include(o => o.Workers)
            .Include(o => o.Cars)
            .ToListAsync();
    }

    public override async Task<IEnumerable<Organization>> GetAllAsync()
    {
        return await DbSet
            .Include(o => o.Records)
            .Include(o => o.Licenses)
            .Include(o => o.Workers)
            .Include(o => o.Cars)
            .Include(o => o.Client)
            .ToListAsync();
    }

    public override async Task<Organization?> GetByIdAsync(Guid id)
    {
        return await DbSet
            .Include(o => o.Records)
            .Include(o => o.Licenses)
            .Include(o => o.Workers)
            .Include(o => o.Cars)
            .Include(o => o.Client)
            .FirstOrDefaultAsync(o => o.Id == id);
    }
}