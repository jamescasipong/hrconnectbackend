﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AutoMapper;
using Azure;
using hrconnectbackend.Config.Settings;
using hrconnectbackend.Data;
using hrconnectbackend.Exceptions;
using hrconnectbackend.Interface.Services.Clients;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs.AuthDTOs;
using hrconnectbackend.Models.RequestModel;
using hrconnectbackend.Services.ExternalServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TransactionException = System.Transactions.TransactionException;

namespace hrconnectbackend.Services.Clients;

public class AuthService(
    DataContext context,
    IConfiguration configuration,
    IOptions<JwtSettings> jwtSettings,
    IUserAccountServices accountServices,
    ILogger<AuthService> logger,
    IMapper mapper)
    : IAuthService
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;

    public async Task<AuthResponse?> Signin(string email, string password, bool remember)
    {
        var user = await context.UserAccounts.Include(a => a.Employee).FirstOrDefaultAsync(a => a.Email == email);
        
        if (user == null) return null;
        

        if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
        {
            return null;
        }

        var userDto = mapper.Map<UserAccountDto>(user);

        List<Claim> claims = await GetUserClaims(userDto);

        DateTime dateTime = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessExpiration);
        string accessToken = GenerateAcessToken(user, dateTime);
        string refreshToken = await GenerateRefreshToken(user, remember);

        // var aes = new AES256Encrpytion(configuration.GetValue<string>("EncryptionSettings:Key")!);
        // var encrypted = aes.Encrypt(accessToken);
        
        return new AuthResponse(accessToken, refreshToken);
    }

    public async Task<UserAccount?> SignUpAdmin(UserAccount userAccount)
    {
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(userAccount.Password);

        try
        {
            var newUser = new UserAccount
            {
                UserName = userAccount.UserName,
                Password = hashedPassword,
                Email = userAccount.Email,
                OrganizationId = null,
                Role = "Admin",
            };

            var createUser = await accountServices.AddAsync(newUser);

            return createUser;

        }
        catch (Exception ex)
        {
            throw new Exception($"Error creating user account: {ex.Message}", ex);
        }
    }

    public async Task<UserAccount?> SignUpEmployee(CreateUser user)
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
            Role = "Employee"
        };
        
        await context.UserAccounts.AddAsync(employee);
        await context.SaveChangesAsync();

        return employee;
    }

    public async Task<UserAccount?> SignUpOperator(CreateUserOperator user)
    {
        var employee = new UserAccount
        {
            // UserId = GenerateRandomNumber(0, int.MaxValue),
            UserName = user.UserName,
            Email = user.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(user.Password),
            EmailVerified = false,
            SmsVerified = false,
            OrganizationId = null,
            ChangePassword = false,
            Role = "Operator"
        };
        
        if (employee == null) return null;

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

    private Task<List<Claim>> GetUserClaims(UserAccountDto user)
    {
        var claims = new List<Claim>
        {
            new Claim("Subscription", "Premium"),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("Role", user.Role),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.UserName)
        };

        if (user.OrganizationId.HasValue)
        {
            claims.Add(new Claim("organizationId", user.OrganizationId.Value.ToString()));
        }

        if (user.Employee != null)
        {
            logger.LogInformation($"EmployeeId: {user.Employee.Id}");
            claims.Add(new Claim("EmployeeId", user.Employee.Id.ToString()));
        }
        else
        {
            logger.LogWarning($"Employee is missing");
        }

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

    public ClaimsPrincipal? GetPrincipalFromAccessToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false, // optional: set true if you use Issuer
                ValidateAudience = false, // optional: set true if you use Audience
                ValidateLifetime = false,
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
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
        
        DateTime exp = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessExpiration);
        
        logger.LogInformation($"Date: {exp}");
        
        string token = GenerateAcessToken(user, exp);
        
        logger.LogInformation($"token: {token}");

        return token;
    }
    
    private string GenerateAcessToken(UserAccount user, DateTime dateTime)
    {
        var userDto = mapper.Map<UserAccountDto>(user);

        var userClaims = GetUserClaims(userDto).Result;

        JwtService jwtService = new JwtService(_jwtSettings.Key, _jwtSettings.Issuer, _jwtSettings.Audience);
    
        var token = jwtService.GenerateToken(userClaims, dateTime);
        return token;
    }

    public async Task<AuthResponse> GenerateTokens(UserAccount user)
    {
        DateTime exp = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessExpiration);

        string accessToken = GenerateAcessToken(user, exp);
        string refreshToken = await GenerateRefreshToken(user, false);
        return new AuthResponse(accessToken, refreshToken);
    }

    public void SetAccessTokenCookie(AuthResponse tokens, HttpResponse response)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessExpiration)
        };

        response.Cookies.Append("at_session", tokens.AccessToken, new CookieOptions
        {
            HttpOnly = true,  // Secure from JavaScript (prevent XSS)
            SameSite = SameSiteMode.None, // Prevent CSRF attacks
            Secure = true,
            Path = "/",
            Expires = DateTime.Now.AddMinutes(_jwtSettings.AccessExpiration)
        });

        // Append refresh token to response cookies
        response.Cookies.Append("backend_rt", tokens.RefreshToken, new CookieOptions
        {
            HttpOnly = true,  // Secure from JavaScript (prevent XSS)
            SameSite = SameSiteMode.None, // Prevent CSRF attacks
            Secure = true,
            Path = "/",
            Expires = DateTime.Now.AddMinutes(_jwtSettings.RefreshExpiration)
        });
    }

    private async Task<string> GenerateRefreshToken(UserAccount user, bool remember)
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
                UserId = user.UserId,
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