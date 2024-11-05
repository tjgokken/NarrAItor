namespace NarrAItor.Services.Commands;

/// <summary>
///     Handles dropping items from player's inventory into the current room
/// </summary>
public class DropCommand : ICommand
{
    private static readonly HashSet<string> DropCommands = new(StringComparer.OrdinalIgnoreCase)
    {
        "drop", "put", "place", "leave", "discard"
    };

    public bool CanHandle(CommandContext context)
    {
        return DropCommands.Contains(context.Command);
    }

    public Task ExecuteAsync(CommandContext context)
    {
        if (context.Arguments.Length == 0)
        {
            context.AddToLog("What do you want to drop?");
            return Task.CompletedTask;
        }

        if (!context.GameState.Inventory.Any())
        {
            context.AddToLog("You're not carrying anything.");
            return Task.CompletedTask;
        }

        var itemName = context.FullArgument;
        var item = context.GameState.Inventory.FirstOrDefault(i =>
            i.Name.Contains(itemName, StringComparison.OrdinalIgnoreCase));

        if (item == null)
        {
            context.AddToLog($"You're not carrying {itemName}.");
            return Task.CompletedTask;
        }

        context.GameState.Inventory.Remove(item);
        context.CurrentRoom.Items.Add(item);
        context.AddToLog($"Dropped: {item.Name}");

        return Task.CompletedTask;
    }
}