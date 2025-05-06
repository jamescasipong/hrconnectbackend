namespace hrconnectbackend.Middlewares
{
    public class SubscriptionMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context) 
        {
            var organizationId = context.User.FindFirst("organizationId")?.Value;

            if (organizationId == null)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { message = "Unauthorized", status = false });
                return;
            }

            if (!int.TryParse(organizationId, out var orgId))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new { message = "Invalid organization ID", status = false });
                return;
            }

            await next(context);
        }
    }
}
