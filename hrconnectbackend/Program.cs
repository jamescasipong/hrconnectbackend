using System.Security.Claims;
using System.Threading.RateLimiting;
using AspNetCoreRateLimit;
using hrconnectbackend.Data.Seed;
using hrconnectbackend.Extensions;
using hrconnectbackend.Middlewares;
using hrconnectbackend.SignalR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging();
builder.Services.AddCorsHandler();
builder.Services.AddServices();
builder.Services.AddProfileMapper();
builder.Services.AddCustomConfigSettings();
builder.Services.AddDbContext(builder.Configuration);
builder.Services.AddControllers().AddNewtonsoftJson();



var apiVersioningBuilder = builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});

builder.Services.AddCustomJwtBearer();
builder.Services.AddCustomAuthorization();

builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(30); // Set your session timeout duration
        options.Cookie.HttpOnly = true; // Prevent client-side access to the session cookie
        options.Cookie.SameSite = SameSiteMode.None; // Specify SameSite policy
        options.Cookie.IsEssential = true; // Make session essential for app functionality
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Ensure the cookie is only sent over HTTPS
    });

// Register SignalR services
builder.Services.AddSignalR();

// Add distributed memory cache
builder.Services.AddDistributedMemoryCache();
// Add session state services
builder.Services.AddMemoryCache();

// Add rate limiting services
builder.Services.AddRateLimiter(_ => _
    .AddFixedWindowLimiter(policyName: "fixed", configureOptions: options =>
    {
        options.PermitLimit = 4;
        options.Window = TimeSpan.FromSeconds(12);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 2;
    }));
// builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("RateLimitPolicies"));
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddInMemoryRateLimiting();

// Swagger setup
builder.Services.AddEndpointsApiExplorer();




builder.Services.AddSwaggerGen(c =>
{
    c.CustomSchemaIds(type => type.FullName);

    // Add API Key definition
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Description = "API Key needed to access the endpoints"
    });

    // Add security requirements
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            new string[] {}
        },
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});


//builder.WebHost.ConfigureKestrel(serverOptions =>
//{
//    serverOptions.ConfigureHttpsDefaults(httpsOptions =>
//    {
//        httpsOptions.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
//    });
//});

var app = builder.Build();
// app.UseMiddleware<SensitiveDataFilterMiddleware>();
// app.MapOpenApi();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI();
    app.UseSwagger();
    // app.UseSwagger(opt =>
    // {
    //     opt.RouteTemplate = "openapi/{documentName}.json";
    // });
    // app.MapScalarApiReference(opt =>
    // {
    //     opt.Title = "Scalar Example";
    //     opt.Theme = ScalarTheme.Mars;
    //     opt.DefaultHttpClient = new(ScalarTarget.Http, ScalarClient.Http11);
    // });
    
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await SubscriptionSeed.Initialize(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

//app.UseHttpsRedirection();
app.UseHsts();
app.UseStatusCodePages();
app.UseRouting(); // Add UseRouting before UseCors
app.UseCorsHandler(); // Use the CORS middleware
app.UseSession(); // Add session middleware
app.UseAuthentication();
app.UseAuthorization();

app.UseWebSockets(); // Place UseWebSockets before MapControllers
app.MapControllers();
app.UseMiddleware<ErrorExceptionMiddleware>();

// Map SignalR hubs
app.MapHub<NotificationHub>("/notificationHub");

app.Run();