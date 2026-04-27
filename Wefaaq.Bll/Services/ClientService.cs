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
        var client = await _clientRepository.GetWithOrganizationsReadOnlyAsync(id);
        if (client == null) return null;

        var clientDto = _mapper.Map<ClientDto>(client);
        DecryptPasswordsInClientDto(clientDto);
        return clientDto;
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

        // Reload using AsNoTracking — no need for change tracking after insert
        var reloaded = await _clientRepository.GetWithOrganizationsReadOnlyAsync(client.Id);
        var clientDto = _mapper.Map<ClientDto>(reloaded);
        DecryptPasswordsInClientDto(clientDto);
        return clientDto;
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

        // Reload using AsNoTracking — no need for change tracking after update
        var reloaded = await _clientRepository.GetWithOrganizationsReadOnlyAsync(existingClient.Id);
        var clientDto = _mapper.Map<ClientDto>(reloaded);
        DecryptPasswordsInClientDto(clientDto);
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

        // Reload using AsNoTracking — no need for change tracking after insert
        var reloaded = await _clientRepository.GetWithOrganizationsReadOnlyAsync(client.Id);
        var clientDto = _mapper.Map<ClientDto>(reloaded);
        DecryptPasswordsInClientDto(clientDto);
        return clientDto;
    }

    public async Task<ClientDto?> EditClientWithDetailsAsync(Guid id, ClientWithDetailsUpdateDto dto)
    {
        var existingClient = await _clientRepository.GetWithOrganizationsAsync(id);
        if (existingClient == null)
        {
            return null;
        }

        if (await _clientRepository.EmailExistsAsync(dto.Email, id))
        {
            throw new InvalidOperationException($"Client with email '{dto.Email}' already exists");
        }

        // Update client scalar properties
        existingClient.Name = dto.Name;
        existingClient.Email = dto.Email;
        existingClient.PhoneNumber = dto.PhoneNumber;
        existingClient.Classification = dto.Classification;
        existingClient.Balance = dto.Balance;

        // Merge nested collections: items with matching Id → update, null Id → insert, missing → soft-delete
        MergeOrganizations(existingClient.Organizations, dto.Organizations, existingClient.Id, null);
        MergeBranches(existingClient.ClientBranches, dto.Branches, existingClient.Id);
        MergeExternalWorkers(existingClient.ExternalWorkers, dto.ExternalWorkers, existingClient.Id, null);

        await _context.SaveChangesAsync();

        // Reload using AsNoTracking — no need for change tracking after update
        var reloaded = await _clientRepository.GetWithOrganizationsReadOnlyAsync(existingClient.Id);
        var clientDto = _mapper.Map<ClientDto>(reloaded);
        DecryptPasswordsInClientDto(clientDto);
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
        var clientExists = await _context.Clients.AnyAsync(c => c.Id == clientId);
        if (!clientExists)
        {
            throw new InvalidOperationException($"Client with ID {clientId} not found");
        }

        var branch = _mapper.Map<ClientBranch>(branchDto);
        branch.Id = Guid.NewGuid();
        branch.ParentClientId = clientId;

        _context.ClientBranches.Add(branch);
        await _context.SaveChangesAsync();

        return _mapper.Map<ClientBranchDto>(branch);
    }

    public async Task<ExternalWorkerDto> AddExternalWorkerToClientAsync(Guid clientId, ExternalWorkerCreateDto workerDto)
    {
        var clientExists = await _context.Clients.AnyAsync(c => c.Id == clientId);
        if (!clientExists)
        {
            throw new InvalidOperationException($"Client with ID {clientId} not found");
        }

        var worker = CreateExternalWorkerEntity(workerDto, clientId, null);

        _context.ExternalWorkers.Add(worker);
        await _context.SaveChangesAsync();

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

    // ===== MERGE HELPERS — update existing, add new, soft-delete removed =====

    private void MergeOrganizations(
        ICollection<Organization> existing, List<OrganizationUpdateDto>? incoming,
        Guid? clientId, Guid? branchId)
    {
        incoming ??= new();
        var existingById = existing.ToDictionary(o => o.Id);
        var incomingIds = incoming.Where(o => o.Id.HasValue).Select(o => o.Id!.Value).ToHashSet();

        foreach (var org in existing.Where(o => !incomingIds.Contains(o.Id)).ToList())
        {
            org.IsDeleted = true;
            org.DeletedAt = DateTime.UtcNow;
        }

        foreach (var dto in incoming)
        {
            if (dto.Id.HasValue && existingById.TryGetValue(dto.Id.Value, out var org))
            {
                org.Name = dto.Name;
                org.CardExpiringSoon = dto.CardExpiringSoon;
                MergeRecords(org.Records, dto.Records, org.Id);
                MergeLicenses(org.Licenses, dto.Licenses, org.Id);
                MergeWorkers(org.Workers, dto.Workers, org.Id);
                MergeCars(org.Cars, dto.Cars, org.Id);
                MergeUsernames(org.Usernames, dto.Usernames, org.Id);
            }
            else
            {
                _context.Organizations.Add(NewOrganizationFromDto(dto, clientId, branchId));
            }
        }
    }

    private void MergeBranches(
        ICollection<ClientBranch> existing, List<ClientBranchWithDetailsUpdateDto>? incoming,
        Guid parentClientId)
    {
        incoming ??= new();
        var existingById = existing.ToDictionary(b => b.Id);
        var incomingIds = incoming.Where(b => b.Id.HasValue).Select(b => b.Id!.Value).ToHashSet();

        foreach (var branch in existing.Where(b => !incomingIds.Contains(b.Id)).ToList())
        {
            foreach (var org in branch.Organizations) { org.IsDeleted = true; org.DeletedAt = DateTime.UtcNow; }
            foreach (var w in branch.ExternalWorkers) { w.IsDeleted = true; w.DeletedAt = DateTime.UtcNow; }
            branch.IsDeleted = true;
            branch.DeletedAt = DateTime.UtcNow;
        }

        foreach (var dto in incoming)
        {
            if (dto.Id.HasValue && existingById.TryGetValue(dto.Id.Value, out var branch))
            {
                branch.Name = dto.Name;
                branch.Email = dto.Email;
                branch.PhoneNumber = dto.PhoneNumber;
                branch.Classification = dto.Classification;
                branch.Balance = dto.Balance;
                branch.BranchType = dto.BranchType;
                MergeOrganizations(branch.Organizations, dto.Organizations, null, branch.Id);
                MergeExternalWorkers(branch.ExternalWorkers, dto.ExternalWorkers, null, branch.Id);
            }
            else
            {
                var branch2 = new ClientBranch
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name, Email = dto.Email, PhoneNumber = dto.PhoneNumber,
                    Classification = dto.Classification, Balance = dto.Balance,
                    BranchType = dto.BranchType, ParentClientId = parentClientId
                };
                _context.ClientBranches.Add(branch2);
                foreach (var orgDto in dto.Organizations ?? new())
                    _context.Organizations.Add(NewOrganizationFromDto(orgDto, null, branch2.Id));
                foreach (var wDto in dto.ExternalWorkers ?? new())
                    _context.ExternalWorkers.Add(NewExternalWorkerFromDto(wDto, null, branch2.Id));
            }
        }
    }

    private void MergeExternalWorkers(
        ICollection<ExternalWorker> existing, List<ExternalWorkerUpdateDto>? incoming,
        Guid? clientId, Guid? branchId)
    {
        incoming ??= new();
        var existingById = existing.ToDictionary(w => w.Id);
        var incomingIds = incoming.Where(w => w.Id.HasValue).Select(w => w.Id!.Value).ToHashSet();

        foreach (var w in existing.Where(w => !incomingIds.Contains(w.Id)).ToList())
        {
            w.IsDeleted = true;
            w.DeletedAt = DateTime.UtcNow;
        }

        foreach (var dto in incoming)
        {
            if (dto.Id.HasValue && existingById.TryGetValue(dto.Id.Value, out var w))
            {
                w.Name = dto.Name;
                w.WorkerType = dto.WorkerType;
                w.ResidenceNumber = dto.ResidenceNumber ?? string.Empty;
                w.ResidenceImagePath = dto.ResidenceImagePath;
                w.ExpiryDate = dto.ExpiryDate ?? w.ExpiryDate;
            }
            else
            {
                _context.ExternalWorkers.Add(NewExternalWorkerFromDto(dto, clientId, branchId));
            }
        }
    }

    private void MergeRecords(ICollection<OrganizationRecord> existing, List<OrganizationRecordUpdateItemDto>? incoming, Guid orgId)
    {
        incoming ??= new();
        var existingById = existing.ToDictionary(r => r.Id);
        var incomingIds = incoming.Where(r => r.Id.HasValue).Select(r => r.Id!.Value).ToHashSet();
        foreach (var r in existing.Where(r => !incomingIds.Contains(r.Id)).ToList()) { r.IsDeleted = true; r.DeletedAt = DateTime.UtcNow; }
        foreach (var dto in incoming)
        {
            if (dto.Id.HasValue && existingById.TryGetValue(dto.Id.Value, out var r))
            { r.Name = dto.Name; r.Number = dto.Number; r.ExpiryDate = dto.ExpiryDate; r.ImagePath = dto.ImagePath; }
            else
            { _context.Set<OrganizationRecord>().Add(new OrganizationRecord { Id = Guid.NewGuid(), Name = dto.Name, Number = dto.Number, ExpiryDate = dto.ExpiryDate, ImagePath = dto.ImagePath, OrganizationId = orgId }); }
        }
    }

    private void MergeLicenses(ICollection<OrganizationLicense> existing, List<OrganizationLicenseUpdateItemDto>? incoming, Guid orgId)
    {
        incoming ??= new();
        var existingById = existing.ToDictionary(l => l.Id);
        var incomingIds = incoming.Where(l => l.Id.HasValue).Select(l => l.Id!.Value).ToHashSet();
        foreach (var l in existing.Where(l => !incomingIds.Contains(l.Id)).ToList()) { l.IsDeleted = true; l.DeletedAt = DateTime.UtcNow; }
        foreach (var dto in incoming)
        {
            if (dto.Id.HasValue && existingById.TryGetValue(dto.Id.Value, out var l))
            { l.Name = dto.Name; l.Number = dto.Number; l.ExpiryDate = dto.ExpiryDate; l.ImagePath = dto.ImagePath; }
            else
            { _context.Set<OrganizationLicense>().Add(new OrganizationLicense { Id = Guid.NewGuid(), Name = dto.Name, Number = dto.Number, ExpiryDate = dto.ExpiryDate, ImagePath = dto.ImagePath, OrganizationId = orgId }); }
        }
    }

    private void MergeWorkers(ICollection<OrganizationWorker> existing, List<OrganizationWorkerUpdateItemDto>? incoming, Guid orgId)
    {
        incoming ??= new();
        var existingById = existing.ToDictionary(w => w.Id);
        var incomingIds = incoming.Where(w => w.Id.HasValue).Select(w => w.Id!.Value).ToHashSet();
        foreach (var w in existing.Where(w => !incomingIds.Contains(w.Id)).ToList()) { w.IsDeleted = true; w.DeletedAt = DateTime.UtcNow; }
        foreach (var dto in incoming)
        {
            if (dto.Id.HasValue && existingById.TryGetValue(dto.Id.Value, out var w))
            { w.Name = dto.Name; w.ResidenceNumber = dto.ResidenceNumber; w.ResidenceImagePath = dto.ResidenceImagePath; w.ExpiryDate = dto.ExpiryDate; }
            else
            { _context.Set<OrganizationWorker>().Add(new OrganizationWorker { Id = Guid.NewGuid(), Name = dto.Name, ResidenceNumber = dto.ResidenceNumber, ResidenceImagePath = dto.ResidenceImagePath, ExpiryDate = dto.ExpiryDate, OrganizationId = orgId }); }
        }
    }

    private void MergeCars(ICollection<OrganizationCar> existing, List<OrganizationCarUpdateItemDto>? incoming, Guid orgId)
    {
        incoming ??= new();
        var existingById = existing.ToDictionary(c => c.Id);
        var incomingIds = incoming.Where(c => c.Id.HasValue).Select(c => c.Id!.Value).ToHashSet();
        foreach (var c in existing.Where(c => !incomingIds.Contains(c.Id)).ToList()) { c.IsDeleted = true; c.DeletedAt = DateTime.UtcNow; }
        foreach (var dto in incoming)
        {
            if (dto.Id.HasValue && existingById.TryGetValue(dto.Id.Value, out var c))
            { c.PlateNumber = dto.PlateNumber; c.Color = dto.Color; c.SerialNumber = dto.SerialNumber; c.ImagePath = dto.ImagePath; c.OperatingCardExpiry = dto.OperatingCardExpiry; }
            else
            { _context.Set<OrganizationCar>().Add(new OrganizationCar { Id = Guid.NewGuid(), PlateNumber = dto.PlateNumber, Color = dto.Color, SerialNumber = dto.SerialNumber, ImagePath = dto.ImagePath, OperatingCardExpiry = dto.OperatingCardExpiry, OrganizationId = orgId }); }
        }
    }

    private void MergeUsernames(ICollection<OrganizationUsername> existing, List<OrganizationUsernameUpdateItemDto>? incoming, Guid orgId)
    {
        incoming ??= new();
        var existingById = existing.ToDictionary(u => u.Id);
        var incomingIds = incoming.Where(u => u.Id.HasValue).Select(u => u.Id!.Value).ToHashSet();
        foreach (var u in existing.Where(u => !incomingIds.Contains(u.Id)).ToList()) { u.IsDeleted = true; u.DeletedAt = DateTime.UtcNow; }
        foreach (var dto in incoming)
        {
            if (dto.Id.HasValue && existingById.TryGetValue(dto.Id.Value, out var u))
            { u.SiteName = dto.SiteName; u.Username = dto.Username; u.Password = _passwordEncryption.Encrypt(dto.Password); }
            else
            { _context.Set<OrganizationUsername>().Add(new OrganizationUsername { Id = Guid.NewGuid(), SiteName = dto.SiteName, Username = dto.Username, Password = _passwordEncryption.Encrypt(dto.Password), OrganizationId = orgId }); }
        }
    }

    private Organization NewOrganizationFromDto(OrganizationUpdateDto dto, Guid? clientId, Guid? branchId)
    {
        var org = new Organization
        {
            Id = Guid.NewGuid(), Name = dto.Name, CardExpiringSoon = dto.CardExpiringSoon,
            ClientId = clientId, ClientBranchId = branchId
        };
        if (dto.Records?.Any() == true)
            org.Records = dto.Records.Select(r => new OrganizationRecord { Id = Guid.NewGuid(), Name = r.Name, Number = r.Number, ExpiryDate = r.ExpiryDate, ImagePath = r.ImagePath, OrganizationId = org.Id }).ToList();
        if (dto.Licenses?.Any() == true)
            org.Licenses = dto.Licenses.Select(l => new OrganizationLicense { Id = Guid.NewGuid(), Name = l.Name, Number = l.Number, ExpiryDate = l.ExpiryDate, ImagePath = l.ImagePath, OrganizationId = org.Id }).ToList();
        if (dto.Workers?.Any() == true)
            org.Workers = dto.Workers.Select(w => new OrganizationWorker { Id = Guid.NewGuid(), Name = w.Name, ResidenceNumber = w.ResidenceNumber, ResidenceImagePath = w.ResidenceImagePath, ExpiryDate = w.ExpiryDate, OrganizationId = org.Id }).ToList();
        if (dto.Cars?.Any() == true)
            org.Cars = dto.Cars.Select(c => new OrganizationCar { Id = Guid.NewGuid(), PlateNumber = c.PlateNumber, Color = c.Color, SerialNumber = c.SerialNumber, ImagePath = c.ImagePath, OperatingCardExpiry = c.OperatingCardExpiry, OrganizationId = org.Id }).ToList();
        if (dto.Usernames?.Any() == true)
            org.Usernames = dto.Usernames.Select(u => new OrganizationUsername { Id = Guid.NewGuid(), SiteName = u.SiteName, Username = u.Username, Password = _passwordEncryption.Encrypt(u.Password), OrganizationId = org.Id }).ToList();
        return org;
    }

    private ExternalWorker NewExternalWorkerFromDto(ExternalWorkerUpdateDto dto, Guid? clientId, Guid? branchId)
    {
        return new ExternalWorker
        {
            Id = Guid.NewGuid(), Name = dto.Name, WorkerType = dto.WorkerType,
            ResidenceNumber = dto.ResidenceNumber ?? string.Empty, ResidenceImagePath = dto.ResidenceImagePath,
            ExpiryDate = dto.ExpiryDate ?? DateTime.UtcNow.AddYears(1), ClientId = clientId, ClientBranchId = branchId
        };
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