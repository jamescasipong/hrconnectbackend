using hrconnectbackend.Interface.Repositories;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;

namespace hrconnectbackend.Interface.Services.Clients;

public interface ISubscriptionServices
{
    Task<SubscriptionDto> CreateSubscriptionAsync(int userId, int planId, BillingCycle billingCycle, bool includeTrialPeriod = false);
    Task<bool> CancelSubscriptionAsync(int subscriptionId);
    Task<bool> ChangeSubscriptionPlanAsync(int subscriptionId, int newPlanId);
    Task<SubscriptionDto> GetSubscriptionByIdAsync(int subscriptionId);
    Task<IEnumerable<SubscriptionDto>> GetSubscriptionsByUserIdAsync(int userId);
    Task RecordUsageAsync(int subscriptionId, string resourceType, int quantity);
    Task<IEnumerable<SubscriptionDto>> GetExpiredSubscriptionsAsync();
    Task<IEnumerable<SubscriptionDto>> GetTrialEndingSubscriptionsAsync(int daysThreshold = 3);
}