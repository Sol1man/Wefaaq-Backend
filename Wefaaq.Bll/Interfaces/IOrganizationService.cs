using Wefaaq.Bll.DTOs;

namespace Wefaaq.Bll.Interfaces;

/// <summary>
/// Organization service interface
/// </summary>
public interface IOrganizationService
{
    /// <summary>
    /// Get all organizations
    /// </summary>
    /// <returns>List of organization DTOs</returns>
    Task<IEnumerable<OrganizationDto>> GetAllAsync();

    /// <summary>
    /// Get organization by ID
    /// </summary>
    /// <param name="id">Organization ID</param>
    /// <returns>Organization DTO or null</returns>
    Task<OrganizationDto?> GetByIdAsync(Guid id);

    /// <summary>
    /// Create new organization
    /// </summary>
    /// <param name="organizationCreateDto">Organization creation data</param>
    /// <returns>Created organization DTO</returns>
    Task<OrganizationDto> CreateAsync(OrganizationCreateDto organizationCreateDto);

    /// <summary>
    /// Update existing organization
    /// </summary>
    /// <param name="id">Organization ID</param>
    /// <param name="organizationUpdateDto">Organization update data</param>
    /// <returns>Updated organization DTO or null if not found</returns>
    Task<OrganizationDto?> UpdateAsync(Guid id, OrganizationUpdateDto organizationUpdateDto);

    /// <summary>
    /// Delete organization
    /// </summary>
    /// <param name="id">Organization ID</param>
    /// <returns>True if deleted, false if not found</returns>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    /// Get organization with all details
    /// </summary>
    /// <param name="id">Organization ID</param>
    /// <returns>Organization DTO with details or null</returns>
    Task<OrganizationDto?> GetWithDetailsAsync(Guid id);

    /// <summary>
    /// Get organizations with expiring cards
    /// </summary>
    /// <returns>List of organizations with expiring cards</returns>
    Task<IEnumerable<OrganizationDto>> GetWithExpiringCardsAsync();

    // Organization Records
    /// <summary>
    /// Add record to organization
    /// </summary>
    /// <param name="organizationId">Organization ID</param>
    /// <param name="recordCreateDto">Record creation data</param>
    /// <returns>Created record DTO</returns>
    Task<OrganizationRecordDto> AddRecordAsync(Guid organizationId, OrganizationRecordCreateDto recordCreateDto);

    /// <summary>
    /// Update organization record
    /// </summary>
    /// <param name="organizationId">Organization ID</param>
    /// <param name="recordId">Record ID</param>
    /// <param name="recordUpdateDto">Record update data</param>
    /// <returns>Updated record DTO or null</returns>
    Task<OrganizationRecordDto?> UpdateRecordAsync(Guid organizationId, Guid recordId, OrganizationRecordUpdateDto recordUpdateDto);

    /// <summary>
    /// Delete organization record
    /// </summary>
    /// <param name="organizationId">Organization ID</param>
    /// <param name="recordId">Record ID</param>
    /// <returns>True if deleted, false if not found</returns>
    Task<bool> DeleteRecordAsync(Guid organizationId, Guid recordId);

    // Organization Workers
    /// <summary>
    /// Add worker to organization
    /// </summary>
    /// <param name="organizationId">Organization ID</param>
    /// <param name="workerCreateDto">Worker creation data</param>
    /// <returns>Created worker DTO</returns>
    Task<OrganizationWorkerDto> AddWorkerAsync(Guid organizationId, OrganizationWorkerCreateDto workerCreateDto);

    /// <summary>
    /// Update organization worker
    /// </summary>
    /// <param name="organizationId">Organization ID</param>
    /// <param name="workerId">Worker ID</param>
    /// <param name="workerUpdateDto">Worker update data</param>
    /// <returns>Updated worker DTO or null</returns>
    Task<OrganizationWorkerDto?> UpdateWorkerAsync(Guid organizationId, Guid workerId, OrganizationWorkerUpdateDto workerUpdateDto);

    /// <summary>
    /// Delete organization worker
    /// </summary>
    /// <param name="organizationId">Organization ID</param>
    /// <param name="workerId">Worker ID</param>
    /// <returns>True if deleted, false if not found</returns>
    Task<bool> DeleteWorkerAsync(Guid organizationId, Guid workerId);
}