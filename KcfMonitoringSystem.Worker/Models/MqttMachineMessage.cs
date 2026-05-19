using System.Text.Json.Serialization;

namespace KcfMonitoringSystem.Worker.Models;

public class MqttPayloadMachineData
{
    [JsonPropertyName("OPERATOR")]
    public string Operator { get; set; } = string.Empty;

    [JsonPropertyName("STATUS")]
    public int Status { get; set; }

    [JsonPropertyName("TIME COUNTER")]
    public uint TimeCounter { get; set; }

    [JsonPropertyName("PRODUCT")]
    public string Product { get; set; } = string.Empty;

    [JsonPropertyName("QTY")]
    public int Qty { get; set; }
}

public class MqttMachineMessage
{
    [JsonPropertyName("Machine")]
    public MqttPayloadMachineData Machine { get; set; } = new();

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
}
