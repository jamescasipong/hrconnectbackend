using hrconnectbackend.Config.Settings;
using hrconnectbackend.Interface.Services.Clients;
using hrconnectbackend.Middlewares;
using hrconnectbackend.Services.Clients;
using Microsoft.Extensions.Options;

namespace hrconnectbackend.Middlewares
{
    public class RefreshTokenMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory, IOptions<JwtSettings> jwtSettings)
    {
        private readonly JwtSettings _jwtSettings = jwtSettings.Value;

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User?.Identity == null || !context.User.Identity.IsAuthenticated)
            {
                var refreshToken = context.Request.Cookies["backend_rt"];

                if (string.IsNullOrEmpty(refreshToken))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(new { message = "Unauthorized", status = false });
                    return;
                }

                using var scope = scopeFactory.CreateScope();
                var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

                var newAccessToken = await authService.GenerateAccessToken(refreshToken);

                if (newAccessToken == null)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(new { message = "Unauthorized", status = false });
                    return;
                }

                context.Response.Cookies.Append("at_session", newAccessToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Path = "/",
                    Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessExpiration)
                });

                var principal = authService.GetPrincipalFromAccessToken(newAccessToken);

                if (principal == null)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(new { message = "Unauthorized", status = false });
                    return;
                }

                context.User = principal;
            }

            await next(context);
        }

    }
}

    public static class RefreshTokenMiddlewareExtensions
{
    public static IApplicationBuilder UseRefreshTokenMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RefreshTokenMiddleware>();
    }
}