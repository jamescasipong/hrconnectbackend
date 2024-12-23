using hrconnectbackend.Models;

namespace hrconnectbackend.IRepositories;

public interface IAuthRepositories : IGenericRepository<Auth>
{
    Task<Auth> GetAuthByEmail(string email);
    Task<string> ConfirmPhone(int id, int code);
    Task<string> ConfirmEmail(int id, int code);
    Task GenerateOTP(int id);
    Task<string> VerifyOTP(int id, int code);
}