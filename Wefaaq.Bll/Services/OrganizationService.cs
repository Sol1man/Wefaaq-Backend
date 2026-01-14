using AutoMapper;
using FluentValidation;
using Wefaaq.Bll.DTOs;
using Wefaaq.Bll.Interfaces;
using Wefaaq.Dal.Entities;
using Wefaaq.Dal.RepositoriesInterfaces;

namespace Wefaaq.Bll.Services;

/// <summary>
/// Organization service implementation
/// </summary>
public class OrganizationService : IOrganizationService
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<OrganizationCreateDto> _createValidator;
    private readonly IValidator<OrganizationUpdateDto> _updateValidator;

    public OrganizationService(
        IOrganizationRepository organizationRepository,
        IClientRepository clientRepository,
        IMapper mapper,
        IValidator<OrganizationCreateDto> createValidator,
        IValidator<OrganizationUpdateDto> updateValidator)
    {
        _organizationRepository = organizationRepository;
        _clientRepository = clientRepository;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IEnumerable<OrganizationDto>> GetAllAsync()
    {
        var organizations = await _organizationRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<OrganizationDto>>(organizations);
    }

    public async Task<OrganizationDto?> GetByIdAsync(Guid id)
    {
        var organization = await _organizationRepository.GetByIdAsync(id);
        return organization == null ? null : _mapper.Map<OrganizationDto>(organization);
    }

    public async Task<OrganizationDto> CreateAsync(OrganizationCreateDto organizationCreateDto)
    {
        var validationResult = await _createValidator.ValidateAsync(organizationCreateDto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // Verify exactly one of ClientId or ClientBranchId is set
        if (!organizationCreateDto.ClientId.HasValue && !organizationCreateDto.ClientBranchId.HasValue)
        {
            throw new InvalidOperationException("Organization must belong to either a Client or a ClientBranch");
        }
        if (organizationCreateDto.ClientId.HasValue && organizationCreateDto.ClientBranchId.HasValue)
        {
            throw new InvalidOperationException("Organization cannot belong to both a Client and a ClientBranch");
        }

        // Verify client or branch exists
        if (organizationCreateDto.ClientId.HasValue)
        {
            var client = await _clientRepository.GetByIdAsync(organizationCreateDto.ClientId.Value);
            if (client == null)
            {
                throw new InvalidOperationException($"Client with ID {organizationCreateDto.ClientId} not found");
            }
        }
        // Note: ClientBranch verification would go here when ClientBranchRepository is available

        var organization = _mapper.Map<Organization>(organizationCreateDto);
        organization.Id = Guid.NewGuid();

        var createdOrganization = await _organizationRepository.AddAsync(organization);
        return _mapper.Map<OrganizationDto>(createdOrganization);
    }

    public async Task<OrganizationDto?> UpdateAsync(Guid id, OrganizationUpdateDto organizationUpdateDto)
    {
        var validationResult = await _updateValidator.ValidateAsync(organizationUpdateDto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var existingOrganization = await _organizationRepository.GetByIdAsync(id);
        if (existingOrganization == null)
        {
            return null;
        }

        // Verify exactly one of ClientId or ClientBranchId is set
        if (!organizationUpdateDto.ClientId.HasValue && !organizationUpdateDto.ClientBranchId.HasValue)
        {
            throw new InvalidOperationException("Organization must belong to either a Client or a ClientBranch");
        }
        if (organizationUpdateDto.ClientId.HasValue && organizationUpdateDto.ClientBranchId.HasValue)
        {
            throw new InvalidOperationException("Organization cannot belong to both a Client and a ClientBranch");
        }

        // Verify client or branch exists
        if (organizationUpdateDto.ClientId.HasValue)
        {
            var client = await _clientRepository.GetByIdAsync(organizationUpdateDto.ClientId.Value);
            if (client == null)
            {
                throw new InvalidOperationException($"Client with ID {organizationUpdateDto.ClientId} not found");
            }
        }
        // Note: ClientBranch verification would go here when ClientBranchRepository is available

        _mapper.Map(organizationUpdateDto, existingOrganization);

        var updatedOrganization = await _organizationRepository.UpdateAsync(existingOrganization);
        return _mapper.Map<OrganizationDto>(updatedOrganization);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _organizationRepository.DeleteAsync(id);
    }

    public async Task<OrganizationDto?> GetWithDetailsAsync(Guid id)
    {
        var organization = await _organizationRepository.GetWithDetailsAsync(id);
        return organization == null ? null : _mapper.Map<OrganizationDto>(organization);
    }

    public async Task<IEnumerable<OrganizationDto>> GetWithExpiringCardsAsync()
    {
        var organizations = await _organizationRepository.GetWithExpiringCardsAsync();
        return _mapper.Map<IEnumerable<OrganizationDto>>(organizations);
    }

    // Simplified implementations for sub-entity operations
    // Note: In a full implementation, these would have their own repositories and services
    public async Task<OrganizationRecordDto> AddRecordAsync(Guid organizationId, OrganizationRecordCreateDto recordCreateDto)
    {
        // Implementation would add record to organization
        throw new NotImplementedException("Record operations to be implemented in full version");
    }

    public async Task<OrganizationRecordDto?> UpdateRecordAsync(Guid organizationId, Guid recordId, OrganizationRecordUpdateDto recordUpdateDto)
    {
        throw new NotImplementedException("Record operations to be implemented in full version");
    }

    public async Task<bool> DeleteRecordAsync(Guid organizationId, Guid recordId)
    {
        throw new NotImplementedException("Record operations to be implemented in full version");
    }

    public async Task<OrganizationWorkerDto> AddWorkerAsync(Guid organizationId, OrganizationWorkerCreateDto workerCreateDto)
    {
        throw new NotImplementedException("Worker operations to be implemented in full version");
    }

    public async Task<OrganizationWorkerDto?> UpdateWorkerAsync(Guid organizationId, Guid workerId, OrganizationWorkerUpdateDto workerUpdateDto)
    {
        throw new NotImplementedException("Worker operations to be implemented in full version");
    }

    public async Task<bool> DeleteWorkerAsync(Guid organizationId, Guid workerId)
    {
        throw new NotImplementedException("Worker operations to be implemented in full version");
    }

    public async Task<OrganizationUsernameDto> AddUsernameAsync(Guid organizationId, OrganizationUsernameCreateDto usernameCreateDto)
    {
        throw new NotImplementedException("Username operations to be implemented in full version");
    }

    public async Task<OrganizationUsernameDto?> UpdateUsernameAsync(Guid organizationId, Guid usernameId, OrganizationUsernameUpdateDto usernameUpdateDto)
    {
        throw new NotImplementedException("Username operations to be implemented in full version");
    }

    public async Task<bool> DeleteUsernameAsync(Guid organizationId, Guid usernameId)
    {
        throw new NotImplementedException("Username operations to be implemented in full version");
    }
}