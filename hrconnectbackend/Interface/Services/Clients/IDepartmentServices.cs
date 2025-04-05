using hrconnectbackend.Interface.Repositories;
using hrconnectbackend.Models;

namespace hrconnectbackend.Interface.Services.Clients;

public interface IDepartmentServices : IGenericRepository<Department>
{
    Task UpdateEmployeeDepartment(int employeeId, int departmentId);
    Task UpdateDepartmentByAsync(Department department);
    Task AddEmployeToDepartment(int employeeId, int departmentId);
    Task AddSupervisor(int employeeId, int departmentId);
    Task<Department?> GetDepartmentByEmployee(int id);
    Task<EmployeeDepartment?> UpdateEmployeeDepartmentSupervisor(int employeeId, int departmentId);
    Task<EmployeeDepartment?> GetEmployeeDepartment(int departmentId);
    Task<EmployeeDepartment?> GetDepartmentByManagerId(int id);
    Task<List<EmployeeDepartment>> RetrieveDepartment(int? pageIndex, int? pageSize);
}