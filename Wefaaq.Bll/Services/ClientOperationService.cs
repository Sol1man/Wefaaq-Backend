using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Wefaaq.Bll.DTOs;
using Wefaaq.Bll.Interfaces;
using Wefaaq.Dal;
using Wefaaq.Dal.Entities;

namespace Wefaaq.Bll.Services;

public class ClientOperationService : IClientOperationService
{
    private readonly WefaaqContext _context;
    private readonly IMapper _mapper;

    public ClientOperationService(WefaaqContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // ── Queries ────────────────────────────────────────────────────────────────

    public async Task<IEnumerable<ClientOperationDto>> GetAllAsync()
    {
        var ops = await BaseQuery()
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return ops.Select(MapToDto);
    }

    public async Task<ClientOperationDto?> GetByIdAsync(Guid id)
    {
        var op = await BaseQuery().FirstOrDefaultAsync(o => o.Id == id);
        return op == null ? null : MapToDto(op);
    }

    public async Task<IEnumerable<ClientOperationDto>> GetByClientAsync(Guid clientId)
    {
        // Collect IDs of all branches and organizations under this client
        var branchIds = await _context.ClientBranches
            .Where(b => b.ParentClientId == clientId)
            .Select(b => b.Id)
            .ToListAsync();

        var orgIds = await _context.Organizations
            .Where(o => o.ClientId == clientId || (o.ClientBranchId != null && branchIds.Contains(o.ClientBranchId.Value)))
            .Select(o => o.Id)
            .ToListAsync();

        var ops = await BaseQuery()
            .Where(o =>
                o.ClientId == clientId ||
                (o.ClientBranchId != null && branchIds.Contains(o.ClientBranchId.Value)) ||
                (o.OrganizationId != null && orgIds.Contains(o.OrganizationId.Value)))
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return ops.Select(MapToDto);
    }

    public async Task<IEnumerable<ClientOperationDto>> GetByBranchAsync(Guid branchId)
    {
        var orgIds = await _context.Organizations
            .Where(o => o.ClientBranchId == branchId)
            .Select(o => o.Id)
            .ToListAsync();

        var ops = await BaseQuery()
            .Where(o =>
                o.ClientBranchId == branchId ||
                (o.OrganizationId != null && orgIds.Contains(o.OrganizationId.Value)))
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return ops.Select(MapToDto);
    }

    public async Task<IEnumerable<ClientOperationDto>> GetByOrganizationAsync(Guid organizationId)
    {
        var ops = await BaseQuery()
            .Where(o => o.OrganizationId == organizationId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return ops.Select(MapToDto);
    }

    // ── Mutations ──────────────────────────────────────────────────────────────

    public async Task<ClientOperationDto> CreateAsync(ClientOperationCreateDto dto, int performedByUserId)
    {
        var op = new ClientOperation
        {
            Id = Guid.NewGuid(),
            Kind = OperationKind.Service,
            Type = dto.Type,
            CustomType = dto.Type == OperationType.Other ? dto.CustomType : null,
            TargetType = dto.TargetType,
            Price = dto.Price,
            Notes = dto.Notes,
            ClientId = dto.ClientId,
            ClientBranchId = dto.ClientBranchId,
            OrganizationId = dto.OrganizationId,
            ExternalPersonName = dto.ExternalPersonName,
            ExternalPersonIdNumber = dto.ExternalPersonIdNumber,
            PerformedByUserId = performedByUserId,
            // Operations are completed the moment they are recorded.
            CompletedAt = DateTime.UtcNow,
        };

        _context.ClientOperations.Add(op);

        // A service debits the target balance immediately.
        await AdjustBalanceAsync(op, BalanceEffect(op));

        await _context.SaveChangesAsync();

        return MapToDto(await BaseQuery().FirstAsync(o => o.Id == op.Id));
    }

    public async Task<ClientOperationDto> CreatePaymentAsync(ClientOperationPaymentCreateDto dto, int performedByUserId)
    {
        var op = new ClientOperation
        {
            Id = Guid.NewGuid(),
            Kind = OperationKind.Payment,
            Type = null,
            TargetType = dto.TargetType,
            Price = dto.Amount,
            Notes = dto.Notes,
            ClientId = dto.ClientId,
            ClientBranchId = dto.ClientBranchId,
            OrganizationId = dto.OrganizationId,
            ExternalPersonName = dto.ExternalPersonName,
            ExternalPersonIdNumber = dto.ExternalPersonIdNumber,
            PerformedByUserId = performedByUserId,
            CompletedAt = DateTime.UtcNow,
        };

        _context.ClientOperations.Add(op);

        // A payment credits the target balance immediately.
        await AdjustBalanceAsync(op, BalanceEffect(op));

        await _context.SaveChangesAsync();

        return MapToDto(await BaseQuery().FirstAsync(o => o.Id == op.Id));
    }

    public async Task<ClientOperationDto?> UpdateAsync(Guid id, ClientOperationUpdateDto dto)
    {
        var op = await BaseQuery().FirstOrDefaultAsync(o => o.Id == id);
        if (op == null) return null;

        // Reverse the previous balance effect, apply the new one (price may have changed).
        var previousEffect = BalanceEffect(op);

        op.Type = dto.Type;
        op.CustomType = dto.Type == OperationType.Other ? dto.CustomType : null;
        op.Price = dto.Price;
        op.Notes = dto.Notes;

        var delta = BalanceEffect(op) - previousEffect;
        if (delta != 0)
        {
            await AdjustBalanceAsync(op, delta);
        }

        await _context.SaveChangesAsync();
        return MapToDto(await BaseQuery().FirstAsync(o => o.Id == id));
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var op = await _context.ClientOperations.FindAsync(id);
        if (op == null) return false;

        // Undo this record's effect on the target balance before removing it.
        var effect = BalanceEffect(op);
        if (effect != 0)
        {
            await AdjustBalanceAsync(op, -effect);
        }

        op.IsDeleted = true;
        op.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    private IQueryable<ClientOperation> BaseQuery() =>
        _context.ClientOperations
            .Include(o => o.Client)
            .Include(o => o.ClientBranch)
            .Include(o => o.Organization)
            .Include(o => o.PerformedByUser);

    /// <summary>
    /// Signed effect this record has on the target balance:
    /// a service debits (negative), a payment credits (positive).
    /// </summary>
    private static decimal BalanceEffect(ClientOperation op)
    {
        var amount = op.Price ?? 0m;
        return op.Kind == OperationKind.Payment ? amount : -amount;
    }

    /// <summary>
    /// Add a signed amount to the appropriate balance (positive = credit, negative = debit):
    /// Client → Client.Balance, Branch → ClientBranch.Balance,
    /// Organization → its owner (Client or Branch).
    /// ExternalPerson → no balance touched.
    /// </summary>
    private async Task AdjustBalanceAsync(ClientOperation op, decimal signedAmount)
    {
        if (signedAmount == 0) return;

        switch (op.TargetType)
        {
            case OperationTargetType.Client when op.ClientId.HasValue:
                var client = await _context.Clients.FindAsync(op.ClientId.Value);
                if (client != null) client.Balance += signedAmount;
                break;

            case OperationTargetType.ClientBranch when op.ClientBranchId.HasValue:
                var branch = await _context.ClientBranches.FindAsync(op.ClientBranchId.Value);
                if (branch != null) branch.Balance += signedAmount;
                break;

            case OperationTargetType.Organization when op.OrganizationId.HasValue:
                var org = await _context.Organizations
                    .Include(o => o.Client)
                    .Include(o => o.ClientBranch)
                    .FirstOrDefaultAsync(o => o.Id == op.OrganizationId.Value);

                if (org?.Client != null)
                    org.Client.Balance += signedAmount;
                else if (org?.ClientBranch != null)
                    org.ClientBranch.Balance += signedAmount;
                break;
        }
    }

    private static ClientOperationDto MapToDto(ClientOperation op) => new()
    {
        Id = op.Id,
        Kind = op.Kind,
        Type = op.Type,
        CustomType = op.CustomType,
        TypeDisplay = GetTypeDisplay(op),
        TargetType = op.TargetType,
        Price = op.Price,
        Notes = op.Notes,
        ClientId = op.ClientId,
        ClientName = op.Client?.Name,
        ClientBranchId = op.ClientBranchId,
        ClientBranchName = op.ClientBranch?.Name,
        OrganizationId = op.OrganizationId,
        OrganizationName = op.Organization?.Name,
        ExternalPersonName = op.ExternalPersonName,
        ExternalPersonIdNumber = op.ExternalPersonIdNumber,
        PerformedByUserId = op.PerformedByUserId,
        PerformedByUserName = op.PerformedByUser?.Name ?? string.Empty,
        CompletedAt = op.CompletedAt,
        CreatedAt = op.CreatedAt,
        UpdatedAt = op.UpdatedAt,
    };

    /// <summary>Human-readable label for a record — payment, free-text "Other", or a known operation type.</summary>
    private static string GetTypeDisplay(ClientOperation op)
    {
        if (op.Kind == OperationKind.Payment)
            return "دفعة من العميل";

        if (op.Type == null)
            return string.Empty;

        if (op.Type == OperationType.Other)
            return string.IsNullOrWhiteSpace(op.CustomType) ? "أخرى" : op.CustomType;

        return GetTypeDisplay(op.Type.Value);
    }

    private static string GetTypeDisplay(OperationType type) => type switch
    {
        OperationType.RenewCommercialRecord => "تجديد سجل تجاري",
        OperationType.RenewCommercialLicense => "تجديد رخصة تجارية",
        OperationType.RenewOrganizationResidence => "تجديد اقامة موسسة",
        OperationType.RenewHouseholdWorkerResidence => "تجديد اقامة عامل منزلي",
        OperationType.RenewShepherdWorkerResidence => "تجديد اقامة عامل راعي جديد",
        OperationType.RenewAgriculturalWorkerResidence => "تجديد اقامة عامل زراعي جديد",
        OperationType.IssueOrganizationResidence => "اصدار اقامة موسسة",
        OperationType.IssueHouseholdResidence => "اصدار اقامة منزلي",
        OperationType.IssueShepherdResidence => "اصدار اقامة راعي",
        OperationType.IssueAgriculturalResidence => "اصدار اقامة زراعي",
        OperationType.MedicalInsurance => "تامين طبي",
        OperationType.CarInsurance => "تامين سيارة",
        OperationType.ExtendVisit => "تمديد زيارة",
        OperationType.VisitVisa => "تاشيرة زيارة",
        OperationType.IssueOperatingCard => "اصدار كرت تشغيل",
        OperationType.RenewOperatingCard => "تجديد كرت تشغيل",
        OperationType.DriverCard => "كرت سائق",
        OperationType.RenewCarWithLetter => "تجديد سيارة بخطاب",
        OperationType.RenewCarFromAbsher => "تجديد سيارة من ابشر",
        OperationType.TransferCarOwnershipWithLetter => "نقل ملكية سيارة بخطاب",
        OperationType.ChangePlatePublicToPrivate => "تغير لوحة من عام الي خاص",
        OperationType.ChangePlatePrivateToPublic => "تغير لوحة من خاص الي عام",
        OperationType.PayQiwa => "سداد قوي",
        OperationType.RenewQiwa => "تجديد قوي",
        OperationType.RenewAbsher115 => "تجديد ابشر 115",
        OperationType.RenewAbsher287 => "تجديد ابشر 287",
        OperationType.RenewAbsher => "تجديد ابشر",
        OperationType.Employment => "توظيف",
        OperationType.IssueCommercialRecord => "اصدار سجل تجاري",
        OperationType.RenewLicense => "تجديد رخصة",
        OperationType.IssueLicense => "اصدار رخصة",
        OperationType.IssueVisa => "اصدار تأشيرة",
        OperationType.Authorization => "تفويض",
        OperationType.Other => "أخرى",
        _ => type.ToString(),
    };
}
