using hrconnectbackend.Data;
using hrconnectbackend.Models;
using hrconnectbackend.Services;
using hrconnectbackend.Services.Clients;

namespace hrconnectbackend.Interface.Services;

public interface IAuthService
{
    Task<AuthResponse> Signin(string email, string password, bool rememberMe);
    Task<RefreshToken> GetRefreshToken(int userId);
    Task<string> GenerateAccessToken(string refreshToken);
    Task<RefreshToken> LogoutRefreshToken(string refreshToken);
    Task<RefreshToken> GetRefreshToken(string refreshToken);
    Task UpdateRefreshToken(Func<DataContext, Task> update);
}