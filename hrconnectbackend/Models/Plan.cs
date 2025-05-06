using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hrconnectbackend.Models;

public class Plan
{
    [Key]
    public int PlanId { get; set; }
    [Required, StringLength(50)]
    public string Name { get; set; }
    [Required, StringLength(500)]
    public string Description { get; set; }
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal MonthlyPrice { get; set; }
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal AnnualPrice { get; set; }
    [Required]
    public bool IsActive { get; set; }


    public virtual ICollection<PlanFeature>? Features { get; set; }
    public virtual ICollection<Subscription>? Subscriptions { get; set; }
}

public class PlanFeature
{
    [Key]
    public int FeatureId { get; set; }

    public int PlanId { get; set; }

    [Required, StringLength(100)]
    public string FeatureName { get; set; }

    [Required, StringLength(500)]
    public string Description { get; set; }

    // For features with limits (e.g., "Up to 10 projects")
    public int? Limit { get; set; }

    // Navigation property
    public virtual Plan Plan { get; set; }
}