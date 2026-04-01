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
/// Service for client branch operations
/// </summary>
public class ClientBranchService : IClientBranchService
{
    private readonly IGenericRepository<ClientBranch> _branchRepository;
    private readonly IClientRepository _clientRepository;
    private readonly WefaaqContext _context;
    private readonly IMapper _mapper;
    private readonly IValidator<ClientBranchCreateDto> _createValidator;
    private readonly IValidator<ClientBranchUpdateDto> _updateValidator;
    private readonly IPasswordEncryptionService _passwordEncryption;

    public ClientBranchService(
        IGenericRepository<ClientBranch> branchRepository,
        IClientRepository clientRepository,
        WefaaqContext context,
        IMapper mapper,
        IValidator<ClientBranchCreateDto> createValidator,
        IValidator<ClientBranchUpdateDto> updateValidator,
        IPasswordEncryptionService passwordEncryption)
    {
        _branchRepository = branchRepository;
        _clientRepository = clientRepository;
        _context = context;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _passwordEncryption = passwordEncryption;
    }

    public async Task<IEnumerable<ClientBranchDto>> GetAllAsync()
    {
        var branches = await _context.ClientBranches
            .Include(b => b.ParentClient)
            .Include(b => b.Organizations)
            .Include(b => b.ExternalWorkers)
            .ToListAsync();

        return _mapper.Map<IEnumerable<ClientBranchDto>>(branches);
    }

    public async Task<ClientBranchDto?> GetByIdAsync(Guid id)
    {
        var branch = await _context.ClientBranches
            .Include(b => b.ParentClient)
            .FirstOrDefaultAsync(b => b.Id == id);

        return branch == null ? null : _mapper.Map<ClientBranchDto>(branch);
    }

    public async Task<ClientBranchDto?> GetWithDetailsAsync(Guid id)
    {
        var branch = await _context.ClientBranches
            .Include(b => b.ParentClient)
            .Include(b => b.Organizations)
            .Include(b => b.ExternalWorkers)
            .FirstOrDefaultAsync(b => b.Id == id);

        return branch == null ? null : _mapper.Map<ClientBranchDto>(branch);
    }

    public async Task<IEnumerable<ClientBranchDto>> GetByClientIdAsync(Guid clientId)
    {
        var branches = await _context.ClientBranches
            .Include(b => b.ParentClient)
            .Include(b => b.Organizations)
            .Include(b => b.ExternalWorkers)
            .Where(b => b.ParentClientId == clientId)
            .ToListAsync();

        return _mapper.Map<IEnumerable<ClientBranchDto>>(branches);
    }

    public async Task<ClientBranchDto> CreateAsync(ClientBranchCreateDto branchCreateDto)
    {
        var validationResult = await _createValidator.ValidateAsync(branchCreateDto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // ParentClientId is required for standalone branch creation
        if (!branchCreateDto.ParentClientId.HasValue)
        {
            throw new InvalidOperationException("Parent client ID is required when creating a branch directly");
        }

        // Verify parent client exists
        var parentClient = await _clientRepository.GetByIdAsync(branchCreateDto.ParentClientId.Value);
        if (parentClient == null)
        {
            throw new InvalidOperationException($"Parent client with ID {branchCreateDto.ParentClientId} not found");
        }

        var branch = _mapper.Map<ClientBranch>(branchCreateDto);
        branch.Id = Guid.NewGuid();
        branch.ParentClientId = branchCreateDto.ParentClientId.Value;

        var createdBranch = await _branchRepository.AddAsync(branch);
        return _mapper.Map<ClientBranchDto>(createdBranch);
    }

    public async Task<ClientBranchDto?> UpdateAsync(Guid id, ClientBranchUpdateDto branchUpdateDto)
    {
        var validationResult = await _updateValidator.ValidateAsync(branchUpdateDto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var existingBranch = await _branchRepository.GetByIdAsync(id);
        if (existingBranch == null)
        {
            return null;
        }

        // Verify parent client exists
        var parentClient = await _clientRepository.GetByIdAsync(branchUpdateDto.ParentClientId);
        if (parentClient == null)
        {
            throw new InvalidOperationException($"Parent client with ID {branchUpdateDto.ParentClientId} not found");
        }

        _mapper.Map(branchUpdateDto, existingBranch);

        var updatedBranch = await _branchRepository.UpdateAsync(existingBranch);
        return _mapper.Map<ClientBranchDto>(updatedBranch);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _branchRepository.DeleteAsync(id);
    }

    // ===== GRANULAR OPERATIONS (Add items to branch) =====

    public async Task<OrganizationDto> AddOrganizationToBranchAsync(Guid branchId, OrganizationCreateDto organizationDto)
    {
        // Verify branch exists
        var branch = await _branchRepository.GetByIdAsync(branchId);
        if (branch == null)
        {
            throw new InvalidOperationException($"Branch with ID {branchId} not found");
        }

        // Create organization with full nested entities support
        var organization = new Organization
        {
            Id = Guid.NewGuid(),
            Name = organizationDto.Name,
            CardExpiringSoon = organizationDto.CardExpiringSoon,
            ClientId = null,  // Belongs to branch, not client
            ClientBranchId = branchId
        };

        // Create organization records
        if (organizationDto.Records != null && organizationDto.Records.Any())
        {
            organization.Records = organizationDto.Records.Select(recordDto => new OrganizationRecord
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
        if (organizationDto.Licenses != null && organizationDto.Licenses.Any())
        {
            organization.Licenses = organizationDto.Licenses.Select(licenseDto => new OrganizationLicense
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
        if (organizationDto.Workers != null && organizationDto.Workers.Any())
        {
            organization.Workers = organizationDto.Workers.Select(workerDto => new OrganizationWorker
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
        if (organizationDto.Cars != null && organizationDto.Cars.Any())
        {
            organization.Cars = organizationDto.Cars.Select(carDto => new OrganizationCar
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
        if (organizationDto.Usernames != null && organizationDto.Usernames.Any())
        {
            organization.Usernames = organizationDto.Usernames.Select(usernameDto => new OrganizationUsername
            {
                Id = Guid.NewGuid(),
                SiteName = usernameDto.SiteName,
                Username = usernameDto.Username,
                Password = _passwordEncryption.Encrypt(usernameDto.Password),
                OrganizationId = organization.Id
            }).ToList();
        }

        _context.Organizations.Add(organization);
        await _context.SaveChangesAsync();

        // Reload with all nested entities
        var organizationWithDetails = await _context.Organizations
            .Include(o => o.Records)
            .Include(o => o.Licenses)
            .Include(o => o.Workers)
            .Include(o => o.Cars)
            .Include(o => o.Usernames)
            .FirstOrDefaultAsync(o => o.Id == organization.Id);

        var orgDto = _mapper.Map<OrganizationDto>(organizationWithDetails);
        DecryptPasswordsInOrganizationDto(orgDto);
        return orgDto;
    }

    public async Task<ExternalWorkerDto> AddExternalWorkerToBranchAsync(Guid branchId, ExternalWorkerCreateDto workerDto)
    {
        // Verify branch exists
        var branch = await _branchRepository.GetByIdAsync(branchId);
        if (branch == null)
        {
            throw new InvalidOperationException($"Branch with ID {branchId} not found");
        }

        // Create external worker
        var worker = new ExternalWorker
        {
            Id = Guid.NewGuid(),
            Name = workerDto.Name,
            WorkerType = workerDto.WorkerType,
            ResidenceNumber = workerDto.ResidenceNumber ?? string.Empty,
            ResidenceImagePath = workerDto.ResidenceImagePath,
            ExpiryDate = workerDto.ExpiryDate ?? DateTime.UtcNow.AddYears(1), // Default to 1 year from now if not specified
            ClientId = null,  // Belongs to branch, not client
            ClientBranchId = branchId
        };

        _context.ExternalWorkers.Add(worker);
        await _context.SaveChangesAsync();

        return _mapper.Map<ExternalWorkerDto>(worker);
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
