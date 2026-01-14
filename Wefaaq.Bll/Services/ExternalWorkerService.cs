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
/// Service for external worker operations
/// </summary>
public class ExternalWorkerService : IExternalWorkerService
{
    private readonly IGenericRepository<ExternalWorker> _workerRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IGenericRepository<ClientBranch> _branchRepository;
    private readonly WefaaqContext _context;
    private readonly IMapper _mapper;
    private readonly IValidator<ExternalWorkerCreateDto> _createValidator;
    private readonly IValidator<ExternalWorkerUpdateDto> _updateValidator;

    public ExternalWorkerService(
        IGenericRepository<ExternalWorker> workerRepository,
        IClientRepository clientRepository,
        IGenericRepository<ClientBranch> branchRepository,
        WefaaqContext context,
        IMapper mapper,
        IValidator<ExternalWorkerCreateDto> createValidator,
        IValidator<ExternalWorkerUpdateDto> updateValidator)
    {
        _workerRepository = workerRepository;
        _clientRepository = clientRepository;
        _branchRepository = branchRepository;
        _context = context;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IEnumerable<ExternalWorkerDto>> GetAllAsync()
    {
        var workers = await _context.ExternalWorkers
            .Include(w => w.Client)
            .Include(w => w.ClientBranch)
            .ToListAsync();

        return _mapper.Map<IEnumerable<ExternalWorkerDto>>(workers);
    }

    public async Task<ExternalWorkerDto?> GetByIdAsync(Guid id)
    {
        var worker = await _context.ExternalWorkers
            .Include(w => w.Client)
            .Include(w => w.ClientBranch)
            .FirstOrDefaultAsync(w => w.Id == id);

        return worker == null ? null : _mapper.Map<ExternalWorkerDto>(worker);
    }

    public async Task<IEnumerable<ExternalWorkerDto>> GetByClientIdAsync(Guid clientId)
    {
        var workers = await _context.ExternalWorkers
            .Include(w => w.Client)
            .Include(w => w.ClientBranch)
            .Where(w => w.ClientId == clientId)
            .ToListAsync();

        return _mapper.Map<IEnumerable<ExternalWorkerDto>>(workers);
    }

    public async Task<IEnumerable<ExternalWorkerDto>> GetByClientBranchIdAsync(Guid branchId)
    {
        var workers = await _context.ExternalWorkers
            .Include(w => w.Client)
            .Include(w => w.ClientBranch)
            .Where(w => w.ClientBranchId == branchId)
            .ToListAsync();

        return _mapper.Map<IEnumerable<ExternalWorkerDto>>(workers);
    }

    public async Task<ExternalWorkerDto> CreateAsync(ExternalWorkerCreateDto workerCreateDto)
    {
        var validationResult = await _createValidator.ValidateAsync(workerCreateDto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // Verify exactly one of ClientId or ClientBranchId is set
        if (!workerCreateDto.ClientId.HasValue && !workerCreateDto.ClientBranchId.HasValue)
        {
            throw new InvalidOperationException("External worker must belong to either a Client or a ClientBranch");
        }
        if (workerCreateDto.ClientId.HasValue && workerCreateDto.ClientBranchId.HasValue)
        {
            throw new InvalidOperationException("External worker cannot belong to both a Client and a ClientBranch");
        }

        // Verify client or branch exists
        if (workerCreateDto.ClientId.HasValue)
        {
            var client = await _clientRepository.GetByIdAsync(workerCreateDto.ClientId.Value);
            if (client == null)
            {
                throw new InvalidOperationException($"Client with ID {workerCreateDto.ClientId} not found");
            }
        }
        else if (workerCreateDto.ClientBranchId.HasValue)
        {
            var branch = await _branchRepository.GetByIdAsync(workerCreateDto.ClientBranchId.Value);
            if (branch == null)
            {
                throw new InvalidOperationException($"Client branch with ID {workerCreateDto.ClientBranchId} not found");
            }
        }

        var worker = _mapper.Map<ExternalWorker>(workerCreateDto);
        worker.Id = Guid.NewGuid();

        var createdWorker = await _workerRepository.AddAsync(worker);
        return _mapper.Map<ExternalWorkerDto>(createdWorker);
    }

    public async Task<ExternalWorkerDto?> UpdateAsync(Guid id, ExternalWorkerUpdateDto workerUpdateDto)
    {
        var validationResult = await _updateValidator.ValidateAsync(workerUpdateDto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var existingWorker = await _workerRepository.GetByIdAsync(id);
        if (existingWorker == null)
        {
            return null;
        }

        // Verify exactly one of ClientId or ClientBranchId is set
        if (!workerUpdateDto.ClientId.HasValue && !workerUpdateDto.ClientBranchId.HasValue)
        {
            throw new InvalidOperationException("External worker must belong to either a Client or a ClientBranch");
        }
        if (workerUpdateDto.ClientId.HasValue && workerUpdateDto.ClientBranchId.HasValue)
        {
            throw new InvalidOperationException("External worker cannot belong to both a Client and a ClientBranch");
        }

        // Verify client or branch exists
        if (workerUpdateDto.ClientId.HasValue)
        {
            var client = await _clientRepository.GetByIdAsync(workerUpdateDto.ClientId.Value);
            if (client == null)
            {
                throw new InvalidOperationException($"Client with ID {workerUpdateDto.ClientId} not found");
            }
        }
        else if (workerUpdateDto.ClientBranchId.HasValue)
        {
            var branch = await _branchRepository.GetByIdAsync(workerUpdateDto.ClientBranchId.Value);
            if (branch == null)
            {
                throw new InvalidOperationException($"Client branch with ID {workerUpdateDto.ClientBranchId} not found");
            }
        }

        _mapper.Map(workerUpdateDto, existingWorker);

        var updatedWorker = await _workerRepository.UpdateAsync(existingWorker);
        return _mapper.Map<ExternalWorkerDto>(updatedWorker);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _workerRepository.DeleteAsync(id);
    }
}
