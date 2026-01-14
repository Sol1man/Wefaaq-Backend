namespace Wefaaq.Dal.Entities;

/// <summary>
/// External worker type enum (نوع العامل الخارجي)
/// </summary>
public enum WorkerType
{
    /// <summary>
    /// عامل منزلي - Household Worker
    /// </summary>
    HouseholdWorker = 1,

    /// <summary>
    /// طباخ منزلي - Home Cook
    /// </summary>
    HomeCook = 2,

    /// <summary>
    /// سائق خاص - Private Driver
    /// </summary>
    PrivateDriver = 3,

    /// <summary>
    /// مزارع - Farmer
    /// </summary>
    Farmer = 4,

    /// <summary>
    /// راعي - Shepherd
    /// </summary>
    Shepherd = 5,

    /// <summary>
    /// نحال - Beekeeper
    /// </summary>
    Beekeeper = 6
}
