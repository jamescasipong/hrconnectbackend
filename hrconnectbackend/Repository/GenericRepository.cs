using hrconnectbackend.Constants;
using hrconnectbackend.Data;
using hrconnectbackend.Exceptions;
using hrconnectbackend.Interface.Repositories;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

namespace hrconnectbackend.Repository;

public class GenericRepository<T>(DataContext context) : IGenericRepository<T>
    where T : class
{
    private string _entityName = typeof(T).Name;
    protected readonly DataContext _context = context;

    private string EntityNameCapsLocked() => _entityName.ToUpper();

    public async Task<T> GetByIdAsync(int id)
    {
        var getObject = await _context.Set<T>().FindAsync(id);

        if (getObject == null)
        {
            throw new NotFoundException($"{EntityNameCapsLocked() + "_NOT_FOUND"}", $"{EntityNameCapsLocked()} with id {id} not found.");
        }

        return getObject;
    }

    public async Task<List<T>> GetAllAsync()
    {
        var allObjects = await _context.Set<T>().ToListAsync();

        if (allObjects == null || allObjects.Count == 0)
        {
            throw new NotFoundException($"{EntityNameCapsLocked() + "_NOT_FOUND"}", $"{EntityNameCapsLocked()} not found.");
        }

        return allObjects;
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