using hrconnectbackend.Data;
using hrconnectbackend.Models;
using hrconnectbackend.Models.RequestModel;
using hrconnectbackend.Services.Clients;

namespace hrconnectbackend.Interface.Services.Clients;

public interface IAuthService
{
    Task<AuthResponse> Signin(string email, string password, bool rememberMe);
    Task<UserAccount> SignUp(CreateUser user);
    Task<bool> ChangePassword(string email, string password);
    Task<IEnumerable<UserAccount>> GetUsers(int tenantId);
    Task<RefreshToken> GetRefreshToken(int userId);
    Task<string> GenerateAccessToken(string refreshToken);
    Task<RefreshToken> LogoutRefreshToken(string refreshToken);
    Task<RefreshToken> GetRefreshToken(string refreshToken);
    Task UpdateRefreshToken(Func<DataContext, Task> update);
}