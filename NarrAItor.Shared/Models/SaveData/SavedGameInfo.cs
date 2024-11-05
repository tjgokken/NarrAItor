namespace NarrAItor.Shared.Models;

public class SavedGameInfo(string gameId, string theme)
{
    public string GameId { get; set; } = gameId;
    public string Theme { get; set; } = theme;
    public string Title { get; set; } = "Untitled Adventure";
    public DateTime SaveDate { get; set; } = DateTime.UtcNow;
}