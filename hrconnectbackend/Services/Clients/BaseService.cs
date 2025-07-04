
using hrconnectbackend.Data;

public class BaseService
{
    private readonly DataContext _context;

    public BaseService(DataContext context)
    {
        _context = context;
    }

    protected DataContext Context => _context;


    public async Task ExecuteTransactionAsync(Func<DataContext, Task> action, string? exceptionMessage = null)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            await action(Context);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            
            if (!string.IsNullOrEmpty(exceptionMessage))
            {
                throw new Exception(exceptionMessage);
            }
            throw;
        }
    }
    
    public async Task<T> ExecuteTransactionAsync<T>(Func<DataContext, Task<T>> action, string? exceptionMessage = null)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            T result = await action(Context);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return result;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();

            if (!string.IsNullOrEmpty(exceptionMessage))
            {
                throw new Exception(exceptionMessage);
            }
            throw;
        }
    }
}