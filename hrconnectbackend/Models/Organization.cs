using hrconnectbackend.Models.EmployeeModels;
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
    public string City { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string Zipcode { get; set; } = string.Empty;
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
            City = City,
            Website = Website,
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

    public virtual ICollection<Department>? Departments { get; set; }
    public virtual ICollection<LeaveBalance>? LeaveBalances { get; set; }
    public virtual ICollection<UserAccount>? Users { get; set; }
    public virtual ICollection<Subscription>? Subscriptions { get; set; }
    public virtual ICollection<Employee>? Employees { get; set; }
    public virtual ICollection<Payroll>? Payrolls { get; set; }
    public virtual ICollection<Shift>? Shifts { get; set; }
    public virtual ICollection<Leaves>? Leaves { get; set; }
    public virtual ICollection<AttendanceCertification>? AttendanceCertifications { get; set; }
    public virtual ICollection<EmployeeDepartment>? EmployeeDepartments { get; set; }
    public virtual ICollection<EmployeePosition>? EmployeePositions { get; set; }
    public virtual ICollection<LeaveApplication>? LeaveApplications { get; set; }
    public virtual ICollection<Notifications>? Notifications { get; set; }
}