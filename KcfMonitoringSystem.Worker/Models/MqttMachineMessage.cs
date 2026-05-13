using System.Text.Json.Serialization;

namespace KcfMonitoringSystem.Worker.Models;

public class MqttMachineMessage
{
    [JsonPropertyName("machine_id")]
    public int MachineId { get; set; }

    [JsonPropertyName("user")]
    public string User { get; set; } = string.Empty;
}
