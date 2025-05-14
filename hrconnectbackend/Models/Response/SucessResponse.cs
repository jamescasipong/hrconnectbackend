namespace hrconnectbackend.Models.Response
{
    public class SuccessResponse<T>
    {
        public bool Success { get; } = true;
        public string? Message { get; } = null;
        public T Data { get; }

        public SuccessResponse(T data, string? message = null)
        {
            Message = message;
            Data = data;
        }
    }

    public class SuccessResponse
    {
        public bool Success { get; } = true;
        public string? Message { get; } = null;

        public SuccessResponse(string? message = null)
        {
            Message = message;
        }
    }
}