using hrconnectbackend.Models;

namespace hrconnectbackend.IRepositories;

public interface IDepartmentRepositories : IGenericRepository<Department>
{

    Task UpdateDepartmentByAsync(Department department);
    Task<Department> GetDepartmentByManagerId(int id);
}