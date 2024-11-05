namespace NarrAItor.Services.Commands;

public class TakeCommand : ICommand
{
    private static readonly HashSet<string> TakeCommands = new(StringComparer.OrdinalIgnoreCase)
    {
        "take", "get", "grab", "pick", "collect"
    };

    public bool CanHandle(CommandContext context)
    {
        return TakeCommands.Contains(context.Command);
    }

    public Task ExecuteAsync(CommandContext context)
    {
        if (context.Arguments.Length == 0)
        {
            context.AddToLog("What do you want to take?");
            return Task.CompletedTask;
        }

        var itemName = context.FullArgument;
        var item = context.CurrentRoom.Items.FirstOrDefault(i =>
            i.Name.Contains(itemName, StringComparison.OrdinalIgnoreCase));

        if (item == null)
        {
            context.AddToLog($"I don't see {itemName} here.");
            return Task.CompletedTask;
        }

        context.CurrentRoom.Items.Remove(item);
        context.GameState.Inventory.Add(item);
        context.AddToLog($"Taken: {item.Name}");

        return Task.CompletedTask;
    }
}