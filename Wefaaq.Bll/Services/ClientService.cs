using AutoMapper;
using FluentValidation;
using Wefaaq.Bll.DTOs;
using Wefaaq.Bll.Interfaces;
using Wefaaq.Dal;
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
    private readonly WefaaqContext _context;
    private readonly IMapper _mapper;
    private readonly IValidator<ClientCreateDto> _createValidator;
    private readonly IValidator<ClientUpdateDto> _updateValidator;
    private readonly IValidator<ClientWithOrganizationsCreateDto> _createWithOrgsValidator;
    private readonly IValidator<ClientWithOrganizationsUpdateDto> _updateWithOrgsValidator;

    public ClientService(
        IClientRepository clientRepository,
        IOrganizationRepository organizationRepository,
        WefaaqContext context,
        IMapper mapper,
        IValidator<ClientCreateDto> createValidator,
        IValidator<ClientUpdateDto> updateValidator,
        IValidator<ClientWithOrganizationsCreateDto> createWithOrgsValidator,
        IValidator<ClientWithOrganizationsUpdateDto> updateWithOrgsValidator)
    {
        _clientRepository = clientRepository;
        _organizationRepository = organizationRepository;
        _context = context;
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

        // Soft-delete all existing organizations
        if (existingClient.Organizations != null && existingClient.Organizations.Any())
        {
            foreach (var org in existingClient.Organizations.ToList())
            {
                org.IsDeleted = true;
                org.DeletedAt = DateTime.UtcNow;
            }
        }

        // Add new organizations with all nested entities
        if (dto.Organizations.Any())
        {
            foreach (var orgDto in dto.Organizations)
            {
                var organization = new Organization
                {
                    Id = Guid.NewGuid(),
                    Name = orgDto.Name,
                    CardExpiringSoon = orgDto.CardExpiringSoon,
                    ClientId = existingClient.Id
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
                        OrganizationId = organization.Id
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
                        OrganizationId = organization.Id
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
                        OrganizationId = organization.Id
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
                        OrganizationId = organization.Id
                    }).ToList();
                }

                _context.Organizations.Add(organization);
            }
        }

        // Save all changes in a single transaction
        await _context.SaveChangesAsync();

        // Reload the client with all nested entities from database
        var clientWithDetails = await _clientRepository.GetWithOrganizationsAsync(existingClient.Id);
        return _mapper.Map<ClientDto>(clientWithDetails);
    }

    // ===== BULK OPERATIONS (Create/Edit with all details) =====

    public async Task<ClientDto> AddClientWithDetailsAsync(ClientWithDetailsCreateDto dto)
    {
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

        // Create direct organizations
        if (dto.Organizations.Any())
        {
            client.Organizations = dto.Organizations.Select(orgDto => CreateOrganizationEntity(orgDto, client.Id, null)).ToList();
        }

        // Create client branches with their organizations and external workers
        if (dto.Branches.Any())
        {
            client.ClientBranches = dto.Branches.Select(branchDto =>
            {
                var branch = new ClientBranch
                {
                    Id = Guid.NewGuid(),
                    Name = branchDto.Name,
                    Email = branchDto.Email,
                    PhoneNumber = branchDto.PhoneNumber,
                    Classification = branchDto.Classification,
                    Balance = branchDto.Balance,
                    BranchType = branchDto.BranchType,
                    ParentClientId = client.Id
                };

                // Create branch organizations
                if (branchDto.Organizations.Any())
                {
                    branch.Organizations = branchDto.Organizations.Select(orgDto => CreateOrganizationEntity(orgDto, null, branch.Id)).ToList();
                }

                // Create branch external workers
                if (branchDto.ExternalWorkers.Any())
                {
                    branch.ExternalWorkers = branchDto.ExternalWorkers.Select(workerDto => CreateExternalWorkerEntity(workerDto, null, branch.Id)).ToList();
                }

                return branch;
            }).ToList();
        }

        // Create direct external workers
        if (dto.ExternalWorkers.Any())
        {
            client.ExternalWorkers = dto.ExternalWorkers.Select(workerDto => CreateExternalWorkerEntity(workerDto, client.Id, null)).ToList();
        }

        await _clientRepository.AddAsync(client);

        // Reload the client with all nested entities from database
        var clientWithDetails = await _clientRepository.GetWithOrganizationsAsync(client.Id);
        return _mapper.Map<ClientDto>(clientWithDetails);
    }

    public async Task<ClientDto?> EditClientWithDetailsAsync(Guid id, ClientWithDetailsUpdateDto dto)
    {
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

        // Soft-delete all existing organizations (direct organizations)
        if (existingClient.Organizations != null && existingClient.Organizations.Any())
        {
            foreach (var org in existingClient.Organizations.ToList())
            {
                org.IsDeleted = true;
                org.DeletedAt = DateTime.UtcNow;
            }
        }

        // Soft-delete all existing branches and their nested entities
        if (existingClient.ClientBranches != null && existingClient.ClientBranches.Any())
        {
            foreach (var branch in existingClient.ClientBranches.ToList())
            {
                // Soft-delete branch organizations
                if (branch.Organizations != null)
                {
                    foreach (var org in branch.Organizations)
                    {
                        org.IsDeleted = true;
                        org.DeletedAt = DateTime.UtcNow;
                    }
                }
                // Soft-delete branch external workers
                if (branch.ExternalWorkers != null)
                {
                    foreach (var worker in branch.ExternalWorkers)
                    {
                        worker.IsDeleted = true;
                        worker.DeletedAt = DateTime.UtcNow;
                    }
                }
                // Soft-delete the branch itself
                branch.IsDeleted = true;
                branch.DeletedAt = DateTime.UtcNow;
            }
        }

        // Soft-delete all existing direct external workers
        if (existingClient.ExternalWorkers != null && existingClient.ExternalWorkers.Any())
        {
            foreach (var worker in existingClient.ExternalWorkers.ToList())
            {
                worker.IsDeleted = true;
                worker.DeletedAt = DateTime.UtcNow;
            }
        }

        // Add new direct organizations
        if (dto.Organizations.Any())
        {
            foreach (var orgDto in dto.Organizations)
            {
                var newOrg = CreateOrganizationEntityFromUpdate(orgDto, existingClient.Id, null);
                _context.Organizations.Add(newOrg);
            }
        }

        // Add new branches with their organizations and external workers
        if (dto.Branches.Any())
        {
            foreach (var branchDto in dto.Branches)
            {
                var branch = new ClientBranch
                {
                    Id = Guid.NewGuid(),
                    Name = branchDto.Name,
                    Email = branchDto.Email,
                    PhoneNumber = branchDto.PhoneNumber,
                    Classification = branchDto.Classification,
                    Balance = branchDto.Balance,
                    BranchType = branchDto.BranchType,
                    ParentClientId = existingClient.Id
                };
                _context.ClientBranches.Add(branch);

                // Add branch organizations
                if (branchDto.Organizations.Any())
                {
                    foreach (var orgDto in branchDto.Organizations)
                    {
                        var newOrg = CreateOrganizationEntityFromUpdate(orgDto, null, branch.Id);
                        _context.Organizations.Add(newOrg);
                    }
                }

                // Add branch external workers
                if (branchDto.ExternalWorkers.Any())
                {
                    foreach (var workerDto in branchDto.ExternalWorkers)
                    {
                        var newWorker = new ExternalWorker
                        {
                            Id = Guid.NewGuid(),
                            Name = workerDto.Name,
                            WorkerType = workerDto.WorkerType,
                            ResidenceNumber = workerDto.ResidenceNumber ?? string.Empty,
                            ResidenceImagePath = workerDto.ResidenceImagePath,
                            ExpiryDate = workerDto.ExpiryDate ?? DateTime.UtcNow.AddYears(1),
                            ClientId = null,
                            ClientBranchId = branch.Id
                        };
                        _context.ExternalWorkers.Add(newWorker);
                    }
                }
            }
        }

        // Add new direct external workers
        if (dto.ExternalWorkers.Any())
        {
            foreach (var workerDto in dto.ExternalWorkers)
            {
                var newWorker = new ExternalWorker
                {
                    Id = Guid.NewGuid(),
                    Name = workerDto.Name,
                    WorkerType = workerDto.WorkerType,
                    ResidenceNumber = workerDto.ResidenceNumber ?? string.Empty,
                    ResidenceImagePath = workerDto.ResidenceImagePath,
                    ExpiryDate = workerDto.ExpiryDate ?? DateTime.UtcNow.AddYears(1),
                    ClientId = existingClient.Id,
                    ClientBranchId = null
                };
                _context.ExternalWorkers.Add(newWorker);
            }
        }

        // Save all changes in a single transaction
        await _context.SaveChangesAsync();

        // Reload the client with all nested entities from database
        var clientWithDetails = await _clientRepository.GetWithOrganizationsAsync(existingClient.Id);
        return _mapper.Map<ClientDto>(clientWithDetails);
    }

    // ===== GRANULAR OPERATIONS (Add individual items to existing client) =====

    public async Task<OrganizationDto> AddOrganizationToClientAsync(Guid clientId, OrganizationCreateDto organizationDto)
    {
        // Verify client exists
        var client = await _clientRepository.GetByIdAsync(clientId);
        if (client == null)
        {
            throw new InvalidOperationException($"Client with ID {clientId} not found");
        }

        // Create organization
        var organization = CreateOrganizationEntity(organizationDto, clientId, null);
        var createdOrganization = await _organizationRepository.AddAsync(organization);

        return _mapper.Map<OrganizationDto>(createdOrganization);
    }

    public async Task<ClientBranchDto> AddBranchToClientAsync(Guid clientId, ClientBranchCreateDto branchDto)
    {
        // Verify client exists
        var client = await _clientRepository.GetByIdAsync(clientId);
        if (client == null)
        {
            throw new InvalidOperationException($"Client with ID {clientId} not found");
        }

        // Create branch
        var branch = _mapper.Map<ClientBranch>(branchDto);
        branch.Id = Guid.NewGuid();
        // Always use the clientId from route parameter (overrides DTO value if present)
        branch.ParentClientId = clientId;

        // Note: Branch will be added through the client repository or a dedicated branch repository
        // For now, we'll add it to the client's branches collection
        if (client.ClientBranches == null)
        {
            client.ClientBranches = new List<ClientBranch>();
        }
        client.ClientBranches.Add(branch);
        await _clientRepository.UpdateAsync(client);

        return _mapper.Map<ClientBranchDto>(branch);
    }

    public async Task<ExternalWorkerDto> AddExternalWorkerToClientAsync(Guid clientId, ExternalWorkerCreateDto workerDto)
    {
        // Verify client exists
        var client = await _clientRepository.GetByIdAsync(clientId);
        if (client == null)
        {
            throw new InvalidOperationException($"Client with ID {clientId} not found");
        }

        // Create external worker
        var worker = CreateExternalWorkerEntity(workerDto, clientId, null);

        // Note: Worker will be added through a dedicated external worker repository
        // For now, we'll add it to the client's external workers collection
        if (client.ExternalWorkers == null)
        {
            client.ExternalWorkers = new List<ExternalWorker>();
        }
        client.ExternalWorkers.Add(worker);
        await _clientRepository.UpdateAsync(client);

        return _mapper.Map<ExternalWorkerDto>(worker);
    }

    // ===== HELPER METHODS =====

    private Organization CreateOrganizationEntity(OrganizationCreateDto dto, Guid? clientId, Guid? branchId)
    {
        var organization = new Organization
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            CardExpiringSoon = dto.CardExpiringSoon,
            ClientId = clientId,
            ClientBranchId = branchId
        };

        // Create organization records
        if (dto.Records != null && dto.Records.Any())
        {
            organization.Records = dto.Records.Select(recordDto => new OrganizationRecord
            {
                Id = Guid.NewGuid(),
                Number = recordDto.Number,
                ExpiryDate = recordDto.ExpiryDate,
                ImagePath = recordDto.ImagePath,
                OrganizationId = organization.Id
            }).ToList();
        }

        // Create organization licenses
        if (dto.Licenses != null && dto.Licenses.Any())
        {
            organization.Licenses = dto.Licenses.Select(licenseDto => new OrganizationLicense
            {
                Id = Guid.NewGuid(),
                Number = licenseDto.Number,
                ExpiryDate = licenseDto.ExpiryDate,
                ImagePath = licenseDto.ImagePath,
                OrganizationId = organization.Id
            }).ToList();
        }

        // Create organization workers
        if (dto.Workers != null && dto.Workers.Any())
        {
            organization.Workers = dto.Workers.Select(workerDto => new OrganizationWorker
            {
                Id = Guid.NewGuid(),
                Name = workerDto.Name,
                ResidenceNumber = workerDto.ResidenceNumber,
                ResidenceImagePath = workerDto.ResidenceImagePath,
                ExpiryDate = workerDto.ExpiryDate,
                OrganizationId = organization.Id
            }).ToList();
        }

        // Create organization cars
        if (dto.Cars != null && dto.Cars.Any())
        {
            organization.Cars = dto.Cars.Select(carDto => new OrganizationCar
            {
                Id = Guid.NewGuid(),
                PlateNumber = carDto.PlateNumber,
                Color = carDto.Color,
                SerialNumber = carDto.SerialNumber,
                ImagePath = carDto.ImagePath,
                OperatingCardExpiry = carDto.OperatingCardExpiry,
                OrganizationId = organization.Id
            }).ToList();
        }

        return organization;
    }

    private ExternalWorker CreateExternalWorkerEntity(ExternalWorkerCreateDto dto, Guid? clientId, Guid? branchId)
    {
        return new ExternalWorker
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            WorkerType = dto.WorkerType,
            ResidenceNumber = dto.ResidenceNumber,
            ResidenceImagePath = dto.ResidenceImagePath,
            ExpiryDate = dto.ExpiryDate ?? DateTime.UtcNow.AddYears(1), // Default to 1 year from now if not specified
            ClientId = clientId,
            ClientBranchId = branchId
        };
    }

    private Organization CreateOrganizationEntityFromUpdate(OrganizationUpdateDto dto, Guid? clientId, Guid? branchId)
    {
        var organization = new Organization
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            CardExpiringSoon = dto.CardExpiringSoon,
            ClientId = clientId,
            ClientBranchId = branchId
        };

        // Create organization records
        if (dto.Records != null && dto.Records.Any())
        {
            organization.Records = dto.Records.Select(recordDto => new OrganizationRecord
            {
                Id = Guid.NewGuid(),
                Number = recordDto.Number,
                ExpiryDate = recordDto.ExpiryDate,
                ImagePath = recordDto.ImagePath,
                OrganizationId = organization.Id
            }).ToList();
        }

        // Create organization licenses
        if (dto.Licenses != null && dto.Licenses.Any())
        {
            organization.Licenses = dto.Licenses.Select(licenseDto => new OrganizationLicense
            {
                Id = Guid.NewGuid(),
                Number = licenseDto.Number,
                ExpiryDate = licenseDto.ExpiryDate,
                ImagePath = licenseDto.ImagePath,
                OrganizationId = organization.Id
            }).ToList();
        }

        // Create organization workers
        if (dto.Workers != null && dto.Workers.Any())
        {
            organization.Workers = dto.Workers.Select(workerDto => new OrganizationWorker
            {
                Id = Guid.NewGuid(),
                Name = workerDto.Name,
                ResidenceNumber = workerDto.ResidenceNumber,
                ResidenceImagePath = workerDto.ResidenceImagePath,
                ExpiryDate = workerDto.ExpiryDate,
                OrganizationId = organization.Id
            }).ToList();
        }

        // Create organization cars
        if (dto.Cars != null && dto.Cars.Any())
        {
            organization.Cars = dto.Cars.Select(carDto => new OrganizationCar
            {
                Id = Guid.NewGuid(),
                PlateNumber = carDto.PlateNumber,
                Color = carDto.Color,
                SerialNumber = carDto.SerialNumber,
                ImagePath = carDto.ImagePath,
                OperatingCardExpiry = carDto.OperatingCardExpiry,
                OrganizationId = organization.Id
            }).ToList();
        }

        return organization;
    }
}