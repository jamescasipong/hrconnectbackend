using System.Runtime.Serialization;

[Serializable]
public sealed class CustomException : Exception
{
    public CustomException(string message) : base(message) { }
    public CustomException(string message, Exception innerException) : base(message, innerException) { }
    public CustomException(string message, string? errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }
    public CustomException(string message, string? errorCode, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }

    protected CustomException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        ErrorCode = info.GetString(nameof(ErrorCode));
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(ErrorCode), ErrorCode);
    }

    public string? ErrorCode { get; }
}