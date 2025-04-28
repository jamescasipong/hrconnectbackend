using hrconnectbackend.Interface.Repositories;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;
using hrconnectbackend.Models.RequestModel;
using Microsoft.AspNetCore.JsonPatch;

namespace hrconnectbackend.Interface.Services.Clients;

public interface IOrganizationServices: IGenericRepository<Organization>
{
    Task<Organization> CreateOrganization(CreateOrganization createOrganization);
    Task<bool> UpdateOrganization(OrganizationsDto organization);
    Task<bool> DeleteOrganization(int organizationId);
    Task<(Organization? original, Organization? patched, bool isValid)> ApplyPatchAsync(int organizationId,
        JsonPatchDocument<Organization> patch);
    Task SaveChangesAsync();
    bool IsNameDuplicate(string name, int organizationId);
    Task<long> GetStorageUsedByOrganizationAsync(int organizationId);
}