using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Wefaaq.Bll.DTOs;
using Wefaaq.Bll.Interfaces;
using Wefaaq.Dal;
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
    private readonly WefaaqContext _context;
    private readonly IMapper _mapper;
    private readonly IValidator<OrganizationCreateDto> _createValidator;
    private readonly IValidator<OrganizationUpdateDto> _updateValidator;
    private readonly IPasswordEncryptionService _passwordEncryption;
    private readonly IAccessControlService _access;

    public OrganizationService(
        IOrganizationRepository organizationRepository,
        IClientRepository clientRepository,
        WefaaqContext context,
        IMapper mapper,
        IValidator<OrganizationCreateDto> createValidator,
        IValidator<OrganizationUpdateDto> updateValidator,
        IPasswordEncryptionService passwordEncryption,
        IAccessControlService access)
    {
        _organizationRepository = organizationRepository;
        _clientRepository = clientRepository;
        _context = context;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _passwordEncryption = passwordEncryption;
        _access = access;
    }

    public async Task<IEnumerable<OrganizationDto>> GetAllAsync()
    {
        var query = _context.Organizations
            .Include(o => o.Records)
            .Include(o => o.Licenses)
            .Include(o => o.Workers)
            .Include(o => o.Cars)
            .Include(o => o.Client)
            .AsQueryable();

        var organizations = await _access.FilterOrganizations(query).ToListAsync();
        return _mapper.Map<IEnumerable<OrganizationDto>>(organizations);
    }

    public async Task<OrganizationDto?> GetByIdAsync(Guid id)
    {
        if (!await _access.CanAccessOrganizationAsync(id)) return null;

        var organization = await _organizationRepository.GetByIdAsync(id);
        if (organization == null) return null;

        var orgDto = _mapper.Map<OrganizationDto>(organization);
        DecryptPasswordsInOrganizationDto(orgDto);
        return orgDto;
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
            if (!await _access.CanAccessClientAsync(organizationCreateDto.ClientId.Value))
                throw new UnauthorizedAccessException("Client is not assigned to current user");

            var client = await _clientRepository.GetByIdAsync(organizationCreateDto.ClientId.Value);
            if (client == null)
            {
                throw new InvalidOperationException($"Client with ID {organizationCreateDto.ClientId} not found");
            }
        }
        else if (organizationCreateDto.ClientBranchId.HasValue)
        {
            if (!await _access.CanAccessBranchAsync(organizationCreateDto.ClientBranchId.Value))
                throw new UnauthorizedAccessException("Client branch is not assigned to current user");
        }

        var organization = _mapper.Map<Organization>(organizationCreateDto);
        organization.Id = Guid.NewGuid();

        var createdOrganization = await _organizationRepository.AddAsync(organization);
        var orgDto = _mapper.Map<OrganizationDto>(createdOrganization);
        DecryptPasswordsInOrganizationDto(orgDto);
        return orgDto;
    }

    public async Task<OrganizationDto?> UpdateAsync(Guid id, OrganizationUpdateDto organizationUpdateDto)
    {
        var validationResult = await _updateValidator.ValidateAsync(organizationUpdateDto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        if (!await _access.CanAccessOrganizationAsync(id))
            throw new UnauthorizedAccessException("Organization is not assigned to current user");

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
        var orgDto = _mapper.Map<OrganizationDto>(updatedOrganization);
        DecryptPasswordsInOrganizationDto(orgDto);
        return orgDto;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        if (!await _access.CanAccessOrganizationAsync(id))
            throw new UnauthorizedAccessException("Organization is not assigned to current user");

        return await _organizationRepository.DeleteAsync(id);
    }

    public async Task<OrganizationDto?> GetWithDetailsAsync(Guid id)
    {
        if (!await _access.CanAccessOrganizationAsync(id)) return null;

        var organization = await _organizationRepository.GetWithDetailsAsync(id);
        return organization == null ? null : _mapper.Map<OrganizationDto>(organization);
    }

    public async Task<IEnumerable<OrganizationDto>> GetWithExpiringCardsAsync()
    {
        var query = _context.Organizations
            .Where(o => o.CardExpiringSoon)
            .Include(o => o.Client)
            .AsQueryable();

        var organizations = await _access.FilterOrganizations(query).ToListAsync();
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

    /// <summary>
    /// Decrypt passwords in organization usernames
    /// </summary>
    private void DecryptPasswordsInOrganizationDto(OrganizationDto orgDto)
    {
        if (orgDto.Usernames != null)
        {
            foreach (var username in orgDto.Usernames)
            {
                username.Password = _passwordEncryption.Decrypt(username.Password);
            }
        }
    }
}