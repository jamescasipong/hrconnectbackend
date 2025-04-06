using hrconnectbackend.Data;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Interface.Services.Clients;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace hrconnectbackend.Controllers.v1.Clients
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionController(ISubscriptionServices services, DataContext dbContext) : ControllerBase
    {

        [HttpPost("generate-dates")]
        public async Task<IActionResult> GenerateDates()
        {
            var plans = await dbContext.SubscriptionPlans.ToListAsync();

            foreach (var plan in plans)
            {
                plan.CreatedAt = DateTime.UtcNow;
                
                dbContext.Update(plan);
                await dbContext.SaveChangesAsync(); 
            }

            return Ok(plans);
        }
        
        [HttpPost("generate")]
        public async Task<IActionResult> GenerateSubscriptionPlan()
        {
            await services.GenerateSubscriptionPlan();
            
            return Ok();
        }

        [HttpPost("delete-all")]
        public async Task<IActionResult> DeleteAllSubscriptionPlans()
        {
            await services.DeleteAllSubscriptionPlans();
            return Ok();
        }

        [HttpPost("subscribe-plan")]
        public async Task<IActionResult> Subscribe(int userId, int planId)
        {
            var subPlan = await services.GetSubscriptionPlanById(planId);

            if (subPlan == null)
            {
                return NotFound($"No subscription plan found with id {planId}");
            }
            
            await services.Subscribe(userId, subPlan);
            
            return Ok();
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllSubscriptionPlans()
        {
            var subscriptionPlans = await services.GetAllSubscriptionPlans();

            var plans = subscriptionPlans.GroupBy(a => a.Name, a => new
            {
                Id = a.Id,
                Duration = a.DurationDays,
                Type = a.DurationDays == 365 ? "Yearly" : "Monthly",
            }, (key, values) => new { key, values });
            
            return Ok(plans);
        }

        [HttpPost("renew-plan")]
        public async Task<IActionResult> RenewPlan(int userId, int planId)
        {
            var plan = await services.GetSubscriptionPlanById(planId);

            return Ok();
        }
    }
}
