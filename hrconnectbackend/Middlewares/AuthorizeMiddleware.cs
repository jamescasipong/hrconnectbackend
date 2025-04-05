namespace hrconnectbackend.Helper;

public class AuthorizationMiddleware
{
    private readonly RequestDelegate _next;

    public AuthorizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {

        // Example of custom authorization check
        var user = context.User;


        if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
        {
            context.Response.StatusCode = 401; // Unauthorized
            await context.Response.WriteAsJsonAsync(new
            {
                message = "Unauthorized"
            });
            return;
        }

        // If authorized, continue to the next middleware
        await _next(context);
    }
}
