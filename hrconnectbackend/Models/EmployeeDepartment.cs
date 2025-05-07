using hrconnectbackend.Models.DTOs;
using hrconnectbackend.Models.EmployeeModels;
using System.Text.Json.Serialization;

namespace hrconnectbackend.Models;

public class EmployeeDepartment
{
    public int Id { get; set; }
    public required int DepartmentId { get; set; } // foreign-primary key
    public required int SupervisorId { get; set; } // foreign-primary key
    public int OrganizationId { get; set; }
    public Organization? Organization { get; set; } // foreign key
    public Department? Department { get; set; }
    public List<Employee>? Employees { get; set; }
    
}

public class ReadEmployeeDepartmentDTO
{
    public int Id { get; set; }
    public int DepartmentId { get; set; }
    public int SupervisorId { get; set; }
    public int OrganizationId { get; set; }
    public OrganizationsDto? Organization { get; set; } // foreign key
    public ReadDepartmentDto? Department { get; set; }
    public List<ReadEmployeeDto>? Employees { get; set; }

}