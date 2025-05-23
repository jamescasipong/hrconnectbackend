using hrconnectbackend.Interface.Repositories;
using hrconnectbackend.Models;
using hrconnectbackend.Models.Sessions;

namespace hrconnectbackend.Interface.Services.Clients;

public interface IUserAccountServices : IGenericRepository<UserAccount>
{
    Task<UserAccount?> CreateUserAccount(int? organizationId, UserAccount userAccount);
    Task<UserAccount?> CreateEmployeeUserAccount(UserAccount userAccount, int employeeId);
    Task<UserAccount?> AutomateCreateUserAccount(UserAccount userAccount);
    Task<UserAccount?> GetUserAccountByEmail(string email);
    Task<UserAccount?> GetUserAccountByRefreshToken(string refreshToken);
    Task<string> RetrievePassword(string email);
    Task<string> GenerateOtp(int id, DateTime expiry);
    Task VerifyOtp(int id, int code);
    Task<string> RetrieveUsername(string email);
    Task<bool> IsVerified(string email);
    Task UpdateEmail(int employeeId, string email);
    Task ResetTokenExist(string token);
    Task<ResetPasswordSession?> GetResetPasswordSession(string token);
    Task CreatePasswordSession (ResetPasswordSession resetPasswordSession);
    Task DeleteResetPassword(string token);
}