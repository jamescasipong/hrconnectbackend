namespace hrconnectbackend.Helper.Authorization;

using System;
using System.Linq;
using System.Security.Claims;

public class SubscriptionAuthorizationHelper
{
    public bool IsSubscriptionValid(ClaimsPrincipal user, string[] allowedPlans)
    {
        // Check if the user has a valid subscription claim
        var subscriptionClaim = user.Claims.FirstOrDefault(claim => claim.Type == "Subscription");
        if (subscriptionClaim == null || !allowedPlans.Contains(subscriptionClaim.Value))
        {
            return false;
        }

        // Check if the user has an expiration date claim
        var expirationClaim = user.FindFirst(claim => claim.Type == "ExpirationDate");
        if (expirationClaim == null || !DateTime.TryParse(expirationClaim.Value, out DateTime expirationDate))
        {
            return false;
        }

        // Ensure that the expiration date is in the future
        return expirationDate >= DateTime.Now;
    }
}
