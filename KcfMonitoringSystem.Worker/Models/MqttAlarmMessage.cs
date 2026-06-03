using System;
using System.Text.Json.Serialization;

namespace KcfMonitoringSystem.Worker.Models;

public class MqttAlarmMessage
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("trigger_time")]
    public DateTime TriggerTime { get; set; }

    [JsonPropertyName("recover_time")]
    public DateTime? RecoverTime { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("ts")]
    public DateTime Ts { get; set; }
}
