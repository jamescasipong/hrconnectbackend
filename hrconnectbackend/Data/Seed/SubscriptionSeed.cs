using hrconnectbackend.Models;
using Microsoft.EntityFrameworkCore;

namespace hrconnectbackend.Data.Seed
{
    public class SubscriptionSeed
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var context = new DataContext(
                serviceProvider.GetRequiredService<DbContextOptions<DataContext>>());

            // Check if DB has been seeded
            if (context.Plans.Any())
                return;

            // Seed subscription plans
            var basicPlan = new Plan
            {
                Name = "Basic",
                Description = "Basic subscription with essential features",
                MonthlyPrice = 9.99m,
                AnnualPrice = 99.99m,
                IsActive = true
            };

            var proPlan = new Plan
            {
                Name = "Pro",
                Description = "Professional subscription with advanced features",
                MonthlyPrice = 19.99m,
                AnnualPrice = 199.99m,
                IsActive = true
            };

            var enterprisePlan = new Plan
            {
                Name = "Enterprise",
                Description = "Enterprise-grade subscription with all features and premium support",
                MonthlyPrice = 49.99m,
                AnnualPrice = 499.99m,
                IsActive = true
            };

            context.Plans.AddRange(basicPlan, proPlan, enterprisePlan);
            await context.SaveChangesAsync();

            // Seed plan features
            var basicFeatures = new[]
            {
                new PlanFeature { PlanId = basicPlan.PlanId, FeatureName = "5 Projects", Description = "Create up to 5 projects", Limit = 5 },
                new PlanFeature { PlanId = basicPlan.PlanId, FeatureName = "Basic Support", Description = "Email support with 48-hour response time" },
                new PlanFeature { PlanId = basicPlan.PlanId, FeatureName = "Standard Templates", Description = "Access to standard templates" }
            };

            var proFeatures = new[]
            {
                new PlanFeature { PlanId = proPlan.PlanId, FeatureName = "20 Projects", Description = "Create up to 20 projects", Limit = 20 },
                new PlanFeature { PlanId = proPlan.PlanId, FeatureName = "Priority Support", Description = "Email support with 24-hour response time" },
                new PlanFeature { PlanId = proPlan.PlanId, FeatureName = "All Templates", Description = "Access to all templates" },
                new PlanFeature { PlanId = proPlan.PlanId, FeatureName = "Advanced Analytics", Description = "Access to advanced analytics dashboard" }
            };

            var enterpriseFeatures = new[]
            {
                new PlanFeature { PlanId = enterprisePlan.PlanId, FeatureName = "Unlimited Projects", Description = "Create unlimited projects" },
                new PlanFeature { PlanId = enterprisePlan.PlanId, FeatureName = "Premium Support", Description = "24/7 phone and email support" },
                new PlanFeature { PlanId = enterprisePlan.PlanId, FeatureName = "All Templates", Description = "Access to all templates" },
                new PlanFeature { PlanId = enterprisePlan.PlanId, FeatureName = "Advanced Analytics", Description = "Access to advanced analytics dashboard" },
                new PlanFeature { PlanId = enterprisePlan.PlanId, FeatureName = "API Access", Description = "Full API access" },
                new PlanFeature { PlanId = enterprisePlan.PlanId, FeatureName = "Dedicated Account Manager", Description = "Personal account manager" }
            };

            context.PlanFeatures.AddRange(basicFeatures);
            context.PlanFeatures.AddRange(proFeatures);
            context.PlanFeatures.AddRange(enterpriseFeatures);

            await context.SaveChangesAsync();

            // Seed test user
            var organization = context.Organizations.Where(o => o.Id == 4).FirstOrDefault()!;

            // Create a subscription for the test user
            var testSubscription = new Subscription
            {
                OrganizationId = organization.Id,
                PlanId = basicPlan.PlanId,
                StartDate = DateTime.UtcNow,
                NextBillingDate = DateTime.UtcNow.AddMonths(1),
                BillingCycle = BillingCycle.Monthly,
                Status = SubscriptionStatus.Active,
                CurrentPrice = basicPlan.MonthlyPrice
            };

            context.Subscriptions.Add(testSubscription);
            await context.SaveChangesAsync();

            // Add an initial payment
            var initialPayment = new Payment
            {
                SubscriptionId = testSubscription.SubscriptionId,
                Amount = basicPlan.MonthlyPrice,
                PaymentDate = DateTime.UtcNow,
                TransactionId = "test-transaction-001",
                Status = PaymentStatus.Successful,
                PaymentMethod = "Credit Card"
            };

            context.Payments.Add(initialPayment);
            await context.SaveChangesAsync();
        }
    }
}
