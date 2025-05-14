using hrconnectbackend.Exceptions;
using hrconnectbackend.Models.Response;
using System.Net;

namespace hrconnectbackend.Middlewares
{
    public class ErrorExceptionMiddleware(RequestDelegate next, ILogger<ErrorExceptionMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (ValidationException ex)
            {
                await HandleValidationException(context, ex);
            }
            catch (ApiException ex)
            {
                await HandleApiException(context, ex);
            }
            catch (Exception ex)
            {
                await HandleUnknownException(context, ex);
            }
        }

        private async Task HandleValidationException(HttpContext context, ValidationException ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new ErrorResponse(ex.ErrorCode, ex.Message));

        }

        private async Task HandleApiException(HttpContext context, ApiException ex)
        {
            logger.LogError(ex, "API error occurred: {Message}", ex.Message);
            context.Response.StatusCode = (int)ex.StatusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new ErrorResponse(ex.ErrorCode, ex.Message));
        }

        private async Task HandleUnknownException(HttpContext context, Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred: {Message}", ex.Message);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new ErrorResponse("INTERNAL_SERVER_ERROR", "An unexpected error occurred."));
        }
    }
}
