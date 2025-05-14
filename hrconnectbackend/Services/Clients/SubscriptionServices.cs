using hrconnectbackend.Constants;
using hrconnectbackend.Data;
using hrconnectbackend.Exceptions;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Interface.Services.Clients;
using hrconnectbackend.Interface.Services.ExternalServices;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;
using hrconnectbackend.Repository;
using Microsoft.EntityFrameworkCore;


namespace hrconnectbackend.Services.Clients;

public class SubscriptionServices : GenericRepository<Plan>, ISubscriptionServices
{
    private readonly IEmailServices _notificationService;

    public SubscriptionServices(DataContext context, IEmailServices notificationService) : base(context)
    {
        _notificationService = notificationService;
    }

    public async Task<SubscriptionDto> CreateSubscriptionAsync(int organizationId, int planId, BillingCycle billingCycle, bool includeTrialPeriod = false)
    {
        var user = await _context.Organizations.FindAsync(organizationId);
        if (user == null)
            throw new ArgumentException("User not found");

        var plan = await _context.Plans.FindAsync(planId);
        if (plan == null || !plan.IsActive)
            throw new ArgumentException("Plan not found or inactive");

        var now = DateTime.UtcNow;
        var subscription = new Subscription
        {
            OrganizationId = organizationId,
            PlanId = planId,
            StartDate = now,
            BillingCycle = billingCycle,
            Status = includeTrialPeriod ? SubscriptionStatus.TrialPeriod : SubscriptionStatus.Active,
            CurrentPrice = billingCycle == BillingCycle.Monthly ? plan.MonthlyPrice : plan.AnnualPrice
        };

        if (includeTrialPeriod)
        {
            subscription.TrialEndsAt = now.AddDays(14); // 14-day trial
            subscription.NextBillingDate = subscription.TrialEndsAt.Value;
        }
        else
        {
            subscription.NextBillingDate = billingCycle == BillingCycle.Monthly ?
                now.AddMonths(1) : now.AddYears(1);
        }

        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        // Convert to DTO for returning
        return new SubscriptionDto
        {
            SubscriptionId = subscription.SubscriptionId,
            PlanName = plan.Name,
            StartDate = subscription.StartDate,
            EndDate = subscription.EndDate,
            NextBillingDate = subscription.NextBillingDate,
            BillingCycle = subscription.BillingCycle.ToString(),
            Status = subscription.Status.ToString(),
            CurrentPrice = subscription.CurrentPrice,
            TrialEndsAt = subscription.TrialEndsAt
        };
    }

    public async Task<bool> CancelSubscriptionAsync(int subscriptionId)
    {
        var subscription = await _context.Subscriptions.FindAsync(subscriptionId);
        if (subscription == null)
            return false;

        subscription.Status = SubscriptionStatus.Cancelled;
        subscription.EndDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ChangeSubscriptionPlanAsync(int subscriptionId, int newPlanId)
    {
        var subscription = await _context.Subscriptions.FindAsync(subscriptionId);
        if (subscription == null)
            return false;

        var newPlan = await _context.Plans.FindAsync(newPlanId);
        if (newPlan == null || !newPlan.IsActive)
            return false;

        subscription.PlanId = newPlanId;
        subscription.CurrentPrice = subscription.BillingCycle == BillingCycle.Monthly ?
            newPlan.MonthlyPrice : newPlan.AnnualPrice;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<SubscriptionDto> GetSubscriptionByIdAsync(int subscriptionId)
    {
        var subscription = await _context.Subscriptions
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.SubscriptionId == subscriptionId);

        if (subscription == null)
            return null;

        return new SubscriptionDto
        {
            SubscriptionId = subscription.SubscriptionId,
            PlanName = subscription.Plan.Name,
            StartDate = subscription.StartDate,
            EndDate = subscription.EndDate,
            NextBillingDate = subscription.NextBillingDate,
            BillingCycle = subscription.BillingCycle.ToString(),
            Status = subscription.Status.ToString(),
            CurrentPrice = subscription.CurrentPrice,
            TrialEndsAt = subscription.TrialEndsAt
        };
    }

    public async Task<IEnumerable<SubscriptionDto>> GetSubscriptionsByUserIdAsync(int organizationId)
    {
        var user = await _context.Organizations.FindAsync(organizationId);
        if (user == null)
            return null;

        return await _context.Subscriptions
            .Include(s => s.Plan)
            .Where(s => s.OrganizationId == organizationId)
            .Select(s => new SubscriptionDto
            {
                SubscriptionId = s.SubscriptionId,
                PlanName = s.Plan.Name,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                NextBillingDate = s.NextBillingDate,
                BillingCycle = s.BillingCycle.ToString(),
                Status = s.Status.ToString(),
                CurrentPrice = s.CurrentPrice,
                TrialEndsAt = s.TrialEndsAt
            })
            .ToListAsync();
    }

    public async Task RecordUsageAsync(int subscriptionId, string resourceType, int quantity)
    {
        var subscription = await _context.Subscriptions.FindAsync(subscriptionId);
        if (subscription == null)
            throw new NotFoundException(ErrorCodes.SubscriptionNotFound, $"Subscription with id {subscriptionId} not found");

        if (subscription.Status != SubscriptionStatus.Active)
            throw new ForbiddenException(ErrorCodes.SubscriptionNotActive, "Cannot record usage for an inactive subscription");

        var usageRecord = new UsageRecord
        {
            SubscriptionId = subscriptionId,
            ResourceType = resourceType,
            Quantity = quantity,
            RecordedAt = DateTime.UtcNow
        };

        _context.UsageRecords.Add(usageRecord);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<SubscriptionDto>> GetExpiredSubscriptionsAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.Subscriptions
            .Include(s => s.Plan)
            .Where(s => s.Status == SubscriptionStatus.Active && s.NextBillingDate < now)
            .Select(s => new SubscriptionDto
            {
                SubscriptionId = s.SubscriptionId,
                PlanName = s.Plan.Name,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                NextBillingDate = s.NextBillingDate,
                BillingCycle = s.BillingCycle.ToString(),
                Status = s.Status.ToString(),
                CurrentPrice = s.CurrentPrice,
                TrialEndsAt = s.TrialEndsAt
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<SubscriptionDto>> GetTrialEndingSubscriptionsAsync(int daysThreshold = 3)
    {
        var thresholdDate = DateTime.UtcNow.AddDays(daysThreshold);
        return await _context.Subscriptions
            .Include(s => s.Plan)
            .Where(s => s.Status == SubscriptionStatus.TrialPeriod &&
                       s.TrialEndsAt.HasValue &&
                       s.TrialEndsAt.Value <= thresholdDate)
            .Select(s => new SubscriptionDto
            {
                SubscriptionId = s.SubscriptionId,
                PlanName = s.Plan.Name,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                NextBillingDate = s.NextBillingDate,
                BillingCycle = s.BillingCycle.ToString(),
                Status = s.Status.ToString(),
                CurrentPrice = s.CurrentPrice,
                TrialEndsAt = s.TrialEndsAt
            })
            .ToListAsync();
    }

}