namespace KcfMonitoringSystem.Worker.Configuration;

public class MqttSettings
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 1883;
    public string ClientId { get; set; } = "KcfWorker";
    public string? Username { get; set; }
    public string? Password { get; set; }
    public List<string> Topics { get; set; } = new();
}
