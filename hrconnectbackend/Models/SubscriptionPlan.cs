using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hrconnectbackend.Models;

public class SubscriptionPlan
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Price { get; set; }
    public int TenantId { get; set; }
    public int DurationDays { get; set; }
    public bool IsActive { get; set; }
    
    public List<Subscription>? Subscriptions { get; set; }
}