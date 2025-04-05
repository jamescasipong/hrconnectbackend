using System.Security.Claims;
using System.Transactions;
using hrconnectbackend.Config.Settings;
using hrconnectbackend.Data;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Interface.Services.Clients;
using hrconnectbackend.Models;
using hrconnectbackend.Models.RequestModel;
using hrconnectbackend.Services.ExternalServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace hrconnectbackend.Services.Clients;

public class AuthService(
    DataContext context,
    IConfiguration configuration,
    IOptions<JwtSettings> jwtSettings,
    IUserAccountServices accountServices,
    ILogger<AuthService> logger)
    : IAuthService
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;

    public async Task<AuthResponse?> Signin(string email, string password, bool remember)
    {
        var user = await context.UserAccounts.FirstOrDefaultAsync(a => a.Email == email);
        
        if (user == null) return null;


        if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
        {
            return null;
        }
        
        List<Claim> claims = await GetUserClaims(user);

        DateTime dateTime = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessExpiration);
        string accessToken = GenerateAcessToken(claims, dateTime);
        string refreshToken = await GenerateRefreshToken(user.UserId, remember);

        var aes = new AES256Encrpytion(configuration.GetValue<string>("EncryptionSettings:Key")!);
        var encrypted = aes.Encrypt(accessToken);
        
        return new AuthResponse(encrypted, refreshToken);
    }
    
    private static readonly Random Random = new Random();

    // Generate a random number between min and max (inclusive)
    public static int GenerateRandomNumber(int min, int max)
    {
        return Random.Next(min, max); // Generates a number between min and max-1
    }

    public async Task<UserAccount?> SignUp(CreateUser user)
    {
        var employee = new UserAccount
        {
            // UserId = GenerateRandomNumber(0, int.MaxValue),
            UserName = user.UserName,
            Email = user.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(user.Password),
            EmailVerified = false,
            SmsVerified = false,
            OrganizationId = user.OrganizationId,
            ChangePassword = false,
            Role = user.Role
        };
        
        await context.UserAccounts.AddAsync(employee);
        await context.SaveChangesAsync();

        return employee;
    }

    public async Task<bool> ChangePassword(string email, string password)
    {
        var userAccount = await context.UserAccounts.FirstOrDefaultAsync(a => a.Email == email);
        
        if (userAccount == null) return false;
        
        userAccount.Password = BCrypt.Net.BCrypt.HashPassword(password);
        
        // context.Entry(userAccount).State = EntityState.Modified;
        context.UserAccounts.Update(userAccount);
        await context.SaveChangesAsync();
        
        return true;
    }

    public async Task<IEnumerable<UserAccount>> GetUsers(int tenantId)
    {
        return await context.UserAccounts.Where(a => a.OrganizationId == tenantId).ToListAsync();
    }

    private Task<List<Claim>> GetUserClaims(UserAccount user)
    {
        var claims = new List<Claim>
        {
            new Claim("organizationId", user.UserId.ToString()),
            new Claim(ClaimTypes.Role, user.GetRoleAsString()),
            // new Claim(ClaimTypes.Role, employee.IsAdmin ? "Admin" : "User"),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.UserName)
        };

        // var empDept = await context.Employees.Include(a => a.EmployeeDepartment)
        //     .Where(a => a.UserId == user.UserId).Select(a => a.EmployeeDepartment).Include(a => a.Department)
        //     .Select(a => a.Department).FirstOrDefaultAsync();
        //
        // if (empDept != null)
        // {
        //     claims.Add(new Claim("Department", empDept.DeptName));
        // }
        
        return Task.FromResult(claims);
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
        
        var claims = await GetUserClaims(user);
        
        
        logger.LogInformation("claims: {claims}", claims);

        DateTime exp = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessExpiration);
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
                    ? DateTime.UtcNow.AddMinutes(_jwtSettings.RefreshExpirationRemember)
                    : DateTime.UtcNow.AddMinutes(_jwtSettings.RefreshExpiration)
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

    public async Task<RefreshToken?> GetRefreshToken(int userId)
    {
        var refresh = await context.RefreshTokens.OrderByDescending(a => a.CreateAt).FirstOrDefaultAsync(a => a.UserId == userId);
        
        return refresh;
    }

    public async Task<RefreshToken?> LogoutRefreshToken(string refreshToken)
    {
        var refresh = await context.RefreshTokens.FirstOrDefaultAsync(a => a.RefreshTokenId == refreshToken);
        
        if (refresh == null) return null;
        
        refresh.IsActive = false;
        context.RefreshTokens.Update(refresh);
        await context.SaveChangesAsync();
        
        return refresh;
    }

    public async Task<RefreshToken?> GetRefreshToken(string refreshToken)
    {
        var existingToken = await context.RefreshTokens.FirstOrDefaultAsync(a => a.RefreshTokenId == refreshToken);

        return existingToken;
    }

    public async Task UpdateRefreshToken(Func<DataContext, Task> update)
    {
        await update(context);
    }

}

public class AuthResponse(string accessToken, string refreshToken)
{
    public string AccessToken { get; set; } = accessToken;
    public string RefreshToken { get; set; } = refreshToken;
}