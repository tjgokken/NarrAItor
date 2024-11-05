using NarrAItor.Shared.Models;

namespace NarrAItor.Services.Commands.Configuration;

public class CommandDocumentationService(Dictionary<string, CommandDocumentation> documentation) : ICommandDocumentation
{
    public CommandDocumentation? GetDocumentation(string command)
    {
        return documentation.TryGetValue(command.ToLower(), out var doc) ? doc : null;
    }

    public IEnumerable<string> GetAllCommands()
    {
        return documentation.Keys;
    }

    public IEnumerable<CommandSuggestion> GetSuggestions(GameState gameState)
    {
        var suggestions = new List<CommandSuggestion>();

        if (gameState.CurrentRoomId == null || !gameState.Rooms.ContainsKey(gameState.CurrentRoomId))
            return suggestions; // Return an empty list if gameState or current room is null or invalid
        var currentRoom = gameState.Rooms[gameState.CurrentRoomId];

        // Suggest looking around in a new room
        if (!gameState.GameFlags.ContainsKey($"visited_{currentRoom.Id}"))
            suggestions.Add(new CommandSuggestion(
                "look around",
                "Get a better view of your surroundings",
                100));

        // Suggest taking items
        if (currentRoom.Items.Any())
            suggestions.Add(new CommandSuggestion(
                $"take {currentRoom.Items.First().Name}",
                "Pick up items you find",
                90));

        // Suggest talking to NPCs
        if (currentRoom.NPCs.Any())
            suggestions.Add(new CommandSuggestion(
                $"talk to {currentRoom.NPCs.First().Name}",
                "Talk to characters you meet",
                85));

        // Suggest using items in inventory
        if (gameState.Inventory.Any())
            suggestions.Add(new CommandSuggestion(
                $"use {gameState.Inventory.First().Name}",
                "Try using items you've collected",
                80));

        return suggestions;
    }

    public IEnumerable<CommandDocumentation> GetCommandsByCategory(CommandCategory category)
    {
        return documentation.Values.Where(d => d.Category == category);
    }
}