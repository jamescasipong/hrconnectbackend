using hrconnectbackend.Data;
using hrconnectbackend.Interface.Services.Clients;
using hrconnectbackend.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace hrconnectbackend.Services.Clients
{
    public class PlanService : IPlanService
    {
        private readonly DataContext _context;

        public PlanService(DataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PlanDto>> GetActivePlansAsync()
        {
            return await _context.Plans
                .Where(p => p.IsActive)
                .Select(p => new PlanDto
                {
                    PlanId = p.PlanId,
                    Name = p.Name,
                    Description = p.Description,
                    MonthlyPrice = p.MonthlyPrice,
                    AnnualPrice = p.AnnualPrice,
                    Features = p.Features.Select(f => new PlanFeatureDto
                    {
                        FeatureId = f.FeatureId,
                        FeatureName = f.FeatureName,
                        Description = f.Description,
                        Limit = f.Limit
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<PlanDto> GetPlanByIdAsync(int planId)
        {
            var plan = await _context.Plans.Include(p => p.Features)
                .FirstOrDefaultAsync(p => p.PlanId == planId);

            if (plan == null)
                return null;

            return new PlanDto
            {
                PlanId = plan.PlanId,
                Name = plan.Name,
                Description = plan.Description,
                MonthlyPrice = plan.MonthlyPrice,
                AnnualPrice = plan.AnnualPrice,
                Features = plan.Features.Select(f => new PlanFeatureDto
                {
                    FeatureId = f.FeatureId,
                    FeatureName = f.FeatureName,
                    Description = f.Description,
                    Limit = f.Limit
                }).ToList()
            };
        }

        public async Task<bool> IsPlanActiveAsync(int planId)
        {
            var plan = await _context.Plans.FindAsync(planId);
            return plan != null && plan.IsActive;
        }
    }

}
