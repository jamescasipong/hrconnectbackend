namespace hrconnectbackend.Models.Response
{
    public class ErrorResponse
    {
        public bool Success { get; } = false;
        public ErrorDetails Error { get; }

        public ErrorResponse(string code, string message, string? details = null)
        {
            Error = new ErrorDetails(code, message, details);
        }

    }

    public record ErrorDetails(
        string Code,      // Machine-readable (e.g., "EMPLOYEE_NOT_FOUND")
        string Message,   // Human-readable
        string? Details = null  // Debugging info (optional)
    );
}
