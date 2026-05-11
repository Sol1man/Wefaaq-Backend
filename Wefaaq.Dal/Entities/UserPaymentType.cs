namespace Wefaaq.Dal.Entities;

/// <summary>
/// Distinguishes a deduction from the user's account (Payment) from a profit entry that doesn't touch the balance (Profit).
/// </summary>
public enum UserPaymentType
{
    Payment = 0,
    Profit = 1
}
