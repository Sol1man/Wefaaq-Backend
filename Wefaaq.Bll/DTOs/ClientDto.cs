using Wefaaq.Dal.Entities;

namespace Wefaaq.Bll.DTOs;

/// <summary>
/// Client DTO for responses
/// </summary>
public class ClientDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public ClientClassification Classification { get; set; }
    public decimal Balance { get; set; }
    public int ExternalWorkersCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<OrganizationDto> Organizations { get; set; } = new();
}

/// <summary>
/// Client DTO for creating new clients
/// </summary>
public class ClientCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public ClientClassification Classification { get; set; }
    public decimal Balance { get; set; }
    public int ExternalWorkersCount { get; set; }
    public List<Guid> OrganizationIds { get; set; } = new();
}

/// <summary>
/// Client DTO for updating existing clients
/// </summary>
public class ClientUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public ClientClassification Classification { get; set; }
    public decimal Balance { get; set; }
    public int ExternalWorkersCount { get; set; }
    public List<Guid> OrganizationIds { get; set; } = new();
}