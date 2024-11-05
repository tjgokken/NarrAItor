using NarrAItor.Shared.Models;

namespace NarrAItor.Services.Commands;

/// <summary>
///     Handles conversations with NPCs including dialogue options
/// </summary>
public class TalkCommand : ICommand
{
    private static readonly HashSet<string> TalkCommands = new(StringComparer.OrdinalIgnoreCase)
    {
        "talk", "speak", "ask", "tell", "chat", "converse"
    };

    public bool CanHandle(CommandContext context)
    {
        return TalkCommands.Contains(context.Command);
    }

    public Task ExecuteAsync(CommandContext context)
    {
        if (context.Arguments.Length == 0)
        {
            context.AddToLog("Who do you want to talk to?");
            return Task.CompletedTask;
        }

        if (!context.CurrentRoom.NPCs.Any())
        {
            context.AddToLog("There's no one here to talk to.");
            return Task.CompletedTask;
        }

        var targetName = context.Arguments.Length > 1 && context.Arguments[0] == "to"
            ? string.Join(" ", context.Arguments.Skip(1))
            : context.FullArgument;

        var npc = context.CurrentRoom.NPCs.FirstOrDefault(n =>
            n.Name.Contains(targetName, StringComparison.OrdinalIgnoreCase));

        if (npc == null)
        {
            context.AddToLog($"You don't see {targetName} here.");
            return Task.CompletedTask;
        }

        HandleDialogue(context, npc);
        return Task.CompletedTask;
    }

    private void HandleDialogue(CommandContext context, NPC npc)
    {
        if (!npc.Dialogue.Any())
        {
            context.AddToLog($"{npc.Name} has nothing to say right now.");
            return;
        }

        if (npc.Dialogue.TryGetValue("greeting", out var greeting))
        {
            context.AddToLog($"{npc.Name}: {greeting}");
            var topics = npc.Dialogue.Keys.Where(k => k != "greeting").ToList();

            if (topics.Any())
            {
                context.AddToLog("You can ask about: " + string.Join(", ", topics));
            }
        }
        else
        {
            context.AddToLog($"{npc.Name}: {npc.Dialogue.First().Value}");
        }
    }
}