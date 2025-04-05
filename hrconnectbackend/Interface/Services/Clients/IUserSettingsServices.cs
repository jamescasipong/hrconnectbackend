using hrconnectbackend.Interface.Repositories;
using hrconnectbackend.Models;

namespace hrconnectbackend.Interface.Services
{
    public interface IUserSettingsServices: IGenericRepository<UserSettings>
    {
        Task CreateDefaultSettings(int employeeId);
        Task ResetSettings(int employeeId);
    }
}
