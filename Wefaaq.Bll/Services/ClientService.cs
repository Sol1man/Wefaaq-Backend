using AutoMapper;
using FluentValidation;
using Wefaaq.Bll.DTOs;
using Wefaaq.Bll.Interfaces;
using Wefaaq.Dal.Entities;
using Wefaaq.Dal.RepositoriesInterfaces;

namespace Wefaaq.Bll.Services;

/// <summary>
/// Client service implementation
/// </summary>
public class ClientService : IClientService
{
    private readonly IClientRepository _clientRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<ClientCreateDto> _createValidator;
    private readonly IValidator<ClientUpdateDto> _updateValidator;

    public ClientService(
        IClientRepository clientRepository,
        IOrganizationRepository organizationRepository,
        IMapper mapper,
        IValidator<ClientCreateDto> createValidator,
        IValidator<ClientUpdateDto> updateValidator)
    {
        _clientRepository = clientRepository;
        _organizationRepository = organizationRepository;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IEnumerable<ClientDto>> GetAllAsync()
    {
        var clients = await _clientRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<ClientDto>>(clients);
    }

    public async Task<ClientDto?> GetByIdAsync(Guid id)
    {
        var client = await _clientRepository.GetByIdAsync(id);
        return client == null ? null : _mapper.Map<ClientDto>(client);
    }

    public async Task<ClientDto> CreateAsync(ClientCreateDto clientCreateDto)
    {
        var validationResult = await _createValidator.ValidateAsync(clientCreateDto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // Check if email already exists
        if (await _clientRepository.EmailExistsAsync(clientCreateDto.Email))
        {
            throw new InvalidOperationException($"Client with email '{clientCreateDto.Email}' already exists");
        }

        var client = _mapper.Map<Client>(clientCreateDto);
        client.Id = Guid.NewGuid();

        // Handle organization relationships
        if (clientCreateDto.OrganizationIds.Any())
        {
            var organizations = new List<Organization>();
            foreach (var orgId in clientCreateDto.OrganizationIds)
            {
                var org = await _organizationRepository.GetByIdAsync(orgId);
                if (org != null)
                {
                    organizations.Add(org);
                }
            }
            client.Organizations = organizations;
        }

        var createdClient = await _clientRepository.AddAsync(client);
        return _mapper.Map<ClientDto>(createdClient);
    }

    public async Task<ClientDto?> UpdateAsync(Guid id, ClientUpdateDto clientUpdateDto)
    {
        var validationResult = await _updateValidator.ValidateAsync(clientUpdateDto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var existingClient = await _clientRepository.GetByIdAsync(id);
        if (existingClient == null)
        {
            return null;
        }

        // Check if email already exists for another client
        if (await _clientRepository.EmailExistsAsync(clientUpdateDto.Email, id))
        {
            throw new InvalidOperationException($"Client with email '{clientUpdateDto.Email}' already exists");
        }

        // Update client properties
        _mapper.Map(clientUpdateDto, existingClient);

        // Handle organization relationships
        existingClient.Organizations.Clear();
        if (clientUpdateDto.OrganizationIds.Any())
        {
            var organizations = new List<Organization>();
            foreach (var orgId in clientUpdateDto.OrganizationIds)
            {
                var org = await _organizationRepository.GetByIdAsync(orgId);
                if (org != null)
                {
                    organizations.Add(org);
                }
            }
            existingClient.Organizations = organizations;
        }

        var updatedClient = await _clientRepository.UpdateAsync(existingClient);
        return _mapper.Map<ClientDto>(updatedClient);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _clientRepository.DeleteAsync(id);
    }

    public async Task<ClientDto?> GetWithOrganizationsAsync(Guid id)
    {
        var client = await _clientRepository.GetWithOrganizationsAsync(id);
        return client == null ? null : _mapper.Map<ClientDto>(client);
    }

    public async Task<IEnumerable<ClientDto>> GetCreditorsAsync()
    {
        var clients = await _clientRepository.GetCreditorsAsync();
        return _mapper.Map<IEnumerable<ClientDto>>(clients);
    }

    public async Task<IEnumerable<ClientDto>> GetDebtorsAsync()
    {
        var clients = await _clientRepository.GetDebtorsAsync();
        return _mapper.Map<IEnumerable<ClientDto>>(clients);
    }
}