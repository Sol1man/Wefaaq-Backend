namespace Wefaaq.Dal.Entities;

/// <summary>
/// Join table for Client-Organization many-to-many relationship
/// </summary>
public class ClientOrganization
{
    /// <summary>
    /// Client identifier
    /// </summary>
    public Guid ClientId { get; set; }

    /// <summary>
    /// Organization identifier
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Navigation property to Client
    /// </summary>
    public virtual Client Client { get; set; } = null!;

    /// <summary>
    /// Navigation property to Organization
    /// </summary>
    public virtual Organization Organization { get; set; } = null!;
}