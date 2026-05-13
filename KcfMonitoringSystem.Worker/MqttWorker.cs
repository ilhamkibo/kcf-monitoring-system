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

        _logger.LogInformation("Message received on [{Topic}]: {Payload}", topic, payload);

        try
        {
            var message = JsonSerializer.Deserialize<MqttMachineMessage>(payload);

            if (message == null || string.IsNullOrWhiteSpace(message.User))
            {
                _logger.LogWarning("Invalid message on [{Topic}]: missing user field.", topic);
                return;
            }

            await ProcessUserAsync(message);
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

    private async Task ProcessUserAsync(MqttMachineMessage message)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var existingUser = await db.Users
            .FirstOrDefaultAsync(u => u.Name.ToLower() == message.User.ToLower());

        if (existingUser != null)
        {
            _logger.LogInformation("User '{User}' already exists (Id: {Id}). Skipping insert.", message.User, existingUser.Id);
            // TODO: nanti bisa update data mesin di sini jika entity Machine sudah ada
            return;
        }

        // User belum ada, tambahkan ke database
        var newUser = new KcfMonitoringSystem.Domain.Entities.User
        {
            Name = message.User,
            CreatedAt = DateTime.UtcNow
        };

        db.Users.Add(newUser);
        await db.SaveChangesAsync();

        _logger.LogInformation("New user '{User}' created with Id: {Id}.", newUser.Name, newUser.Id);
    }
}
