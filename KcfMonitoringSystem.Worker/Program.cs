using KcfMonitoringSystem.Infrastructure.Persistence;
using KcfMonitoringSystem.Worker;
using KcfMonitoringSystem.Worker.Configuration;
using Microsoft.EntityFrameworkCore;
using Serilog;

// Setup Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File("Logs/worker-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Starting KCF Monitoring Worker...");

    var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
    {
        Args = args,
        ContentRootPath = AppContext.BaseDirectory
    });

    // Enable Windows Service support
    builder.Services.AddWindowsService();

    // Replace default logging with Serilog
    builder.Services.AddSerilog();

    // Bind MQTT settings from configuration
    builder.Services.Configure<MqttSettings>(
        builder.Configuration.GetSection("Mqtt"));

    // Register AppDbContext
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseMySQL(builder.Configuration.GetConnectionString("DefaultConnection") ?? ""));

    // Register MqttWorker as a hosted service
    builder.Services.AddHostedService<MqttWorker>();

    var host = builder.Build();
    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Worker terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
