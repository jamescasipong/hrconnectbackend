using hrconnectbackend.Interface.Repositories;
using hrconnectbackend.Models;

namespace hrconnectbackend.Interface.Services;

public interface IDepartmentServices : IGenericRepository<Department>
{

    Task UpdateDepartmentByAsync(Department department);
    Task<Department> GetDepartmentByManagerId(int id);
}