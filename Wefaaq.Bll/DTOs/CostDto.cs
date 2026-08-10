namespace Wefaaq.Bll.DTOs;

/// <summary>
/// Business cost / expense response DTO (المصروفات)
/// </summary>
public class CostDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CostDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for creating a new cost. CostDate is optional — defaults to "now" when omitted.
/// </summary>
public class CostCreateDto
{
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public DateTime? CostDate { get; set; }
}
