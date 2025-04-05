using System.Security.Claims;
using System.Transactions;
using hrconnectbackend.Config;
using hrconnectbackend.Data;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Models;
using hrconnectbackend.Models.EmployeeModels;
using hrconnectbackend.Services.ExternalServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace hrconnectbackend.Services.Clients;

public class AuthService(
    DataContext context,
    IConfiguration configuration,
    IOptions<JwtSettings> jwtSettings,
    IEmployeeServices employeeServices,
    ISubscriptionServices subscriptionServices,
    IUserAccountServices accountServices,
    ILogger<AuthService> logger)
    : IAuthService
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;
    private readonly ISubscriptionServices _subscriptionServices = subscriptionServices;

    public async Task<AuthResponse> Signin(string email, string password, bool remember)
    {
        var emp = context.Employees.Include(a => a.UserAccount).Where(a => a.Email == email);
        
        if (emp == null || !emp.Any()) return null;

        var user = await emp.Select(a => a.UserAccount).FirstOrDefaultAsync();

        if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
        {
            return null;
        }

        var employee = await emp.FirstOrDefaultAsync();
        
        if (employee == null) return null;

        List<Claim> claims = await GetUserClaims(employee, user);

        DateTime dateTime = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessExpiration);
        string accessToken = GenerateAcessToken(claims, dateTime);
        string refreshToken = await GenerateRefreshToken(user.UserId, remember);

        var aes = new AES256Encrpytion(configuration.GetValue<string>("EncryptionSettings:Key")!);
        var encrypted = aes.Encrypt(accessToken);
        
        return new AuthResponse(encrypted, refreshToken);
    }

    private async Task<List<Claim>> GetUserClaims(Employee employee, UserAccount user)
    {
        var claims = new List<Claim>
        {
            // new Claim(ClaimTypes.Role, employee.IsAdmin ? "Admin" : "User"),
            new Claim(ClaimTypes.Email, employee.Email!),
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.UserName)
        };

        var empDept = await context.Employees.Include(a => a.EmployeeDepartment)
            .Where(a => a.UserId == employee.UserId).Select(a => a.EmployeeDepartment).Include(a => a.Department)
            .Select(a => a.Department).FirstOrDefaultAsync();

        if (empDept != null)
        {
            claims.Add(new Claim("Department", empDept.DeptName));
        }
        
        return claims;
    }

    public async Task<string> GenerateAccessToken(string refreshToken)
    {
        var user = await accountServices.GetUserAccountByRefreshToken(refreshToken);
        
        if (user == null) return string.Empty;

        var isTokenActive = user.RefreshTokens!.FirstOrDefault(x => x.RefreshTokenId == refreshToken && x.IsActive);

        if (isTokenActive == null)
        {
            return string.Empty;
        }
        
        var employeeByEmail = await employeeServices.GetEmployeeByEmail(user.Email!);

        var claims = await GetUserClaims(employeeByEmail, user);
        
        
        logger.LogInformation($"claims", claims);

        DateTime exp = DateTime.UtcNow.ToLocalTime().AddMinutes(_jwtSettings.AccessExpiration);
        // DateTime utcTime = DateTime.UtcNow.AddMinutes(15); // This gets the current time in UTC
        // TimeZoneInfo myTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time"); // For UTC+8
        // DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, myTimeZone);
        
        logger.LogInformation($"Date: {exp}");
        
        string token = GenerateAcessToken(claims, exp);
        
        logger.LogInformation($"token: {token}");

        return token;
    }
    
    
    private string GenerateAcessToken(List<Claim> claims, DateTime dateTime)
    {
        JwtService jwtService = new JwtService(_jwtSettings.Key, _jwtSettings.Issuer, _jwtSettings.Audience);
    
        var token = jwtService.GenerateToken(claims, dateTime);
        return token;
    }

    private async Task<string> GenerateRefreshToken(int userId, bool remember)
    {
        string refreshToken = Guid.NewGuid().ToString();
        
        var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            await context.RefreshTokens.ExecuteDeleteAsync();
            await context.SaveChangesAsync();
            // $"backend_rt_{Guid.NewGuid().ToString()}"
            var newRefreshToken = new RefreshToken
            {
                RefreshTokenId = refreshToken,
                UserId = userId,
                CookieName = $"{Guid.NewGuid().ToString()}",
                IsActive = true,
                Expires = remember
                    ? DateTime.UtcNow.ToLocalTime().AddMinutes(_jwtSettings.RefreshExpirationRemember)
                    : DateTime.UtcNow.ToLocalTime().AddMinutes(_jwtSettings.RefreshExpiration)
            };
        
            await context.RefreshTokens.AddAsync(newRefreshToken);
            await context.SaveChangesAsync();
            
            await transaction.CommitAsync();
        
            return refreshToken;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            
            throw new TransactionException("Something went wrong", ex);
        }
    }

    public async Task<RefreshToken> GetRefreshToken(int userId)
    {
        var refresh = await context.RefreshTokens.OrderByDescending(a => a.CreateAt).FirstOrDefaultAsync(a => a.UserId == userId);
        
        return refresh;
    }

    public async Task<RefreshToken> LogoutRefreshToken(string refreshToken)
    {
        var refresh = await context.RefreshTokens.FirstOrDefaultAsync(a => a.RefreshTokenId == refreshToken);
        
        if (refresh == null) return null;
        
        refresh.IsActive = false;
        context.RefreshTokens.Update(refresh);
        await context.SaveChangesAsync();
        
        return refresh;
    }

    public async Task<RefreshToken> GetRefreshToken(string refreshToken)
    {
        var existingToken = await context.RefreshTokens.FirstOrDefaultAsync(a => a.RefreshTokenId == refreshToken);

        return existingToken;
    }

    public async Task UpdateRefreshToken(Func<DataContext, Task> update)
    {
        await update(context);
    }

}

public class AuthResponse
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public AuthResponse(string accessToken, string refreshToken)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
    }
}