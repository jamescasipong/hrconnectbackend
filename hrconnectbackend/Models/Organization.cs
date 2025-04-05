using System.ComponentModel.DataAnnotations;

namespace hrconnectbackend.Models;

public class Organization
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public string ContactEmail { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public virtual ICollection<Subscription>? Subscriptions { get; set; }
    public virtual ICollection<UserAccount>? Users { get; set; }
}
