namespace NarrAItor.Services.Commands;

public class InventoryCommand : ICommand
{
    private static readonly HashSet<string> InventoryCommands = new(StringComparer.OrdinalIgnoreCase)
    {
        "inventory", "i", "inv", "items", "carrying"
    };

    public bool CanHandle(CommandContext context)
    {
        return InventoryCommands.Contains(context.Command);
    }

    public Task ExecuteAsync(CommandContext context)
    {
        if (!context.GameState.Inventory.Any())
        {
            context.AddToLog("Your inventory is empty.");
            return Task.CompletedTask;
        }

        context.AddToLog("You are carrying:");
        foreach (var item in context.GameState.Inventory)
        {
            context.AddToLog($"- {item.Name}");
        }

        return Task.CompletedTask;
    }
}