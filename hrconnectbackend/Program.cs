using Microsoft.EntityFrameworkCore;
using hrconnectbackend.Models;
using hrconnectbackend.Data;
using MongoDB.Driver.Core.Configuration;
using hrconnectbackend.Repositories;
using hrconnectbackend.IRepositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IEmployeeRepositories, EmployeeRepositories>();
builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll",
            builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
    });
// Add services to the container.
builder.Services.AddDbContext<DataContext>(options => options.UseSqlServer("Data Source=Arisu;Initial Catalog=hrbackend;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False"));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.CustomSchemaIds(type => type.FullName);

});

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();


app.UseAuthorization();

app.MapControllers();

app.Run();
