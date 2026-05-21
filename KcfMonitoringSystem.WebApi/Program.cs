using KcfMonitoringSystem.Application.Interfaces.Repositories;
using KcfMonitoringSystem.Application.Services;
using KcfMonitoringSystem.Infrastructure.Data;
using KcfMonitoringSystem.Infrastructure.Persistence;
using KcfMonitoringSystem.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Scalar.AspNetCore;
using KcfMonitoringSystem.WebApi.Endpoints;
using KcfMonitoringSystem.Application.Interfaces.Services;
using KcfMonitoringSystem.Domain.Entities;

// Setup Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware", Serilog.Events.LogEventLevel.Fatal)
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
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });

    builder.Services.AddOpenApi();
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IProductionRepository, ProductionRepository>();
    builder.Services.AddScoped<IProductionService, ProductionService>();
    builder.Services.AddScoped<IProductRepository, ProductRepository>();
    builder.Services.AddScoped<IProductService, ProductService>();
    builder.Services.AddScoped<IStatusRepository, StatusRepository>();
    builder.Services.AddScoped<IStatusService, StatusService>();

    var app = builder.Build();

    // Enable CORS
    app.UseCors("AllowAll");

    // Global Exception Handler
    app.UseExceptionHandler(exceptionHandlerApp =>
    {
        exceptionHandlerApp.Run(async context =>
        {
            var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
            var exception = exceptionHandlerPathFeature?.Error;

            context.Response.ContentType = "application/json";

            if (exception is BadHttpRequestException badRequestEx)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;

                // Parse "Failed to bind parameter "int limit" from "as""
                // var regex = new System.Text.RegularExpressions.Regex(@"Failed to bind parameter ""([^""]+)"" from ""([^""]+)""");
                var regex = new System.Text.RegularExpressions.Regex(@"Failed to bind parameter ""([^""]+)"" from ""([^""]*)""");

                var match = regex.Match(badRequestEx.Message);

                if (match.Success)
                {
                    var paramPart = match.Groups[1].Value.Split(' ').Last();
                    var fieldName = char.ToUpper(paramPart[0]) + paramPart.Substring(1);
                    var invalidValue = match.Groups[2].Value;

                    var errors = new System.Collections.Generic.Dictionary<string, string[]>
                    {
                        { fieldName, new[] { $"The value '{invalidValue}' is not valid for {fieldName}." } }
                    };

                    var response = KcfMonitoringSystem.Application.Common.ApiResponse<object>.Error("Error Validations", errors);
                    await context.Response.WriteAsJsonAsync(response);
                }
                else
                {
                    var response = KcfMonitoringSystem.Application.Common.ApiResponse<object>.Error($"Invalid request: {badRequestEx.Message}");
                    await context.Response.WriteAsJsonAsync(response);
                }
            }
            else if (exception != null)
            {
                Log.Error(exception, "An unhandled exception occurred in the API");
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                var response = KcfMonitoringSystem.Application.Common.ApiResponse<object>.Error("An unexpected error occurred.");
                await context.Response.WriteAsJsonAsync(response);
            }
        });
    });

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
    app.MapUserEndpoints();
    app.MapProductionEndpoints();
    app.MapProductEndpoints();
    app.MapStatusEndpoints();

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
