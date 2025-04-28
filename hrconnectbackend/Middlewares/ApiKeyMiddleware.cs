namespace hrconnectbackend.middlewares;
public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private const string APIKEYNAME = "X-API-KEY";
    private readonly ILogger<ApiKeyMiddleware> _logger;

    public ApiKeyMiddleware(RequestDelegate next, ILogger<ApiKeyMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var keys = context.Request.Headers.Authorization.ToString().Replace("X-API-KEY ", "");
        var ipAddress = context.Connection.RemoteIpAddress;
        var userAgent = context.Request.Headers["User-Agent"];

        var informations = new {
            IpAddress = ipAddress,
            UserAgent = userAgent
        };

        _logger.LogInformation($"Request from: {informations}");

        _logger.LogInformation($"Key: {keys}");

        if (string.IsNullOrEmpty(keys))
        {
            _logger.LogError("API Key is not set");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { message = "Internal Server Error", error = "API Key is not set" });
            return;
        }

        var appSettings = context.RequestServices.GetRequiredService<IConfiguration>();
        

        var apiKey = appSettings.GetSection("X-API-KEY").Get<List<string>>();
        if (apiKey == null || !apiKey.Any())
        {
            _logger.LogError("API Key is not set");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { message = "Internal Server Error", error = "API Key is not set" });
            return;
        }   

        if (!apiKey.Any(k => k == keys))
        {
            _logger.LogError("Invalid API Key");
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { message = "Unauthorized", error = "Invalid API Key" });
            return;
        }

        await _next(context);
    }
}

public static class ApiKeyMiddlewareExtensions
{
    public static IApplicationBuilder UseApiKeyMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ApiKeyMiddleware>();
    }
}