using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using KcfMonitoringSystem.Infrastructure.Persistence;
using KcfMonitoringSystem.Worker.Configuration;
using KcfMonitoringSystem.Worker.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Packets;
using MQTTnet.Protocol;

namespace KcfMonitoringSystem.Worker;

public class MqttWorker : BackgroundService
{
    private readonly ILogger<MqttWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly MqttSettings _mqttSettings;
    private IMqttClient? _mqttClient;
    private readonly ConcurrentDictionary<int, bool> _initializedMachines = new();

    private const int ReconnectDelaySeconds = 5;

    public MqttWorker(
        ILogger<MqttWorker> logger,
        IServiceScopeFactory scopeFactory,
        IOptions<MqttSettings> mqttSettings)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _mqttSettings = mqttSettings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConnectAndSubscribeAsync(stoppingToken);

                // Keep alive loop — reconnect if disconnected
                while (!stoppingToken.IsCancellationRequested)
                {
                    if (_mqttClient == null || !_mqttClient.IsConnected)
                    {
                        _logger.LogWarning("MQTT client disconnected. Reconnecting in {Delay}s...", ReconnectDelaySeconds);
                        await Task.Delay(TimeSpan.FromSeconds(ReconnectDelaySeconds), stoppingToken);
                        break; // break inner loop to reconnect via outer loop
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Graceful shutdown
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MQTT connection error. Retrying in {Delay}s...", ReconnectDelaySeconds);
                await Task.Delay(TimeSpan.FromSeconds(ReconnectDelaySeconds), stoppingToken);
            }
        }

        // Cleanup
        if (_mqttClient is { IsConnected: true })
        {
            await _mqttClient.DisconnectAsync();
            _logger.LogInformation("MQTT client disconnected gracefully.");
        }
    }

    private async Task ConnectAndSubscribeAsync(CancellationToken stoppingToken)
    {
        var factory = new MqttClientFactory();
        _mqttClient = factory.CreateMqttClient();

        // Build connection options
        var optionsBuilder = new MqttClientOptionsBuilder()
            .WithTcpServer(_mqttSettings.Host, _mqttSettings.Port)
            .WithClientId(_mqttSettings.ClientId)
            .WithCleanSession();

        if (!string.IsNullOrWhiteSpace(_mqttSettings.Username))
        {
            optionsBuilder.WithCredentials(_mqttSettings.Username, _mqttSettings.Password);
        }

        var options = optionsBuilder.Build();

        // Wire up the message handler
        _mqttClient.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;

        _mqttClient.DisconnectedAsync += args =>
        {
            if (args.Exception != null)
                _logger.LogWarning(args.Exception, "MQTT disconnected unexpectedly.");
            else
                _logger.LogInformation("MQTT disconnected.");
            return Task.CompletedTask;
        };

        // Connect
        _logger.LogInformation("Connecting to MQTT broker at {Host}:{Port}...", _mqttSettings.Host, _mqttSettings.Port);
        await _mqttClient.ConnectAsync(options, stoppingToken);
        _logger.LogInformation("Connected to MQTT broker successfully.");

        // Subscribe to all configured topics
        var subscribeOptions = new MqttClientSubscribeOptionsBuilder();
        foreach (var topic in _mqttSettings.Topics)
        {
            subscribeOptions.WithTopicFilter(topic, MqttQualityOfServiceLevel.AtLeastOnce);
            _logger.LogInformation("Subscribed to topic: {Topic}", topic);
        }

        await _mqttClient.SubscribeAsync(subscribeOptions.Build(), stoppingToken);
    }

    private async Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        var topic = args.ApplicationMessage.Topic;
        var payload = Encoding.UTF8.GetString(args.ApplicationMessage.Payload);

        _logger.LogDebug("Message received on [{Topic}]: {Payload}", topic, payload);

        try
        {
            var message = JsonSerializer.Deserialize<MqttMachineMessage>(payload);

            if (message == null || message.Machine == null)
            {
                _logger.LogWarning("Invalid message on [{Topic}]: missing Machine data.", topic);
                return;
            }

            await ProcessMachineDataAsync(topic, message);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize MQTT payload on [{Topic}]: {Payload}", topic, payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message on [{Topic}]", topic);
        }
    }

    private async Task ProcessMachineDataAsync(string topic, MqttMachineMessage message)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var timestamp = message.Timestamp != default ? message.Timestamp : DateTime.UtcNow;
        int machineId = GetMachineIdFromTopic(topic);

        var operatorName = message.Machine.Operator?.Trim() ?? string.Empty;
        var productName = message.Machine.Product?.Trim() ?? string.Empty;

        int userId = await GetUserIdAsync(db, operatorName);
        int productId = await GetProductIdAsync(db, productName);

        bool statusChanged = await ProcessStatusAsync(db, machineId, message.Machine.Status, timestamp);
        bool productionChanged = await ProcessProductionAsync(db, machineId, userId, productId, message.Machine.Qty, timestamp);

        await db.SaveChangesAsync();

        if (statusChanged || productionChanged)
        {
            _logger.LogInformation("Data updated for [{Topic}]. Status Changed: {StatusChanged}, Production Changed: {ProductionChanged}", topic, statusChanged, productionChanged);
        }
        else
        {
            _logger.LogDebug("Successfully processed machine data for [{Topic}] (No state changes)", topic);
        }
    }

    private int GetMachineIdFromTopic(string topic)
    {
        var match = System.Text.RegularExpressions.Regex.Match(topic, @"\d+");
        return match.Success && int.TryParse(match.Value, out int parsedId) ? parsedId : 1;
    }

    private async Task<int> GetUserIdAsync(AppDbContext db, string operatorName)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Name.ToLower() == operatorName.ToLower());
        return user?.Id ?? 1;
    }

    private async Task<int> GetProductIdAsync(AppDbContext db, string productName)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => 
            p.PartName.ToLower() == productName.ToLower() || 
            p.ProductNo.ToLower() == productName.ToLower() || 
            p.PartNo.ToLower() == productName.ToLower());
        return product?.Id ?? 1;
    }

    private async Task<bool> ProcessStatusAsync(AppDbContext db, int machineId, int statusCode, DateTime timestamp)
    {
        bool isFirstMessageSinceStartup = _initializedMachines.TryAdd(machineId, true);

        if (isFirstMessageSinceStartup)
        {
            db.Statuses.Add(new KcfMonitoringSystem.Domain.Entities.Status
            {
                MachineId = machineId,
                Code = statusCode,
                CreatedAt = timestamp,
                UpdatedAt = timestamp
            });
            return true; // Status changed (forced new entry)
        }

        var lastStatus = await db.Statuses
            .Where(s => s.MachineId == machineId)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync();

        if (lastStatus != null && lastStatus.Code == statusCode)
        {
            lastStatus.UpdatedAt = timestamp;
            db.Statuses.Update(lastStatus);
            return false; // Status didn't change
        }

        db.Statuses.Add(new KcfMonitoringSystem.Domain.Entities.Status
        {
            MachineId = machineId,
            Code = statusCode,
            CreatedAt = timestamp,
            UpdatedAt = timestamp
        });
        return true; // Status changed
    }

    private async Task<bool> ProcessProductionAsync(AppDbContext db, int machineId, int userId, int productId, int qty, DateTime timestamp)
    {
        var lastProduction = await db.Productions
            .Where(p => p.MachineId == machineId)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync();

        if (lastProduction != null && lastProduction.UserId == userId && lastProduction.ProductId == productId)
        {
            lastProduction.Quantity = qty;
            lastProduction.UpdatedAt = timestamp;
            db.Productions.Update(lastProduction);
            return false; // Production didn't change
        }

        db.Productions.Add(new KcfMonitoringSystem.Domain.Entities.Production
        {
            MachineId = machineId,
            UserId = userId,
            ProductId = productId,
            Quantity = qty,
            CreatedAt = timestamp,
            UpdatedAt = timestamp
        });
        return true; // Production changed
    }
}
