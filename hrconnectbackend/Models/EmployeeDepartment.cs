using hrconnectbackend.Models.EmployeeModels;

namespace hrconnectbackend.Models;

public class EmployeeDepartment
{
    public int Id { get; set; }
    public required int DepartmentId { get; set; } // foreign-primary key
    public required int SupervisorId { get; set; } // foreign-primary key
    
    public Department? Department { get; set; }
    public Employee? Supervisor { get; set; }
    public List<Employee>? Employees { get; set; }
}