using hrconnectbackend.Attributes.Authorization.Requirements;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace hrconnectbackend.Helper.Authorization;

public class UserRolePolicyProvider(IOptions<AuthorizationOptions> options, ILogger<UserRolePolicyProvider> logger)
    : IAuthorizationPolicyProvider
{
    // private readonly IMemoryCache _cache;
    private const string POLICY_PREFIX = "UserRole";
    private DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; } = new(options);

    // _cache = cache;

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() =>
        FallbackPolicyProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() =>
        FallbackPolicyProvider.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        Console.WriteLine($"Requested policy: {policyName}");
        // logger.LogInformation($"Created new policy: {policyName} with age requirement {policyName.Substring(POLICY_PREFIX.Length)}");

        // if (_cache.TryGetValue(policyName, out AuthorizationPolicy? cachedPolicy))
        // {
        //     return Task.FromResult(cachedPolicy);
        // }

        if (policyName.StartsWith(POLICY_PREFIX, StringComparison.OrdinalIgnoreCase))
        {
            var policy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme);
            policy.AddRequirements(new UserRoleAttribute(policyName.Substring(POLICY_PREFIX.Length)));

            // Cache the policy
            // _cache.Set(policyName, policy.Build(), TimeSpan.FromMinutes(10));

            logger.LogInformation($"Created new policy: {policyName} with age requirement {policyName.Substring(POLICY_PREFIX.Length)}");

            return Task.FromResult<AuthorizationPolicy?>(policy.Build());
        }
        else
        {
            logger.LogWarning($"Policy not found: {policyName}");
        }

        return Task.FromResult<AuthorizationPolicy?>(null);
    }
}