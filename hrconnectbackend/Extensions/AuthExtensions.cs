using hrconnectbackend.Config.Authentication;
using hrconnectbackend.Config.Authorization;
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
            var configurator = serviceProvider.GetRequiredService<AuthorizationPolicyConfigurator>();
            // Configure the policies
            configurator.SubscriptionPolicies(options);
            configurator.DepartmentPolicies(options);
            configurator.ManagerPolicies(options);
            configurator.UserRoleBased(options);
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