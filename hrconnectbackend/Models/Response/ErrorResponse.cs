namespace hrconnectbackend.Models.Response
{
    public class ErrorResponse
    {
        public bool Success { get; } = false;
        public ErrorDetails Error { get; }

        public ErrorResponse(string code, string message, string type)
        {
            Error = new ErrorDetails(code, message, type);
        }

    }

    public record ErrorDetails(
        string Code,      // Machine-readable (e.g., "EMPLOYEE_NOT_FOUND")
        string Message,   // Human-readable
        string? ExceptionType = null // Exception type (optional)
    );
}
