using hrconnectbackend.Interface.Repositories;
using hrconnectbackend.Models;

namespace hrconnectbackend.Interface.Services.Clients;

public interface IDepartmentServices : IGenericRepository<Department>
{
    Task<Department?> GetDepartmentByGuid(Guid guid);
    Task UpdateEmployeeDepartment(int employeeId, int departmentId);
    Task UpdateDepartmentByAsync(Department department);
    Task AddEmployeToDepartment(int employeeId, int departmentId);
    Task AddSupervisor(int employeeId, int departmentId);
    Task CreateEmployeeDepartment(int supervisorId, int departmentId, int organizationId);
    Task<Department?> GetDepartmentByEmployee(int id);
    Task<object> RetrieveDepartment(int organizationId);
    Task<EmployeeDepartment?> UpdateEmployeeDepartmentSupervisor(int employeeId, int departmentId);
    Task<EmployeeDepartment?> GetEmployeeDepartment(int departmentId);
    Task<EmployeeDepartment?> GetDepartmentByManagerId(int id);
    Task<List<EmployeeDepartment>> RetrieveEmployeeDepartments(int OrganizationId, int? pageIndex, int? pageSize);
}