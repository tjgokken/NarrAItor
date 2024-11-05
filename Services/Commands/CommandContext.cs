using NarrAItor.Shared.Models;

namespace NarrAItor.Services.Commands;

public record CommandContext(
    GameState GameState,
    string Command,
    string[] Arguments
)
{
    public Room CurrentRoom
    {
        get
        {
            if (GameState == null)
                throw new InvalidOperationException("GameState is null.");

            if (GameState.CurrentRoomId == null)
                throw new InvalidOperationException("CurrentRoomId is null.");

            if (!GameState.Rooms.ContainsKey(GameState.CurrentRoomId))
                throw new KeyNotFoundException($"Room with ID '{GameState.CurrentRoomId}' does not exist in GameState.Rooms.");

            return GameState.Rooms[GameState.CurrentRoomId];
        }
    }

    public string FullArgument => string.Join(" ", Arguments);

    public void AddToLog(string message)
    {
        GameState.AddToLog(message);
    }
}