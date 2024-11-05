using System.Text.Json.Serialization;

namespace NarrAItor.Shared.Models;

[JsonConverter(typeof(GameItemConverter))]
public class GameItem(string name, string? id = null)
{
    public string Id { get; set; } = id ?? Guid.NewGuid().ToString();
    public string Name { get; set; } = name;
    public string Description { get; set; } = string.Empty;
    public List<string> Interactions { get; set; } = new();
    public Dictionary<string, string> Properties { get; set; } = new();

}