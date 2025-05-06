using System.ComponentModel.DataAnnotations;

namespace hrconnectbackend.Models
{
    public class UsageRecord
    {
        [Key]
        public int UsageId { get; set; }

        public int SubscriptionId { get; set; }

        [Required, StringLength(100)]
        public string ResourceType { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public DateTime RecordedAt { get; set; }

        // Navigation property
        public virtual Subscription Subscription { get; set; }
    }
}
