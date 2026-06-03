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

            // Subscribe to alarm topic for this machine
            var alarmTopic = $"{topic}/alarm";
            subscribeOptions.WithTopicFilter(alarmTopic, MqttQualityOfServiceLevel.AtLeastOnce);
            _logger.LogInformation("Subscribed to alarm topic: {Topic}", alarmTopic);
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
            if (topic.EndsWith("/alarm", StringComparison.OrdinalIgnoreCase))
            {
                var alarmMessage = JsonSerializer.Deserialize<MqttAlarmMessage>(payload);
                if (alarmMessage == null)
                {
                    _logger.LogWarning("Invalid alarm message on [{Topic}]", topic);
                    return;
                }
                await ProcessAlarmDataAsync(topic, alarmMessage);
            }
            else
            {
                var message = JsonSerializer.Deserialize<MqttMachineMessage>(payload);

                if (message == null || message.Machine == null)
                {
                    _logger.LogWarning("Invalid message on [{Topic}]: missing Machine data.", topic);
                    return;
                }

                await ProcessMachineDataAsync(topic, message);
            }
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

    private async Task ProcessAlarmDataAsync(string topic, MqttAlarmMessage message)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Find the machine by extracting machine ID from topic
        int topicId = GetMachineIdFromTopic(topic);
        var machine = await db.Machines.FindAsync(topicId);
        if (machine == null)
        {
            _logger.LogWarning("Skipping alarm message on [{Topic}]: Machine with ID {MachineId} not found in database.", topic, topicId);
            return;
        }

        bool isRecovered = string.Equals(message.Status, "recovered", StringComparison.OrdinalIgnoreCase);

        if (isRecovered)
        {
            // Try to find the corresponding active triggered alarm to update it
            var existingAlarm = await db.AlarmHistories
                .Where(a => a.MachineId == machine.Id && 
                            a.TriggerTime == message.TriggerTime && 
                            a.Message == message.Message && 
                            a.Status == "triggered")
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync();

            if (existingAlarm != null)
            {
                existingAlarm.Status = "recovered";
                existingAlarm.RecoverTime = message.RecoverTime ?? message.Ts;
                existingAlarm.Timestamp = message.Ts;
                existingAlarm.UpdatedAt = DateTime.UtcNow;

                db.AlarmHistories.Update(existingAlarm);
                await db.SaveChangesAsync();

                _logger.LogInformation("Updated alarm history to recovered for Machine '{MachineName}' on [{Topic}]: {Message}", machine.Name, topic, message.Message);
                return;
            }
        }

        // If it's a trigger, or we couldn't find the triggered alarm, create a new record
        var alarmHistory = new KcfMonitoringSystem.Domain.Entities.AlarmHistory
        {
            MachineId = machine.Id,
            Status = message.Status.Trim(),
            TriggerTime = message.TriggerTime,
            RecoverTime = message.RecoverTime,
            Message = message.Message.Trim(),
            Timestamp = message.Ts,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.AlarmHistories.Add(alarmHistory);
        await db.SaveChangesAsync();

        _logger.LogInformation("Saved new alarm history for Machine '{MachineName}' on [{Topic}]: {Status} - {Message}", machine.Name, topic, message.Status, message.Message);
    }

    private async Task ProcessMachineDataAsync(string topic, MqttMachineMessage message)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var timestamp = message.Timestamp != default ? message.Timestamp : DateTime.UtcNow;

        int? machineId = await GetMachineIdAsync(db, message.Machine.Name, topic);
        if (machineId == null)
        {
            _logger.LogWarning("Skipping message on [{Topic}]: Machine '{MachineName}' or topic ID not found in database.", topic, message.Machine.Name);
            return;
        }

        var operatorName = message.Machine.Operator?.Trim() ?? string.Empty;
        var productName = message.Machine.Work?.Trim() ?? string.Empty;

        int userId = await GetUserIdAsync(db, operatorName);
        int productId = await GetProductIdAsync(db, productName);

        bool statusChanged = await ProcessStatusAsync(db, machineId.Value, message.Machine.Status, timestamp, userId, productId);
        bool productionChanged = await ProcessProductionAsync(db, machineId.Value, userId, productId, message.Machine.Qty, timestamp);

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

    private async Task<int?> GetMachineIdAsync(AppDbContext db, string machineName, string topic)
    {
        // 1. Try to find by NAME if name is not empty or a default placeholder
        if (!string.IsNullOrWhiteSpace(machineName))
        {
            var machine = await db.Machines.FirstOrDefaultAsync(m => m.Name.ToLower() == machineName.ToLower());
            if (machine != null)
            {
                return machine.Id;
            }
        }

        // 2. Fallback to topic parsing
        int topicId = GetMachineIdFromTopic(topic);
        var machineByTopicId = await db.Machines.FindAsync(topicId);
        if (machineByTopicId != null)
        {
            return machineByTopicId.Id;
        }

        return null;
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

    private async Task<bool> ProcessStatusAsync(AppDbContext db, int machineId, int statusCode, DateTime timestamp, int userId, int productId)
    {
        bool isFirstMessageSinceStartup = _initializedMachines.TryAdd(machineId, true);

        var lastStatus = await db.Statuses
            .Where(s => s.MachineId == machineId)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync();

        // Check if the gap since the last update is too large (e.g. worker was offline or machine was powered off)
        // If the gap is > 2 minutes, we treat this as a new session and do not stretch the previous duration.
        bool isGapTooLarge = lastStatus != null && 
            (timestamp - (lastStatus.UpdatedAt ?? lastStatus.CreatedAt)).TotalMinutes > 2;

        if (isFirstMessageSinceStartup || 
            isGapTooLarge || 
            lastStatus == null || 
            lastStatus.Code != statusCode || 
            lastStatus.UserId != userId || 
            lastStatus.ProductId != productId)
        {
            db.Statuses.Add(new KcfMonitoringSystem.Domain.Entities.Status
            {
                MachineId = machineId,
                Code = statusCode,
                UserId = userId,
                ProductId = productId,
                CreatedAt = timestamp,
                UpdatedAt = timestamp,
                Duration = 0
            });
            return true; // New status or session started
        }

        // Continuous update within the same session
        lastStatus.UpdatedAt = timestamp;
        lastStatus.Duration = (int)(timestamp - lastStatus.CreatedAt).TotalSeconds;
        db.Statuses.Update(lastStatus);
        return false; // Status didn't change
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
