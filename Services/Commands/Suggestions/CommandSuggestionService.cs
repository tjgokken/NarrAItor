using NarrAItor.Services.Commands.Configuration;
using NarrAItor.Shared.Models;

namespace NarrAItor.Services.Commands.Suggestions;

public class CommandSuggestionService : ICommandSuggestionService
{
    private readonly List<(Func<GameState, bool> Trigger, CommandSuggestion Suggestion)> _triggers = new();

    public CommandSuggestionService()
    {
        // Register default triggers
        RegisterCommonTriggers();
    }

    public void RegisterSuggestionTrigger(Func<GameState, bool> trigger, CommandSuggestion suggestion)
    {
        _triggers.Add((trigger, suggestion));
    }

    public IEnumerable<CommandSuggestion> GetSuggestions(GameState gameState)
    {
        return _triggers
            .Where(t => t.Trigger(gameState))
            .Select(t => t.Suggestion)
            .OrderByDescending(s => s.Priority)
            .Take(3);
    }

    private void RegisterCommonTriggers()
    {
        // New room trigger
        RegisterSuggestionTrigger(
            state => state.CurrentRoomId != null && !state.GameFlags.ContainsKey($"visited_{state.CurrentRoomId}"),
            new CommandSuggestion("look around", "Explore your new surroundings", 100)
        );

        // Inventory full trigger
        RegisterSuggestionTrigger(
            state => state.Inventory.Count >= 5,
            new CommandSuggestion("inventory", "Check what you're carrying", 80)
        );

        // NPC present trigger
        RegisterSuggestionTrigger(
            state => state.CurrentRoomId != null && state.Rooms[state.CurrentRoomId].NPCs.Count != 0,
            new CommandSuggestion("talk", "You might learn something useful", 90)
        );

        // No progress trigger
        RegisterSuggestionTrigger(
            state => (DateTime.UtcNow - state.LastActionTime).TotalMinutes > 5,
            new CommandSuggestion("help", "Try checking available commands", 70)
        );

        // Items present trigger
        RegisterSuggestionTrigger(
            state => state.CurrentRoomId != null && state.Rooms[state.CurrentRoomId].Items.Count != 0,
            new CommandSuggestion("examine", "There might be something interesting to look at", 85)
        );

        // Has items but hasn't taken any
        RegisterSuggestionTrigger(
            state => state.CurrentRoomId != null && state.Rooms[state.CurrentRoomId].Items.Count != 0 && !state.Inventory.Any(),
            new CommandSuggestion("take", "You can pick up useful items", 95)
        );

        // Has items in inventory that haven't been used
        RegisterSuggestionTrigger(
            state => state.Inventory.Any(i => !state.GameFlags.ContainsKey($"used_{i.Id}")),
            new CommandSuggestion("use", "Try using items you've collected", 75)
        );

        RegisterSuggestionTrigger(
        state =>
        {
            if (state.CurrentRoomId == null) return false;
            var room = state.Rooms[state.CurrentRoomId];
            return room.Exits.Any(e => !string.IsNullOrEmpty(e.Value.Condition)) &&
                   state.Inventory.Any(i => i.Properties.ContainsKey("usableWith"));
        },
        new CommandSuggestion("use", "You might be able to unlock a path forward", 95)
    );

        // After using item trigger
        RegisterSuggestionTrigger(
            state =>
            {
                if (state.CurrentRoomId == null) return false;
                var room = state.Rooms[state.CurrentRoomId];
                return room.Exits.Any(e => string.IsNullOrEmpty(e.Value.Condition)) &&
                       state.GameFlags.Any(f => f.Key.StartsWith("used_"));
            },
            new CommandSuggestion("look around", "The room might have changed", 90)
        );

        // Has usable item for current room
        RegisterSuggestionTrigger(
            state =>
            {
                if (state.CurrentRoomId == null) return false;
                return state.Inventory.Any(item =>
                    item.Properties.TryGetValue("usableWith", out var target) &&
                    (state.Rooms[state.CurrentRoomId].Exits.Any(e => e.Value.TargetId.Contains(target)) ||
                     state.Rooms[state.CurrentRoomId].Description.Initial.Contains(target, StringComparison.OrdinalIgnoreCase)));
            },
            new CommandSuggestion("use", "One of your items might be useful here", 98)
        );

        // Recently used item trigger
        RegisterSuggestionTrigger(
            state =>
            {
                if (state.CurrentRoomId == null) return false;
                var room = state.Rooms[state.CurrentRoomId];
                var recentlyUsed = state.GameFlags
                    .Where(f => f.Key.StartsWith("used_") && f.Value)
                    .OrderByDescending(_ => state.LastActionTime)
                    .FirstOrDefault();

                return recentlyUsed.Key != null &&
                       room.Exits.Any(e => string.IsNullOrEmpty(e.Value.Condition));
            },
            new CommandSuggestion("go", "Try moving to a new area", 85)
        );

        // Modified inventory check trigger
        RegisterSuggestionTrigger(
            state => state.Inventory.Any(i =>
                i.Properties.ContainsKey("usableWith") &&
                !state.GameFlags.ContainsKey($"used_{i.Id}")),
            new CommandSuggestion("examine", "Take a closer look at your items", 88)
        );
    }
}