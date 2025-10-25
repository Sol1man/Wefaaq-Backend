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

/// <summary>
/// Client DTO for creating a client with organizations in a single request
/// </summary>
public class ClientWithOrganizationsCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public ClientClassification Classification { get; set; }
    public decimal Balance { get; set; }
    public int ExternalWorkersCount { get; set; }
    public List<OrganizationCreateDtoSimple> Organizations { get; set; } = new();
}

/// <summary>
/// Client DTO for updating a client with organizations in a single request
/// </summary>
public class ClientWithOrganizationsUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public ClientClassification Classification { get; set; }
    public decimal Balance { get; set; }
    public int ExternalWorkersCount { get; set; }
    public List<OrganizationCreateDtoSimple> Organizations { get; set; } = new();
}

/// <summary>
/// Full organization DTO for creating organizations within a client (without ClientId as it will be set automatically)
/// </summary>
public class OrganizationCreateDtoSimple
{
    public string Name { get; set; } = string.Empty;
    public bool CardExpiringSoon { get; set; }
    public List<OrganizationRecordCreateDto> Records { get; set; } = new();
    public List<OrganizationLicenseCreateDto> Licenses { get; set; } = new();
    public List<OrganizationWorkerCreateDto> Workers { get; set; } = new();
    public List<OrganizationCarCreateDto> Cars { get; set; } = new();
}