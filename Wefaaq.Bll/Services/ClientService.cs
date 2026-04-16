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
    private readonly IPasswordEncryptionService _passwordEncryption;

    public ClientService(
        IClientRepository clientRepository,
        IOrganizationRepository organizationRepository,
        WefaaqContext context,
        IMapper mapper,
        IValidator<ClientCreateDto> createValidator,
        IValidator<ClientUpdateDto> updateValidator,
        IValidator<ClientWithOrganizationsCreateDto> createWithOrgsValidator,
        IValidator<ClientWithOrganizationsUpdateDto> updateWithOrgsValidator,
        IPasswordEncryptionService passwordEncryption)
    {
        _clientRepository = clientRepository;
        _organizationRepository = organizationRepository;
        _context = context;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _createWithOrgsValidator = createWithOrgsValidator;
        _updateWithOrgsValidator = updateWithOrgsValidator;
        _passwordEncryption = passwordEncryption;
    }

    public async Task<IEnumerable<ClientDto>> GetAllAsync()
    {
        var clients = await _clientRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<ClientDto>>(clients);
    }

    public async Task<ClientDto?> GetByIdAsync(Guid id)
    {
        var client = await _clientRepository.GetByIdAsync(id);
        if (client == null) return null;

        var clientDto = _mapper.Map<ClientDto>(client);
        DecryptPasswordsInClientDto(clientDto);
        return clientDto;
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

        // Handle organization relationships — single query instead of N round-trips
        existingClient.Organizations.Clear();
        if (clientUpdateDto.OrganizationIds.Any())
        {
            var orgIds = clientUpdateDto.OrganizationIds;
            var organizations = await _context.Organizations
                .Where(o => orgIds.Contains(o.Id))
                .ToListAsync();
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
        var clientDto = await BuildClientDtoQuery(_context.Clients.AsNoTracking().Where(c => c.Id == id))
            .FirstOrDefaultAsync();

        if (clientDto == null) return null;

        DecryptPasswordsInClientDto(clientDto);
        return clientDto;
    }

    /// <summary>
    /// Builds a single-query projection from Client entities to ClientDto with all nested
    /// collections (organizations, branches, workers, records, licenses, cars, usernames).
    /// Replaces the previous ~14 split-query Include chain that was the main perf bottleneck —
    /// EF translates this to one optimized SELECT, skipping change tracking and AutoMapper.
    /// Password decryption still happens in C# on the resulting DTO.
    /// </summary>
    private IQueryable<ClientDto> BuildClientDtoQuery(IQueryable<Client> source)
    {
        return source.Select(c => new ClientDto
        {
            Id = c.Id,
            Name = c.Name,
            Email = c.Email,
            PhoneNumber = c.PhoneNumber,
            Classification = c.Classification,
            Balance = c.Balance,
            IsDeleted = c.IsDeleted,
            DeletedAt = c.DeletedAt,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,

            Organizations = c.Organizations.Select(o => new OrganizationDto
            {
                Id = o.Id,
                Name = o.Name,
                CardExpiringSoon = o.CardExpiringSoon,
                ClientId = o.ClientId,
                Client = c.Name,
                ClientBranchId = o.ClientBranchId,
                ClientBranch = null,
                IsDeleted = o.IsDeleted,
                DeletedAt = o.DeletedAt,
                CreatedAt = o.CreatedAt,
                UpdatedAt = o.UpdatedAt,
                Records = o.Records.Select(r => new OrganizationRecordDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Number = r.Number,
                    ExpiryDate = r.ExpiryDate,
                    ImagePath = r.ImagePath,
                    OrganizationId = r.OrganizationId,
                    IsDeleted = r.IsDeleted,
                    DeletedAt = r.DeletedAt,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                }).ToList(),
                Licenses = o.Licenses.Select(l => new OrganizationLicenseDto
                {
                    Id = l.Id,
                    Name = l.Name,
                    Number = l.Number,
                    ExpiryDate = l.ExpiryDate,
                    ImagePath = l.ImagePath,
                    OrganizationId = l.OrganizationId,
                    IsDeleted = l.IsDeleted,
                    DeletedAt = l.DeletedAt,
                    CreatedAt = l.CreatedAt,
                    UpdatedAt = l.UpdatedAt,
                }).ToList(),
                Workers = o.Workers.Select(w => new OrganizationWorkerDto
                {
                    Id = w.Id,
                    Name = w.Name,
                    ResidenceNumber = w.ResidenceNumber,
                    ResidenceImagePath = w.ResidenceImagePath,
                    ExpiryDate = w.ExpiryDate,
                    OrganizationId = w.OrganizationId,
                    IsDeleted = w.IsDeleted,
                    DeletedAt = w.DeletedAt,
                    CreatedAt = w.CreatedAt,
                    UpdatedAt = w.UpdatedAt,
                }).ToList(),
                Cars = o.Cars.Select(ca => new OrganizationCarDto
                {
                    Id = ca.Id,
                    PlateNumber = ca.PlateNumber,
                    Color = ca.Color,
                    SerialNumber = ca.SerialNumber,
                    ImagePath = ca.ImagePath,
                    OperatingCardExpiry = ca.OperatingCardExpiry,
                    OrganizationId = ca.OrganizationId,
                    IsDeleted = ca.IsDeleted,
                    DeletedAt = ca.DeletedAt,
                    CreatedAt = ca.CreatedAt,
                    UpdatedAt = ca.UpdatedAt,
                }).ToList(),
                Usernames = o.Usernames.Select(u => new OrganizationUsernameDto
                {
                    Id = u.Id,
                    SiteName = u.SiteName,
                    Username = u.Username,
                    Password = u.Password,
                    OrganizationId = u.OrganizationId,
                    IsDeleted = u.IsDeleted,
                    DeletedAt = u.DeletedAt,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt,
                }).ToList(),
            }).ToList(),

            ExternalWorkers = c.ExternalWorkers.Select(w => new ExternalWorkerDto
            {
                Id = w.Id,
                Name = w.Name,
                WorkerType = w.WorkerType,
                ResidenceNumber = w.ResidenceNumber,
                ResidenceImagePath = w.ResidenceImagePath,
                ExpiryDate = w.ExpiryDate,
                ClientId = w.ClientId,
                Client = c.Name,
                ClientBranchId = w.ClientBranchId,
                ClientBranch = null,
                IsDeleted = w.IsDeleted,
                DeletedAt = w.DeletedAt,
                CreatedAt = w.CreatedAt,
                UpdatedAt = w.UpdatedAt,
            }).ToList(),

            ClientBranches = c.ClientBranches.Select(b => new ClientBranchDto
            {
                Id = b.Id,
                Name = b.Name,
                Email = b.Email ?? string.Empty,
                PhoneNumber = b.PhoneNumber,
                Classification = b.Classification,
                Balance = b.Balance,
                ParentClientId = b.ParentClientId,
                ParentClient = c.Name,
                BranchType = b.BranchType,
                IsDeleted = b.IsDeleted,
                DeletedAt = b.DeletedAt,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt,
                Organizations = b.Organizations.Select(o => new OrganizationDto
                {
                    Id = o.Id,
                    Name = o.Name,
                    CardExpiringSoon = o.CardExpiringSoon,
                    ClientId = o.ClientId,
                    Client = null,
                    ClientBranchId = o.ClientBranchId,
                    ClientBranch = b.Name,
                    IsDeleted = o.IsDeleted,
                    DeletedAt = o.DeletedAt,
                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt,
                    Records = o.Records.Select(r => new OrganizationRecordDto
                    {
                        Id = r.Id,
                        Name = r.Name,
                        Number = r.Number,
                        ExpiryDate = r.ExpiryDate,
                        ImagePath = r.ImagePath,
                        OrganizationId = r.OrganizationId,
                        IsDeleted = r.IsDeleted,
                        DeletedAt = r.DeletedAt,
                        CreatedAt = r.CreatedAt,
                        UpdatedAt = r.UpdatedAt,
                    }).ToList(),
                    Licenses = o.Licenses.Select(l => new OrganizationLicenseDto
                    {
                        Id = l.Id,
                        Name = l.Name,
                        Number = l.Number,
                        ExpiryDate = l.ExpiryDate,
                        ImagePath = l.ImagePath,
                        OrganizationId = l.OrganizationId,
                        IsDeleted = l.IsDeleted,
                        DeletedAt = l.DeletedAt,
                        CreatedAt = l.CreatedAt,
                        UpdatedAt = l.UpdatedAt,
                    }).ToList(),
                    Workers = o.Workers.Select(w => new OrganizationWorkerDto
                    {
                        Id = w.Id,
                        Name = w.Name,
                        ResidenceNumber = w.ResidenceNumber,
                        ResidenceImagePath = w.ResidenceImagePath,
                        ExpiryDate = w.ExpiryDate,
                        OrganizationId = w.OrganizationId,
                        IsDeleted = w.IsDeleted,
                        DeletedAt = w.DeletedAt,
                        CreatedAt = w.CreatedAt,
                        UpdatedAt = w.UpdatedAt,
                    }).ToList(),
                    Cars = o.Cars.Select(ca => new OrganizationCarDto
                    {
                        Id = ca.Id,
                        PlateNumber = ca.PlateNumber,
                        Color = ca.Color,
                        SerialNumber = ca.SerialNumber,
                        ImagePath = ca.ImagePath,
                        OperatingCardExpiry = ca.OperatingCardExpiry,
                        OrganizationId = ca.OrganizationId,
                        IsDeleted = ca.IsDeleted,
                        DeletedAt = ca.DeletedAt,
                        CreatedAt = ca.CreatedAt,
                        UpdatedAt = ca.UpdatedAt,
                    }).ToList(),
                    Usernames = o.Usernames.Select(u => new OrganizationUsernameDto
                    {
                        Id = u.Id,
                        SiteName = u.SiteName,
                        Username = u.Username,
                        Password = u.Password,
                        OrganizationId = u.OrganizationId,
                        IsDeleted = u.IsDeleted,
                        DeletedAt = u.DeletedAt,
                        CreatedAt = u.CreatedAt,
                        UpdatedAt = u.UpdatedAt,
                    }).ToList(),
                }).ToList(),
                ExternalWorkers = b.ExternalWorkers.Select(w => new ExternalWorkerDto
                {
                    Id = w.Id,
                    Name = w.Name,
                    WorkerType = w.WorkerType,
                    ResidenceNumber = w.ResidenceNumber,
                    ResidenceImagePath = w.ResidenceImagePath,
                    ExpiryDate = w.ExpiryDate,
                    ClientId = w.ClientId,
                    Client = null,
                    ClientBranchId = w.ClientBranchId,
                    ClientBranch = b.Name,
                    IsDeleted = w.IsDeleted,
                    DeletedAt = w.DeletedAt,
                    CreatedAt = w.CreatedAt,
                    UpdatedAt = w.UpdatedAt,
                }).ToList(),
            }).ToList(),
        });
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
                        Name = recordDto.Name,
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
                        Name = licenseDto.Name,
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

                // Create organization usernames (with password encryption)
                if (orgDto.Usernames != null && orgDto.Usernames.Any())
                {
                    organization.Usernames = orgDto.Usernames.Select(usernameDto => new OrganizationUsername
                    {
                        Id = Guid.NewGuid(),
                        SiteName = usernameDto.SiteName,
                        Username = usernameDto.Username,
                        Password = _passwordEncryption.Encrypt(usernameDto.Password),
                        OrganizationId = organization.Id,
                        Organization = organization
                    }).ToList();
                }

                return organization;
            }).ToList();

            client.Organizations = organizations;
        }

        await _clientRepository.AddAsync(client);

        // Reload via single-query projection — same shape, ~14× fewer round-trips than the old Include chain
        var clientDto = await BuildClientDtoQuery(_context.Clients.AsNoTracking().Where(c => c.Id == client.Id))
            .FirstOrDefaultAsync();
        if (clientDto != null)
        {
            DecryptPasswordsInClientDto(clientDto);
        }
        return clientDto!;
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

        // Reload via single-query projection — same shape, ~14× fewer round-trips than the old Include chain
        var clientDto = await BuildClientDtoQuery(_context.Clients.AsNoTracking().Where(c => c.Id == existingClient.Id))
            .FirstOrDefaultAsync();
        if (clientDto != null)
        {
            DecryptPasswordsInClientDto(clientDto);
        }
        return clientDto;
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

        // Reload via single-query projection — same shape, ~14× fewer round-trips than the old Include chain
        var clientDto = await BuildClientDtoQuery(_context.Clients.AsNoTracking().Where(c => c.Id == client.Id))
            .FirstOrDefaultAsync();
        if (clientDto != null)
        {
            DecryptPasswordsInClientDto(clientDto);
        }
        return clientDto!;
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

        // Reload via single-query projection — same shape, ~14× fewer round-trips than the old Include chain
        var clientDto = await BuildClientDtoQuery(_context.Clients.AsNoTracking().Where(c => c.Id == existingClient.Id))
            .FirstOrDefaultAsync();
        if (clientDto != null)
        {
            DecryptPasswordsInClientDto(clientDto);
        }
        return clientDto;
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

        var orgDto = _mapper.Map<OrganizationDto>(createdOrganization);
        DecryptPasswordsInOrganizationDto(orgDto);
        return orgDto;
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
                Name = recordDto.Name,
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
                Name = licenseDto.Name,
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

        // Create organization usernames (with password encryption)
        if (dto.Usernames != null && dto.Usernames.Any())
        {
            organization.Usernames = dto.Usernames.Select(usernameDto => new OrganizationUsername
            {
                Id = Guid.NewGuid(),
                SiteName = usernameDto.SiteName,
                Username = usernameDto.Username,
                Password = _passwordEncryption.Encrypt(usernameDto.Password),
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
                Name = recordDto.Name,
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
                Name = licenseDto.Name,
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

        // Create organization usernames (with password encryption)
        if (dto.Usernames != null && dto.Usernames.Any())
        {
            organization.Usernames = dto.Usernames.Select(usernameDto => new OrganizationUsername
            {
                Id = Guid.NewGuid(),
                SiteName = usernameDto.SiteName,
                Username = usernameDto.Username,
                Password = _passwordEncryption.Encrypt(usernameDto.Password),
                OrganizationId = organization.Id
            }).ToList();
        }

        return organization;
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

    /// <summary>
    /// Decrypt passwords in all organization usernames for a client DTO
    /// </summary>
    private void DecryptPasswordsInClientDto(ClientDto clientDto)
    {
        if (clientDto.Organizations != null)
        {
            foreach (var org in clientDto.Organizations)
            {
                DecryptPasswordsInOrganizationDto(org);
            }
        }

        if (clientDto.ClientBranches != null)
        {
            foreach (var branch in clientDto.ClientBranches)
            {
                if (branch.Organizations != null)
                {
                    foreach (var org in branch.Organizations)
                    {
                        DecryptPasswordsInOrganizationDto(org);
                    }
                }
            }
        }
    }
}