using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace hrconnectbackend.CustomAttributeAnnotation
{
    public class RateLimitPolicyAttribute : ActionFilterAttribute
    {
        private readonly string _policyName;

        public RateLimitPolicyAttribute(string policyName)
        {
            _policyName = policyName;

        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            context.HttpContext.Items["RateLimitPolicies"] = _policyName;
            base.OnActionExecuting(context);
        }
        
    }
}

