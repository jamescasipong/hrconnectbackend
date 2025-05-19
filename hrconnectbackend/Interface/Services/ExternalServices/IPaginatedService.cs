using System.Linq.Expressions;

public interface IPaginatedService<T>
{
    Task<PagedResponse<IEnumerable<T>>> GetPaginatedAsync(PaginationParams paginationParams,
        Expression<Func<T, bool>> filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        string includeProperties = "");
}