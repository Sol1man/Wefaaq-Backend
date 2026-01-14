namespace Wefaaq.Bll.DTOs;

/// <summary>
/// Organization DTO for responses
/// </summary>
public class OrganizationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool CardExpiringSoon { get; set; }
    public Guid? ClientId { get; set; }
    public string? Client { get; set; }
    public Guid? ClientBranchId { get; set; }
    public string? ClientBranch { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<OrganizationRecordDto> Records { get; set; } = new();
    public List<OrganizationLicenseDto> Licenses { get; set; } = new();
    public List<OrganizationWorkerDto> Workers { get; set; } = new();
    public List<OrganizationCarDto> Cars { get; set; } = new();
    public List<OrganizationUsernameDto> Usernames { get; set; } = new();
}

/// <summary>
/// Organization DTO for creating new organizations
/// </summary>
public class OrganizationCreateDto
{
    public string Name { get; set; } = string.Empty;
    public bool CardExpiringSoon { get; set; }
    public Guid? ClientId { get; set; }
    public Guid? ClientBranchId { get; set; }
}

/// <summary>
/// Organization DTO for updating existing organizations
/// </summary>
public class OrganizationUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public bool CardExpiringSoon { get; set; }
    public Guid? ClientId { get; set; }
    public Guid? ClientBranchId { get; set; }
}

/// <summary>
/// Organization Record DTO
/// </summary>
public class OrganizationRecordDto
{
    public Guid Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public string? ImagePath { get; set; }
    public Guid OrganizationId { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Organization Record DTO for creating new records
/// </summary>
public class OrganizationRecordCreateDto
{
    public string Number { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public string? ImagePath { get; set; }
}

/// <summary>
/// Organization Record DTO for updating existing records
/// </summary>
public class OrganizationRecordUpdateDto
{
    public string Number { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public string? ImagePath { get; set; }
}

/// <summary>
/// Organization License DTO
/// </summary>
public class OrganizationLicenseDto
{
    public Guid Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public string? ImagePath { get; set; }
    public Guid OrganizationId { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Organization License DTO for creating new licenses
/// </summary>
public class OrganizationLicenseCreateDto
{
    public string Number { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public string? ImagePath { get; set; }
}

/// <summary>
/// Organization License DTO for updating existing licenses
/// </summary>
public class OrganizationLicenseUpdateDto
{
    public string Number { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public string? ImagePath { get; set; }
}

/// <summary>
/// Organization Worker DTO
/// </summary>
public class OrganizationWorkerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ResidenceNumber { get; set; } = string.Empty;
    public string? ResidenceImagePath { get; set; }
    public DateTime ExpiryDate { get; set; }
    public Guid OrganizationId { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Organization Worker DTO for creating new workers
/// </summary>
public class OrganizationWorkerCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string ResidenceNumber { get; set; } = string.Empty;
    public string? ResidenceImagePath { get; set; }
    public DateTime ExpiryDate { get; set; }
}

/// <summary>
/// Organization Worker DTO for updating existing workers
/// </summary>
public class OrganizationWorkerUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public string ResidenceNumber { get; set; } = string.Empty;
    public string? ResidenceImagePath { get; set; }
    public DateTime ExpiryDate { get; set; }
}

/// <summary>
/// Organization Car DTO
/// </summary>
public class OrganizationCarDto
{
    public Guid Id { get; set; }
    public string PlateNumber { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string? ImagePath { get; set; }
    public DateTime OperatingCardExpiry { get; set; }
    public Guid OrganizationId { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Organization Car DTO for creating new cars
/// </summary>
public class OrganizationCarCreateDto
{
    public string PlateNumber { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string? ImagePath { get; set; }
    public DateTime OperatingCardExpiry { get; set; }
}

/// <summary>
/// Organization Car DTO for updating existing cars
/// </summary>
public class OrganizationCarUpdateDto
{
    public string PlateNumber { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string? ImagePath { get; set; }
    public DateTime OperatingCardExpiry { get; set; }
}