using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace hrconnectbackend.Models
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        public int SubscriptionId { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required, StringLength(50)]
        public string TransactionId { get; set; }

        [Required]
        public PaymentStatus Status { get; set; }

        [StringLength(255)]
        public string PaymentMethod { get; set; }

        // Navigation property
        public virtual Subscription Subscription { get; set; }
    }
}

public enum PaymentStatus
{
    Pending,
    Successful,
    Failed,
    Refunded
}