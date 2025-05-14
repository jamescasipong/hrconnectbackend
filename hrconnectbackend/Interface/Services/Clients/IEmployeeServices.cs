using hrconnectbackend.Interface.Repositories;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;
using hrconnectbackend.Models.DTOs.AuthDTOs;
using hrconnectbackend.Models.EmployeeModels;

namespace hrconnectbackend.Interface.Services.Clients
{
    public interface IEmployeeServices: IGenericRepository<Employee>
    {
        Task<Employee?> GetEmployeeByEmail(string email);
        Task<Employee?> GetEmployeeById(int id);
        List<Employee> GetEmployeesPagination(List<Employee> employees, int? pageIndex, int? pageSize);
        Task<List<Employee>> GetEmployeeByDepartment(int deptId, int? pageIndex, int? pageSize);
        Task<List<Employee>> GetSubordinates(int id);
        Task CreateEmployee(CreateEmployeeDto employee, int orgId, bool? createAccount = false);
        Task<List<Employee>> RetrieveEmployees(int? pageIndex, int? pageSize);
        Task<List<Employee>> GenerateEmployeesWithEmail(List<GenerateEmployeeDto> employeesDto, int orgId);
    }
}
