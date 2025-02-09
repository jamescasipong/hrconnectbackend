using System.Text;
using hrconnectbackend.Data;
using hrconnectbackend.Helper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
ServicesInjection.CorsHandler(builder.Services);
ServicesInjection.IRepositories(builder.Services);
ServicesInjection.ProfileMapper(builder.Services);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<DataContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddControllers();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(
            builder.Configuration.GetSection("JWT:Key").Value
            )),
            ValidateIssuer = false,
            // ValidIssuer = builder.Configuration.GetSection("JWT:Issuer").Value,
            ValidateAudience = false,
            // ValidAudience = builder.Configuration.GetSection("JWT:Audience").Value,
            ValidateLifetime = true,
            // ClockSkew = TimeSpan.Zero
        };
    });


builder.Services.AddAuthorization(options =>
{
    // options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireAdmin", policy =>
    {
        policy.RequireAssertion(context =>
        {
            return context.User.HasClaim(claim => claim.Type == "Role" && claim.Value == "Admin" || claim.Value == "SuperAdmin");
        });


    });

    options.AddPolicy("RequireUser", policy =>
    {
        policy.RequireAssertion(context =>
        {
            return context.User.HasClaim(claim => claim.Type == "Role" && claim.Value == "User");
        });
    });

});
// Swagger setup
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.CustomSchemaIds(type => type.FullName);

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter your bearer token",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
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
app.UseCors("AllowAll");


// Remove these if you're not using authentication
app.UseAuthentication();  // Only needed if you are using authentication middleware
app.UseAuthorization();   // Only needed if authorization is required

app.MapControllers();

app.Run();
