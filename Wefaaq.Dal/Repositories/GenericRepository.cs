using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Wefaaq.Dal.Interfaces;
using Wefaaq.Dal.RepositoriesInterfaces;

namespace Wefaaq.Dal.Repositories;

/// <summary>
/// Generic repository implementation for common CRUD operations
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly WefaaqContext Context;
    protected readonly DbSet<T> DbSet;

    public GenericRepository(WefaaqContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await DbSet.ToListAsync();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        return await DbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression)
    {
        return await DbSet.Where(expression).ToListAsync();
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        await DbSet.AddAsync(entity);
        await Context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        DbSet.Update(entity);
        await Context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null)
            return false;

        return await DeleteAsync(entity);
    }

    public virtual async Task<bool> DeleteAsync(T entity)
    {
        // Check if entity supports soft delete
        if (entity is ISoftDeletable softDeletable)
        {
            // Soft delete: mark as deleted
            softDeletable.IsDeleted = true;
            softDeletable.DeletedAt = DateTime.UtcNow;
            DbSet.Update(entity);
        }
        else
        {
            // Hard delete: physically remove from database
            DbSet.Remove(entity);
        }

        await Context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Permanently delete an entity (hard delete), bypassing soft delete
    /// Use with caution - this cannot be undone!
    /// </summary>
    public virtual async Task<bool> HardDeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null)
            return false;

        return await HardDeleteAsync(entity);
    }

    /// <summary>
    /// Permanently delete an entity (hard delete), bypassing soft delete
    /// Use with caution - this cannot be undone!
    /// </summary>
    public virtual async Task<bool> HardDeleteAsync(T entity)
    {
        DbSet.Remove(entity);
        await Context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Restore a soft-deleted entity
    /// </summary>
    public virtual async Task<bool> RestoreAsync(Guid id)
    {
        // Need to query with IgnoreQueryFilters to find soft-deleted entities
        var entity = await DbSet.IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id);

        if (entity == null)
            return false;

        if (entity is ISoftDeletable softDeletable && softDeletable.IsDeleted)
        {
            softDeletable.IsDeleted = false;
            softDeletable.DeletedAt = null;
            DbSet.Update(entity);
            await Context.SaveChangesAsync();
            return true;
        }

        return false;
    }
}