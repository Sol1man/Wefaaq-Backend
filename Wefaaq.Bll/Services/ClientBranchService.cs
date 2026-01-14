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

    public ClientBranchService(
        IGenericRepository<ClientBranch> branchRepository,
        IClientRepository clientRepository,
        WefaaqContext context,
        IMapper mapper,
        IValidator<ClientBranchCreateDto> createValidator,
        IValidator<ClientBranchUpdateDto> updateValidator)
    {
        _branchRepository = branchRepository;
        _clientRepository = clientRepository;
        _context = context;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
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

        // Verify parent client exists
        var parentClient = await _clientRepository.GetByIdAsync(branchCreateDto.ParentClientId);
        if (parentClient == null)
        {
            throw new InvalidOperationException($"Parent client with ID {branchCreateDto.ParentClientId} not found");
        }

        var branch = _mapper.Map<ClientBranch>(branchCreateDto);
        branch.Id = Guid.NewGuid();

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
}
