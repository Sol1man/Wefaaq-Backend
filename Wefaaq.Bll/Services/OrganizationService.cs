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

        var organization = _mapper.Map<Organization>(organizationCreateDto);
        organization.Id = Guid.NewGuid();

        // Handle client relationships
        if (organizationCreateDto.ClientIds.Any())
        {
            var clients = new List<Client>();
            foreach (var clientId in organizationCreateDto.ClientIds)
            {
                var client = await _clientRepository.GetByIdAsync(clientId);
                if (client != null)
                {
                    clients.Add(client);
                }
            }
            organization.Clients = clients;
        }

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

        _mapper.Map(organizationUpdateDto, existingOrganization);

        // Handle client relationships
        existingOrganization.Clients.Clear();
        if (organizationUpdateDto.ClientIds.Any())
        {
            var clients = new List<Client>();
            foreach (var clientId in organizationUpdateDto.ClientIds)
            {
                var client = await _clientRepository.GetByIdAsync(clientId);
                if (client != null)
                {
                    clients.Add(client);
                }
            }
            existingOrganization.Clients = clients;
        }

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
}