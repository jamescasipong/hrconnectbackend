using hrconnectbackend.Config;
using hrconnectbackend.Config.Settings;

namespace hrconnectbackend.Extensions;

public static class ConfigurationExtensions
{
    public static void AddCustomConfigSettings(this IServiceCollection services)
    {
        // Get the environment name (e.g., Development, Production)
        var envName = services.BuildServiceProvider().GetRequiredService<IWebHostEnvironment>().EnvironmentName;

        // Manually build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())  // Ensures the app is in the correct directory
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)  // Default settings
            .AddJsonFile($"appsettings.{envName}.json", optional: true, reloadOnChange: true)  // Environment-specific settings
            .AddEnvironmentVariables()  // Add environment variables
            .Build();

        // Configure the settings
        services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));
        services.Configure<JwtSettings>(configuration.GetSection("JWT"));
    }
}