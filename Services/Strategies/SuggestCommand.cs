using NarrAItor.Services.Commands.Configuration;
using NarrAItor.Services.Commands;

namespace NarrAItor.Services.Strategies;

public class SuggestCommand : ICommand
{
    private static readonly HashSet<string> SuggestCommands = new(StringComparer.OrdinalIgnoreCase)
    {
        "suggest", "suggestions", "hint", "hints", "what now", "help me"
    };

    public bool CanHandle(CommandContext context) =>
        SuggestCommands.Contains(context.Command) ||
        context.FullArgument.Contains("what should i do", StringComparison.OrdinalIgnoreCase) ||
        context.FullArgument.Contains("what now", StringComparison.OrdinalIgnoreCase);

    public Task ExecuteAsync(CommandContext context)
    {
        var suggestions = GetContextualSuggestions(context);

        context.AddToLog("\nSuggestions based on your situation:");
        foreach (var suggestion in suggestions)
        {
            context.AddToLog($"- {suggestion.Command} ({suggestion.Reason})");
        }

        return Task.CompletedTask;
    }

    private List<CommandSuggestion> GetContextualSuggestions(CommandContext context)
    {
        var suggestions = new List<CommandSuggestion>();

        // For items in inventory
        if (context.GameState.Inventory.Any())
        {
            var item = context.GameState.Inventory.First();
            suggestions.Add(new CommandSuggestion(
                $"use {item.Name}",
                "Try using items you've collected",
                90));

            suggestions.Add(new CommandSuggestion(
                $"examine {item.Name}",
                "Look more closely at items you're carrying",
                85));
        }

        // For items in room
        if (context.CurrentRoom.Items.Any())
        {
            var item = context.CurrentRoom.Items.First();
            suggestions.Add(new CommandSuggestion(
                $"examine {item.Name}",
                "Inspect items you see",
                80));

            suggestions.Add(new CommandSuggestion(
                $"take {item.Name}",
                "Pick up useful items",
                75));
        }

        // For available exits
        if (context.CurrentRoom.Exits.Any())
        {
            var exit = context.CurrentRoom.Exits.First().Key;
            suggestions.Add(new CommandSuggestion(
                $"go {exit}",
                "Try exploring in this direction",
                70));
        }

        // General commands based on context
        if (!context.GameState.GameFlags.ContainsKey($"examined_{context.CurrentRoom.Id}"))
        {
            suggestions.Add(new CommandSuggestion(
                "look around",
                "Get a better view of your surroundings",
                95));
        }

        if (context.GameState.Inventory.Any())
        {
            suggestions.Add(new CommandSuggestion(
                "inventory",
                "Check what you're carrying",
                60));
        }

        return suggestions.OrderByDescending(s => s.Priority).Take(3).ToList();
    }
}