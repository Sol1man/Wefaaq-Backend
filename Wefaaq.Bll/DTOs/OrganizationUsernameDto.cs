namespace Wefaaq.Bll.DTOs;

/// <summary>
/// Organization Username DTO for responses
/// </summary>
public class OrganizationUsernameDto
{
    public Guid Id { get; set; }
    public string SiteName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public Guid OrganizationId { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Organization Username DTO for creating new usernames
/// </summary>
public class OrganizationUsernameCreateDto
{
    public string SiteName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Organization Username DTO for updating existing usernames
/// </summary>
public class OrganizationUsernameUpdateDto
{
    public string SiteName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
