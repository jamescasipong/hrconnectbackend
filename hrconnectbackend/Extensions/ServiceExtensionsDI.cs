using AutoMapper;
using hrconnectbackend.Attributes.Authorization.Requirements;
using hrconnectbackend.Attributes.Authorization.Requirements.Handler;
using hrconnectbackend.Config.Authentication;
using hrconnectbackend.Helper;
using hrconnectbackend.Helper.Authorization;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Interface.Services.Clients;
using hrconnectbackend.Interface.Services.ExternalServices;
using hrconnectbackend.Services.Clients;
using hrconnectbackend.Services.ExternalServices;
using Microsoft.AspNetCore.Authorization;

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
            services.AddScoped<IAttendanceCertificationServices, AttendanceCertificationServices>();
            services.AddScoped<ILeaveApplicationServices, LeaveApplicationServices>();
            services.AddScoped<IPayrollServices, PayrollServices>();
            services.AddScoped<ILeaveBalanceServices, LeaveBalanceServices>();
            services.AddScoped<IOTApplicationServices, OtApplicationServices>();
            services.AddScoped<IUserNotificationServices, UserNotificationServices>();
            services.AddScoped<ISubscriptionServices, SubscriptionServices>();
            services.AddScoped<IEmailServices, EmailService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<AuthenticationConfiguration>();
            services.AddScoped<IOrganizationServices, OrganizationServices>();
            // services.AddSingleton<IAuthorizationHandler, UserPermissionHandler>();
            services.AddSingleton<IAuthorizationHandler, UserRolehandler>();
            // services.AddSingleton<IAuthorizationRequirement, UserRoleAttribute>();
            // services.AddSingleton<AuthorizeAttribute, UserRoleAttribute>();

            services.AddSingleton<IAuthorizationPolicyProvider, UserRolePolicyProvider>();

            // Class Based Object Injections
            services.AddSingleton<SubscriptionAuthorizationHelper>();
            services.AddSingleton<AuthorizationPolicyConfigurator>();
            services.AddScoped<AuthenticationServices>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }
        
        public static void AddCorsHandler(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                               .AllowAnyMethod()
                               .AllowAnyHeader();
                    });
            });
        }
       
    }
}
