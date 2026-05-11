using System.ComponentModel.DataAnnotations;
using Wefaaq.Dal.Interfaces;

namespace Wefaaq.Dal.Entities;

/// <summary>
/// User payment entity for tracking payments made by users during their work day
/// </summary>
public class UserPayment : ISoftDeletable
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Payment amount
    /// </summary>
    [Required]
    public decimal Amount { get; set; }

    /// <summary>
    /// Payment description
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Payment type — Payment deducts from the user's account, Profit is recorded but does not deduct
    /// </summary>
    public UserPaymentType Type { get; set; } = UserPaymentType.Payment;

    /// <summary>
    /// Optional link from a Profit entry back to the Payment entry from the same client operation
    /// </summary>
    public Guid? RelatedPaymentId { get; set; }

    /// <summary>
    /// Navigation property to the related payment (only populated for Profit rows that link back to a Payment)
    /// </summary>
    public virtual UserPayment? RelatedPayment { get; set; }

    /// <summary>
    /// User ID (foreign key)
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Navigation property to User
    /// </summary>
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// Payment creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indicates if the payment is soft deleted
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Timestamp when the payment was soft deleted
    /// </summary>
    public DateTime? DeletedAt { get; set; }
}
