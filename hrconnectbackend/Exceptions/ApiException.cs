using System.Net;

namespace hrconnectbackend.Exceptions
{
    public class ApiException : Exception
    {
        public string ErrorCode { get; }
        public string TypeException { get; set; } = null!;
        public HttpStatusCode StatusCode { get; }
        public ApiException(string message, HttpStatusCode statusCode, string errorCode) : base(message)
        {
            ErrorCode = errorCode;
            StatusCode = statusCode;
        }
    }
    // 404 Not Found
    public class NotFoundException : ApiException
    {
        public NotFoundException(string errorCode, string message)
            : base(errorCode, HttpStatusCode.NotFound, message)
        {
            TypeException = GetType().Name;
        }
    }

    // 400 Bad Request
    public class BadRequestException : ApiException
    {
        public BadRequestException(string errorCode, string message)
            : base(errorCode, HttpStatusCode.BadRequest, message) { }
    }

    public class UnauthorizedException : ApiException
    {
        public UnauthorizedException(string errorCode, string message)
            : base(errorCode, HttpStatusCode.Unauthorized, message)
        {
            TypeException = GetType().Name;

        }
    }

    // 403 Forbidden
    public class ForbiddenException : ApiException
    {
        public ForbiddenException(string errorCode, string message)
            : base(errorCode, HttpStatusCode.Forbidden, message)
        {
            TypeException = GetType().Name;

        }
    }

    // 409 Conflict
    public class ConflictException : ApiException
    {
        public ConflictException(string errorCode, string message)
            : base(errorCode, HttpStatusCode.Conflict, message)
        {
            TypeException = GetType().Name;

        }
    }

    // 500 Internal Server Error
    public class InternalServerErrorException : ApiException
    {
        public InternalServerErrorException(string errorCode, string message)
            : base(errorCode, HttpStatusCode.InternalServerError, message)
        {
            TypeException = GetType().Name;

        }
    }
}

