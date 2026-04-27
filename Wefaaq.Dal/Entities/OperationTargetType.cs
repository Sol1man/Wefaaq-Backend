namespace Wefaaq.Dal.Entities;

/// <summary>
/// Target type of an operation — who the operation is performed for (نوع المستهدف)
/// </summary>
public enum OperationTargetType
{
    /// <summary>عميل مباشر</summary>
    Client = 1,

    /// <summary>فرع عميل</summary>
    ClientBranch = 2,

    /// <summary>مؤسسة</summary>
    Organization = 3,

    /// <summary>عميل خارجي</summary>
    ExternalPerson = 4,
}
