using System.IdentityModel.Tokens.Jwt;

namespace hrconnectbackend.Middlewares;
public class TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Extract the JWT token from the cookie
        var token = context.Request.Cookies["auth_session"];

        if (string.IsNullOrEmpty(token))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Authentication token is missing.");
            return;
        }

        // Validate the JWT token
        var tenantId = ValidateJwtToken(token);

        if (string.IsNullOrEmpty(tenantId))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Invalid token.");
            return;
        }

        // Store the TenantId in HttpContext.Items for later use
        context.Items["TenantId"] = tenantId;

        // Continue processing the request
        await next(context);
    }

    private string ValidateJwtToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            // Extract the TenantId from the JWT claims
            var tenantIdClaim = jsonToken?.Claims.FirstOrDefault(c => c.Type == "TenantId");

            return tenantIdClaim?.Value;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Token validation failed.");
            return null;
        }
    }
}

public static class TenantMiddlewareExtensions {
    public static IApplicationBuilder UseTenantMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TenantMiddleware>();
    }
}