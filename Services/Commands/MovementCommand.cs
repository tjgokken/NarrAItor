using NarrAItor.Shared.Models;

namespace NarrAItor.Services.Commands;

public class MovementCommand : ICommand
{
    private static readonly HashSet<string> MoveCommands = new(StringComparer.OrdinalIgnoreCase)
    {
        "go", "move", "walk", "run", "head"
    };

    private static readonly Dictionary<string, string> DirectionAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        { "n", "north" }, { "s", "south" }, { "e", "east" }, { "w", "west" },
        { "u", "up" }, { "d", "down" },
        { "forward", "north" }, { "backward", "south" }, { "backwards", "south" },
        { "right", "east" }, { "left", "west" }
    };

    public bool CanHandle(CommandContext context)
    {
        return MoveCommands.Contains(context.Command) || DirectionAliases.ContainsKey(context.Command);
    }

    public Task ExecuteAsync(CommandContext context)
    {
        var direction = context.Arguments.Length > 0 ? context.Arguments[0] : context.Command;

        if (DirectionAliases.TryGetValue(direction, out var normalizedDirection))
        {
            direction = normalizedDirection;
        }

        if (context.CurrentRoom.Exits.TryGetValue(direction, out var exit))
        {
            if (exit.Condition != "none" && !string.IsNullOrEmpty(exit.Condition) )
            {
                context.AddToLog($"You can't go that way: {exit.Condition}");
                return Task.CompletedTask;
            }

            context.GameState.CurrentRoomId = exit.TargetId;

            if (!string.IsNullOrEmpty(exit.Description))
            {
                context.AddToLog(exit.Description);
            }

            var newRoom = context.GameState.Rooms[exit.TargetId];
            context.AddToLog(newRoom.Description.Initial);
            DescribeRoomDetails(context, newRoom);
        }
        else
        {
            var availableExits = string.Join(", ", context.CurrentRoom.Exits.Keys);
            context.AddToLog(availableExits.Length > 0
                ? $"You can't go {direction} from here. Available exits are: {availableExits}"
                : "There are no visible exits.");
        }

        return Task.CompletedTask;
    }

    private void DescribeRoomDetails(CommandContext context, Room room)
    {
        // Describe atmosphere if present
        if (room.Atmosphere != null)
        {
            if (!string.IsNullOrEmpty(room.Atmosphere.Sights))
                context.AddToLog(room.Atmosphere.Sights);
            if (!string.IsNullOrEmpty(room.Atmosphere.Sounds))
                context.AddToLog(room.Atmosphere.Sounds);
            if (!string.IsNullOrEmpty(room.Atmosphere.Smells))
                context.AddToLog(room.Atmosphere.Smells);
        }

        // List exits
        context.AddToLog(room.Exits.Any()
            ? $"Exits: {string.Join(", ", room.Exits.Keys)}"
            : "There are no visible exits.");

        // List items
        if (room.Items.Any())
        {
            context.AddToLog($"You see: {string.Join(", ", room.Items.Select(i => i.Name))}");
        }

        // List NPCs
        if (room.NPCs.Any())
        {
            context.AddToLog($"Present here: {string.Join(", ", room.NPCs.Select(n => n.Name))}");
        }

        // Update room visited flag
        context.GameState.GameFlags[$"visited_{room.Id}"] = true;
    }
}