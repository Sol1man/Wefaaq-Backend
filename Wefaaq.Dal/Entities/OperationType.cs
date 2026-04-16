namespace Wefaaq.Dal.Entities;

/// <summary>
/// Operation type enum — all service types offered (نوع العملية)
/// </summary>
public enum OperationType
{
    /// <summary>تجديد سجل تجاري</summary>
    RenewCommercialRecord = 1,

    /// <summary>تجديد رخصة تجارية</summary>
    RenewCommercialLicense = 2,

    /// <summary>تجديد اقامة موسسة</summary>
    RenewOrganizationResidence = 3,

    /// <summary>تجديد اقامة عامل منزلي</summary>
    RenewHouseholdWorkerResidence = 4,

    /// <summary>تجديد اقامة عامل راعي جديد</summary>
    RenewShepherdWorkerResidence = 5,

    /// <summary>تجديد اقامة عامل زراعي جديد</summary>
    RenewAgriculturalWorkerResidence = 6,

    /// <summary>اصدار اقامة موسسة</summary>
    IssueOrganizationResidence = 7,

    /// <summary>اصدار اقامة منزلي</summary>
    IssueHouseholdResidence = 8,

    /// <summary>اصدار اقامة راعي</summary>
    IssueShepherdResidence = 9,

    /// <summary>اصدار اقامة زراعي</summary>
    IssueAgriculturalResidence = 10,

    /// <summary>تامين طبي</summary>
    MedicalInsurance = 11,

    /// <summary>تامين سيارة</summary>
    CarInsurance = 12,

    /// <summary>تمديد زيارة</summary>
    ExtendVisit = 13,

    /// <summary>تاشيرة زيارة</summary>
    VisitVisa = 14,

    /// <summary>اصدار كرت تشغيل</summary>
    IssueOperatingCard = 15,

    /// <summary>تجديد كرت تشغيل</summary>
    RenewOperatingCard = 16,

    /// <summary>كرت سائق</summary>
    DriverCard = 17,

    /// <summary>تجديد سيارة بخطاب</summary>
    RenewCarWithLetter = 18,

    /// <summary>تجديد سيارة من ابشر</summary>
    RenewCarFromAbsher = 19,

    /// <summary>نقل ملكية سيارة بخطاب</summary>
    TransferCarOwnershipWithLetter = 20,

    /// <summary>تغير لوحة من عام الي خاص</summary>
    ChangePlatePublicToPrivate = 21,

    /// <summary>تغير لوحة من خاص الي عام</summary>
    ChangePlatePrivateToPublic = 22,

    /// <summary>سداد قوي</summary>
    PayQiwa = 23,

    /// <summary>تجديد قوي</summary>
    RenewQiwa = 24,

    /// <summary>تجديد ابشر 115</summary>
    RenewAbsher115 = 25,

    /// <summary>تجديد ابشر 287</summary>
    RenewAbsher287 = 26,

    /// <summary>تجديد ابشر</summary>
    RenewAbsher = 27,

    /// <summary>توظيف</summary>
    Employment = 28,

    /// <summary>اصدار سجل تجاري</summary>
    IssueCommercialRecord = 29,

    /// <summary>تجديد رخصة</summary>
    RenewLicense = 30,

    /// <summary>اصدار رخصة</summary>
    IssueLicense = 31,
}
