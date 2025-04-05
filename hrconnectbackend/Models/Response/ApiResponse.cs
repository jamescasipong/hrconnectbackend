namespace hrconnectbackend.Models.Response
{
    public class ApiResponse<T>(bool success, string message, T data = default)
    {
        public bool Success { get; set; } = success;
        public string Message { get; set; } = message;
        public T Data { get; set; } = data;
    }

    public class ApiResponse(bool success, string? message)
    {
        public bool Success { get; set; } = success;
        public string? Message { get; set; } = message;
    }
}
