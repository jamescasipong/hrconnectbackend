using hrconnectbackend.Data;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Interface.Services.Clients;
using hrconnectbackend.Interface.Services.ExternalServices;
using hrconnectbackend.Models;
using hrconnectbackend.Repository;
using Microsoft.EntityFrameworkCore;


namespace hrconnectbackend.Services.Clients;

public class SubscriptionServices : GenericRepository<SubscriptionPlan>, ISubscriptionServices
{
    private readonly IEmailServices _notificationService;

    public SubscriptionServices(DataContext context, IEmailServices notificationService) : base(context)
    {
        _notificationService = notificationService;
    }

    public async Task<bool> DeleteAllSubscriptionPlans()
    {
        _context.SubscriptionPlans.RemoveRange(_context.SubscriptionPlans);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> GenerateSubscriptionPlan()
    {
        // Define subscription plans in a list with clear pricing tiers and features
        var subscriptionPlans = new List<SubscriptionPlan>
        {
            new SubscriptionPlan { 
                Name = "Basic Monthly Plan", 
                Description = "Essential services for small organizations", 
                DurationDays = 30, 
                IsActive = true, 
                Price = 1499.99m,
                MaxUsers = 10,
                Features = "Core HR functionality, Employee database, Basic reporting"
            },
            new SubscriptionPlan { 
                Name = "Basic Yearly Plan", 
                Description = "Essential services for small organizations with 15% discount", 
                DurationDays = 365, 
                IsActive = true, 
                Price = 15299.99m, // ~15% yearly discount
                MaxUsers = 10,
                Features = "Core HR functionality, Employee database, Basic reporting"
            },
            new SubscriptionPlan { 
                Name = "Premium Monthly Plan", 
                Description = "Advanced features for growing organizations", 
                DurationDays = 30, 
                IsActive = true, 
                Price = 3499.99m,
                MaxUsers = 50,
                Features = "All Basic features, Advanced reporting, Performance management, Learning management"
            },
            new SubscriptionPlan { 
                Name = "Premium Yearly Plan", 
                Description = "Advanced features for growing organizations with 15% discount", 
                DurationDays = 365, 
                IsActive = true, 
                Price = 35699.89m, // ~15% yearly discount
                MaxUsers = 50,
                Features = "All Basic features, Advanced reporting, Performance management, Learning management"
            },
            new SubscriptionPlan { 
                Name = "Enterprise Monthly Plan", 
                Description = "Comprehensive solution for large organizations", 
                DurationDays = 30, 
                IsActive = true, 
                Price = 15999.99m,
                MaxUsers = 250,
                Features = "All Premium features, Custom reporting, API access, Priority support, Dedicated account manager"
            },
            new SubscriptionPlan { 
                Name = "Enterprise Yearly Plan", 
                Description = "Comprehensive solution for large organizations with 15% discount", 
                DurationDays = 365, 
                IsActive = true, 
                Price = 163199.89m, // ~15% yearly discount
                MaxUsers = 250,
                Features = "All Premium features, Custom reporting, API access, Priority support, Dedicated account manager"
            }
        };

        // Add the list of subscription plans to the context and save changes
        await _context.AddRangeAsync(subscriptionPlans);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<SubscriptionPlan>> CreateSubscriptionPlans(IEnumerable<SubscriptionPlan> subscriptionPlans)
    {
        if (subscriptionPlans == null) throw new ArgumentNullException(nameof(subscriptionPlans));
        
        await _context.SubscriptionPlans.AddRangeAsync(subscriptionPlans);
        await _context.SaveChangesAsync();
        return await _context.SubscriptionPlans.ToListAsync();
    }
    
    public async Task<SubscriptionPlan?> CreateSubscriptionPlan(SubscriptionPlan plan)
    {
        if (plan == null) throw new ArgumentNullException(nameof(plan));
        
        var createdPlan = await AddAsync(plan);
        return createdPlan;
    }

    public async Task<List<SubscriptionPlan>> GetAllSubscriptionPlans()
    {
        return await _context.SubscriptionPlans
            .Where(p => p.IsActive)
            .OrderBy(p => p.Price)
            .ToListAsync();
    }

    public async Task<SubscriptionPlan?> GetSubscriptionPlanById(int id)
    {
        var subscriptionPlan = await GetByIdAsync(id);
        
        if (subscriptionPlan == null) throw new KeyNotFoundException($"Subscription plan with ID {id} not found");

        return subscriptionPlan;
    }

    public async Task<bool> DeleteSubscriptionPlan(int id)
    {
        var subscriptionPlan = await GetByIdAsync(id);

        if (subscriptionPlan == null) throw new KeyNotFoundException($"Subscription plan with ID {id} not found");
        
        // Instead of hard delete, mark as inactive
        subscriptionPlan.IsActive = false;
        await UpdateAsync(subscriptionPlan);
        
        return true;
    }

    public async Task<List<Subscription>> GetSubscriptionHistoryByOrganization(int organizationId)
    {
        return await _context.Subscriptions
            .Where(s => s.OrganizationId == organizationId)
            .Include(s => s.SubscriptionPlan)
            .OrderByDescending(s => s.StartDate)
            .ToListAsync();
    }

    public async Task<Subscription?> GetCurrentOrganizationSubscription(int organizationId)
    {
        return await _context.Subscriptions
            .Where(s => s.OrganizationId == organizationId && s.IsActive && s.EndDate > DateTime.Now)
            .Include(s => s.SubscriptionPlan)
            .OrderByDescending(s => s.StartDate)
            .FirstOrDefaultAsync();
    }
    
    public async Task<Subscription> ChangeSubscriptionPlan(int organizationId, int newPlanId)
    {
        var currentSubscription = await GetActiveSubscriptionPlan(organizationId);
        var newPlan = await GetSubscriptionPlanById(newPlanId);
        
        if (newPlan == null) throw new KeyNotFoundException($"Subscription plan with ID {newPlanId} not found");
        
        // If there's an active subscription, end it
        if (currentSubscription != null)
        {
            currentSubscription.IsActive = false;
            _context.Subscriptions.Update(currentSubscription);
        }

        // Create new subscription
        var newSubscription = new Subscription
        {
            OrganizationId = organizationId,
            SubscriptionId = newPlanId,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddDays(newPlan.DurationDays),
            IsActive = true,
            LastBillingDate = DateTime.Now,
            NextBillingDate = DateTime.Now.AddDays(newPlan.DurationDays)
        };
        
        await _context.Subscriptions.AddAsync(newSubscription);
        await _context.SaveChangesAsync();

        // Notify organization admins about plan change
        await NotifySubscriptionChanged(organizationId, newPlan);

        return newSubscription;
    }

    public async Task<Subscription?> GetActiveSubscriptionPlan(int organizationId)
    {
        return await _context.Subscriptions
            .Where(s => s.OrganizationId == organizationId && 
                   s.IsActive == true && 
                   s.EndDate > DateTime.Now)
            .Include(s => s.SubscriptionPlan)
            .OrderByDescending(s => s.StartDate)
            .FirstOrDefaultAsync();
    }

    public async Task<Subscription> Subscribe(int organizationId, int planId)
    {
        var plan = await GetSubscriptionPlanById(planId);
        if (plan == null) throw new KeyNotFoundException($"Subscription plan with ID {planId} not found");
        
        // Check if organization already has an active subscription
        var activeSubscription = await GetActiveSubscriptionPlan(organizationId);
        
        if (activeSubscription != null)
        {
            // Upgrade/downgrade scenario - end current subscription
            activeSubscription.IsActive = false;
            _context.Subscriptions.Update(activeSubscription);
        }

        // Create new subscription
        var newSubscription = new Subscription
        {
            OrganizationId = organizationId,
            SubscriptionId = planId,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddDays(plan.DurationDays),
            IsActive = true,
            LastBillingDate = DateTime.Now,
            NextBillingDate = DateTime.Now.AddDays(plan.DurationDays)
        };
        
        await _context.Subscriptions.AddAsync(newSubscription);
        await _context.SaveChangesAsync();

        // Notify organization admins about new subscription
        await NotifyNewSubscription(organizationId, plan);

        return newSubscription;
    }

    public async Task<bool> CancelSubscription(int organizationId)
    {
        var activeSubscription = await GetActiveSubscriptionPlan(organizationId);

        if (activeSubscription == null) 
            throw new KeyNotFoundException($"No active subscription found for organization ID {organizationId}");
        
        activeSubscription.IsActive = false;
        activeSubscription.CancellationDate = DateTime.Now;
        
        _context.Subscriptions.Update(activeSubscription);
        await _context.SaveChangesAsync();
        
        // Notify organization admins about cancellation
        await NotifySubscriptionCancelled(organizationId, activeSubscription.SubscriptionPlan);
        
        return true;
    }

    public async Task<Subscription> RenewSubscription(int organizationId)
    {
        var lastSubscription = await _context.Subscriptions
            .Where(s => s.OrganizationId == organizationId)
            .Include(s => s.SubscriptionPlan)
            .OrderByDescending(s => s.EndDate)
            .FirstOrDefaultAsync();
        
        if (lastSubscription == null || lastSubscription.SubscriptionPlan == null) 
            throw new KeyNotFoundException($"No previous subscription found for organization ID {organizationId}");

        // Deactivate any currently active subscription
        var activeSubscription = await GetActiveSubscriptionPlan(organizationId);
        if (activeSubscription != null)
        {
            activeSubscription.IsActive = false;
            _context.Subscriptions.Update(activeSubscription);
        }

        // Create new subscription with same plan
        var newSubscription = new Subscription
        {
            OrganizationId = organizationId,
            SubscriptionId = lastSubscription.SubscriptionId,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddDays(lastSubscription.SubscriptionPlan.DurationDays),
            IsActive = true,
            LastBillingDate = DateTime.Now,
            NextBillingDate = DateTime.Now.AddDays(lastSubscription.SubscriptionPlan.DurationDays)
        };
        
        await _context.Subscriptions.AddAsync(newSubscription);
        await _context.SaveChangesAsync();
        
        // Notify organization admins about renewal
        await NotifySubscriptionRenewed(organizationId, lastSubscription.SubscriptionPlan);
        
        return newSubscription;
    }

    public async Task<bool> IsSubscriptionExpiringSoon(int organizationId, int daysThreshold = 7)
    {
        var activeSubscription = await GetActiveSubscriptionPlan(organizationId);
        
        if (activeSubscription == null) return false;
        
        var daysRemaining = (activeSubscription.EndDate - DateTime.Now).Days;
        return daysRemaining <= daysThreshold;
    }

    public async Task<List<Subscription>> GetExpiringSubscriptions(int daysThreshold = 7)
    {
        return await _context.Subscriptions
            .Where(s => s.IsActive && 
                   s.EndDate > DateTime.Now &&
                   EF.Functions.DateDiffDay(DateTime.Now, s.EndDate) <= daysThreshold)
            .Include(s => s.SubscriptionPlan)
            .ToListAsync();
    }

    public async Task<bool> CheckAndUpdateSubscriptionStatus()
    {
        // Find all active subscriptions that have expired
        var expiredSubscriptions = await _context.Subscriptions
            .Where(s => s.IsActive && s.EndDate < DateTime.Now)
            .Include(s => s.SubscriptionPlan)
            .ToListAsync();

        foreach (var subscription in expiredSubscriptions)
        {
            subscription.IsActive = false;
            
            // Notify organization admins about expiration
            await NotifySubscriptionExpired(subscription.OrganizationId, subscription.SubscriptionPlan);
        }

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> SendExpirationReminders(int[] reminderDays = null)
    {
        if (reminderDays == null)
        {
            reminderDays = new[] { 30, 14, 7, 3, 1 }; // Default reminder days
        }

        bool notificationsSent = false;

        foreach (var days in reminderDays)
        {
            var expiringSubscriptions = await _context.Subscriptions
                .Where(s => s.IsActive && 
                       EF.Functions.DateDiffDay(DateTime.Now, s.EndDate) == days)
                .Include(s => s.SubscriptionPlan)
                .ToListAsync();

            foreach (var subscription in expiringSubscriptions)
            {
                await NotifySubscriptionExpiring(subscription.OrganizationId, subscription.SubscriptionPlan, days);
                notificationsSent = true;
            }
        }

        return notificationsSent;
    }

    #region Notification Methods
    private async Task NotifyNewSubscription(int organizationId, SubscriptionPlan plan)
    {
        var admins = await GetOrganizationAdmins(organizationId);
        foreach (var admin in admins)
        {
            await _notificationService.SendEmailAsync(
                admin.Email,
                "New Subscription Activated",
                $"Your organization has successfully subscribed to the {plan.Name} plan. " +
                $"Your subscription is now active and will expire on {DateTime.Now.AddDays(plan.DurationDays):d}."
            );
        }
    }

    private async Task NotifySubscriptionExpiring(int organizationId, SubscriptionPlan plan, int daysRemaining)
    {
        var admins = await GetOrganizationAdmins(organizationId);
        foreach (var admin in admins)
        {
            await _notificationService.SendEmailAsync(
                admin.Email,
                $"Subscription Expiring in {daysRemaining} days",
                $"Your {plan.Name} subscription will expire in {daysRemaining} days on {DateTime.Now.AddDays(daysRemaining):d}. " +
                $"Please renew your subscription to avoid service interruption. " +
                $"Login to your account to renew or change your subscription plan."
            );

            // // Also send in-app notification
            // await _notificationService.SendEmailAsync(
            //     admin.Id,
            //     $"Your subscription expires in {daysRemaining} days",
            //     $"Renew now to avoid service interruption",
            //     NotificationType.SubscriptionExpiring
            // );
        }
    }

    private async Task NotifySubscriptionExpired(int organizationId, SubscriptionPlan plan)
    {
        var admins = await GetOrganizationAdmins(organizationId);
        foreach (var admin in admins)
        {
            await _notificationService.SendEmailAsync(
                admin.Email,
                "Subscription Expired",
                $"Your {plan.Name} subscription has expired. " +
                $"Your organization's access to premium features has been limited. " +
                $"Please renew your subscription to restore full access."
            );

            // Also send in-app notification
            // await _notificationService.SendEmailAsync(
            //     admin.Id,
            //     "Your subscription has expired",
            //     "Renew now to restore full access",
            //     NotificationType.SubscriptionExpired
            // );
        }
    }

    private async Task NotifySubscriptionRenewed(int organizationId, SubscriptionPlan plan)
    {
        var admins = await GetOrganizationAdmins(organizationId);
        foreach (var admin in admins)
        {
            // await _notificationService.SendEmailNotification(
            //     admin.Email,
            //     "Subscription Renewed",
            //     $"Your {plan.Name} subscription has been successfully renewed. " +
            //     $"Your subscription is now active until {DateTime.Now.AddDays(plan.DurationDays):d}."
            // );
            //
            // // Also send in-app notification
            // await _notificationService.SendInAppNotification(
            //     admin.Id,
            //     "Subscription successfully renewed",
            //     $"Active until {DateTime.Now.AddDays(plan.DurationDays):d}",
            //     NotificationType.SubscriptionRenewed
            // );
        }
    }

    private async Task NotifySubscriptionChanged(int organizationId, SubscriptionPlan newPlan)
    {
        var admins = await GetOrganizationAdmins(organizationId);
        foreach (var admin in admins)
        {
            // await _notificationService.SendEmailNotification(
            //     admin.Email,
            //     "Subscription Plan Changed",
            //     $"Your organization's subscription has been changed to the {newPlan.Name} plan. " +
            //     $"Your new subscription is now active until {DateTime.Now.AddDays(newPlan.DurationDays):d}."
            // );
            //
            // // Also send in-app notification
            // await _notificationService.SendInAppNotification(
            //     admin.Id,
            //     "Subscription plan changed",
            //     $"You're now on the {newPlan.Name} plan",
            //     NotificationType.SubscriptionChanged
            // );
        }
    }

    private async Task NotifySubscriptionCancelled(int organizationId, SubscriptionPlan plan)
    {
        var admins = await GetOrganizationAdmins(organizationId);
        foreach (var admin in admins)
        {
            await _notificationService.SendEmailAsync(
                admin.Email,
                "Subscription Cancelled",
                $"Your {plan.Name} subscription has been cancelled. " +
                $"If this was a mistake, please contact our support team or resubscribe from your account."
            );

            // // Also send in-app notification
            // await _notificationService.SendEmailAsync(
            //     admin.Id,
            //     "Subscription cancelled",
            //     "Contact support if this was a mistake",
            //     NotificationType.SubscriptionCancelled
            // );
        }
    }

    private async Task<List<UserAccount>> GetOrganizationAdmins(int organizationId)
    {
        // This is a placeholder - implement according to your user management system
        return await _context.UserAccounts
            .Where(u => u.OrganizationId == organizationId && u.Role == "Admin")
            .ToListAsync();
    }
    #endregion
}