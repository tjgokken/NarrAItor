using NarrAItor.Shared.Models;

namespace NarrAItor.Services.Commands;

/// <summary>
///     Handles using items from inventory or in the room
/// </summary>
public class UseCommand : ICommand
{
    private static readonly HashSet<string> UseCommands = new(StringComparer.OrdinalIgnoreCase)
    {
        "use", "activate", "operate", "apply", "enter", "type"
    };

    public bool CanHandle(CommandContext context)
    {
        return UseCommands.Contains(context.Command);
    }

    public Task ExecuteAsync(CommandContext context)
    {
        if (context.Arguments.Length == 0)
        {
            context.AddToLog("What do you want to use?");
            return Task.CompletedTask;
        }

        var itemName = context.FullArgument;
        var item = FindUsableItem(context, itemName);

        if (item == null)
        {
            context.AddToLog($"You don't have {itemName} to use.");
            return Task.CompletedTask;
        }

        // Check if this item can be used here
        var usableWith = item.Properties.GetValueOrDefault("usableWith", "");
        var currentRoom = context.CurrentRoom;

        if (!string.IsNullOrEmpty(usableWith))
        {
            // If this item is meant to unlock an exit
            foreach (var exit in currentRoom.Exits)
            {
                if (exit.Value.Condition != null && exit.Value.TargetId.Contains(usableWith))
                {
                    // Show the interaction text
                    foreach (var interaction in item.Interactions)
                    {
                        context.AddToLog(interaction);
                    }

                    // Clear the condition (unlock the exit)
                    exit.Value.Condition = null;

                    // Add a hint about the newly available exit
                    context.AddToLog($"\nThe way {exit.Key} is now accessible.");
                    return Task.CompletedTask;
                }
            }
        }

        if (!item.Interactions.Any())
        {
            context.AddToLog($"You can't figure out how to use the {item.Name} here.");
            return Task.CompletedTask;
        }

        foreach (var interaction in item.Interactions)
        {
            context.AddToLog(interaction);
        }

        // Handle item effects
        if (item.Properties.TryGetValue("effect", out var effect))
        {
            context.AddToLog($"\n{effect}");

            // Update game state if needed
            if (item.Properties.TryGetValue("consumed", out var consumed) &&
                bool.TryParse(consumed, out bool isConsumed) &&
                isConsumed)
            {
                context.GameState.Inventory.Remove(item);
                context.AddToLog($"The {item.Name} has been consumed.");
            }
        }

        return Task.CompletedTask;
    }

    private GameItem? FindUsableItem(CommandContext context, string itemName)
    {
        // Check inventory first
        var item = context.GameState.Inventory.FirstOrDefault(i =>
            // Then check room items
            i.Name.Contains(itemName, StringComparison.OrdinalIgnoreCase)) ?? context.CurrentRoom.Items.FirstOrDefault(i =>
            i.Name.Contains(itemName, StringComparison.OrdinalIgnoreCase));

        return item;
    }
}