using System.Text.Json.Serialization;

namespace NarrAItor.Shared.Models;

public class RoomDescription(string initial)
{
    [JsonPropertyName("initial")] public string Initial { get; set; } = initial;

    [JsonPropertyName("subsequent")] public string? Subsequent { get; set; }

    [JsonPropertyName("detailed")] public string? Detailed { get; set; }
}