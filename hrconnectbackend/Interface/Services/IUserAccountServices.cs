using hrconnectbackend.Interface.Repositories;
using hrconnectbackend.Models;

namespace hrconnectbackend.Interface.Services;

public interface IUserAccountServices : IGenericRepository<UserAccount>
{
    Task<UserAccount> GetUserAccountByEmail(string email);
    Task<string> RetrievePassword(string email);
    Task<string> ConfirmPhone(int id, int code);
    Task<string> ConfirmEmail(int id, int code);
    Task GenerateOTP(int id);
    Task<string> VerifyOTP(int id, int code);
    Task<string> RetrieveUsername(string email);
}