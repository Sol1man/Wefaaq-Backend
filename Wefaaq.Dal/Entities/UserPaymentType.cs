namespace Wefaaq.Dal.Entities;

/// <summary>
/// Payment-history row classification.
/// Payment deducts from the user's account; Profit is recorded but does not deduct;
/// Initial is an admin-issued top-up that adds to the balance and is logged as a row for traceability.
/// </summary>
public enum UserPaymentType
{
    Payment = 0,
    Profit = 1,
    Initial = 2
}
