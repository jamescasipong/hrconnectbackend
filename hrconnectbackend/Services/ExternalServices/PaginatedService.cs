using System.Linq.Expressions;
using hrconnectbackend.Data;
using Microsoft.EntityFrameworkCore;

public class PaginatedService<T> : IPaginatedService<T> where T : class
{
    private readonly DataContext _context;
    private readonly DbSet<T> _dbSet;

    public PaginatedService(DataContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<PagedResponse<IEnumerable<T>>> GetPaginatedAsync(
        PaginationParams paginationParams,
        Expression<Func<T, bool>> filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        string includeProperties = "")
    {
        IQueryable<T> query = _dbSet;

        if (filter != null)
        {
            query = query.Where(filter);
        }

        foreach (var includeProperty in includeProperties.Split
            (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        {
            query = query.Include(includeProperty);
        }

        if (orderBy != null)
        {
            query = orderBy(query);
        }

        var count = await query.CountAsync();
        var items = await query
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync();

        return new PagedResponse<IEnumerable<T>>(items, new PaginationDetails(paginationParams.PageNumber, paginationParams.PageSize, count));
    }
}