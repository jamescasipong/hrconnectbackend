using Microsoft.AspNetCore.JsonPatch;

namespace hrconnectbackend.Interface.Repositories;

public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<List<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task UpdatePartialAsync(T entity, JsonPatchDocument patchDoc);
    Task SaveChangesAsync();
}