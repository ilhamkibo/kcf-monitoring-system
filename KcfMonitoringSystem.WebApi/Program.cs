using KcfMonitoringSystem.Application.Interfaces.Repositories;
using KcfMonitoringSystem.Application.Services;
using KcfMonitoringSystem.Application.Filters;
using KcfMonitoringSystem.Infrastructure;
using KcfMonitoringSystem.Infrastructure.Data;
using KcfMonitoringSystem.Infrastructure.Persistence;
using KcfMonitoringSystem.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Scalar.AspNetCore;

// Setup Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File("Logs/webapi-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Starting KCF Monitoring WebApi...");

    var builder = WebApplication.CreateBuilder(args);

    // Replace default logging with Serilog
    builder.Host.UseSerilog();

    // Add services to the container.
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
        app.MapScalarApiReference(opt =>
        {
            opt.Title = "KCF Monitoring System";
            opt.Theme = ScalarTheme.Default;
            opt.Layout = ScalarLayout.Classic;
        });
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
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
