using hrconnectbackend.Interface.Repositories;
using hrconnectbackend.Models;

namespace hrconnectbackend.Interface.Services;

public interface ISubscriptionServices: IGenericRepository<SubscriptionPlan>
{
    Task<bool> DeleteAllSubscriptionPlans();
    Task<SubscriptionPlan> CreateSubscriptionPlan(SubscriptionPlan plan);
    Task<List<SubscriptionPlan>> GetAllSubscriptionPlans();
    Task<SubscriptionPlan> GetSubscriptionPlanById(int id);
    Task<bool> DeleteSubscriptionPlan(int id);
    Task<List<Subscription>> GetSubscriptionHistoryByUser(int userId);
    Task<Subscription> GetUserSubscription(int userId);
    Task<Subscription> Subscribe(int userId, SubscriptionPlan plan);
    Task<bool> CancelSubscription(int userSubscriptionId);
    Task<bool> RenewSubscriptionPlan(int userSubscriptionId);
    Task<bool> GenerateSubscriptionPlan();
}