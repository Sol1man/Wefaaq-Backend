namespace Wefaaq.Bll.DTOs;

/// <summary>
/// Result of a deletion operation with detailed information
/// </summary>
public class DeletionResultDto
{
    /// <summary>
    /// Whether the deletion was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if deletion failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Type of deletion performed (Soft or Hard)
    /// </summary>
    public string DeletionType { get; set; } = string.Empty;

    /// <summary>
    /// Total number of entities affected by the deletion
    /// </summary>
    public int TotalEntitiesAffected { get; set; }

    /// <summary>
    /// Breakdown of deleted entities by type
    /// </summary>
    public Dictionary<string, int> DeletedEntities { get; set; } = new();

    /// <summary>
    /// Warnings about the deletion (e.g., "This client has 5 active organizations")
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Timestamp when the deletion occurred
    /// </summary>
    public DateTime DeletedAt { get; set; } = DateTime.UtcNow;
}
