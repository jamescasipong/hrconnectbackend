namespace hrconnectbackend.Interface.Services.Clients
{
    public interface ITransactionService
    {
        Task ExecuteAsync(Func<Task> action, Action<Exception>? onError = null);
        Task<T> ExecuteAsync<T>(Func<Task<T>> action, Action<Exception>? onError = null);
    }

}
