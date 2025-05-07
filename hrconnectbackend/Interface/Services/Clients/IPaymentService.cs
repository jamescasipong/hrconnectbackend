using hrconnectbackend.Models.DTOs;

namespace hrconnectbackend.Interface.Services.Clients
{
    public interface IPaymentService
    {
        Task<PaymentDto> ProcessPaymentAsync(int subscriptionId, decimal amount, string transactionId, string paymentMethod);
        Task<IEnumerable<PaymentDto>> GetPaymentsBySubscriptionIdAsync(int subscriptionId);
    }
}
