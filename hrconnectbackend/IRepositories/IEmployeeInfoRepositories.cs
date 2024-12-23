using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;

namespace hrconnectbackend.IRepositories
{
    public interface IEmployeeInfoRepositories
    {
        Task<EmployeeInfo> GetEmployeeInfoByIdAsync(int id);
        Task<ICollection<EmployeeInfo>> GetAllEmployeeInfosAsync();
        Task AddEmployeeInfoAsync(EmployeeInfo employeeInfo);

        
    }
}
