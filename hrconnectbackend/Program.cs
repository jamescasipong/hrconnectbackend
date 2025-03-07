using System.Text;
using System.Threading.RateLimiting;
using AspNetCoreRateLimit;
using hrconnectbackend.Data;
using hrconnectbackend.Helper;
using hrconnectbackend.middlewares;
using hrconnectbackend.SignalR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
ServicesInjection.CorsHandler(builder.Services);
ServicesInjection.IRepositories(builder.Services);
ServicesInjection.ProfileMapper(builder.Services);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<DataContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddControllers();

var apiVersioningBuilder = builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});

apiVersioningBuilder.AddEndpointsApiExplorer();

builder.Services.AddAuthentication()
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(
            builder.Configuration.GetValue<string>("JWT:Key")!
            )),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = (context) =>
            {
                context.Token = context.Request.Cookies["token"]; // JWT from cookie
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy =>
    {
        policy.RequireAssertion(context =>
        {
            return context.User.HasClaim(claim => claim.Type == "Role" && (claim.Value == "Admin" || claim.Value == "SuperAdmin"));
        }).RequireAuthenticatedUser();
    });

    options.AddPolicy("RequireUser", policy =>
    {
        policy.RequireAssertion(context =>
        {
            return context.User.HasClaim(claim => claim.Type == "Role" && claim.Value == "User");
        }).RequireAuthenticatedUser();
    });

    options.AddPolicy("HR Department", policy =>
    {
        policy.RequireRole("HR Department");
    });
});

builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(30); // Set your session timeout duration
        options.Cookie.HttpOnly = true; // Prevent client-side access to the session cookie
    });


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials()
               .WithOrigins("http://localhost:3000", "https://hrconnect.vercel.app", "https://hr-management-system-vi10.onrender.com"); // Replace with your React app's URL
    });
});

// Register SignalR services
builder.Services.AddSignalR();

// Add distributed memory cache
builder.Services.AddDistributedMemoryCache();
builder.Services.AddMemoryCache();
// Add session services
builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30); // Set the session timeout
            options.Cookie.HttpOnly = true; // Prevent client-side access to session cookie
            options.Cookie.IsEssential = true; // Make session essential for app functionality
        });

// Future implementation of caching
// Configure Redis distributed cache
// builder.Services.AddStackExchangeRedisCache(options =>
// {
//     options.Configuration = "localhost:6379"; // Redis server address
//     options.InstanceName = "SampleApp_"; // Optional prefix for keys
// });

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStatusCodePages();
app.UseHttpsRedirection();
app.UseSession(); // Add session middleware
app.UseIpRateLimiting();
app.UseCors("AllowAll");
app.UseMiddleware<ApiKeyMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.UseWebSockets();
// Map SignalR hubs
app.MapHub<NotificationHub>("/notificationHub");

app.Run();