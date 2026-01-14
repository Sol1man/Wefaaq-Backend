using System.Linq.Expressions;

namespace Wefaaq.Dal.RepositoriesInterfaces;

/// <summary>
/// Generic repository interface for common CRUD operations
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public interface IGenericRepository<T> where T : class
{
    /// <summary>
    /// Get all entities
    /// </summary>
    /// <returns>All entities</returns>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Get entity by ID
    /// </summary>
    /// <param name="id">Entity ID</param>
    /// <returns>Entity or null</returns>
    Task<T?> GetByIdAsync(Guid id);

    /// <summary>
    /// Find entities matching the expression
    /// </summary>
    /// <param name="expression">Search expression</param>
    /// <returns>Matching entities</returns>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression);

    /// <summary>
    /// Add new entity
    /// </summary>
    /// <param name="entity">Entity to add</param>
    /// <returns>Added entity</returns>
    Task<T> AddAsync(T entity);

    /// <summary>
    /// Update existing entity
    /// </summary>
    /// <param name="entity">Entity to update</param>
    /// <returns>Updated entity</returns>
    Task<T> UpdateAsync(T entity);

    /// <summary>
    /// Delete entity by ID
    /// </summary>
    /// <param name="id">Entity ID</param>
    /// <returns>True if deleted, false if not found</returns>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    /// Delete entity
    /// </summary>
    /// <param name="entity">Entity to delete</param>
    /// <returns>True if deleted</returns>
    Task<bool> DeleteAsync(T entity);

    /// <summary>
    /// Permanently delete entity by ID (hard delete), bypassing soft delete
    /// Use with caution - this cannot be undone!
    /// </summary>
    /// <param name="id">Entity ID</param>
    /// <returns>True if deleted, false if not found</returns>
    Task<bool> HardDeleteAsync(Guid id);

    /// <summary>
    /// Permanently delete entity (hard delete), bypassing soft delete
    /// Use with caution - this cannot be undone!
    /// </summary>
    /// <param name="entity">Entity to delete</param>
    /// <returns>True if deleted</returns>
    Task<bool> HardDeleteAsync(T entity);

    /// <summary>
    /// Restore a soft-deleted entity
    /// </summary>
    /// <param name="id">Entity ID</param>
    /// <returns>True if restored, false if not found or not soft-deleted</returns>
    Task<bool> RestoreAsync(Guid id);
}