namespace hrconnectbackend.Models.DTOs
{
    public class PlanDto
    {
        public int PlanId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal MonthlyPrice { get; set; }
        public decimal AnnualPrice { get; set; }
        public List<PlanFeatureDto> Features { get; set; }
    }

    public class PlanFeatureDto
    {
        public int FeatureId { get; set; }
        public string FeatureName { get; set; }
        public string Description { get; set; }
        public int? Limit { get; set; }
    }

    public class SubscriptionDto
    {
        public int SubscriptionId { get; set; }
        public string PlanName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime NextBillingDate { get; set; }
        public string BillingCycle { get; set; }
        public string Status { get; set; }
        public decimal CurrentPrice { get; set; }
        public DateTime? TrialEndsAt { get; set; }
    }

    public class CreateSubscriptionDto
    {
        public int UserId { get; set; }
        public int PlanId { get; set; }
        public BillingCycle BillingCycle { get; set; }
        public bool IncludeTrialPeriod { get; set; }
    }

    public class ChangePlanDto
    {
        public int SubscriptionId { get; set; }
        public int NewPlanId { get; set; }
    }

    public class PaymentDto
    {
        public int PaymentId { get; set; }
        public int SubscriptionId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string TransactionId { get; set; }
        public string Status { get; set; }
        public string PaymentMethod { get; set; }
    }

    public class ProcessPaymentDto
    {
        public int SubscriptionId { get; set; }
        public decimal Amount { get; set; }
        public string TransactionId { get; set; }
        public string PaymentMethod { get; set; }
    }

    public class RecordUsageDto
    {
        public int SubscriptionId { get; set; }
        public string ResourceType { get; set; }
        public int Quantity { get; set; }
    }
}
