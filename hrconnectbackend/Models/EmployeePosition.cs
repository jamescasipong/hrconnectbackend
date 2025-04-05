using hrconnectbackend.Models.EmployeeModels;

namespace hrconnectbackend.Models;

public class EmployeePosition
{
    public int Id { get; set; }
    public string Position { get; set; }
    
    public ICollection<Employee>? Employees { get; set; }
}