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
        return await _dbSet
            .Include(c => c.Organizations)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Client>> GetByClassificationAsync(ClientClassification classification)
    {
        return await _dbSet
            .Where(c => c.Classification == classification)
            .ToListAsync();
    }

    public async Task<IEnumerable<Client>> GetCreditorsAsync()
    {
        return await _dbSet
            .Where(c => c.Balance > 0)
            .ToListAsync();
    }

    public async Task<IEnumerable<Client>> GetDebtorsAsync()
    {
        return await _dbSet
            .Where(c => c.Balance < 0)
            .ToListAsync();
    }

    public async Task<bool> EmailExistsAsync(string email, Guid? excludeClientId = null)
    {
        var query = _dbSet.Where(c => c.Email.ToLower() == email.ToLower());

        if (excludeClientId.HasValue)
        {
            query = query.Where(c => c.Id != excludeClientId.Value);
        }

        return await query.AnyAsync();
    }

    public override async Task<IEnumerable<Client>> GetAllAsync()
    {
        return await _dbSet
            .Include(c => c.Organizations)
            .ToListAsync();
    }

    public override async Task<Client?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(c => c.Organizations)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
}