using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Wefaaq.Bll.DTOs;
using Wefaaq.Bll.Interfaces;
using Wefaaq.Dal;
using Wefaaq.Dal.Entities;

namespace Wefaaq.Bll.Services;

/// <summary>
/// User payment service implementation
/// </summary>
public class UserPaymentService : IUserPaymentService
{
    private readonly WefaaqContext _context;
    private readonly IMapper _mapper;
    private readonly IValidator<UserPaymentCreateDto> _createValidator;

    public UserPaymentService(
        WefaaqContext context,
        IMapper mapper,
        IValidator<UserPaymentCreateDto> createValidator)
    {
        _context = context;
        _mapper = mapper;
        _createValidator = createValidator;
    }

    public async Task<IEnumerable<UserPaymentDto>> GetAllAsync()
    {
        var payments = await _context.UserPayments
            .Include(p => p.User)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return _mapper.Map<IEnumerable<UserPaymentDto>>(payments);
    }

    public async Task<UserPaymentDto?> GetByIdAsync(Guid id)
    {
        var payment = await _context.UserPayments
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id);

        return payment == null ? null : _mapper.Map<UserPaymentDto>(payment);
    }

    public async Task<UserPaymentDto> CreateAsync(int userId, UserPaymentCreateDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var payment = new UserPayment
        {
            Id = Guid.NewGuid(),
            Amount = dto.Amount,
            Description = dto.Description,
            UserId = userId
        };

        _context.UserPayments.Add(payment);
        await _context.SaveChangesAsync();

        // Reload with user info
        var createdPayment = await _context.UserPayments
            .Include(p => p.User)
            .FirstAsync(p => p.Id == payment.Id);

        return _mapper.Map<UserPaymentDto>(createdPayment);
    }

    public async Task<IEnumerable<UserPaymentDto>> GetMyPaymentsAsync(int userId)
    {
        var payments = await _context.UserPayments
            .Include(p => p.User)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return _mapper.Map<IEnumerable<UserPaymentDto>>(payments);
    }

    public async Task<IEnumerable<UserPaymentDto>> GetPaymentsByDateRangeAsync(DateTime from, DateTime to)
    {
        var payments = await _context.UserPayments
            .Include(p => p.User)
            .Where(p => p.CreatedAt >= from && p.CreatedAt <= to)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return _mapper.Map<IEnumerable<UserPaymentDto>>(payments);
    }

    public async Task<IEnumerable<UserPaymentDto>> GetPaymentsByUserAsync(int userId)
    {
        var payments = await _context.UserPayments
            .Include(p => p.User)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return _mapper.Map<IEnumerable<UserPaymentDto>>(payments);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var payment = await _context.UserPayments.FindAsync(id);
        if (payment == null)
        {
            return false;
        }

        // Soft delete
        payment.IsDeleted = true;
        payment.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<decimal> GetTotalAmountByDateRangeAsync(DateTime from, DateTime to)
    {
        return await _context.UserPayments
            .Where(p => p.CreatedAt >= from && p.CreatedAt <= to)
            .SumAsync(p => p.Amount);
    }

    public async Task<decimal> GetTotalAmountByUserAsync(int userId)
    {
        return await _context.UserPayments
            .Where(p => p.UserId == userId)
            .SumAsync(p => p.Amount);
    }
}
