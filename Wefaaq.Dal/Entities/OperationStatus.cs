namespace Wefaaq.Dal.Entities;

/// <summary>
/// Operation status enum (حالة العملية)
/// </summary>
public enum OperationStatus
{
    /// <summary>قيد الانتظار</summary>
    Pending = 1,

    /// <summary>جاري التنفيذ</summary>
    InProgress = 2,

    /// <summary>مكتملة</summary>
    Completed = 3,

    /// <summary>ملغاة</summary>
    Cancelled = 4,
}
