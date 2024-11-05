using System.Text.Json.Serialization;

namespace NarrAItor.Shared.Models;

// NPC - Non-playable character => controlled by the game or the computer
[JsonConverter(typeof(NPCConverter))]
public class NPC(string name, string? id = null)
{
    public string Id { get; set; } = id ?? Guid.NewGuid().ToString();
    public string Name { get; set; } = name;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, string> Dialogue { get; set; } = new();
    public string? CurrentDialog { get; set; }
    public List<GameItem> Inventory { get; set; } = new();
}