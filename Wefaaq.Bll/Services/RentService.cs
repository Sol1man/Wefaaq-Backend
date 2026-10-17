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
/// Rent management service implementation (الإيجارات).
///
/// A rent is a monthly arrangement. Rather than recompute each month's due on the fly,
/// every month from the rent's start through the current month is materialized as a
/// <see cref="RentCharge"/> row the first time the page asks for it. Once written a
/// charge is frozen, so raising the agreed rent only affects months not yet recorded and
/// the history stays truthful. The admin can override any single month.
///
/// Month boundaries resolve in the business timezone (Asia/Riyadh), matching the
/// payments, costs and salaries pages.
/// </summary>
public class RentService : IRentService
{
    private readonly WefaaqContext _context;
    private readonly IMapper _mapper;
    private readonly IValidator<RentCreateDto> _createValidator;
    private readonly IValidator<RentPaymentCreateDto> _paymentValidator;
    private readonly ILogger<RentService> _logger;

    public RentService(
        WefaaqContext context,
        IMapper mapper,
        IValidator<RentCreateDto> createValidator,
        IValidator<RentPaymentCreateDto> paymentValidator,
        ILogger<RentService> logger)
    {
        _context = context;
        _mapper = mapper;
        _createValidator = createValidator;
        _paymentValidator = paymentValidator;
        _logger = logger;
    }

    // ── Reads ──────────────────────────────────────────────────────────────────

    public async Task<IEnumerable<RentRowDto>> GetByMonthAsync(int year, int month)
    {
        ValidatePeriod(year, month);
        await EnsureChargesAsync();

        var rents = await BaseQuery().ToListAsync();
        if (rents.Count == 0)
        {
            return Enumerable.Empty<RentRowDto>();
        }

        var rentIds = rents.Select(r => r.Id).ToList();

        var charges = await _context.RentCharges
            .Where(c => rentIds.Contains(c.RentId))
            .ToListAsync();

        var payments = await _context.RentPayments
            .Where(p => rentIds.Contains(p.RentId))
            .ToListAsync();

        var rows = rents
            .Select(rent => BuildRow(rent, charges, payments, year, month))
            .OrderByDescending(r => r.MonthRemaining)
            .ThenBy(r => r.AssetName)
            .ToList();

        return rows;
    }

    public async Task<RentDetailsDto?> GetByIdAsync(Guid id, int year, int month)
    {
        ValidatePeriod(year, month);
        await EnsureChargesAsync();

        var rent = await BaseQuery().FirstOrDefaultAsync(r => r.Id == id);
        if (rent == null)
        {
            return null;
        }

        var charges = await _context.RentCharges
            .Where(c => c.RentId == id)
            .ToListAsync();

        var payments = await _context.RentPayments
            .Where(p => p.RentId == id)
            .ToListAsync();

        var row = BuildRow(rent, charges, payments, year, month);

        var details = new RentDetailsDto
        {
            Year = year,
            Month = month,
            // Monthly dues, newest month first, each with what was paid against it.
            Charges = charges
                .OrderByDescending(c => c.Year)
                .ThenByDescending(c => c.Month)
                .Select(c =>
                {
                    var paid = SumPaymentsIn(payments, c.Year, c.Month);
                    return new RentChargeDto
                    {
                        Id = c.Id,
                        Year = c.Year,
                        Month = c.Month,
                        Amount = c.Amount,
                        Paid = paid,
                        Remaining = c.Amount - paid,
                        CreatedAt = c.CreatedAt,
                    };
                })
                .ToList(),
            // Payment history, newest first.
            Payments = payments
                .OrderByDescending(p => p.PaymentDate)
                .Select(p => _mapper.Map<RentPaymentDto>(p))
                .ToList(),
        };

        CopyRowInto(row, details);
        return details;
    }

    // ── Mutations ──────────────────────────────────────────────────────────────

    public async Task<RentRowDto> CreateAsync(RentCreateDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        await EnsureRenterExistsAsync(dto);

        var rent = new Rent
        {
            Id = Guid.NewGuid(),
            AssetName = dto.AssetName.Trim(),
            Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),
            MonthlyAmount = dto.MonthlyAmount,
            TargetType = dto.TargetType,
            // Only the FK matching the target type is kept, so a stale id can't leak through.
            ClientId = dto.TargetType == OperationTargetType.Client ? dto.ClientId : null,
            ClientBranchId = dto.TargetType == OperationTargetType.ClientBranch ? dto.ClientBranchId : null,
            OrganizationId = dto.TargetType == OperationTargetType.Organization ? dto.OrganizationId : null,
            ExternalPersonName = dto.TargetType == OperationTargetType.ExternalPerson
                ? dto.ExternalPersonName?.Trim()
                : null,
            ExternalPersonIdNumber = dto.TargetType == OperationTargetType.ExternalPerson
                ? dto.ExternalPersonIdNumber?.Trim()
                : null,
            StartDate = dto.StartDate ?? DateTime.UtcNow,
            IsActive = true,
        };

        _context.Rents.Add(rent);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "[Rents] Created rent {RentId} asset={Asset} monthlyAmount={Amount}",
            rent.Id, rent.AssetName, rent.MonthlyAmount);

        // Bill every month from the start date through today straight away.
        await EnsureChargesAsync();

        var created = await GetByIdAsync(rent.Id, BusinessNow().Year, BusinessNow().Month);
        return created!;
    }

    public async Task<RentRowDto?> UpdateAsync(Guid id, RentUpdateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.AssetName))
        {
            throw new ValidationException("Asset name is required (اسم الأصل مطلوب)");
        }
        if (dto.AssetName.Length > 255)
        {
            throw new ValidationException("Asset name cannot exceed 255 characters (اسم الأصل لا يمكن أن يتجاوز 255 حرف)");
        }
        if (dto.MonthlyAmount < 0m)
        {
            throw new ValidationException("Monthly amount cannot be negative (قيمة الإيجار لا يمكن أن تكون سالبة)");
        }

        var rent = await _context.Rents.FirstOrDefaultAsync(r => r.Id == id);
        if (rent == null)
        {
            return null;
        }

        rent.AssetName = dto.AssetName.Trim();
        rent.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();
        // Only months not yet materialized pick this up — past dues keep their figures.
        rent.MonthlyAmount = dto.MonthlyAmount;
        rent.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();

        var now = BusinessNow();
        return await GetByIdAsync(id, now.Year, now.Month);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var rent = await _context.Rents.FirstOrDefaultAsync(r => r.Id == id);
        if (rent == null)
        {
            return false;
        }

        var now = DateTime.UtcNow;
        rent.IsDeleted = true;
        rent.DeletedAt = now;

        // The dues and payments belong to the rent — they go with it.
        var charges = await _context.RentCharges.Where(c => c.RentId == id).ToListAsync();
        foreach (var charge in charges)
        {
            charge.IsDeleted = true;
            charge.DeletedAt = now;
        }

        var payments = await _context.RentPayments.Where(p => p.RentId == id).ToListAsync();
        foreach (var payment in payments)
        {
            payment.IsDeleted = true;
            payment.DeletedAt = now;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<RentChargeDto?> SetChargeAsync(Guid rentId, RentChargeSetDto dto)
    {
        ValidatePeriod(dto.Year, dto.Month);
        if (dto.Amount < 0m)
        {
            throw new ValidationException("Amount cannot be negative (المبلغ لا يمكن أن يكون سالباً)");
        }

        var rent = await _context.Rents.FirstOrDefaultAsync(r => r.Id == rentId);
        if (rent == null)
        {
            return null;
        }

        var charge = await _context.RentCharges
            .FirstOrDefaultAsync(c => c.RentId == rentId && c.Year == dto.Year && c.Month == dto.Month);

        if (charge == null)
        {
            charge = new RentCharge
            {
                Id = Guid.NewGuid(),
                RentId = rentId,
                Year = dto.Year,
                Month = dto.Month,
                Amount = dto.Amount,
            };
            _context.RentCharges.Add(charge);
        }
        else
        {
            charge.Amount = dto.Amount;
        }

        await _context.SaveChangesAsync();

        var payments = await _context.RentPayments.Where(p => p.RentId == rentId).ToListAsync();
        var paid = SumPaymentsIn(payments, charge.Year, charge.Month);

        return new RentChargeDto
        {
            Id = charge.Id,
            Year = charge.Year,
            Month = charge.Month,
            Amount = charge.Amount,
            Paid = paid,
            Remaining = charge.Amount - paid,
            CreatedAt = charge.CreatedAt,
        };
    }

    public async Task<RentPaymentDto> AddPaymentAsync(RentPaymentCreateDto dto)
    {
        var validationResult = await _paymentValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var rentExists = await _context.Rents.AnyAsync(r => r.Id == dto.RentId);
        if (!rentExists)
        {
            throw new ValidationException("Rent not found (الإيجار غير موجود)");
        }

        var payment = new RentPayment
        {
            Id = Guid.NewGuid(),
            RentId = dto.RentId,
            Amount = dto.Amount,
            Description = dto.Description?.Trim() ?? string.Empty,
            // Blank date → record it against the moment of creation.
            PaymentDate = dto.PaymentDate ?? DateTime.UtcNow,
        };

        _context.RentPayments.Add(payment);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "[Rents] Added payment {PaymentId} rent={RentId} amount={Amount}",
            payment.Id, payment.RentId, payment.Amount);

        return _mapper.Map<RentPaymentDto>(payment);
    }

    public async Task<bool> DeletePaymentAsync(Guid id)
    {
        var payment = await _context.RentPayments.FirstOrDefaultAsync(p => p.Id == id);
        if (payment == null)
        {
            return false;
        }

        payment.IsDeleted = true;
        payment.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    private IQueryable<Rent> BaseQuery() =>
        _context.Rents
            .Include(r => r.Client)
            .Include(r => r.ClientBranch)
            .Include(r => r.Organization);

    /// <summary>
    /// Materializes any missing monthly due, for every active rent, from the month it
    /// started through the current month. Idempotent — it only inserts gaps, so the dues
    /// already recorded (and any amount the admin overrode) are never touched.
    ///
    /// A gap backfilled long after the fact is seeded from the rent's current agreed
    /// amount, since nothing better is known; the admin can override that month.
    /// </summary>
    private async Task EnsureChargesAsync()
    {
        var now = BusinessNow();
        var currentPeriod = ToPeriod(now.Year, now.Month);

        var activeRents = await _context.Rents
            .Where(r => r.IsActive)
            .Select(r => new { r.Id, r.StartDate, r.MonthlyAmount })
            .ToListAsync();

        if (activeRents.Count == 0)
        {
            return;
        }

        var rentIds = activeRents.Select(r => r.Id).ToList();
        var existing = await _context.RentCharges
            .Where(c => rentIds.Contains(c.RentId))
            .Select(c => new { c.RentId, c.Year, c.Month })
            .ToListAsync();

        var existingKeys = existing
            .Select(c => (c.RentId, Period: ToPeriod(c.Year, c.Month)))
            .ToHashSet();

        var toAdd = new List<RentCharge>();

        foreach (var rent in activeRents)
        {
            // The start date is stored UTC; the month it falls in is a business-timezone month.
            var start = ToBusinessTime(rent.StartDate);
            var startPeriod = ToPeriod(start.Year, start.Month);

            for (var period = startPeriod; period <= currentPeriod; period++)
            {
                if (existingKeys.Contains((rent.Id, period)))
                {
                    continue;
                }

                var (year, month) = FromPeriod(period);
                toAdd.Add(new RentCharge
                {
                    Id = Guid.NewGuid(),
                    RentId = rent.Id,
                    Year = year,
                    Month = month,
                    Amount = rent.MonthlyAmount,
                });
            }
        }

        if (toAdd.Count == 0)
        {
            return;
        }

        _context.RentCharges.AddRange(toAdd);

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("[Rents] Materialized {Count} monthly due(s)", toAdd.Count);
        }
        catch (DbUpdateException ex)
        {
            // Another request billed the same month first — the unique index did its job.
            // The rows are already there, so there is nothing left to do.
            _logger.LogWarning(ex, "[Rents] Concurrent charge materialization lost the race; continuing");
            foreach (var entry in toAdd)
            {
                _context.Entry(entry).State = EntityState.Detached;
            }
        }
    }

    private RentRowDto BuildRow(
        Rent rent,
        List<RentCharge> allCharges,
        List<RentPayment> allPayments,
        int year,
        int month)
    {
        var charges = allCharges.Where(c => c.RentId == rent.Id).ToList();
        var payments = allPayments.Where(p => p.RentId == rent.Id).ToList();

        var monthDue = charges
            .Where(c => c.Year == year && c.Month == month)
            .Sum(c => c.Amount);
        var monthPaid = SumPaymentsIn(payments, year, month);

        var totalDue = charges.Sum(c => c.Amount);
        var totalPaid = payments.Sum(p => p.Amount);

        return new RentRowDto
        {
            Id = rent.Id,
            AssetName = rent.AssetName,
            Notes = rent.Notes,
            MonthlyAmount = rent.MonthlyAmount,
            IsActive = rent.IsActive,
            StartDate = rent.StartDate,
            TargetType = rent.TargetType,
            ClientId = rent.ClientId,
            ClientName = rent.Client?.Name,
            ClientBranchId = rent.ClientBranchId,
            ClientBranchName = rent.ClientBranch?.Name,
            OrganizationId = rent.OrganizationId,
            OrganizationName = rent.Organization?.Name,
            ExternalPersonName = rent.ExternalPersonName,
            ExternalPersonIdNumber = rent.ExternalPersonIdNumber,
            RenterDisplay = GetRenterDisplay(rent),
            MonthDue = monthDue,
            MonthPaid = monthPaid,
            MonthRemaining = monthDue - monthPaid,
            IsSettled = monthDue > 0m && monthPaid >= monthDue,
            TotalDue = totalDue,
            TotalPaid = totalPaid,
            TotalRemaining = totalDue - totalPaid,
            CreatedAt = rent.CreatedAt,
            UpdatedAt = rent.UpdatedAt,
        };
    }

    /// <summary>Copies the computed row figures onto a details DTO.</summary>
    private static void CopyRowInto(RentRowDto source, RentDetailsDto target)
    {
        target.Id = source.Id;
        target.AssetName = source.AssetName;
        target.Notes = source.Notes;
        target.MonthlyAmount = source.MonthlyAmount;
        target.IsActive = source.IsActive;
        target.StartDate = source.StartDate;
        target.TargetType = source.TargetType;
        target.ClientId = source.ClientId;
        target.ClientName = source.ClientName;
        target.ClientBranchId = source.ClientBranchId;
        target.ClientBranchName = source.ClientBranchName;
        target.OrganizationId = source.OrganizationId;
        target.OrganizationName = source.OrganizationName;
        target.ExternalPersonName = source.ExternalPersonName;
        target.ExternalPersonIdNumber = source.ExternalPersonIdNumber;
        target.RenterDisplay = source.RenterDisplay;
        target.MonthDue = source.MonthDue;
        target.MonthPaid = source.MonthPaid;
        target.MonthRemaining = source.MonthRemaining;
        target.IsSettled = source.IsSettled;
        target.TotalDue = source.TotalDue;
        target.TotalPaid = source.TotalPaid;
        target.TotalRemaining = source.TotalRemaining;
        target.CreatedAt = source.CreatedAt;
        target.UpdatedAt = source.UpdatedAt;
    }

    private static string GetRenterDisplay(Rent rent) => rent.TargetType switch
    {
        OperationTargetType.Client => rent.Client?.Name ?? string.Empty,
        OperationTargetType.ClientBranch => rent.ClientBranch?.Name ?? string.Empty,
        OperationTargetType.Organization => rent.Organization?.Name ?? string.Empty,
        OperationTargetType.ExternalPerson => rent.ExternalPersonName ?? string.Empty,
        _ => string.Empty,
    };

    /// <summary>Payments whose business-timezone month matches the given period.</summary>
    private static decimal SumPaymentsIn(List<RentPayment> payments, int year, int month)
    {
        return payments
            .Where(p =>
            {
                var local = ToBusinessTime(p.PaymentDate);
                return local.Year == year && local.Month == month;
            })
            .Sum(p => p.Amount);
    }

    private async Task EnsureRenterExistsAsync(RentCreateDto dto)
    {
        switch (dto.TargetType)
        {
            case OperationTargetType.Client:
                if (dto.ClientId == null || !await _context.Clients.AnyAsync(c => c.Id == dto.ClientId))
                {
                    throw new ValidationException("Client not found (العميل غير موجود)");
                }
                break;

            case OperationTargetType.ClientBranch:
                if (dto.ClientBranchId == null ||
                    !await _context.ClientBranches.AnyAsync(b => b.Id == dto.ClientBranchId))
                {
                    throw new ValidationException("Client branch not found (فرع العميل غير موجود)");
                }
                break;

            case OperationTargetType.Organization:
                if (dto.OrganizationId == null ||
                    !await _context.Organizations.AnyAsync(o => o.Id == dto.OrganizationId))
                {
                    throw new ValidationException("Organization not found (المؤسسة غير موجودة)");
                }
                break;

            case OperationTargetType.ExternalPerson:
                if (string.IsNullOrWhiteSpace(dto.ExternalPersonName))
                {
                    throw new ValidationException("Renter name is required (اسم المستأجر مطلوب)");
                }
                break;

            default:
                throw new ValidationException("Renter type is not valid (نوع المستأجر غير صالح)");
        }
    }

    // ── Period / timezone helpers ──────────────────────────────────────────────

    private static readonly TimeZoneInfo BusinessTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("Asia/Riyadh");

    private static DateTime ToBusinessTime(DateTime utc)
    {
        // EF hands these back as Kind=Utc thanks to UtcDateTimeConverter.
        var asUtc = utc.Kind == DateTimeKind.Utc
            ? utc
            : DateTime.SpecifyKind(utc, DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(asUtc, BusinessTimeZone);
    }

    private static DateTime BusinessNow() => ToBusinessTime(DateTime.UtcNow);

    /// <summary>Months since year 0, so a period is one comparable integer.</summary>
    private static int ToPeriod(int year, int month) => year * 12 + (month - 1);

    private static (int Year, int Month) FromPeriod(int period) =>
        (period / 12, period % 12 + 1);

    private static void ValidatePeriod(int year, int month)
    {
        if (month < 1 || month > 12)
        {
            throw new ValidationException("Month must be between 1 and 12 (الشهر يجب أن يكون بين 1 و 12)");
        }
        if (year < 2000 || year > 2999)
        {
            throw new ValidationException("Year is out of range (السنة خارج النطاق)");
        }
    }
}
