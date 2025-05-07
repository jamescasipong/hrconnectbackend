using hrconnectbackend.Interface.Services.Clients;
using hrconnectbackend.Interface.Services.ExternalServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SubscriptionService
{
    public class SubscriptionBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SubscriptionBackgroundService> _logger;

        public SubscriptionBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<SubscriptionBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Processing subscriptions at: {time}", DateTimeOffset.Now);
                    await ProcessSubscriptions();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing subscriptions");
                }

                // Run once per day
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }

        private async Task ProcessSubscriptions()
        {
            using var scope = _serviceProvider.CreateScope();
            var subscriptionService = scope.ServiceProvider.GetRequiredService<ISubscriptionServices>();
            var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailServices>();

            // Process expired subscriptions
            var expiredSubscriptions = await subscriptionService.GetExpiredSubscriptionsAsync();
            foreach (var subscription in expiredSubscriptions)
            {
                _logger.LogInformation("Processing expired subscription: {id}", subscription.SubscriptionId);

                try
                {
                    // In a real implementation, this would attempt to charge the customer automatically
                    // via integration with a payment provider

                    // Example of what this might look like:
                    // var paymentResult = await _paymentProvider.ChargeCustomer(subscription.UserId, subscription.CurrentPrice);
                    // if (paymentResult.Successful)
                    // {
                    //     await paymentService.ProcessPaymentAsync(
                    //         subscription.SubscriptionId,
                    //         subscription.CurrentPrice,
                    //         paymentResult.TransactionId,
                    //         paymentResult.PaymentMethod);
                    // }
                    // else
                    // {
                    //     // Update status to past due
                    //     await subscriptionService.UpdateSubscriptionStatusAsync(subscription.SubscriptionId, SubscriptionStatus.PastDue);
                    //     // Send notification
                    //     await emailService.SendPaymentFailedEmailAsync(subscription.UserId, subscription.SubscriptionId);
                    // }

                    // For this example, we'll just log
                    _logger.LogInformation("Would attempt payment for subscription: {id}", subscription.SubscriptionId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process subscription {id}", subscription.SubscriptionId);
                }
            }

            // Process trial ending subscriptions
            var trialEndingSubscriptions = await subscriptionService.GetTrialEndingSubscriptionsAsync();
            foreach (var subscription in trialEndingSubscriptions)
            {
                _logger.LogInformation("Processing trial ending subscription: {id}", subscription.SubscriptionId);

                // In a real implementation:
                // await emailService.SendTrialEndingEmailAsync(subscription.UserId, subscription.TrialEndsAt.Value);

                // For this example, we'll just log
                _logger.LogInformation("Would send trial ending email for subscription: {id}", subscription.SubscriptionId);
            }
        }
    }


}