namespace hrconnectbackend.Models.RequestModel;


public record NewTokenResponse(string AccessToken);
public record Signin(string Email, string Password);
public record InputEmail(string Email);
public record SigninVerification(string Email, int Code);
public record Signup(string FirstName, string LastName, string Email, string Password);
public record CreateOrganization(string Name, string Address, string ContactEmail, DateTime CreatedAt, bool IsActive);
public record CreateUser(string UserName, string Email, int OrganizationId, string Password, UserRole Role);
