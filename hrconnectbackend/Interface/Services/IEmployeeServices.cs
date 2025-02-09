using hrconnectbackend.Interface.Repositories;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;

namespace hrconnectbackend.Interface.Services
{
    public interface IEmployeeServices: IGenericRepository<Employee>
    {
        Task<Employee> GetEmployeeByEmail(string email);
        Task<List<Employee>> GetEmployeesPagination(int page, int quantity);
        Task<List<Employee>> GetEmployeeByDepartment(int deptId);
        Task<List<Employee>> GetSubordinates(int id);
        Task CreateEmployee(CreateEmployeeDTO employee);
    }
}
