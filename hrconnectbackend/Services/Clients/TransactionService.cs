using hrconnectbackend.Data;
using hrconnectbackend.Interface.Services.Clients;

namespace hrconnectbackend.Services.Clients
{
    public class TransactionService : ITransactionService
    {
        private readonly DataContext _context;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(DataContext context, ILogger<TransactionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task ExecuteAsync(Func<Task> action, Action<Exception>? onError = null)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await action();
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                onError?.Invoke(ex);
                throw;
            }
        }

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> action, Action<Exception>? onError = null)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var result = await action();
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return result;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                onError?.Invoke(ex);
                throw;
            }
        }
    }

}
