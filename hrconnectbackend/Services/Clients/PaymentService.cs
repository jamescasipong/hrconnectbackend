using hrconnectbackend.Interface.Services.Clients;
using hrconnectbackend.Models.DTOs;
using hrconnectbackend.Models;
using hrconnectbackend.Data;
using Microsoft.EntityFrameworkCore;

namespace hrconnectbackend.Services.Clients
{
    public class PaymentService : IPaymentService
    {
        private readonly DataContext _context;

        public PaymentService(DataContext context)
        {
            _context = context;
        }

        public async Task<PaymentDto> ProcessPaymentAsync(int subscriptionId, decimal amount, string transactionId, string paymentMethod)
        {
            var subscription = await _context.Subscriptions.FindAsync(subscriptionId);
            if (subscription == null)
                throw new ArgumentException("Subscription not found");

            var payment = new Payment
            {
                SubscriptionId = subscriptionId,
                Amount = amount,
                PaymentDate = DateTime.UtcNow,
                TransactionId = transactionId,
                PaymentMethod = paymentMethod,
                Status = PaymentStatus.Successful
            };

            _context.Payments.Add(payment);

            // Update next billing date
            subscription.NextBillingDate = subscription.BillingCycle == BillingCycle.Monthly ?
                subscription.NextBillingDate.AddMonths(1) : subscription.NextBillingDate.AddYears(1);

            if (subscription.Status == SubscriptionStatus.PastDue || subscription.Status == SubscriptionStatus.TrialPeriod)
                subscription.Status = SubscriptionStatus.Active;

            await _context.SaveChangesAsync();

            return new PaymentDto
            {
                PaymentId = payment.PaymentId,
                SubscriptionId = payment.SubscriptionId,
                Amount = payment.Amount,
                PaymentDate = payment.PaymentDate,
                TransactionId = payment.TransactionId,
                Status = payment.Status.ToString(),
                PaymentMethod = payment.PaymentMethod
            };
        }

        public async Task<IEnumerable<PaymentDto>> GetPaymentsBySubscriptionIdAsync(int subscriptionId)
        {
            var subscription = await _context.Subscriptions.FindAsync(subscriptionId);
            if (subscription == null)
                return null;

            return await _context.Payments
                .Where(p => p.SubscriptionId == subscriptionId)
                .Select(p => new PaymentDto
                {
                    PaymentId = p.PaymentId,
                    SubscriptionId = p.SubscriptionId,
                    Amount = p.Amount,
                    PaymentDate = p.PaymentDate,
                    TransactionId = p.TransactionId,
                    Status = p.Status.ToString(),
                    PaymentMethod = p.PaymentMethod
                }).ToListAsync();
        }
    }
}
