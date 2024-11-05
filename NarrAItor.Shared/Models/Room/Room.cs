using System.Text.Json.Serialization;

namespace NarrAItor.Shared.Models;

[JsonConverter(typeof(RoomConverter))]
public class Room(string name, string initialDescription)
{
    [JsonPropertyName("id")] public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("name")] public string Name { get; set; } = name;

    [JsonPropertyName("description")] public RoomDescription Description { get; set; } = new(initialDescription);

    [JsonPropertyName("exits")] public Dictionary<string, Exit> Exits { get; set; } = new();

    [JsonPropertyName("items")] public List<GameItem> Items { get; set; } = new();

    [JsonPropertyName("npcs")] public List<NPC> NPCs { get; set; } = new();

    [JsonPropertyName("atmosphere")] public RoomAtmosphere? Atmosphere { get; set; }
}