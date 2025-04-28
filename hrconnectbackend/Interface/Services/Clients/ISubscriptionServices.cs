using hrconnectbackend.Interface.Repositories;
using hrconnectbackend.Models;

namespace hrconnectbackend.Interface.Services.Clients;

public interface ISubscriptionServices
{
    /// <summary>
    /// Deletes all subscription plans from the database
    /// </summary>
    Task<bool> DeleteAllSubscriptionPlans();
    
    /// <summary>
    /// Generates default subscription plans
    /// </summary>
    Task<bool> GenerateSubscriptionPlan();
    
    /// <summary>
    /// Creates multiple subscription plans
    /// </summary>
    /// <param name="subscriptionPlans">Collection of subscription plans to create</param>
    Task<List<SubscriptionPlan>> CreateSubscriptionPlans(IEnumerable<SubscriptionPlan> subscriptionPlans);
    
    /// <summary>
    /// Creates a single subscription plan
    /// </summary>
    /// <param name="plan">Subscription plan to create</param>
    Task<SubscriptionPlan?> CreateSubscriptionPlan(SubscriptionPlan plan);
    
    /// <summary>
    /// Gets all active subscription plans
    /// </summary>
    Task<List<SubscriptionPlan>> GetAllSubscriptionPlans();
    
    /// <summary>
    /// Gets a subscription plan by its ID
    /// </summary>
    /// <param name="id">ID of the subscription plan</param>
    Task<SubscriptionPlan?> GetSubscriptionPlanById(int id);
    
    /// <summary>
    /// Marks a subscription plan as inactive (soft delete)
    /// </summary>
    /// <param name="id">ID of the subscription plan to delete</param>
    Task<bool> DeleteSubscriptionPlan(int id);
    
    /// <summary>
    /// Gets the subscription history for an organization
    /// </summary>
    /// <param name="organizationId">ID of the organization</param>
    Task<List<Subscription>> GetSubscriptionHistoryByOrganization(int organizationId);
    
    /// <summary>
    /// Gets the current active subscription for an organization
    /// </summary>
    /// <param name="organizationId">ID of the organization</param>
    Task<Subscription?> GetCurrentOrganizationSubscription(int organizationId);
    
    /// <summary>
    /// Changes an organization's subscription plan
    /// </summary>
    /// <param name="organizationId">ID of the organization</param>
    /// <param name="newPlanId">ID of the new subscription plan</param>
    Task<Subscription> ChangeSubscriptionPlan(int organizationId, int newPlanId);
    
    /// <summary>
    /// Gets the active subscription plan for an organization
    /// </summary>
    /// <param name="organizationId">ID of the organization</param>
    Task<Subscription?> GetActiveSubscriptionPlan(int organizationId);
    
    /// <summary>
    /// Subscribes an organization to a plan
    /// </summary>
    /// <param name="organizationId">ID of the organization</param>
    /// <param name="planId">ID of the subscription plan</param>
    Task<Subscription> Subscribe(int organizationId, int planId);
    
    /// <summary>
    /// Cancels an organization's active subscription
    /// </summary>
    /// <param name="organizationId">ID of the organization</param>
    Task<bool> CancelSubscription(int organizationId);
    
    /// <summary>
    /// Renews an organization's subscription using their last plan
    /// </summary>
    /// <param name="organizationId">ID of the organization</param>
    Task<Subscription> RenewSubscription(int organizationId);
    
    /// <summary>
    /// Checks if an organization's subscription is expiring soon
    /// </summary>
    /// <param name="organizationId">ID of the organization</param>
    /// <param name="daysThreshold">Days threshold to consider as "expiring soon"</param>
    Task<bool> IsSubscriptionExpiringSoon(int organizationId, int daysThreshold = 7);
    
    /// <summary>
    /// Gets all subscriptions that are expiring within the specified days threshold
    /// </summary>
    /// <param name="daysThreshold">Days threshold to consider as "expiring soon"</param>
    Task<List<Subscription>> GetExpiringSubscriptions(int daysThreshold = 7);
    
    /// <summary>
    /// Checks and updates status of all subscriptions (marks expired ones as inactive)
    /// </summary>
    Task<bool> CheckAndUpdateSubscriptionStatus();
    
    /// <summary>
    /// Sends expiration reminder notifications to organizations based on specified days
    /// </summary>
    /// <param name="reminderDays">Array of days before expiration to send reminders</param>
    Task<bool> SendExpirationReminders(int[] reminderDays = null);
}