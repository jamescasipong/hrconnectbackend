using System.ComponentModel.DataAnnotations;

namespace hrconnectbackend.Models;

public class Organization
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "Organization name cannot be longer than 100 characters.")]
    public string Name { get; set; }

    [StringLength(500, ErrorMessage = "Address cannot be longer than 500 characters.")]
    public string Address { get; set; }

    [Required]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    [StringLength(100, ErrorMessage = "Contact email cannot be longer than 100 characters.")]
    public string ContactEmail { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    public Organization Copy()
    {
        return new Organization
        {
            Id = this.Id,
            Name = this.Name,
            Address = this.Address,
            ContactEmail = this.ContactEmail,
            CreatedAt = this.CreatedAt,
            IsActive = this.IsActive,
        };
    }

    public void Deconstruct(out string name, out string address, out string contactEmail, out DateTime createdAt)
    {
        name = this.Name;
        address = this.Address;
        contactEmail = this.ContactEmail;
        createdAt = this.CreatedAt;
    }

    public virtual ICollection<Subscription>? Subscriptions { get; set; }
    public virtual ICollection<UserAccount>? Users { get; set; }
}
