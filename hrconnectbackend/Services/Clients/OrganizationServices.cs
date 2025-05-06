using hrconnectbackend.Data;
using hrconnectbackend.Interface.Services.Clients;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;
using hrconnectbackend.Models.RequestModel;
using hrconnectbackend.Repository;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

namespace hrconnectbackend.Services.Clients;

public class OrganizationServices(DataContext context): GenericRepository<Organization>(context), IOrganizationServices
{
    public async Task<Organization> CreateOrganization(int userId, Organization organization)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var userAccount = await _context.UserAccounts.FirstOrDefaultAsync(a => a.UserId == userId);
            if (userAccount == null)
            {
                throw new KeyNotFoundException($"User account with id: {userId} does not exist.");
            }

            var newOrg = new Organization
            {
                Name = organization.Name,
                Address = organization.Address,
                ContactEmail = organization.ContactEmail,
                CreatedAt = organization.CreatedAt,
                IsActive = organization.IsActive
            };
            await _context.Organizations.AddAsync(newOrg);
            await _context.SaveChangesAsync();



            userAccount.OrganizationId = newOrg.Id;
            _context.Update(userAccount);
            await _context.SaveChangesAsync();

            return newOrg;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            throw new Exception($"Error creating organization: {ex.Message}", ex);
        }
    }

    public async Task<bool> UpdateOrganization(OrganizationsDto organization)
    {
        var org = await _context.Organizations.FindAsync(organization.Id);
        
        if (org == null) return false;
        
        org.Name = organization.Name;
        org.Address = organization.Address;
        org.ContactEmail = organization.ContactEmail;
        org.IsActive = organization.IsActive;
        
        _context.Organizations.Update(org);
        await _context.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> DeleteOrganization(int organizationId)
    {
        var org = await _context.Organizations.FindAsync(organizationId);
        
        if (org == null) return false;
        
        _context.Organizations.Remove(org);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<(Organization? original, Organization? patched, bool isValid)> ApplyPatchAsync(int organizationId, JsonPatchDocument<Organization> patch)
    {
        var organization = await _context.Organizations.FindAsync(organizationId);

        if (organization == null)
        {
            return (null, null, false); // Organization not found
        }

        // Copy the original for response
        var original = organization.Copy();

        // Apply the patch to the organization entity
        patch.ApplyTo(organization);

        // In this version, we simply return true assuming the patch applied, 
        // but validation is handled in the controller
        return (original, organization, true);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync(); // Save changes to the database
    }

    // Check if an organization name is already taken (for validation in the controller)
    public bool IsNameDuplicate(string name, int organizationId)
    {
        return _context.Organizations
            .Any(o => o.Name == name && o.Id != organizationId); // Exclude the current organization
    }
    
    public async Task<long> GetStorageUsedByOrganizationAsync(int organizationId)
    {
        // Dynamically create the database name using the organizationId (e.g., hrconnectbackend_1, hrconnectbackend_2, etc.)
        var dbName = $"hrconnectbackend";

        // Query to get the database size in bytes for the given database name
        var query = $@"
        SELECT pg_database_size(datname)
        FROM pg_database
        WHERE datname = '{dbName}'";

        // Execute the query and retrieve the result using ExecuteSqlRawAsync
        var result = await _context.Database
            .ExecuteSqlRawAsync(query);

        return result;  // Return the storage size in bytes (should be a long)
    }
}

public class DatabaseSizeResult
{
    public long DatabaseSize { get; set; }
}
