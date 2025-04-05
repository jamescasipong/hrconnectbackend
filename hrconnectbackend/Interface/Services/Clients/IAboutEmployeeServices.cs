using hrconnectbackend.Interface.Repositories;
using hrconnectbackend.Models;

namespace hrconnectbackend.Interface.Services.Clients
{
    public interface IAboutEmployeeServices : IGenericRepository<AboutEmployee>
    {
        Task<EducationBackground> AddEducationBackgroundAsync(EducationBackground educationBackground);
        Task<List<EducationBackground>> GetEmployeeEducationBackgroundAsync(int id);

    }
}
