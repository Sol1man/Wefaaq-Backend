using Wefaaq.Bll.DTOs;

namespace Wefaaq.Bll.Interfaces;

/// <summary>
/// Service for safely deleting clients and their dependencies
/// </summary>
public interface IClientDeletionService
{
    /// <summary>
    /// Soft delete a client and CASCADE to all children (recommended)
    /// Sets IsDeleted=true on client and ALL related entities
    /// Returns detailed report of what was deleted
    /// </summary>
    Task<DeletionResultDto> SoftDeleteClientAsync(Guid clientId);

    /// <summary>
    /// Permanently delete a client and all its dependencies (hard delete)
    /// ⚠️ WARNING: This cannot be undone! Use with extreme caution.
    /// Deletes in proper order to avoid foreign key violations
    /// Returns detailed report of what was deleted
    /// </summary>
    Task<DeletionResultDto> HardDeleteClientAsync(Guid clientId);

    /// <summary>
    /// Restore a soft-deleted client and CASCADE to all children
    /// Restores client and ALL related entities
    /// </summary>
    Task<DeletionResultDto> RestoreClientAsync(Guid clientId);

    /// <summary>
    /// Soft delete a client branch and CASCADE to all children (recommended)
    /// </summary>
    Task<DeletionResultDto> SoftDeleteClientBranchAsync(Guid branchId);

    /// <summary>
    /// Permanently delete a client branch and all its dependencies (hard delete)
    /// ⚠️ WARNING: This cannot be undone!
    /// </summary>
    Task<DeletionResultDto> HardDeleteClientBranchAsync(Guid branchId);

    /// <summary>
    /// Restore a soft-deleted client branch and CASCADE to all children
    /// </summary>
    Task<DeletionResultDto> RestoreClientBranchAsync(Guid branchId);

    /// <summary>
    /// Validate what would be deleted without actually deleting
    /// Useful for showing user a confirmation dialog
    /// </summary>
    Task<DeletionResultDto> ValidateDeletionAsync(Guid clientId);
}
