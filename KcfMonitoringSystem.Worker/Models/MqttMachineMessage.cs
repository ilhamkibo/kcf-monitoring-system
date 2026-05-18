using System.Text.Json.Serialization;

namespace KcfMonitoringSystem.Worker.Models;

public class MqttMachineMessage
{
    [JsonPropertyName("machine_id")]
    public int MachineId { get; set; }

    [JsonPropertyName("operator")]
    public string User { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("duration")]
    public int Duration { get; set; }

    [JsonPropertyName("qty")]
    public int Qty { get; set; }

    [JsonPropertyName("workname")]
    public string Workname { get; set; } = string.Empty;
}
