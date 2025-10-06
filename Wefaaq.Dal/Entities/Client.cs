using System.ComponentModel.DataAnnotations;

namespace Wefaaq.Dal.Entities;

/// <summary>
/// Client entity (العميل)
/// </summary>
public class Client
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Client name (أسم)
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Client email (ايميل)
    /// </summary>
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Client phone number (رقم)
    /// </summary>
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Client classification (تصنيف العميل)
    /// </summary>
    public ClientClassification Classification { get; set; }

    /// <summary>
    /// Client balance (رصيد) - negative = مدين (debtor), positive = دائن (creditor)
    /// </summary>
    [Range(-999999999.99, 999999999.99)]
    public decimal Balance { get; set; }

    /// <summary>
    /// Count of external workers (عمال خارج المؤسسة)
    /// </summary>
    [Range(0, int.MaxValue)]
    public int ExternalWorkersCount { get; set; }

    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Many-to-many relationship with organizations
    /// </summary>
    public virtual ICollection<Organization> Organizations { get; set; } = new List<Organization>();

    /// <summary>
    /// Join table for many-to-many relationship
    /// </summary>
    public virtual ICollection<ClientOrganization> ClientOrganizations { get; set; } = new List<ClientOrganization>();
}