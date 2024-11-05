using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.JSInterop;
using NarrAItor.Shared.Interfaces;
using NarrAItor.Shared.Models;

namespace NarrAItor.Services;

public class GameClientService(HttpClient httpClient, ILogger<GameClientService> logger, IJSRuntime jsRuntime) : IGameService
{
    private const string STORAGE_KEY_PREFIX = "narraitor_save_";
    private const int TimeoutSeconds = 45;
    private const int MaxRetries = 2;
    private const int RetryDelayMs = 1000;


    public async Task<GameState> GenerateNewGame(string theme, CancellationToken cancellationToken = default)
    {
        var retryCount = 0;

        while (true)
            try
            {
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                linkedCts.CancelAfter(TimeSpan.FromSeconds(TimeoutSeconds));

                var response = await httpClient.PostAsync(
                    $"/api/game/generate?theme={Uri.EscapeDataString(theme)}",
                    null,
                    linkedCts.Token);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadFromJsonAsync<ApiError>(
                        linkedCts.Token);

                    logger.LogError("Failed to generate new game. Status: {Status}, Error: {@Error}",
                        response.StatusCode, errorContent);

                    if (response.StatusCode == HttpStatusCode.ServiceUnavailable && retryCount < MaxRetries)
                    {
                        retryCount++;
                        logger.LogInformation("Retrying game generation. Attempt {Attempt} of {MaxRetries}",
                            retryCount, MaxRetries);
                        await Task.Delay(RetryDelayMs * retryCount, cancellationToken);
                        continue;
                    }

                    if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                        throw new GameGenerationTimeoutException(
                            errorContent?.Error ?? "Generation timed out after multiple attempts");

                    throw new GameGenerationException(
                        errorContent?.Error ?? "Failed to generate game");
                }

                //var responseContent = await response.Content.ReadAsStringAsync();
                var gameState = await response.Content.ReadFromJsonAsync<GameState>(
                    linkedCts.Token);

                // Validate and ensure every room has at least one exit
                if (gameState != null) EnsureRoomsHaveExits(gameState);

                return gameState ?? throw new GameGenerationException("Failed to deserialize game state");
            }
            catch (OperationCanceledException) when (retryCount < MaxRetries)
            {
                retryCount++;
                logger.LogWarning(
                    "Game generation attempt {Attempt} of {MaxRetries} timed out after {Timeout} seconds",
                    retryCount, MaxRetries, TimeoutSeconds);
                await Task.Delay(RetryDelayMs * retryCount, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                logger.LogWarning(
                    "Game generation failed after {Attempts} attempts, total time: {TotalTime} seconds",
                    retryCount + 1, (retryCount + 1) * TimeoutSeconds);
                throw new GameGenerationTimeoutException(
                    $"Request timed out after {(retryCount + 1) * TimeoutSeconds} seconds and {retryCount + 1} attempts. Please try again.");
            }
            catch (Exception ex) when (ex is not GameGenerationException && ex is not GameGenerationTimeoutException)
            {
                logger.LogError(ex, "Failed to generate new game");
                throw new GameGenerationException("Failed to generate new game", ex);
            }
    }

    public async Task<bool> SaveGame(GameState gameState, string title)
    {
        try
        {
            logger.LogInformation("Saving game to local storage: {SaveName}", title);
            var serializedGame = JsonSerializer.Serialize(gameState);
            await jsRuntime.InvokeVoidAsync(
                "localStorage.setItem",
                $"{STORAGE_KEY_PREFIX}{title}",
                serializedGame
            );
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save game to local storage");
            return false;
        }
    }

    public async Task<GameState> LoadGame(string gameId)
    {
        try
        {
            var serializedState = await jsRuntime.InvokeAsync<string>(
                "localStorage.getItem",
                $"{STORAGE_KEY_PREFIX}state_{gameId}"
            );

            return string.IsNullOrEmpty(serializedState)
                ? new GameState(string.Empty) // Return a new empty GameState instead of null
                : JsonSerializer.Deserialize<GameState>(serializedState) ?? new GameState(string.Empty);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load game locally");
            return new GameState(string.Empty);
        }
    }

    public async Task<List<SavedGameInfo>> GetSavedGames()
    {
        try
        {
            var saves = new List<SavedGameInfo>();
            var keys = await jsRuntime.InvokeAsync<string[]>("eval", @"
                Object.keys(localStorage)
                    .filter(key => key.startsWith('" + STORAGE_KEY_PREFIX + @"info_'))
            ");

            foreach (var key in keys)
            {
                var serializedInfo = await jsRuntime.InvokeAsync<string>(
                    "localStorage.getItem",
                    key
                );

                if (!string.IsNullOrEmpty(serializedInfo))
                {
                    var saveInfo = JsonSerializer.Deserialize<SavedGameInfo>(serializedInfo);
                    if (saveInfo != null) saves.Add(saveInfo);
                }
            }

            return saves.OrderByDescending(s => s.SaveDate).ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get local saved games");
            throw;
        }
    }

    public async Task<bool> DeleteGame(string gameId)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync(
                "localStorage.removeItem",
                $"{STORAGE_KEY_PREFIX}info_{gameId}"
            );
            await jsRuntime.InvokeVoidAsync(
                "localStorage.removeItem",
                $"{STORAGE_KEY_PREFIX}state_{gameId}"
            );
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete local game");
            return false;
        }
    }

    public void EnsureRoomsHaveExits(GameState gameState)
    {
        var rooms = gameState.Rooms.Values.ToList();
        var random = new Random();

        foreach (var room in rooms)
        {
            if (room.Exits.Count != 0) continue;

            // Pick a random room to create an exit to
            var targetRoom = rooms[random.Next(rooms.Count)];

            // Ensure the target room is not the same as the current room
            while (targetRoom.Id == room.Id) targetRoom = rooms[random.Next(rooms.Count)];

            // Get a random direction for the exit
            var direction = GetRandomDirection();

            // Create a new exit with appropriate arguments for targetId and optional parameters
            var newExit = new Exit(
                targetRoom.Id,
                $"A doorway leading {direction} to {targetRoom.Name}.",
                "none"
            );

            // Add the exit to the room's exits collection
            room.Exits = new Dictionary<string, Exit>
            {
                { direction, newExit }
            };
        }
    }


    // Helper function to get a random direction
    private string GetRandomDirection()
    {
        string[] directions = { "north", "south", "east", "west" };
        return directions[new Random().Next(directions.Length)];
    }
}