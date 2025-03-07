using hrconnectbackend.Interface.Repositories;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;

namespace hrconnectbackend.Interface.Services
{
    public interface IEmployeeServices: IGenericRepository<Employee>
    {
        Task<Employee> GetEmployeeByEmail(string email);
        Task<Employee> GetEmployeeById(int id);
        List<Employee> GetEmployeesPagination(List<Employee> employees, int? pageIndex, int? pageSize);
        Task<List<Employee>> GetEmployeeByDepartment(int deptId, int? pageIndex, int? pageSize);
        Task<List<Employee>> GetSubordinates(int id);
        Task CreateEmployee(CreateEmployeeDTO employee);
    }
}
