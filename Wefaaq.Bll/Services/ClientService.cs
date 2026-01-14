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
    private readonly IValidator<ClientWithOrganizationsCreateDto> _createWithOrgsValidator;
    private readonly IValidator<ClientWithOrganizationsUpdateDto> _updateWithOrgsValidator;

    public ClientService(
        IClientRepository clientRepository,
        IOrganizationRepository organizationRepository,
        IMapper mapper,
        IValidator<ClientCreateDto> createValidator,
        IValidator<ClientUpdateDto> updateValidator,
        IValidator<ClientWithOrganizationsCreateDto> createWithOrgsValidator,
        IValidator<ClientWithOrganizationsUpdateDto> updateWithOrgsValidator)
    {
        _clientRepository = clientRepository;
        _organizationRepository = organizationRepository;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _createWithOrgsValidator = createWithOrgsValidator;
        _updateWithOrgsValidator = updateWithOrgsValidator;
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

    public async Task<ClientDto> AddClientWithOrganizationsAsync(ClientWithOrganizationsCreateDto dto)
    {
        var validationResult = await _createWithOrgsValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // Check if email already exists
        if (await _clientRepository.EmailExistsAsync(dto.Email))
        {
            throw new InvalidOperationException($"Client with email '{dto.Email}' already exists");
        }

        // Create client entity
        var client = new Client
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            Classification = dto.Classification,
            Balance = dto.Balance
        };

        // Create organizations and associate them with the client
        if (dto.Organizations.Any())
        {
            var organizations = dto.Organizations.Select(orgDto =>
            {
                var organization = new Organization
                {
                    Id = Guid.NewGuid(),
                    Name = orgDto.Name,
                    CardExpiringSoon = orgDto.CardExpiringSoon,
                    ClientId = client.Id,
                    Client = client
                };

                // Create organization records
                if (orgDto.Records.Any())
                {
                    organization.Records = orgDto.Records.Select(recordDto => new OrganizationRecord
                    {
                        Id = Guid.NewGuid(),
                        Number = recordDto.Number,
                        ExpiryDate = recordDto.ExpiryDate,
                        ImagePath = recordDto.ImagePath,
                        OrganizationId = organization.Id,
                        Organization = organization
                    }).ToList();
                }

                // Create organization licenses
                if (orgDto.Licenses.Any())
                {
                    organization.Licenses = orgDto.Licenses.Select(licenseDto => new OrganizationLicense
                    {
                        Id = Guid.NewGuid(),
                        Number = licenseDto.Number,
                        ExpiryDate = licenseDto.ExpiryDate,
                        ImagePath = licenseDto.ImagePath,
                        OrganizationId = organization.Id,
                        Organization = organization
                    }).ToList();
                }

                // Create organization workers
                if (orgDto.Workers.Any())
                {
                    organization.Workers = orgDto.Workers.Select(workerDto => new OrganizationWorker
                    {
                        Id = Guid.NewGuid(),
                        Name = workerDto.Name,
                        ResidenceNumber = workerDto.ResidenceNumber,
                        ResidenceImagePath = workerDto.ResidenceImagePath,
                        ExpiryDate = workerDto.ExpiryDate,
                        OrganizationId = organization.Id,
                        Organization = organization
                    }).ToList();
                }

                // Create organization cars
                if (orgDto.Cars.Any())
                {
                    organization.Cars = orgDto.Cars.Select(carDto => new OrganizationCar
                    {
                        Id = Guid.NewGuid(),
                        PlateNumber = carDto.PlateNumber,
                        Color = carDto.Color,
                        SerialNumber = carDto.SerialNumber,
                        ImagePath = carDto.ImagePath,
                        OperatingCardExpiry = carDto.OperatingCardExpiry,
                        OrganizationId = organization.Id,
                        Organization = organization
                    }).ToList();
                }

                return organization;
            }).ToList();

            client.Organizations = organizations;
        }

        await _clientRepository.AddAsync(client);

        // Reload the client with all nested entities from database
        var clientWithDetails = await _clientRepository.GetWithOrganizationsAsync(client.Id);
        return _mapper.Map<ClientDto>(clientWithDetails);
    }

    public async Task<ClientDto?> EditClientWithOrganizationsAsync(Guid id, ClientWithOrganizationsUpdateDto dto)
    {
        var validationResult = await _updateWithOrgsValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var existingClient = await _clientRepository.GetWithOrganizationsAsync(id);
        if (existingClient == null)
        {
            return null;
        }

        // Check if email already exists for another client
        if (await _clientRepository.EmailExistsAsync(dto.Email, id))
        {
            throw new InvalidOperationException($"Client with email '{dto.Email}' already exists");
        }

        // Update client properties
        existingClient.Name = dto.Name;
        existingClient.Email = dto.Email;
        existingClient.PhoneNumber = dto.PhoneNumber;
        existingClient.Classification = dto.Classification;
        existingClient.Balance = dto.Balance;

        // Remove all existing organizations
        if (existingClient.Organizations.Any())
        {
            foreach (var org in existingClient.Organizations.ToList())
            {
                await _organizationRepository.DeleteAsync(org.Id);
            }
        }

        // Add new organizations with all nested entities
        if (dto.Organizations.Any())
        {
            var newOrganizations = dto.Organizations.Select(orgDto =>
            {
                var organization = new Organization
                {
                    Id = Guid.NewGuid(),
                    Name = orgDto.Name,
                    CardExpiringSoon = orgDto.CardExpiringSoon,
                    ClientId = existingClient.Id,
                    Client = existingClient
                };

                // Create organization records
                if (orgDto.Records.Any())
                {
                    organization.Records = orgDto.Records.Select(recordDto => new OrganizationRecord
                    {
                        Id = Guid.NewGuid(),
                        Number = recordDto.Number,
                        ExpiryDate = recordDto.ExpiryDate,
                        ImagePath = recordDto.ImagePath,
                        OrganizationId = organization.Id,
                        Organization = organization
                    }).ToList();
                }

                // Create organization licenses
                if (orgDto.Licenses.Any())
                {
                    organization.Licenses = orgDto.Licenses.Select(licenseDto => new OrganizationLicense
                    {
                        Id = Guid.NewGuid(),
                        Number = licenseDto.Number,
                        ExpiryDate = licenseDto.ExpiryDate,
                        ImagePath = licenseDto.ImagePath,
                        OrganizationId = organization.Id,
                        Organization = organization
                    }).ToList();
                }

                // Create organization workers
                if (orgDto.Workers.Any())
                {
                    organization.Workers = orgDto.Workers.Select(workerDto => new OrganizationWorker
                    {
                        Id = Guid.NewGuid(),
                        Name = workerDto.Name,
                        ResidenceNumber = workerDto.ResidenceNumber,
                        ResidenceImagePath = workerDto.ResidenceImagePath,
                        ExpiryDate = workerDto.ExpiryDate,
                        OrganizationId = organization.Id,
                        Organization = organization
                    }).ToList();
                }

                // Create organization cars
                if (orgDto.Cars.Any())
                {
                    organization.Cars = orgDto.Cars.Select(carDto => new OrganizationCar
                    {
                        Id = Guid.NewGuid(),
                        PlateNumber = carDto.PlateNumber,
                        Color = carDto.Color,
                        SerialNumber = carDto.SerialNumber,
                        ImagePath = carDto.ImagePath,
                        OperatingCardExpiry = carDto.OperatingCardExpiry,
                        OrganizationId = organization.Id,
                        Organization = organization
                    }).ToList();
                }

                return organization;
            }).ToList();

            existingClient.Organizations = newOrganizations;
        }

        await _clientRepository.UpdateAsync(existingClient);

        // Reload the client with all nested entities from database
        var clientWithDetails = await _clientRepository.GetWithOrganizationsAsync(existingClient.Id);
        return _mapper.Map<ClientDto>(clientWithDetails);
    }
}