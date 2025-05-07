using hrconnectbackend.Models.EmployeeModels;

namespace hrconnectbackend.Models;

public class EmployeePosition
{
    public int Id { get; set; }
    public string Position { get; set; }  = string.Empty;
    public int OrganizationId { get; set; }

    public Organization? Organization { get; set; }
    public ICollection<Employee>? Employees { get; set; }
}