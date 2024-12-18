using AutoMapper;
using hrconnectbackend.Data;
using hrconnectbackend.IRepositories;
using hrconnectbackend.Repositories;
using Microsoft.EntityFrameworkCore;

namespace hrconnectbackend.Helper
{
    public static class Services
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
            services.AddTransient<IEmployeeRepositories, EmployeeRepositories>();
            services.AddTransient<IAttendanceRepositories, AttendanceRepositories>();
            services.AddScoped<AuthRepositories>();
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
