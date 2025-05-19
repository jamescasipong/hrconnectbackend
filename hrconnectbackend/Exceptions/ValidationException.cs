using System.Net;

namespace hrconnectbackend.Exceptions
{
    public class ValidationException : Exception
    {
        public string ErrorCode { get; }
        public string TypeException { get; set; } = null!;
        public ValidationException(string message, string errorCode) : base(message)
        {
            ErrorCode = errorCode;
            TypeException = GetType().Name;
        }
    }
}
