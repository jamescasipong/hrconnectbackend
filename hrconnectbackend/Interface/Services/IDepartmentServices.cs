using hrconnectbackend.Interface.Repositories;
using hrconnectbackend.Models;

namespace hrconnectbackend.Interface.Services;

public interface IDepartmentServices : IGenericRepository<Department>
{
    Task UpdateEmployeeDepartment(int employeeId, int departmentId);
    Task UpdateDepartmentByAsync(Department department);
    Task<Department> GetDepartmentByManagerId(int id);
    Task AddEmployeToDepartment(int employeeId, int departmentId);
    Task AddSupervisor(int employeeId, int departmentId);
}