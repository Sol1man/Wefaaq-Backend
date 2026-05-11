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

        // If a Profit row carries a RelatedPaymentId, ensure it points at a real Payment entry on the same user.
        if (dto.Type == UserPaymentType.Profit && dto.RelatedPaymentId.HasValue)
        {
            var related = await _context.UserPayments
                .FirstOrDefaultAsync(p => p.Id == dto.RelatedPaymentId.Value);

            if (related == null)
            {
                throw new ValidationException("Related payment was not found (الدفعة المرتبطة غير موجودة)");
            }
            if (related.Type != UserPaymentType.Payment)
            {
                throw new ValidationException("Related entry must be a Payment, not a Profit (الإدخال المرتبط يجب أن يكون دفع وليس ربح)");
            }
            if (related.UserId != userId)
            {
                throw new ValidationException("Related payment must belong to the same user (الدفعة المرتبطة يجب أن تكون لنفس المستخدم)");
            }
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new ValidationException("User not found (المستخدم غير موجود)");

        var payment = new UserPayment
        {
            Id = Guid.NewGuid(),
            Amount = dto.Amount,
            Description = dto.Description,
            Type = dto.Type,
            RelatedPaymentId = dto.Type == UserPaymentType.Profit ? dto.RelatedPaymentId : null,
            UserId = userId
        };

        _context.UserPayments.Add(payment);

        // Only Payment-type entries deduct from the running account balance; Profit is purely informational.
        if (payment.Type == UserPaymentType.Payment)
        {
            user.CurrentAccountAmount -= payment.Amount;
        }

        await _context.SaveChangesAsync();

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

    public async Task<IEnumerable<UserPaymentDto>> GetPaymentsByUserAndDateRangeAsync(int userId, DateTime from, DateTime to)
    {
        var payments = await _context.UserPayments
            .Include(p => p.User)
            .Where(p => p.UserId == userId && p.CreatedAt >= from && p.CreatedAt <= to)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return _mapper.Map<IEnumerable<UserPaymentDto>>(payments);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var payment = await _context.UserPayments.FindAsync(id);
        if (payment == null || payment.IsDeleted)
        {
            return false;
        }

        // Reverse the balance impact of a deleted Payment so the running total stays consistent.
        if (payment.Type == UserPaymentType.Payment)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == payment.UserId);
            if (user != null)
            {
                user.CurrentAccountAmount += payment.Amount;
            }
        }

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

    public async Task<IEnumerable<UserPaymentSummaryDto>> GetUserSummariesAsync()
    {
        var now = DateTime.UtcNow;
        var todayStart = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
        var todayEnd = todayStart.AddDays(1);
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1);

        // Aggregate per user. Only Payment-type rows contribute to today/month totals (profit is excluded).
        var users = await _context.Users
            .Select(u => new UserPaymentSummaryDto
            {
                UserId = u.Id,
                UserName = u.Name ?? string.Empty,
                UserEmail = u.Email,
                InitialAccountAmount = u.InitialAccountAmount,
                CurrentAccountAmount = u.CurrentAccountAmount,
                TodaysPayments = _context.UserPayments
                    .Where(p => p.UserId == u.Id
                        && p.Type == UserPaymentType.Payment
                        && p.CreatedAt >= todayStart && p.CreatedAt < todayEnd)
                    .Sum(p => (decimal?)p.Amount) ?? 0m,
                TodaysProfit = _context.UserPayments
                    .Where(p => p.UserId == u.Id
                        && p.Type == UserPaymentType.Profit
                        && p.CreatedAt >= todayStart && p.CreatedAt < todayEnd)
                    .Sum(p => (decimal?)p.Amount) ?? 0m,
                CurrentMonthPayments = _context.UserPayments
                    .Where(p => p.UserId == u.Id
                        && p.Type == UserPaymentType.Payment
                        && p.CreatedAt >= monthStart && p.CreatedAt < monthEnd)
                    .Sum(p => (decimal?)p.Amount) ?? 0m
            })
            .ToListAsync();

        return users;
    }

    public async Task<UserDto?> SetInitialAccountAmountAsync(int userId, decimal newInitialAmount)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return null;
        }

        // The admin sets the daily seed but Payment deductions made earlier today should still count —
        // current = newInitial − sum of Payment-type entries since today's start.
        var now = DateTime.UtcNow;
        var todayStart = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);

        var todaysSpend = await _context.UserPayments
            .Where(p => p.UserId == userId
                && p.Type == UserPaymentType.Payment
                && p.CreatedAt >= todayStart)
            .Select(p => (decimal?)p.Amount)
            .SumAsync() ?? 0m;

        user.InitialAccountAmount = newInitialAmount;
        user.CurrentAccountAmount = newInitialAmount - todaysSpend;
        await _context.SaveChangesAsync();

        return _mapper.Map<UserDto>(user);
    }
}
