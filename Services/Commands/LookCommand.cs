using NarrAItor.Shared.Models;

namespace NarrAItor.Services.Commands;

public class LookCommand : ICommand
{
    private static readonly HashSet<string> LookCommands = new(StringComparer.OrdinalIgnoreCase)
    {
        "look", "examine", "inspect", "check", "read", "view", "search", "study"
    };

    private static readonly Dictionary<string, HashSet<string>> RoomElementAliases =
        new(StringComparer.OrdinalIgnoreCase)
        {
            {
                "room", new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                    { "room", "area", "place", "here", "around", "about", "surroundings" }
            },
            {
                "up", new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                    { "up", "ceiling", "above", "overhead" }
            },
            {
                "down", new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                    { "down", "floor", "ground", "below" }
            }
        };

    public bool CanHandle(CommandContext context)
    {
        return LookCommands.Contains(context.Command) ||
               context.FullArgument.Contains("what does it say", StringComparison.OrdinalIgnoreCase) ||
               context.FullArgument.StartsWith("tell me about", StringComparison.OrdinalIgnoreCase);
    }

    public Task ExecuteAsync(CommandContext context)
    {
        // Handle "what does it say" type questions
        if (context.FullArgument.Contains("what does it say", StringComparison.OrdinalIgnoreCase))
        {
            var item = FindMostRelevantItem(context);
            if (item != null)
            {
                context.AddToLog(item.Description);
                return Task.CompletedTask;
            }
        }

        if (context.Arguments.Length == 0 || IsLookingAround(context.FullArgument)) return LookAround(context);

        // Check for room elements (up, down, etc.)
        foreach (var element in RoomElementAliases)
            if (element.Value.Any(alias => context.FullArgument.Contains(alias, StringComparison.OrdinalIgnoreCase)))
                return DescribeRoomElement(context, element.Key);

        // Try to find and examine specific item
        var target = context.FullArgument;
        var foundItem = FindItem(context, target);
        if (foundItem != null)
        {
            context.AddToLog(string.IsNullOrEmpty(foundItem.Description)
                ? $"You see nothing special about the {foundItem.Name}."
                : foundItem.Description);
            return Task.CompletedTask;
        }

        // Check if target matches room description
        if (MatchesRoomDescription(context.CurrentRoom, target))
        {
            context.AddToLog(context.CurrentRoom.Description.Detailed ??
                             context.CurrentRoom.Description.Initial);
            return Task.CompletedTask;
        }

        context.AddToLog($"You don't see {target} here.");
        return Task.CompletedTask;
    }

    private Task LookAround(CommandContext context)
    {
        var room = context.CurrentRoom;
        context.AddToLog(room.Description.Detailed ?? room.Description.Initial);

        if (room.Atmosphere != null)
            if (!string.IsNullOrEmpty(room.Atmosphere.Sights))
                context.AddToLog(room.Atmosphere.Sights);

        // Always show exits
        context.AddToLog(room.Exits.Any()
            ? $"\nExits: {string.Join(", ", room.Exits.Keys)}"
            : "\nThere are no visible exits.");

        // Show items
        if (room.Items.Any()) context.AddToLog($"\nYou see: {string.Join(", ", room.Items.Select(i => i.Name))}");

        return Task.CompletedTask;
    }

    private GameItem? FindMostRelevantItem(CommandContext context)
    {
        // Check inventory first, as that's likely what they're referring to
        var inventoryItem = context.GameState.Inventory.LastOrDefault();
        if (inventoryItem != null) return inventoryItem;

        // Then check room items
        return context.CurrentRoom.Items.LastOrDefault();
    }

    private bool IsLookingAround(string input)
    {
        return RoomElementAliases["room"].Any(alias =>
            input.Contains(alias, StringComparison.OrdinalIgnoreCase));
    }

    private Task DescribeRoomElement(CommandContext context, string element)
    {
        var room = context.CurrentRoom;
        switch (element.ToLower())
        {
            case "up":
            case "ceiling":
                context.AddToLog(room.Atmosphere?.Sights != null
                    ? $"Looking up: {room.Atmosphere.Sights}"
                    : "You see nothing remarkable above.");
                break;

            case "down":
            case "floor":
                context.AddToLog(room.Items.Any()
                    ? $"On the floor you see: {string.Join(", ", room.Items.Select(i => i.Name))}"
                    : "The floor is clear.");
                break;

            default:
                return LookAround(context);
        }

        return Task.CompletedTask;
    }

    private bool MatchesRoomDescription(Room room, string target)
    {
        return room.Description.Initial.Contains(target, StringComparison.OrdinalIgnoreCase) ||
               (room.Description.Detailed?.Contains(target, StringComparison.OrdinalIgnoreCase) ?? false);
    }

    private GameItem? FindItem(CommandContext context, string itemName)
    {
        // Check inventory first
        var item = context.GameState.Inventory.FirstOrDefault(i =>
            ContainsItemName(i.Name, itemName) ||
            // Then check room items
            ContainsItemName(i.Description, itemName)) ?? context.CurrentRoom.Items.FirstOrDefault(i =>
            ContainsItemName(i.Name, itemName) ||
            ContainsItemName(i.Description, itemName));

        return item;
    }

    private bool ContainsItemName(string source, string target)
    {
        // Split into words and check if any word matches
        var sourceWords = source.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var targetWords = target.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return sourceWords.Any(word =>
            targetWords.Any(targetWord =>
                word.Contains(targetWord, StringComparison.OrdinalIgnoreCase)));
    }
}