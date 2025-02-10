using AutoMapper;
using hrconnectbackend.Repositories;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Services;


namespace hrconnectbackend.Helper
{
    public static class ServicesInjection
    {
        public static void ProfileMapper(IServiceCollection services)
        {
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfile());
            });

            IMapper mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);
        }

        public static void IRepositories(IServiceCollection services)
        {
            services.AddScoped<IEmployeeServices, EmployeeServices>();
            services.AddScoped<IAttendanceServices, AttendanceServices>();
            services.AddScoped<IDepartmentServices, DepartmentServices>();
            services.AddScoped<IAboutEmployeeServices, AboutEmployeeServices>();
            services.AddScoped<ILeaveApplicationServices, LeaveApplicationServices>();
            services.AddScoped<IShiftServices, ShiftServices>();
            services.AddScoped<IUserAccountServices, UserAccountServices>();
            services.AddScoped<ISupervisorServices, SupervisorServices>();
            services.AddScoped<INotificationServices, NotificationServices>();
            services.AddScoped<IUserSettingsServices, UserSettingServices>();
            services.AddScoped<IAttendanceCertificationServices, AttendanceCertificationServices>();
            services.AddScoped<ILeaveApplicationServices, LeaveApplicationServices>();
            services.AddScoped<IPayrollServices, PayrollServices>();
            services.AddScoped<ILeaveBalanceServices, LeaveBalanceServices>();
            services.AddScoped<IOTApplicationServices, OTApplicationServices>();

            services.AddScoped<DepartmentServices>();
            services.AddScoped<UserAccountServices>();
            services.AddScoped<SupervisorServices>();
        }


        public static void CorsHandler(IServiceCollection services)
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
