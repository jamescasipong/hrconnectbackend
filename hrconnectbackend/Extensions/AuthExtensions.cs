using hrconnectbackend.Config.Authentication.Configuration;
using hrconnectbackend.Config.Authentication.Configuration;
using hrconnectbackend.Helper.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace hrconnectbackend.Extensions;

public static class AuthExtensions
{
    public static void AddCustomAuthorization(this IServiceCollection services)
    {
        services.AddSingleton<SubscriptionAuthorizationHelper>();
        services.AddSingleton<AuthorizationPolicyConfigurator>();

        // Add Authorization policies
        services.AddAuthorization(options =>
        {
            var serviceProvider = services.BuildServiceProvider();
            var authorizationPolicyConfigurator = serviceProvider.GetRequiredService<AuthorizationPolicyConfigurator>();
            // Configure the policies
            authorizationPolicyConfigurator.SubscriptionPolicies(options);
            authorizationPolicyConfigurator.DepartmentPolicies(options);
            authorizationPolicyConfigurator.ManagerPolicies(options);
        });
    }
     
    public static void AddCustomJwtBearer(this IServiceCollection services)
    {
        services.AddAuthentication().AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options => 
        {
            {
                var authConfig = services.BuildServiceProvider().GetRequiredService<AuthenticationConfiguration>();
                    
                authConfig.JwtOptions(options);
            }
        });
    }
}