using System.Text.Json.Serialization;

namespace KcfMonitoringSystem.Worker.Models;

public class MqttPayloadMachineData
{
    [JsonPropertyName("NAME")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("OPERATORNAME")]
    public string Operator { get; set; } = string.Empty;
    [JsonPropertyName("WORKNAME")]
    public string Work { get; set; } = string.Empty;
    [JsonPropertyName("PRODUCTCOUNTER")]
    public int Qty { get; set; }
    [JsonPropertyName("STATUS")]
    public int Status { get; set; }

    [JsonPropertyName("TIMECOUNTER")]
    public uint TimeCounter { get; set; }
}

public class MqttMachineMessage
{
    [JsonPropertyName("Machine")]
    public MqttPayloadMachineData Machine { get; set; } = new();

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
}
