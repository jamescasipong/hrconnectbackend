using System.Security.Claims;
using System.Threading.RateLimiting;
using AspNetCoreRateLimit;
using hrconnectbackend.Extensions;
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

bool IsSubscriptionValid(ClaimsPrincipal user, IEnumerable<string> allowedPlans)
{
    // Check if the user has a valid subscription claim
    var subscriptionClaim = user.Claims.FirstOrDefault(claim => claim.Type == "Subscription");
    if (subscriptionClaim == null || !allowedPlans.Contains(subscriptionClaim.Value))
    {
        return false;
    }

    // Check if the user has an expiration date claim
    var expirationClaim = user.FindFirst(claim => claim.Type == "ExpirationDate");
    if (expirationClaim == null || !DateTime.TryParse(expirationClaim.Value, out DateTime expirationDate))
    {
        return false;
    }

    // Ensure that the expiration date is in the future
    return expirationDate >= DateTime.Now;
}


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
builder.Services.AddCustomAuthorization();

builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(30); // Set your session timeout duration
        options.Cookie.HttpOnly = true; // Prevent client-side access to the session cookie
        options.Cookie.SameSite = SameSiteMode.None; // Specify SameSite policy
    });


builder.Services.AddCors(options =>
{
    options.AddPolicy("HRConnect", builder =>
    {
        builder.AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials()
               .WithOrigins("https://hrconnect.vercel.app", "http://localhost:3000"); // Replace with your React app's URL
    });
});

// Register SignalR services
builder.Services.AddSignalR();

// Add distributed memory cache
builder.Services.AddDistributedMemoryCache();
builder.Services.AddMemoryCache();
// Add session services
// builder.Services.AddSession(options =>
// {
//     options.IdleTimeout = TimeSpan.FromMinutes(30); // Set the session timeout
//     options.Cookie.HttpOnly = true; // Prevent client-side access to session cookie
//     options.Cookie.IsEssential = true; // Make session essential for app functionality
//     options.Cookie.SameSite = SameSiteMode.None; // Allow cross-site cookies
//     options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Ensure the cookie is only sent over HTTPS
// });

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
builder.Services.AddSwaggerGen(opt => 
{
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });
    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Id = "Bearer",
                            Type = ReferenceType.SecurityScheme
                        }
                    },
                    Array.Empty<string>()
                }
            });
});


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
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ConfigureHttpsDefaults(httpsOptions =>
    {
        httpsOptions.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
    });
});

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

app.UseHttpsRedirection();
app.UseHsts();
app.UseStatusCodePages();
app.UseRouting(); // Add UseRouting before UseCors
app.UseCors("HRConnect"); // Place UseCors after UseRouting
app.UseSession(); // Add session middleware
app.UseAuthentication();
app.UseAuthorization();
// app.Use(async (context, next) =>
// {
//     var dataContext = context.RequestServices.GetService<DataContext>()!;
//     var skipPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "/scalar", "/swagger", "/api/v1/user/account/login", "/api/Subscription", "/api/v1/user/account/login/verify", "/api/v1/user/account",  "/api/auth" };
//
//     if (skipPaths.Any(path => context.Request.Path.StartsWithSegments(path)))
//     {
//         await next(context);
//         return;
//     }
//     
//     var subscription = context.User.HasClaim(claim => claim.Type == CustomClaimTypes.Subscription);
//     var expiration = context.User.FindFirstValue(CustomClaimTypes.Expiration);
//     var subscriptionId = context.User.FindFirstValue(CustomClaimTypes.SubscriptionId);
//     
//     
//     if (int.TryParse(subscriptionId, out int subscriptionIdInt))
//     {
//         var userId = await dataContext.Subscriptions.OrderByDescending(a => a.EndDate).FirstOrDefaultAsync(x => x.Id == subscriptionIdInt);
//     
//         if (userId == null || userId.EndDate < DateTime.Now)
//         {
//             context.Response.StatusCode = 400;
//             await context.Response.WriteAsync("Subscription ID is invalid.");
//             return;
//         }
//     }
//     
//     if (expiration == null)
//     {
//         context.Response.StatusCode = 400;
//         await context.Response.WriteAsync("Subscription expiration is invalid.");
//         return;
//     }
//     else
//     {
//         if (DateTime.Parse(expiration) <= DateTime.Now.AddDays(5))
//         {
//             context.Response.StatusCode = 400;
//             await context.Response.WriteAsJsonAsync(new
//             {
//                 message = $"{subscription} has expired"
//             });
//             return;
//         }
//     }
//
//     if (!subscription)
//     {
//         context.Response.StatusCode = 404;
//         await context.Response.WriteAsJsonAsync(new
//         {
//             message = "Not authorized to access this resource."
//         });
//
//         return;
//     }
//     // Call the next delegate/middleware in the pipeline.
//     await next(context);
// });

app.UseWebSockets(); // Place UseWebSockets before MapControllers
app.MapControllers();

// Map SignalR hubs
app.MapHub<NotificationHub>("/notificationHub");

app.Run();