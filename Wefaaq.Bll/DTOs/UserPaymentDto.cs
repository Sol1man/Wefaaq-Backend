using Wefaaq.Dal.Entities;

namespace Wefaaq.Bll.DTOs;

/// <summary>
/// User payment response DTO
/// </summary>
public class UserPaymentDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public UserPaymentType Type { get; set; }
    public Guid? RelatedPaymentId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for creating a new user payment
/// </summary>
public class UserPaymentCreateDto
{
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public UserPaymentType Type { get; set; } = UserPaymentType.Payment;
    public Guid? RelatedPaymentId { get; set; }
}

/// <summary>
/// DTO for updating an existing user payment
/// </summary>
public class UserPaymentUpdateDto
{
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public UserPaymentType Type { get; set; } = UserPaymentType.Payment;
    public Guid? RelatedPaymentId { get; set; }
}

/// <summary>
/// Aggregated, one-row-per-user view shown on the payments management page
/// </summary>
public class UserPaymentSummaryDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public decimal InitialAccountAmount { get; set; }
    public decimal CurrentAccountAmount { get; set; }
    public decimal TodaysPayments { get; set; }
    public decimal TodaysProfit { get; set; }
    public decimal CurrentMonthPayments { get; set; }
    public decimal CurrentMonthProfit { get; set; }
}

/// <summary>
/// Combined-operation payload: a single client operation can produce a Payment row, a Profit row, or both linked together.
/// </summary>
public class UserPaymentOperationCreateDto
{
    public decimal? PaymentAmount { get; set; }
    public decimal? ProfitAmount { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// Admin top-up payload. Cumulative: the amount is ADDED to the user's account balances and
/// also logged as a UserPayment row of Type=Initial for traceability.
/// </summary>
public class UpdateUserAccountAmountDto
{
    public decimal InitialAccountAmount { get; set; }
    public string? Description { get; set; }
}
