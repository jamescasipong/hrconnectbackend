using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hrconnectbackend.Models;

public class Subscription
{
    [Key]
    public int SubscriptionId { get; set; }
    public int OrganizationId { get; set; }
    public int PlanId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    [Required]
    public DateTime NextBillingDate { get; set; }
    [Required]
    public BillingCycle BillingCycle { get; set; }
    public bool IsActive { get; set; } = true;
    [Required]
    public SubscriptionStatus Status { get; set; }
    public DateTime? TrialEndsAt { get; set; }
    public DateTime LastBillingDate { get; set; }
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal CurrentPrice { get; set; }
    public virtual Plan? Plan { get; set; }
    public virtual Organization? Organization { get; set; }
    public virtual ICollection<UsageRecord> UsageRecords { get; set; }
    public virtual ICollection<Payment> Payments { get; set; }

}

public enum BillingCycle
{
    Monthly,
    Annual
}

public enum SubscriptionStatus
{
    Active,
    PastDue,
    Cancelled,
    Expired,
    TrialPeriod
}