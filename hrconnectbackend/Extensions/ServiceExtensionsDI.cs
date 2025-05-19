using AutoMapper;
using hrconnectbackend.Attributes.Authorization.Requirements.Handler;
using hrconnectbackend.Config.Authentication;
using hrconnectbackend.Config.Authorization;
using hrconnectbackend.Helper;
using hrconnectbackend.Helper.Authorization;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Interface.Services.Clients;
using hrconnectbackend.Interface.Services.ExternalServices;
using hrconnectbackend.Models;
using hrconnectbackend.Services.Clients;
using hrconnectbackend.Services.ExternalServices;
using Microsoft.AspNetCore.Authorization;
using SubscriptionService;

namespace hrconnectbackend.Extensions
{
    public static class Extensions
    {
        public static void AddProfileMapper(this IServiceCollection services)
        {
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfile());
            });

            IMapper mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);
        }

        public static void AddServices(this IServiceCollection services)
        {
            // Interface Based Injections
            services.AddScoped<IEmployeeServices, EmployeeServices>();
            services.AddScoped<IAttendanceServices, AttendanceServices>();
            services.AddScoped<IDepartmentServices, DepartmentServices>();
            services.AddScoped<IAboutEmployeeServices, AboutEmployeeServices>();
            services.AddScoped<ILeaveApplicationServices, LeaveApplicationServices>();
            services.AddScoped<IShiftServices, ShiftServices>();
            services.AddScoped<IUserAccountServices, UserAccountServices>();
            services.AddScoped<INotificationServices, NotificationServices>();
            services.AddScoped<IUserSettingsServices, UserSettingServices>();
            services.AddScoped<IAttendanceCertificationServices, AttendanceServices>();
            services.AddScoped<ILeaveApplicationServices, LeaveApplicationServices>();
            services.AddScoped<IPayrollServices, PayrollServices>();
            services.AddScoped<ILeaveBalanceServices, LeaveBalanceServices>();
            services.AddScoped<IOTApplicationServices, OtApplicationServices>();
            services.AddScoped<IUserNotificationServices, UserNotificationServices>();
            services.AddScoped<ISubscriptionServices, SubscriptionServices>();
            services.AddTransient<IEmailServices, EmailService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<AuthenticationConfiguration>();
            services.AddScoped<ISupervisorServices, SupervisorServices>();
            services.AddScoped<IOrganizationServices, OrganizationServices>();
            services.AddScoped<IPlanService, PlanService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddHostedService<SubscriptionBackgroundService>();
            services.AddScoped(typeof(IPaginatedService<>), typeof(PaginatedService<>));
            // services.AddSingleton<IAuthorizationHandler, UserPermissionHandler>();

            // services.AddSingleton<IAuthorizationRequirement, UserRoleAttribute>();
            // services.AddSingleton<AuthorizeAttribute, UserRoleAttribute>();

            services.AddSingleton<IAuthorizationHandler, UserRolehandler>();
            services.AddSingleton<IAuthorizationPolicyProvider, UserRolePolicyProvider>();

            // Class Based Object Injections
            services.AddSingleton<SubscriptionAuthorizationHelper>();
            services.AddSingleton<AuthorizationPolicyConfigurator>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        public static void AddCorsHandler(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                // Allow all origins, methods, and headers for the default policy
                options.AddPolicy("AllowAll",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                               .AllowAnyMethod()
                               .AllowAnyHeader();
                    });
                // Add a specific policy for your React app
                options.AddPolicy("HRConnect", builder =>
                    {
                        builder.AllowAnyHeader()
                                .AllowAnyMethod()
                                .AllowCredentials()
                                .WithOrigins("https://hrconnect.vercel.app", "http://localhost:3000", "https://localhost:3001", "https://localhost:3000"); // Replace with your React app's URL
                    });
            });
        }

        public static void UseCorsHandler(this IApplicationBuilder app)
        {
            app.UseCors("HRConnect");
            app.UseCors("AllowAll");
        }

    }
}
