using NarrAItor.Services.Commands.Configuration;

namespace NarrAItor.Services.Commands;

public class HelpCommand(ICommandDocumentation documentation) : ICommand
{
    private static readonly HashSet<string> HelpCommands = new(StringComparer.OrdinalIgnoreCase)
    {
        "help", "?", "commands", "hint"
    };

    public bool CanHandle(CommandContext context)
    {
        return HelpCommands.Contains(context.Command);
    }

    public Task ExecuteAsync(CommandContext context)
    {
        // Handle different help requests
        if (context.Command.Equals("hint", StringComparison.OrdinalIgnoreCase))
        {
            ShowSuggestions(context);
            return Task.CompletedTask;
        }

        if (context.Arguments.Length > 0)
        {
            if (context.Arguments[0].Equals("categories", StringComparison.OrdinalIgnoreCase))
                ShowCategories(context);
            else
                ShowSpecificHelp(context, context.Arguments[0]);
        }
        else
        {
            ShowGeneralHelp(context);
        }

        return Task.CompletedTask;
    }

    private void ShowSuggestions(CommandContext context)
    {
        var suggestions = documentation.GetSuggestions(context.GameState)
            .OrderByDescending(s => s.Priority)
            .Take(3);

        context.AddToLog("Suggested actions:");
        foreach (var suggestion in suggestions) context.AddToLog($"- {suggestion.Command} ({suggestion.Reason})");
    }

    private void ShowCategories(CommandContext context)
    {
        context.AddToLog("Command Categories:");
        foreach (CommandCategory category in Enum.GetValues(typeof(CommandCategory)))
        {
            var commands = documentation.GetCommandsByCategory(category).ToList();
            if (commands.Any())
            {
                context.AddToLog($"\n{category}:");
                foreach (var command in commands) context.AddToLog($"  - {command.Description}");
            }
        }
    }

    private void ShowGeneralHelp(CommandContext context)
    {
        context.AddToLog("Available commands (type 'help [command]' for more details):");
        foreach (var command in documentation.GetAllCommands())
        {
            var doc = documentation.GetDocumentation(command);
            if (doc != null) context.AddToLog($"- {command} - {doc.Description}");
        }

        // Show current room exits if any
        var currentRoom = context.CurrentRoom;
        if (currentRoom.Exits.Any()) context.AddToLog($"\nCurrent exits: {string.Join(", ", currentRoom.Exits.Keys)}");
    }

    private void ShowSpecificHelp(CommandContext context, string command)
    {
        var doc = documentation.GetDocumentation(command);
        if (doc == null)
        {
            context.AddToLog($"No help available for '{command}'.");
            return;
        }

        context.AddToLog($"{command.ToUpper()}:");
        context.AddToLog(doc.Description);
        context.AddToLog("\nUsage:");
        foreach (var example in doc.Examples) context.AddToLog($"  {example}");
        context.AddToLog("\nAliases:");
        context.AddToLog($"  {string.Join(", ", doc.Aliases)}");
    }
}