namespace Wefaaq.Dal.Entities;

/// <summary>
/// Kind of client-operation record (نوع القيد):
/// a service performed for the client, or cash the client paid against their fees.
/// </summary>
public enum OperationKind
{
    /// <summary>A service performed for the client — debits the target balance (عملية / خصم)</summary>
    Service = 1,

    /// <summary>Cash the client paid against their fees — credits the target balance (دفعة عميل / إيداع)</summary>
    Payment = 2,
}
