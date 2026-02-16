namespace Wefaaq.Bll.DTOs;

/// <summary>
/// User payment response DTO
/// </summary>
public class UserPaymentDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
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
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// DTO for updating an existing user payment
/// </summary>
public class UserPaymentUpdateDto
{
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
}
