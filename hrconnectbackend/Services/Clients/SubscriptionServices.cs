using hrconnectbackend.Data;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Models;
using hrconnectbackend.Repository;
using Microsoft.EntityFrameworkCore;

namespace hrconnectbackend.Services.Clients;

public class SubscriptionServices(DataContext context)
    : GenericRepository<SubscriptionPlan>(context), ISubscriptionServices
{
    public async Task<bool> DeleteAllSubscriptionPlans()
    {
        _context.SubscriptionPlans.RemoveRange(_context.SubscriptionPlans);
        return await _context.SaveChangesAsync() > 0;
    }
    public async Task<bool> GenerateSubscriptionPlan()
    {
        // Define subscription plans in a list
        var subscriptionPlans = new List<SubscriptionPlan>
        {
            new SubscriptionPlan { Name = "Basic Monthly Plan", Description = "Basic Plan", DurationDays = 30, IsActive = true, Price = 1499.99m },
            new SubscriptionPlan { Name = "Basic Yearly Plan", Description = "Basic Yearly Plan", DurationDays = 365, IsActive = true, Price = 1499.99m },
            new SubscriptionPlan { Name = "Premium Monthly Plan", Description = "Premium Monthly Plan", DurationDays = 30, IsActive = true, Price = 3499.99m },
            new SubscriptionPlan { Name = "Premium Yearly Plan", Description = "Premium Yearly Plan", DurationDays = 365, IsActive = true, Price = 3499.99m },
            new SubscriptionPlan { Name = "Enterprise Monthly Plan", Description = "Enterprise Monthly Plan", DurationDays = 30, IsActive = true, Price = 15999.99m },
            new SubscriptionPlan { Name = "Enterprise Yearly Plan", Description = "Enterprise Yearly Plan", DurationDays = 365, IsActive = true, Price = 15999.99m }
        };

        // Add the list of subscription plans to the context and save changes
        await _context.AddRangeAsync(subscriptionPlans);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<SubscriptionPlan>> CreateSubscriptionPlans(IEnumerable<SubscriptionPlan> subscriptionPlans)
    {
        if (subscriptionPlans == null) throw new ArgumentNullException();
        
        await _context.SubscriptionPlans.AddRangeAsync(subscriptionPlans);
        await _context.SaveChangesAsync();
        return await EntityFrameworkQueryableExtensions.ToListAsync(_context.SubscriptionPlans);
    }
    
    public async Task<SubscriptionPlan> CreateSubscriptionPlan(SubscriptionPlan plan)
    {
        var createdPlan = await AddAsync(plan);
        
        return createdPlan;
    }

    public async Task<List<SubscriptionPlan>> GetAllSubscriptionPlans()
    {
        var subscriptionPlans = await _context.SubscriptionPlans.ToListAsync();
        
        return subscriptionPlans;
    }

    public async Task<SubscriptionPlan?> GetSubscriptionPlanById(int id)
    {
        var subscriptionPlan = await GetByIdAsync(id);
        
        if (subscriptionPlan == null) throw new KeyNotFoundException("Subscription plan not found");

        return subscriptionPlan;
    }

    public async Task<bool> DeleteSubscriptionPlan(int id)
    {
        var subscriptionPlan = await GetByIdAsync(id);

        if (subscriptionPlan == null) throw new KeyNotFoundException("Subscription plan not found");
        
        await DeleteAsync(subscriptionPlan);
        
        return true;
    }

    public async Task<List<Subscription>> GetSubscriptionHistoryByUser(int userId)
    {
        var userSubscriptionPlans = await EntityFrameworkQueryableExtensions.ToListAsync(_context.Subscriptions.Where(s => s.OrganizationId == userId).OrderByDescending(a => a.StartDate));
        
        return userSubscriptionPlans;
    }

    public async Task<Subscription?> GetUserSubscription(int userId)
    {
        var userSubscriptionPlan =
            await _context.SubscriptionPlans.Include(a => a.Subscriptions).SelectMany(a => a.Subscriptions).OrderByDescending(a => a.EndDate > DateTime.Now).Include(a => a.SubscriptionPlan).FirstOrDefaultAsync();
        
        return userSubscriptionPlan;
    }

    public Task<Subscription> Subscribe(Subscription subscriptionPlan)
    {
        throw new NotImplementedException();
    }

    public async Task<Subscription> Subscribe(int orgId, SubscriptionPlan plan)
    {
        var subscription = await _context.Subscriptions.FirstOrDefaultAsync(a => a.OrganizationId == orgId && a.SubscriptionId == plan.Id);

        if (subscription != null)
        {
            subscription.EndDate = subscription.EndDate.AddDays(plan.DurationDays);
            _context.Subscriptions.Update(subscription);
            await _context.SaveChangesAsync();
            return subscription;
        }

        var newSubscription = new Subscription
        {
            OrganizationId = orgId,
            SubscriptionId = plan.Id,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddDays(plan.DurationDays),
        };
        
        await _context.Subscriptions.AddAsync(newSubscription);
        await _context.SaveChangesAsync();

        return newSubscription;
    }

    public async Task<bool> CancelSubscription(int userSubscriptionId)
    {
        var userSubscription = await _context.Subscriptions.FirstOrDefaultAsync(a => a.OrganizationId == userSubscriptionId);

        if (userSubscription == null) throw new ArgumentNullException(nameof(userSubscription));
        
        userSubscription.IsActive = false;
        
        await _context.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> RenewSubscriptionPlan(int userSubscriptionId)
    {
        var userSubscription = await EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(_context.Subscriptions
                .Include(a => a.SubscriptionPlan)
                .OrderByDescending(a => a.EndDate), a => a.OrganizationId == userSubscriptionId);
        
        if (userSubscription == null || userSubscription.SubscriptionPlan == null) throw new ArgumentNullException(nameof(userSubscription));

        int days = userSubscription.SubscriptionPlan.DurationDays;
        
        userSubscription.EndDate = DateTime.Today.AddDays(days);
        
        await _context.SaveChangesAsync();
        
        return true;
    }
}