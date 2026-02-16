namespace Wefaaq.Bll.DTOs;

/// <summary>
/// User DTO for responses
/// </summary>
public class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string FirebaseUid { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public Guid? OrganizationId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

/// <summary>
/// Login request DTO
/// </summary>
public class LoginRequestDto
{
    public string IdToken { get; set; } = string.Empty;
}

/// <summary>
/// Logout request DTO
/// </summary>
public class LogoutRequestDto
{
    public string IdToken { get; set; } = string.Empty;
}

/// <summary>
/// Login response DTO
/// </summary>
public class LoginResponseDto
{
    public bool Success { get; set; }
    public UserDto? User { get; set; }
    public string? Message { get; set; }
}

/// <summary>
/// Logout response DTO
/// </summary>
public class LogoutResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = "Logged out successfully";
}
