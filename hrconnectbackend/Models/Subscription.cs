using System.ComponentModel.DataAnnotations;

namespace hrconnectbackend.Models;

public class Subscription
{
    [Key]
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int SubscriptionId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    
    public SubscriptionPlan? SubscriptionPlan { get; set; }
    public Organization? Organization { get; set; }
}