using System.Text.Json.Serialization;

namespace NarrAItor.Shared.Models;

public class GameState(string? gameId = null)
{
    [JsonPropertyName("gameId")]
    public string GameId { get; set; } = gameId ?? Guid.NewGuid().ToString();

   [JsonPropertyName("rooms")]
    public Dictionary<string, Room> Rooms { get; set; } = new();

    [JsonPropertyName("currentRoomId")]
    public string? CurrentRoomId { get; set; }

    [JsonPropertyName("inventory")]
    public List<GameItem> Inventory { get; set; } = new();

    [JsonPropertyName("gameFlags")]
    public Dictionary<string, bool> GameFlags { get; set; } = new();

    [JsonPropertyName("gameLog")]
    public List<GameLogEntry> GameLog { get; set; } = new();

    [JsonPropertyName("metadata")]
    public GameMetadata Metadata { get; set; } = new();
    public DateTime LastActionTime { get; set; }

    public void AddToLog(string content, MessageType type = MessageType.System)
    {
        GameLog.Add(new GameLogEntry(content, type));
        LastActionTime = DateTime.UtcNow; // Update last action time
    }

    public void UpdateLastAction()
    {
        LastActionTime = DateTime.UtcNow;
    }
}

