using hrconnectbackend.Data;
using hrconnectbackend.Interface.Repositories;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

namespace hrconnectbackend.Repository;

public class GenericRepository<T>(DataContext context) : IGenericRepository<T>
    where T : class
{
    protected readonly DataContext _context = context;

    public async Task<T?> GetByIdAsync(int id)
    {
        return await _context.Set<T>().FindAsync(id);
    }

    public async Task<List<T>> GetAllAsync()
    {
        return await _context.Set<T>().ToListAsync();
    }

    public async Task<T> AddAsync(T entity)
    {
        await _context.Set<T>().AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(T entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(T entity)
    {
        _context.Set<T>().Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdatePartialAsync(T entity, JsonPatchDocument patchDoc)
    {
        // Apply the patch document to the entity
        patchDoc.ApplyTo(entity);

        // Mark the changed properties as modified
        var entry = _context.Entry(entity);

        // Loop through the entity properties and mark only those modified by the patch
        foreach (var property in entry.OriginalValues.Properties)
        {
            // Get the original and current values for each property
            var originalValue = entry.OriginalValues[property];
            var currentValue = entry.CurrentValues[property];

            // If the value has changed, mark it as modified
            if (!object.Equals(originalValue, currentValue))
            {
                entry.Property(property.Name).IsModified = true;
            }
        }

        // Save the changes
        await _context.SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

}