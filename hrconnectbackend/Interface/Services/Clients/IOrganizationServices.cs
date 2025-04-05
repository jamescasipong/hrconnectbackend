using hrconnectbackend.Models;

namespace hrconnectbackend.Interface.Services.Clients;

public interface IOrganizationServices
{
    Task<Organization> CreateOrganization(int organizationId);
}