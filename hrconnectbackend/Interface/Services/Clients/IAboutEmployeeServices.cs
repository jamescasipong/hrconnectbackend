using hrconnectbackend.Interface.Repositories;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;

namespace hrconnectbackend.Interface.Services
{
    public interface IAboutEmployeeServices : IGenericRepository<AboutEmployee>
    {
        Task<EducationBackground> AddEducationBackgroundAsync(EducationBackground educationBackground);
        Task<List<EducationBackground>> GetEmployeeEducationBackgroundAsync(int id);

    }
}
