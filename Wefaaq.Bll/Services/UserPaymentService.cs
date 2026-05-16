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
/// User payment service implementation
/// </summary>
public class UserPaymentService : IUserPaymentService
{
    private readonly WefaaqContext _context;
    private readonly IMapper _mapper;
    private readonly IValidator<UserPaymentCreateDto> _createValidator;
    private readonly ILogger<UserPaymentService> _logger;

    public UserPaymentService(
        WefaaqContext context,
        IMapper mapper,
        IValidator<UserPaymentCreateDto> createValidator,
        ILogger<UserPaymentService> logger)
    {
        _context = context;
        _mapper = mapper;
        _createValidator = createValidator;
        _logger = logger;
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
        _logger.LogInformation(
            "[UserPayments] CreateAsync ENTRY userId={UserId} amount={Amount} descriptionLength={DescLen}",
            userId, dto.Amount, dto.Description?.Length ?? 0);

        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning(
                "[UserPayments] CreateAsync VALIDATION_FAILED userId={UserId} errors={Errors}",
                userId, string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));
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
            Description = dto.Description ?? string.Empty,
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

        _logger.LogInformation(
            "[UserPayments] CreateAsync SAVED paymentId={PaymentId} userId={UserId} amount={Amount}",
            payment.Id, userId, payment.Amount);

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

        // Reverse the balance impact of the deleted row so cumulative totals stay consistent.
        // Payment: was a deduction → refund Current. Profit: no balance effect → no reversal.
        // Initial: was a top-up → subtract from both Initial and Current.
        if (payment.Type == UserPaymentType.Payment || payment.Type == UserPaymentType.Initial)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == payment.UserId);
            if (user != null)
            {
                if (payment.Type == UserPaymentType.Payment)
                {
                    user.CurrentAccountAmount += payment.Amount;
                }
                else // Initial
                {
                    user.InitialAccountAmount -= payment.Amount;
                    user.CurrentAccountAmount -= payment.Amount;
                }
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
        // The "today" and "current month" cards must honor the user's business timezone
        // (Asia/Riyadh, no DST). Computing boundaries against UTC midnight would shift the
        // visible window by 3 hours and make the cards disagree with the frontend "Today"
        // filter and with the user's wall clock. Storage stays UTC; only the boundary math
        // needs the zone applied here.
        var riyadh = TimeZoneInfo.FindSystemTimeZoneById("Asia/Riyadh");
        var nowRiyadh = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, riyadh);
        var todayStartRiyadh = new DateTime(nowRiyadh.Year, nowRiyadh.Month, nowRiyadh.Day, 0, 0, 0);
        var monthStartRiyadh = new DateTime(nowRiyadh.Year, nowRiyadh.Month, 1, 0, 0, 0);

        var todayStart = TimeZoneInfo.ConvertTimeToUtc(todayStartRiyadh, riyadh);
        var todayEnd = TimeZoneInfo.ConvertTimeToUtc(todayStartRiyadh.AddDays(1), riyadh);
        var monthStart = TimeZoneInfo.ConvertTimeToUtc(monthStartRiyadh, riyadh);
        var monthEnd = TimeZoneInfo.ConvertTimeToUtc(monthStartRiyadh.AddMonths(1), riyadh);

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
                    .Sum(p => (decimal?)p.Amount) ?? 0m,
                CurrentMonthProfit = _context.UserPayments
                    .Where(p => p.UserId == u.Id
                        && p.Type == UserPaymentType.Profit
                        && p.CreatedAt >= monthStart && p.CreatedAt < monthEnd)
                    .Sum(p => (decimal?)p.Amount) ?? 0m
            })
            .ToListAsync();

        return users;
    }

    public async Task<IEnumerable<UserPaymentDto>> CreateOperationAsync(int userId, UserPaymentOperationCreateDto dto)
    {
        var payment = (dto.PaymentAmount ?? 0m) > 0m ? dto.PaymentAmount!.Value : (decimal?)null;
        var profit = (dto.ProfitAmount ?? 0m) > 0m ? dto.ProfitAmount!.Value : (decimal?)null;

        if (payment == null && profit == null)
        {
            throw new ValidationException("At least one of payment amount or profit amount must be greater than zero (يجب أن يكون أحد المبلغين أكبر من صفر)");
        }

        var description = dto.Description ?? string.Empty;
        if (description.Length > 500)
        {
            throw new ValidationException("Description cannot exceed 500 characters (الوصف لا يمكن أن يتجاوز 500 حرف)");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new ValidationException("User not found (المستخدم غير موجود)");

        var created = new List<UserPayment>();
        UserPayment? paymentRow = null;

        if (payment.HasValue)
        {
            paymentRow = new UserPayment
            {
                Id = Guid.NewGuid(),
                Amount = payment.Value,
                Description = description,
                Type = UserPaymentType.Payment,
                UserId = userId
            };
            _context.UserPayments.Add(paymentRow);
            user.CurrentAccountAmount -= payment.Value;
            created.Add(paymentRow);
        }

        if (profit.HasValue)
        {
            var profitRow = new UserPayment
            {
                Id = Guid.NewGuid(),
                Amount = profit.Value,
                Description = description,
                Type = UserPaymentType.Profit,
                // Link the profit to the payment when both are submitted together so they stay associated.
                RelatedPaymentId = paymentRow?.Id,
                UserId = userId
            };
            _context.UserPayments.Add(profitRow);
            created.Add(profitRow);
        }

        await _context.SaveChangesAsync();

        var ids = created.Select(c => c.Id).ToList();
        var reloaded = await _context.UserPayments
            .Include(p => p.User)
            .Where(p => ids.Contains(p.Id))
            .ToListAsync();

        return _mapper.Map<IEnumerable<UserPaymentDto>>(reloaded);
    }

    public async Task<UserDto?> SetInitialAccountAmountAsync(int userId, decimal amountToAdd, string? description = null)
    {
        if (amountToAdd <= 0)
        {
            throw new ValidationException("Top-up amount must be greater than zero (مبلغ الإضافة يجب أن يكون أكبر من صفر)");
        }

        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return null;
        }

        // Cumulative: the entered amount is added to the existing balances (it does NOT replace them).
        // We also log a UserPayment row of Type=Initial so the top-up appears in the payment history
        // alongside Payments and Profits, making the running balance fully traceable.
        var topup = new UserPayment
        {
            Id = Guid.NewGuid(),
            Amount = amountToAdd,
            Description = description ?? string.Empty,
            Type = UserPaymentType.Initial,
            UserId = userId
        };
        _context.UserPayments.Add(topup);

        user.InitialAccountAmount += amountToAdd;
        user.CurrentAccountAmount += amountToAdd;

        await _context.SaveChangesAsync();

        return _mapper.Map<UserDto>(user);
    }
}
