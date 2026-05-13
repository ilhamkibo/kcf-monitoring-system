using KcfMonitoringSystem.Application.Interfaces.Repositories;
using KcfMonitoringSystem.Application.Services;
using KcfMonitoringSystem.Application.Filters;
using KcfMonitoringSystem.Infrastructure;
using KcfMonitoringSystem.Infrastructure.Data;
using KcfMonitoringSystem.Infrastructure.Persistence;
using KcfMonitoringSystem.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Automatically apply migrations and run seeder
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    await DbSeeder.SeedAsync(db);
}

// Minimal APIs
var usersGroup = app.MapGroup("/api/users");
// usersGroup.MapGet("/", async ([AsParameters] UserFilter filter, IUserService userService) =>
// {
usersGroup.MapGet("/", async (IUserService userService, [FromQuery] int page = 1, [FromQuery] int limit = 10, [FromQuery] string? search = null, [FromQuery] bool paginate = true) =>
{
    var filter = new UserFilter { Page = page, Limit = limit, Search = search, Paginate = paginate };
    var response = await userService.GetAllAsync(filter);
    return Results.Ok(response);
});

usersGroup.MapGet("/{id}", async (int id, IUserService userService) =>
{
    var response = await userService.GetByIdAsync(id);
    if (!response.Status)
        return Results.NotFound(response);

    return Results.Ok(response);
});

app.Run();
