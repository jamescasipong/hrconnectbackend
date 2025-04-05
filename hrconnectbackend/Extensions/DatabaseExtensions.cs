using hrconnectbackend.Data;
using Microsoft.EntityFrameworkCore;

namespace hrconnectbackend.Extensions;

public static class DatabaseExtensions
{
    public static void AddDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<DataContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Sample")));
/*
        services.AddDbContext<DataContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));*/
    }
}
