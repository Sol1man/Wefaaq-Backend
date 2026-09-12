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
/// Employee salaries service implementation (رواتب الموظفين).
///
/// An "employee" is either an active system user or an ExternalEmployee — external
/// employees are payroll-only rows with no account, no email and no role. Every figure
/// on the page is scoped to one calendar month, with the month boundaries resolved in
/// the business timezone (Asia/Riyadh) so they agree with the payments/costs pages.
/// </summary>
public class EmployeeSalaryService : IEmployeeSalaryService
{
    private readonly WefaaqContext _context;
    private readonly IMapper _mapper;
    private readonly IValidator<ExternalEmployeeCreateDto> _externalValidator;
    private readonly IValidator<EmployeeDeductionCreateDto> _deductionValidator;
    private readonly IValidator<EmployeeLoanCreateDto> _loanValidator;
    private readonly ILogger<EmployeeSalaryService> _logger;

    public EmployeeSalaryService(
        WefaaqContext context,
        IMapper mapper,
        IValidator<ExternalEmployeeCreateDto> externalValidator,
        IValidator<EmployeeDeductionCreateDto> deductionValidator,
        IValidator<EmployeeLoanCreateDto> loanValidator,
        ILogger<EmployeeSalaryService> logger)
    {
        _context = context;
        _mapper = mapper;
        _externalValidator = externalValidator;
        _deductionValidator = deductionValidator;
        _loanValidator = loanValidator;
        _logger = logger;
    }

    public async Task<IEnumerable<EmployeeSalaryRowDto>> GetMonthlyAsync(int year, int month)
    {
        var (monthStart, monthEnd) = MonthBounds(year, month);

        // System users — profit comes from their own Profit-type payment rows for the month.
        // Projected into anonymous types first so the id-to-string conversion happens in memory,
        // not in SQL.
        var users = await _context.Users
            .Where(u => u.IsActive)
            .Select(u => new
            {
                u.Id,
                u.Name,
                u.Email,
                u.Salary,
                u.ProfitPercentage,
                ProfitBase = _context.UserPayments
                    .Where(p => p.UserId == u.Id
                        && p.Type == UserPaymentType.Profit
                        && p.CreatedAt >= monthStart && p.CreatedAt < monthEnd)
                    .Sum(p => (decimal?)p.Amount) ?? 0m,
                Deduction = _context.EmployeeDeductions
                    .Where(d => d.UserId == u.Id
                        && d.DeductionDate >= monthStart && d.DeductionDate < monthEnd)
                    .Sum(d => (decimal?)d.Amount) ?? 0m,
                Loan = _context.EmployeeLoans
                    .Where(l => l.UserId == u.Id
                        && l.LoanDate >= monthStart && l.LoanDate < monthEnd)
                    .Sum(l => (decimal?)l.Amount) ?? 0m
            })
            .ToListAsync();

        var userRows = users.Select(u => new EmployeeSalaryRowDto
        {
            Type = EmployeeRefType.User,
            Id = u.Id.ToString(),
            Name = u.Name ?? u.Email,
            Email = u.Email,
            Salary = u.Salary,
            ProfitPercentage = u.ProfitPercentage,
            ProfitBase = u.ProfitBase,
            Deduction = u.Deduction,
            Loan = u.Loan
        });

        // External employees — payroll only, so no profit history to draw on.
        var externals = await _context.ExternalEmployees
            .Select(e => new
            {
                e.Id,
                e.Name,
                e.Salary,
                Deduction = _context.EmployeeDeductions
                    .Where(d => d.ExternalEmployeeId == e.Id
                        && d.DeductionDate >= monthStart && d.DeductionDate < monthEnd)
                    .Sum(d => (decimal?)d.Amount) ?? 0m,
                Loan = _context.EmployeeLoans
                    .Where(l => l.ExternalEmployeeId == e.Id
                        && l.LoanDate >= monthStart && l.LoanDate < monthEnd)
                    .Sum(l => (decimal?)l.Amount) ?? 0m
            })
            .ToListAsync();

        var externalRows = externals.Select(e => new EmployeeSalaryRowDto
        {
            Type = EmployeeRefType.External,
            Id = e.Id.ToString(),
            Name = e.Name,
            Email = null,
            Salary = e.Salary,
            ProfitPercentage = 0m,
            ProfitBase = 0m,
            Deduction = e.Deduction,
            Loan = e.Loan
        });

        var rows = userRows.Concat(externalRows).ToList();
        foreach (var row in rows)
        {
            FillDerived(row);
        }

        return rows.OrderBy(r => r.Name).ToList();
    }

    public async Task<EmployeeSalaryDetailsDto?> GetDetailsAsync(string type, string id, int year, int month)
    {
        var (monthStart, monthEnd) = MonthBounds(year, month);
        EmployeeSalaryDetailsDto details;

        if (IsExternal(type))
        {
            var employeeId = ParseExternalId(id);
            var employee = await _context.ExternalEmployees.FirstOrDefaultAsync(e => e.Id == employeeId);
            if (employee == null)
            {
                return null;
            }

            details = new EmployeeSalaryDetailsDto
            {
                Type = EmployeeRefType.External,
                Id = employee.Id.ToString(),
                Name = employee.Name,
                Salary = employee.Salary
            };
        }
        else
        {
            var userId = ParseUserId(id);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return null;
            }

            details = new EmployeeSalaryDetailsDto
            {
                Type = EmployeeRefType.User,
                Id = user.Id.ToString(),
                Name = user.Name ?? user.Email,
                Email = user.Email,
                Salary = user.Salary,
                ProfitPercentage = user.ProfitPercentage,
                ProfitBase = await _context.UserPayments
                    .Where(p => p.UserId == user.Id
                        && p.Type == UserPaymentType.Profit
                        && p.CreatedAt >= monthStart && p.CreatedAt < monthEnd)
                    .SumAsync(p => (decimal?)p.Amount) ?? 0m
            };
        }

        details.Year = year;
        details.Month = month;

        // Deductions for the month, latest first.
        var deductions = await DeductionsQuery(details.Type, details.Id)
            .Where(d => d.DeductionDate >= monthStart && d.DeductionDate < monthEnd)
            .OrderByDescending(d => d.DeductionDate)
            .ToListAsync();

        details.Deductions = _mapper.Map<List<EmployeeDeductionDto>>(deductions);
        details.Deduction = deductions.Sum(d => d.Amount);

        // Loans for the month, latest first — a separate history from deductions.
        var loans = await LoansQuery(details.Type, details.Id)
            .Where(l => l.LoanDate >= monthStart && l.LoanDate < monthEnd)
            .OrderByDescending(l => l.LoanDate)
            .ToListAsync();

        details.Loans = _mapper.Map<List<EmployeeLoanDto>>(loans);
        details.Loan = loans.Sum(l => l.Amount);

        FillDerived(details);

        return details;
    }

    public async Task<ExternalEmployeeDto> CreateExternalAsync(ExternalEmployeeCreateDto dto)
    {
        var validationResult = await _externalValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var employee = new ExternalEmployee
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            Salary = dto.Salary
        };

        _context.ExternalEmployees.Add(employee);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "[EmployeeSalaries] Created external employee {EmployeeId} name={Name}", employee.Id, employee.Name);

        return _mapper.Map<ExternalEmployeeDto>(employee);
    }

    public async Task<bool> DeleteExternalAsync(Guid id)
    {
        var employee = await _context.ExternalEmployees.FirstOrDefaultAsync(e => e.Id == id);
        if (employee == null)
        {
            return false;
        }

        employee.IsDeleted = true;
        employee.DeletedAt = DateTime.UtcNow;

        // Their deductions go with them — nothing else references them.
        var deductions = await _context.EmployeeDeductions
            .Where(d => d.ExternalEmployeeId == id)
            .ToListAsync();
        foreach (var deduction in deductions)
        {
            deduction.IsDeleted = true;
            deduction.DeletedAt = DateTime.UtcNow;
        }

        var loans = await _context.EmployeeLoans
            .Where(l => l.ExternalEmployeeId == id)
            .ToListAsync();
        foreach (var loan in loans)
        {
            loan.IsDeleted = true;
            loan.DeletedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateSalaryAsync(string type, string id, decimal salary)
    {
        if (salary < 0m)
        {
            throw new ValidationException("Salary cannot be negative (لا يمكن أن يكون الراتب سالباً)");
        }

        if (IsExternal(type))
        {
            var employee = await _context.ExternalEmployees.FirstOrDefaultAsync(e => e.Id == ParseExternalId(id));
            if (employee == null)
            {
                return false;
            }
            employee.Salary = salary;
        }
        else
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == ParseUserId(id));
            if (user == null)
            {
                return false;
            }
            user.Salary = salary;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<EmployeeDeductionDto> AddDeductionAsync(EmployeeDeductionCreateDto dto)
    {
        var validationResult = await _deductionValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var deduction = new EmployeeDeduction
        {
            Id = Guid.NewGuid(),
            Amount = dto.Amount,
            Description = dto.Description?.Trim() ?? string.Empty,
            // Blank date → record it against the moment of creation.
            DeductionDate = dto.DeductionDate ?? DateTime.UtcNow
        };

        if (IsExternal(dto.Type))
        {
            var employeeId = ParseExternalId(dto.Id);
            var exists = await _context.ExternalEmployees.AnyAsync(e => e.Id == employeeId);
            if (!exists)
            {
                throw new ValidationException("External employee not found (الموظف الخارجي غير موجود)");
            }
            deduction.ExternalEmployeeId = employeeId;
        }
        else
        {
            var userId = ParseUserId(dto.Id);
            var exists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!exists)
            {
                throw new ValidationException("User not found (المستخدم غير موجود)");
            }
            deduction.UserId = userId;
        }

        _context.EmployeeDeductions.Add(deduction);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "[EmployeeSalaries] Added deduction {DeductionId} type={Type} employee={EmployeeId} amount={Amount}",
            deduction.Id, dto.Type, dto.Id, deduction.Amount);

        return _mapper.Map<EmployeeDeductionDto>(deduction);
    }

    public async Task<bool> DeleteDeductionAsync(Guid id)
    {
        var deduction = await _context.EmployeeDeductions.FirstOrDefaultAsync(d => d.Id == id);
        if (deduction == null)
        {
            return false;
        }

        deduction.IsDeleted = true;
        deduction.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<EmployeeLoanDto> AddLoanAsync(EmployeeLoanCreateDto dto)
    {
        var validationResult = await _loanValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var loan = new EmployeeLoan
        {
            Id = Guid.NewGuid(),
            Amount = dto.Amount,
            Description = dto.Description?.Trim() ?? string.Empty,
            // Blank date → record it against the moment of creation.
            LoanDate = dto.LoanDate ?? DateTime.UtcNow
        };

        if (IsExternal(dto.Type))
        {
            var employeeId = ParseExternalId(dto.Id);
            var exists = await _context.ExternalEmployees.AnyAsync(e => e.Id == employeeId);
            if (!exists)
            {
                throw new ValidationException("External employee not found (الموظف الخارجي غير موجود)");
            }
            loan.ExternalEmployeeId = employeeId;
        }
        else
        {
            var userId = ParseUserId(dto.Id);
            var exists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!exists)
            {
                throw new ValidationException("User not found (المستخدم غير موجود)");
            }
            loan.UserId = userId;
        }

        _context.EmployeeLoans.Add(loan);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "[EmployeeSalaries] Added loan {LoanId} type={Type} employee={EmployeeId} amount={Amount}",
            loan.Id, dto.Type, dto.Id, loan.Amount);

        return _mapper.Map<EmployeeLoanDto>(loan);
    }

    public async Task<bool> DeleteLoanAsync(Guid id)
    {
        var loan = await _context.EmployeeLoans.FirstOrDefaultAsync(l => l.Id == id);
        if (loan == null)
        {
            return false;
        }

        loan.IsDeleted = true;
        loan.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    // Helpers ────────────────────────────────────────────────────────────────

    /// <summary>
    /// UTC half-open range [start, end) covering the requested calendar month as the
    /// business timezone sees it — same convention as the payments summaries.
    /// </summary>
    private static (DateTime Start, DateTime End) MonthBounds(int year, int month)
    {
        if (month < 1 || month > 12)
        {
            throw new ValidationException("Month must be between 1 and 12 (الشهر يجب أن يكون بين 1 و 12)");
        }
        if (year < 2000 || year > 2999)
        {
            throw new ValidationException("Year is out of range (السنة خارج النطاق)");
        }

        var riyadh = TimeZoneInfo.FindSystemTimeZoneById("Asia/Riyadh");
        var monthStartRiyadh = new DateTime(year, month, 1, 0, 0, 0);

        return (
            TimeZoneInfo.ConvertTimeToUtc(monthStartRiyadh, riyadh),
            TimeZoneInfo.ConvertTimeToUtc(monthStartRiyadh.AddMonths(1), riyadh)
        );
    }

    /// <summary>Profit cut and net pay are always derived, never stored.</summary>
    private static void FillDerived(EmployeeSalaryRowDto row)
    {
        row.Profit = Math.Round(row.ProfitBase * row.ProfitPercentage / 100m, 2);
        // A loan carries no interest — it comes off the month's pay exactly like a deduction.
        row.Net = row.Salary + row.Profit - row.Deduction - row.Loan;
    }

    private IQueryable<EmployeeDeduction> DeductionsQuery(string type, string id)
    {
        if (IsExternal(type))
        {
            var employeeId = ParseExternalId(id);
            return _context.EmployeeDeductions.Where(d => d.ExternalEmployeeId == employeeId);
        }

        var userId = ParseUserId(id);
        return _context.EmployeeDeductions.Where(d => d.UserId == userId);
    }

    private IQueryable<EmployeeLoan> LoansQuery(string type, string id)
    {
        if (IsExternal(type))
        {
            var employeeId = ParseExternalId(id);
            return _context.EmployeeLoans.Where(l => l.ExternalEmployeeId == employeeId);
        }

        var userId = ParseUserId(id);
        return _context.EmployeeLoans.Where(l => l.UserId == userId);
    }

    private static bool IsExternal(string? type) =>
        string.Equals(type, EmployeeRefType.External, StringComparison.OrdinalIgnoreCase);

    private static Guid ParseExternalId(string id) =>
        Guid.TryParse(id, out var guid)
            ? guid
            : throw new ValidationException("Invalid external employee id (معرف الموظف الخارجي غير صالح)");

    private static int ParseUserId(string id) =>
        int.TryParse(id, out var userId)
            ? userId
            : throw new ValidationException("Invalid user id (معرف المستخدم غير صالح)");
}
