using System.ComponentModel.DataAnnotations;

namespace hrconnectbackend.Models;

public class Subscription
{
    [Key]
    public int Id { get; set; }
    public Guid Guid { get; set; }
    public int OrganizationId { get; set; }
    public int SubscriptionId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int DaysNotifiedAfterExpiration { get; set; } = 0;
    public bool AutoRenew { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public DateTime? CancellationDate { get; set; }
    public DateTime LastBillingDate { get; set; }
    public DateTime NextBillingDate { get; set; }
    
    public SubscriptionPlan? SubscriptionPlan { get; set; }
    public Organization? Organization { get; set; }
}