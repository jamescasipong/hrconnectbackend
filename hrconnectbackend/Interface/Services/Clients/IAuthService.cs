using hrconnectbackend.Data;
using hrconnectbackend.Models;
using hrconnectbackend.Models.RequestModel;
using hrconnectbackend.Services.Clients;
using System.Security.Claims;

namespace hrconnectbackend.Interface.Services.Clients;

public interface IAuthService
{
    Task<AuthResponse?> Signin(string email, string password, bool rememberMe);
    Task<UserAccount?> SignUpAdmin(UserAccount user);
    Task<UserAccount> SignUpEmployee(CreateUser user);
    Task<UserAccount> SignUpOperator(CreateUserOperator user);
    Task<bool> ChangePassword(string email, string password);
    Task<IEnumerable<UserAccount>> GetUsers(int tenantId);
    Task<RefreshToken?> GetRefreshToken(int userId);
    Task<AuthResponse> GenerateTokens(UserAccount user);
    Task<string> GenerateAccessToken(string refreshToken);
    void SetAccessTokenCookie(AuthResponse tokens, HttpResponse response);
    Task<RefreshToken?> LogoutRefreshToken(string refreshToken);
    Task<RefreshToken?> GetRefreshToken(string refreshToken);
    Task UpdateRefreshToken(Func<DataContext, Task> update);
    ClaimsPrincipal? GetPrincipalFromAccessToken(string token);
}