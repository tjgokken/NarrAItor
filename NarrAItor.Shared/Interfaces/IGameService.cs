using NarrAItor.Shared.Models;

namespace NarrAItor.Shared.Interfaces;

public interface IGameService
{
    Task<GameState> GenerateNewGame(string theme, CancellationToken cancellationToken = default);
    Task<bool> SaveGame(GameState gameState, string saveName);
    Task<GameState> LoadGame(string saveName);
    Task<List<SavedGameInfo>> GetSavedGames();
    Task<bool> DeleteGame(string saveName);
}