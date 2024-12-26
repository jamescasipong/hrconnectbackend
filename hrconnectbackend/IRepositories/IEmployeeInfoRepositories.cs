using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;

namespace hrconnectbackend.IRepositories
{
    public interface IEmployeeInfoRepositories : IGenericRepository<EmployeeInfo>
    {

        Task AddEmployeeInfoAsync(EmployeeInfo employeeInfo);
        Task<EducationBackground> AddEducationBackgroundAsync(EducationBackground educationBackground);
        Task<List<EducationBackground>> GetEmployeeEducationBackgroundAsync(int id);
        Task<EmployeeInfo> GetEmployeeInfoByIdAsync(int id);


    }
}
