using hrconnectbackend.Data;
using hrconnectbackend.Interface.Services.Clients;
using hrconnectbackend.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace hrconnectbackend.Controllers.v1.Clients
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionController : ControllerBase
    {
        private readonly ISubscriptionServices _subscriptionService;
        private readonly IPlanService _planService;
        private readonly IPaymentService _paymentService;

        public SubscriptionController(
            ISubscriptionServices subscriptionService,
            IPlanService planService,
            IPaymentService paymentService)
        {
            _subscriptionService = subscriptionService;
            _planService = planService;
            _paymentService = paymentService;
        }

        // GET: api/subscriptions/plans
        [HttpGet("plans")]
        public async Task<ActionResult<IEnumerable<PlanDto>>> GetPlans()
        {
            var plans = await _planService.GetActivePlansAsync();
            return Ok(plans);
        }

        // GET: api/subscriptions/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<SubscriptionDto>>> GetUserSubscriptions(int userId)
        {
            var subscriptions = await _subscriptionService.GetSubscriptionsByUserIdAsync(userId);
            if (subscriptions == null)
                return NotFound("User not found");

            return Ok(subscriptions);
        }

        // POST: api/subscriptions
        [HttpPost]
        public async Task<ActionResult<SubscriptionDto>> CreateSubscription(CreateSubscriptionDto model)
        {
            try
            {
                var subscription = await _subscriptionService.CreateSubscriptionAsync(
                    model.UserId,
                    model.PlanId,
                    model.BillingCycle,
                    model.IncludeTrialPeriod);

                return CreatedAtAction(
                    nameof(GetSubscription),
                    new { id = subscription.SubscriptionId },
                    subscription);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: api/subscriptions/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<SubscriptionDto>> GetSubscription(int id)
        {
            var subscription = await _subscriptionService.GetSubscriptionByIdAsync(id);
            if (subscription == null)
                return NotFound();

            return Ok(subscription);
        }

        // POST: api/subscriptions/{id}/cancel
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelSubscription(int id)
        {
            var result = await _subscriptionService.CancelSubscriptionAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

        // PUT: api/subscriptions/{id}/plan
        [HttpPut("{id}/plan")]
        public async Task<IActionResult> ChangeSubscriptionPlan(int id, ChangePlanDto model)
        {
            if (id != model.SubscriptionId)
                return BadRequest();

            var result = await _subscriptionService.ChangeSubscriptionPlanAsync(id, model.NewPlanId);
            if (!result)
                return NotFound();

            return NoContent();
        }

        // POST: api/subscriptions/{id}/payment
        [HttpPost("{id}/payment")]
        public async Task<ActionResult<PaymentDto>> ProcessPayment(int id, ProcessPaymentDto model)
        {
            if (id != model.SubscriptionId)
                return BadRequest();

            try
            {
                var payment = await _paymentService.ProcessPaymentAsync(
                    model.SubscriptionId,
                    model.Amount,
                    model.TransactionId,
                    model.PaymentMethod);

                return Ok(payment);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: api/subscriptions/{id}/payments
        [HttpGet("{id}/payments")]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetSubscriptionPayments(int id)
        {
            var payments = await _paymentService.GetPaymentsBySubscriptionIdAsync(id);
            if (payments == null)
                return NotFound();

            return Ok(payments);
        }

        // POST: api/subscriptions/{id}/usage
        [HttpPost("{id}/usage")]
        public async Task<IActionResult> RecordUsage(int id, RecordUsageDto model)
        {
            if (id != model.SubscriptionId)
                return BadRequest();

            try
            {
                await _subscriptionService.RecordUsageAsync(
                    model.SubscriptionId,
                    model.ResourceType,
                    model.Quantity);

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
