using KcfMonitoringSystem.Infrastructure.Persistence;
using KcfMonitoringSystem.Worker;
using KcfMonitoringSystem.Worker.Configuration;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

// Bind MQTT settings from configuration
builder.Services.Configure<MqttSettings>(
    builder.Configuration.GetSection("Mqtt"));

// Register AppDbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register MqttWorker as a hosted service
builder.Services.AddHostedService<MqttWorker>();

var host = builder.Build();
host.Run();
