using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Wefaaq.Bll.DTOs;
using Wefaaq.Bll.Interfaces;
using Wefaaq.Dal;
using Wefaaq.Dal.Entities;

namespace Wefaaq.Bll.Services;

/// <summary>
/// Business cost / expense service implementation (المصروفات)
/// </summary>
public class CostService : ICostService
{
    private readonly WefaaqContext _context;
    private readonly IMapper _mapper;
    private readonly IValidator<CostCreateDto> _createValidator;
    private readonly ILogger<CostService> _logger;

    public CostService(
        WefaaqContext context,
        IMapper mapper,
        IValidator<CostCreateDto> createValidator,
        ILogger<CostService> logger)
    {
        _context = context;
        _mapper = mapper;
        _createValidator = createValidator;
        _logger = logger;
    }

    public async Task<IEnumerable<CostDto>> GetAllAsync()
    {
        var costs = await _context.Costs
            .OrderByDescending(c => c.CostDate)
            .ToListAsync();

        return _mapper.Map<IEnumerable<CostDto>>(costs);
    }

    public async Task<IEnumerable<CostDto>> GetByDateRangeAsync(DateTime from, DateTime to)
    {
        var costs = await _context.Costs
            .Where(c => c.CostDate >= from && c.CostDate <= to)
            .OrderByDescending(c => c.CostDate)
            .ToListAsync();

        return _mapper.Map<IEnumerable<CostDto>>(costs);
    }

    public async Task<CostDto> CreateAsync(CostCreateDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var cost = new Cost
        {
            Id = Guid.NewGuid(),
            Amount = dto.Amount,
            Description = dto.Description ?? string.Empty,
            // Blank date → record it against the moment of creation.
            CostDate = dto.CostDate ?? DateTime.UtcNow
        };

        _context.Costs.Add(cost);
        await _context.SaveChangesAsync();

        _logger.LogInformation("[Costs] Created cost {CostId} amount={Amount}", cost.Id, cost.Amount);

        return _mapper.Map<CostDto>(cost);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var cost = await _context.Costs.FindAsync(id);
        if (cost == null || cost.IsDeleted)
        {
            return false;
        }

        cost.IsDeleted = true;
        cost.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }
}
