using System.Text.Json;
using NarrAItor.Shared.Interfaces;
using NarrAItor.Shared.Models;

namespace NarrAItor.Api.Services;

public class GameService(OpenAIService openAIService, ILogger<GameService> logger) : IGameService
{
    public async Task<GameState> GenerateNewGame(string theme, CancellationToken cancellationToken = default)
    {
        try
        {
            var promptGenerator = new PromptGenerator();
            var prompt = promptGenerator.GenerateGamePrompt(theme);

            // Pass the cancellation token to OpenAI service
            var response = await openAIService.GenerateCompletion(prompt, cancellationToken);

            var gameState = new GameState();

            var jsonResponse = JsonSerializer.Deserialize<JsonDocument>(response);
            if (jsonResponse?.RootElement is null) throw new GameGenerationException("Invalid response format");

            ParseGameResponse(jsonResponse.RootElement, gameState);
            //await SaveGame(gameState);
            return gameState;
        }
        catch (OpenAITimeoutException ex)
        {
            logger.LogWarning(ex, "Game generation timed out for theme: {Theme}", theme);
            throw new GameGenerationTimeoutException(
                "Game generation is taking longer than expected. Please try again.", ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to generate new game with theme: {Theme}", theme);
            throw new GameGenerationException("Failed to generate new game", ex);
        }
    }

    public Task<bool> SaveGame(GameState gameState, string saveName)
    {
        return Task.FromResult(true);
    }

    public Task<GameState> LoadGame(string saveName)
    {
        return Task.FromResult(new GameState(string.Empty));
    }

    public Task<List<SavedGameInfo>> GetSavedGames()
    {
        return Task.FromResult(new List<SavedGameInfo>());
    }

    public Task<bool> DeleteGame(string saveName)
    {
        return Task.FromResult(true);
    }

    private void ParseGameResponse(JsonElement root, GameState gameState)
    {
        // Update metadata
        if (root.TryGetProperty("title", out var titleElement))
            gameState.Metadata.Title = titleElement.GetString() ?? "Untitled Adventure";

        // Parse rooms
        if (root.TryGetProperty("rooms", out var roomsElement))
        {
            foreach (var roomElement in roomsElement.EnumerateArray())
            {
                var room = ParseRoom(roomElement);
                gameState.Rooms[room.Id] = room;
            }

            // Link room exits after all rooms are created
            LinkRoomExits(gameState.Rooms);
        }

        // Set initial room
        if (gameState.Rooms.Any()) gameState.CurrentRoomId = gameState.Rooms.First().Key;
    }

    private static Room ParseRoom(JsonElement roomData)
    {
        var name = roomData.GetProperty("name").GetString()
                   ?? throw new JsonException("Room name is required");

        var initialDesc = roomData.GetProperty("description")
                              .GetProperty("initial").GetString()
                          ?? throw new JsonException("Initial Room description is required");

        var room = new Room(name, initialDesc);

        if (roomData.TryGetProperty("description", out var descElement))
        {
            if (descElement.TryGetProperty("subsequent", out var subElement))
                room.Description.Subsequent = subElement.GetString();
            if (descElement.TryGetProperty("detailed", out var detElement))
                room.Description.Detailed = detElement.GetString();
        }

        if (roomData.TryGetProperty("atmosphere", out var atmElement))
            room.Atmosphere = new RoomAtmosphere(
                atmElement.GetProperty("sights").GetString(),
                atmElement.GetProperty("sounds").GetString(),
                atmElement.GetProperty("smells").GetString()
            );

        ParseRoomExits(roomData, room);
        ParseRoomItems(roomData, room);
        ParseRoomNPCs(roomData, room);

        return room;
    }

    private static void ParseRoomExits(JsonElement roomData, Room room)
    {
        if (roomData.TryGetProperty("exits", out var exitsElement))
            foreach (var exit in exitsElement.EnumerateObject())
            {
                var exitData = exit.Value;
                room.Exits[exit.Name] = new Exit(
                    exitData.GetProperty("targetId").GetString()!,
                    exitData.GetProperty("description").GetString(),
                    exitData.TryGetProperty("condition", out var condition) ? condition.GetString() : null
                );
            }
    }

    private static void ParseRoomItems(JsonElement roomData, Room room)
    {
        if (roomData.TryGetProperty("items", out var itemsElement))
            foreach (var itemData in itemsElement.EnumerateArray())
            {
                var item = new GameItem(
                    itemData.GetProperty("name").GetString()!
                )
                {
                    Description = itemData.GetProperty("description").GetString() ?? ""
                };

                if (itemData.TryGetProperty("interactions", out var interactionsElement))
                    foreach (var interaction in interactionsElement.EnumerateArray())
                        item.Interactions.Add(interaction.GetString()!);

                room.Items.Add(item);
            }
    }

    private static void ParseRoomNPCs(JsonElement roomData, Room room)
    {
        if (roomData.TryGetProperty("npcs", out var npcsElement))
            foreach (var npcData in npcsElement.EnumerateArray())
            {
                var npc = new NPC(
                    npcData.GetProperty("name").GetString()!
                )
                {
                    Description = npcData.GetProperty("description").GetString() ?? ""
                };

                if (npcData.TryGetProperty("dialogue", out var dialogueElement))
                    foreach (var dialog in dialogueElement.EnumerateObject())
                        npc.Dialogue[dialog.Name] = dialog.Value.GetString()!;

                room.NPCs.Add(npc);
            }
    }

    private static void LinkRoomExits(Dictionary<string, Room> rooms)
    {
        foreach (var room in rooms.Values)
        foreach (var exit in room.Exits.ToList())
        {
            // Ensure the target room exists
            if (!rooms.ContainsKey(exit.Value.TargetId))
            {
                room.Exits.Remove(exit.Key);
                continue;
            }

            // Add reverse exit if it doesn't exist
            var targetRoom = rooms[exit.Value.TargetId];
            var reverseDirection = GetReverseDirection(exit.Key);
            if (reverseDirection != null && !targetRoom.Exits.ContainsKey(reverseDirection))
                targetRoom.Exits[reverseDirection] = new Exit(
                    room.Id,
                    $"You return to {room.Name}."
                );
        }
    }

    private static string? GetReverseDirection(string direction)
    {
        return direction.ToLower() switch
        {
            "north" => "south",
            "south" => "north",
            "east" => "west",
            "west" => "east",
            "up" => "down",
            "down" => "up",
            _ => null
        };
    }
}

// Custom exceptions
public class GameGenerationException(string message, Exception? inner = null) : Exception(message, inner);

public class GameGenerationTimeoutException(string message, Exception? inner = null) : GameGenerationException(message,
    inner);