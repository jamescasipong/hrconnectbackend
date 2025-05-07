using hrconnectbackend.Models.DTOs;

namespace hrconnectbackend.Interface.Services.Clients
{
    public interface IPlanService
    {
        Task<IEnumerable<PlanDto>> GetActivePlansAsync();
        Task<PlanDto> GetPlanByIdAsync(int planId);
        Task<bool> IsPlanActiveAsync(int planId);
    }
}
