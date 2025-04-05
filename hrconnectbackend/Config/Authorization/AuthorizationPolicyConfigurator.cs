using System.Security.Claims;
using hrconnectbackend.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;

namespace hrconnectbackend.Helper.Authorization
{
    using Microsoft.AspNetCore.Authorization;

    /// <summary>
    /// This class configures various authorization policies used throughout the application.
    /// It ensures that users have the necessary claims or subscription levels to access resources.
    /// </summary>
    public class AuthorizationPolicyConfigurator
    {
        private readonly SubscriptionAuthorizationHelper _subscriptionAuthorizationHelper;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AuthorizationPolicyConfigurator> _logger;

        /// <summary>
        /// Constructor that injects dependencies for subscription validation, service provider, and logging.
        /// </summary>
        /// <param name="subscriptionAuthorizationHelper">Helper for validating subscription levels.</param>
        /// <param name="serviceProvider">Service provider for resolving services, e.g., DataContext.</param>
        /// <param name="logger">Logger to record important information, warnings, and errors.</param>
        public AuthorizationPolicyConfigurator(SubscriptionAuthorizationHelper subscriptionAuthorizationHelper, 
                                                 IServiceProvider serviceProvider, 
                                                 ILogger<AuthorizationPolicyConfigurator> logger)
        {
            _subscriptionAuthorizationHelper = subscriptionAuthorizationHelper;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves an instance of the DataContext using the service provider.
        /// This method allows access to the database context for querying data.
        /// </summary>
        /// <returns>Instance of the DataContext.</returns>
        public DataContext GetDataContext()
        {
            return _serviceProvider.GetRequiredService<DataContext>();
        }

        /// <summary>
        /// Configures subscription-based authorization policies.
        /// These policies control access based on the user's subscription level.
        /// </summary>
        /// <param name="options">The AuthorizationOptions to configure the policies.</param>
        public void SubscriptionPolicies(AuthorizationOptions options)
        {
            _logger.LogInformation("Configuring Subscription Policies.");

            // Policy for Free-trial and Basic subscribers
            options.AddPolicy("Free-trial", policy =>
            {
                policy.RequireAssertion(context =>
                {
                    _logger.LogInformation("Checking Free-trial subscription for user: {User}", context.User.Identity?.Name);
                    return _subscriptionAuthorizationHelper.IsSubscriptionValid(context.User, new[] { "Free-trial", "Basic", "Premium", "Enterprise" });
                });
            });

            // Policy for Basic and above subscribers
            options.AddPolicy("Basic", policy =>
            {
                policy.RequireAssertion(context =>
                {
                    _logger.LogInformation("Checking Basic subscription for user: {User}", context.User.Identity?.Name);
                    return _subscriptionAuthorizationHelper.IsSubscriptionValid(context.User, new[] { "Free-trial", "Basic", "Premium", "Enterprise" });
                });
            });

            // Policy for Premium and above subscribers
            options.AddPolicy("Premium", policy =>
            {
                policy.RequireAssertion(context =>
                {
                    _logger.LogInformation("Checking Premium subscription for user: {User}", context.User.Identity?.Name);
                    return _subscriptionAuthorizationHelper.IsSubscriptionValid(context.User, new[] { "Premium", "Enterprise" });
                });
            });

            // Policy for Enterprise subscribers only
            options.AddPolicy("Enterprise", policy =>
            {
                policy.RequireAssertion(context =>
                {
                    _logger.LogInformation("Checking Enterprise subscription for user: {User}", context.User.Identity?.Name);
                    return _subscriptionAuthorizationHelper.IsSubscriptionValid(context.User, new[] { "Enterprise" });
                });
            });
        }

        /// <summary>
        /// Configures department-based authorization policies.
        /// These policies control access based on the user's associated department.
        /// </summary>
        /// <param name="options">The AuthorizationOptions to configure the policies.</param>
        public void DepartmentPolicies(AuthorizationOptions options)
        {
            var dataContext = GetDataContext();
            var department = dataContext.Departments.ToList();

            // Log how many departments are being configured
            _logger.LogInformation("Configuring Department Policies for {DepartmentCount} departments.", department.Count);

            foreach (var departmentPolicy in department)
            {
                // Add a policy for each department
                options.AddPolicy(departmentPolicy.DeptName, policy =>
                {
                    _logger.LogInformation("Configuring policy for department: {DepartmentName}", departmentPolicy.DeptName);
                    policy.RequireAssertion(context => 
                        context.User.HasClaim("Department", departmentPolicy.DeptName) // User must have a matching department claim
                    );
                });
            }
        }
        
        /// <summary>
        /// Configures a policy for users who are admins.
        /// This policy grants access to users who are idenfied as admins within the system.
        /// </summary>
        /// <param name="options">The AuthorizationOptions to configure the policies.</param>
        public void UserPolicies(AuthorizationOptions options)
        {
            options.AddPolicy("Admin", policy =>
            {
                policy.RequireAssertion(context => context.User.HasClaim("Role", "Admin"));
            });
        }


        /// <summary>
        /// Configures a policy for users who are managers.
        /// This policy grants access to users who are identified as managers within the system.
        /// </summary>
        /// <param name="options">The AuthorizationOptions to configure the policies.</param>
        public void ManagerPolicies(AuthorizationOptions options)
        {
            options.AddPolicy("Manager", policy => policy.RequireAssertion(context => context.User.HasClaim("EmployeeRole", "Manager")));
        }

        public void UserRoleBased(AuthorizationOptions options)
        {
            options.AddPolicy("Employee", policy => policy.RequireAssertion(context => context.User.HasClaim("Role", "Employee")));
            options.AddPolicy("Admin", policy => policy.RequireAssertion(context => context.User.HasClaim("Role", "Admin")));
        }

        // public void RequiredPermissionPolicies(AuthorizationOptions options)
        // {
        //     var rPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme);
        //     
        //     rPolicy.AddRequirements(new P)
        //     
        // }
    }
}
