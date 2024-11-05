using hrconnectbackend.Models;

namespace hrconnectbackend.IRepositories
{
    public interface IEmployeeRepositories
    {
        Task<Employee> GetEmployeeByIdAsync(int id);
        Task<ICollection<Employee>> GetAllEmployeesAsync();
        Task AddEmployeeAsync(Employee employee);
        Task UpdateEmployeeAsync(Employee employee);
        Task DeleteEmployeeAsync(int id);
    }
}
