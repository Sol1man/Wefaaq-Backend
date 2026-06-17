using Wefaaq.Bll.DTOs;
using Wefaaq.Dal.Entities;

namespace Wefaaq.Bll.Interfaces;

public interface IClientOperationService
{
    /// <summary>Get all operations (admin view)</summary>
    Task<IEnumerable<ClientOperationDto>> GetAllAsync();

    /// <summary>Get single operation by ID</summary>
    Task<ClientOperationDto?> GetByIdAsync(Guid id);

    /// <summary>
    /// Get all operations related to a client —
    /// includes operations on the client itself, their branches, and all their organizations
    /// </summary>
    Task<IEnumerable<ClientOperationDto>> GetByClientAsync(Guid clientId);

    /// <summary>Get operations for a specific branch (and its organizations)</summary>
    Task<IEnumerable<ClientOperationDto>> GetByBranchAsync(Guid branchId);

    /// <summary>Get operations for a specific organization</summary>
    Task<IEnumerable<ClientOperationDto>> GetByOrganizationAsync(Guid organizationId);

    /// <summary>Create a new service operation — debits the target balance immediately</summary>
    Task<ClientOperationDto> CreateAsync(ClientOperationCreateDto dto, int performedByUserId);

    /// <summary>Record a client payment — credits the target balance immediately</summary>
    Task<ClientOperationDto> CreatePaymentAsync(ClientOperationPaymentCreateDto dto, int performedByUserId);

    /// <summary>Update operation type, price, notes — re-applies the balance effect for any price change</summary>
    Task<ClientOperationDto?> UpdateAsync(Guid id, ClientOperationUpdateDto dto);

    /// <summary>Soft-delete an operation</summary>
    Task<bool> DeleteAsync(Guid id);
}
