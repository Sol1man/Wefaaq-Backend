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
        return await DbSet
            .Include(c => c.Organizations)
                .ThenInclude(o => o.Records)
            .Include(c => c.Organizations)
                .ThenInclude(o => o.Licenses)
            .Include(c => c.Organizations)
                .ThenInclude(o => o.Workers)
            .Include(c => c.Organizations)
                .ThenInclude(o => o.Cars)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Client>> GetByClassificationAsync(ClientClassification classification)
    {
        return await DbSet
            .Where(c => c.Classification == classification)
            .ToListAsync();
    }

    public async Task<IEnumerable<Client>> GetCreditorsAsync()
    {
        return await DbSet
            .Where(c => c.Balance > 0)
            .ToListAsync();
    }

    public async Task<IEnumerable<Client>> GetDebtorsAsync()
    {
        return await DbSet
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
        return await DbSet.ToListAsync();
    }

    public override async Task<Client?> GetByIdAsync(Guid id)
    {
        // Do NOT include organizations - use GetWithOrganizationsAsync for that
        return await DbSet.FirstOrDefaultAsync(c => c.Id == id);
    }
}