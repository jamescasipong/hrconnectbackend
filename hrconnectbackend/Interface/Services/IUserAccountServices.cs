using hrconnectbackend.Interface.Repositories;
using hrconnectbackend.Models;

namespace hrconnectbackend.Interface.Services;

public interface IUserAccountServices : IGenericRepository<UserAccount>
{
    Task<UserAccount> GetUserAccountByEmail(string email);
    Task<string> RetrievePassword(string email);
    Task GenerateOTP(int id);
    Task VerifyOTP(int id, int code);
    Task<string> RetrieveUsername(string email);
    Task<bool> IsVerified(string email);
    Task UpdateEmail(int employeeId, string email);
}