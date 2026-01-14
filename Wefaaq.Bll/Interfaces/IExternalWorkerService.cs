using Wefaaq.Bll.DTOs;

namespace Wefaaq.Bll.Interfaces;

/// <summary>
/// Service interface for external worker operations
/// </summary>
public interface IExternalWorkerService
{
    /// <summary>
    /// Get all external workers
    /// </summary>
    Task<IEnumerable<ExternalWorkerDto>> GetAllAsync();

    /// <summary>
    /// Get external worker by ID
    /// </summary>
    Task<ExternalWorkerDto?> GetByIdAsync(Guid id);

    /// <summary>
    /// Get all workers for a specific client
    /// </summary>
    Task<IEnumerable<ExternalWorkerDto>> GetByClientIdAsync(Guid clientId);

    /// <summary>
    /// Get all workers for a specific client branch
    /// </summary>
    Task<IEnumerable<ExternalWorkerDto>> GetByClientBranchIdAsync(Guid branchId);

    /// <summary>
    /// Create new external worker
    /// </summary>
    Task<ExternalWorkerDto> CreateAsync(ExternalWorkerCreateDto workerCreateDto);

    /// <summary>
    /// Update existing external worker
    /// </summary>
    Task<ExternalWorkerDto?> UpdateAsync(Guid id, ExternalWorkerUpdateDto workerUpdateDto);

    /// <summary>
    /// Delete external worker (soft delete)
    /// </summary>
    Task<bool> DeleteAsync(Guid id);
}
