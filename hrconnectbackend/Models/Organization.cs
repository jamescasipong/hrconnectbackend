using System.ComponentModel.DataAnnotations;

namespace hrconnectbackend.Models;

public class Organization
{
    [Key]
    public int Id { get; set; }
    [Required]
    [StringLength(100, ErrorMessage = "Organization name cannot be longer than 100 characters.")]
    public string Name { get; set; } = string.Empty;
    [StringLength(500, ErrorMessage = "Address cannot be longer than 500 characters.")]
    public string Address { get; set; } = string.Empty;
    [Required]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    [StringLength(100, ErrorMessage = "Contact email cannot be longer than 100 characters.")]
    public string ContactEmail { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    

    public Organization Copy()
    {
        return new Organization
        {
            Id = Id,
            Name = Name,
            Address = Address,
            ContactEmail = ContactEmail,
            CreatedAt = CreatedAt,
            IsActive = IsActive,
        };
    }

    public void Deconstruct(out string name, out string address, out string contactEmail, out DateTime createdAt)
    {
        name = Name;
        address = Address;
        contactEmail = ContactEmail;
        createdAt = CreatedAt;
    }

    public virtual ICollection<Subscription>? Subscriptions { get; set; }
    public virtual ICollection<UserAccount>? Users { get; set; }
}
