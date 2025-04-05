namespace hrconnectbackend.Helper.CustomExceptions;

public class TransactionException: Exception
{
    public TransactionException(string message): base(message)
    {
        
    }
}