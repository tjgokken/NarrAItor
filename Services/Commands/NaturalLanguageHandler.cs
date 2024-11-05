using NarrAItor.Services.Commands.Models;
using NarrAItor.Shared.Models;

namespace NarrAItor.Services.Commands;

public class NaturalLanguageHandler
{
    private readonly Dictionary<string, string> _naturalPhrases = new(StringComparer.OrdinalIgnoreCase)
    {
        // Examining/Looking/Reading
        { "what does it say", "read" },
        { "what's written", "read" },
        { "what do we have", "examine" },
        { "what's here", "look" },
        { "what is here", "look" },
        { "what can i see", "look" },
        { "tell me about", "examine" },
        { "describe", "examine" },
        { "check out", "examine" },
        { "look at", "examine" },
        { "look in", "examine" },
        { "look inside", "examine" },
        { "peek inside", "examine" },
        { "peek in", "examine" },
        { "inspect", "examine" },
        { "study", "examine" },
        { "view", "examine" },
        { "observe", "examine" },

        // Taking/Picking Up
        { "pick up", "take" },
        { "pickup", "take" },
        { "grab", "take" },
        { "collect", "take" },
        { "get", "take" },

        // Using/Interacting
        { "open", "use" },
        { "activate", "use" },
        { "power up", "use" },
        { "power on", "use" },
        { "switch on", "use" },
        { "turn on", "use" },
        { "access", "use" },
        { "unlock", "use" },
        { "press", "use" },
        { "push", "use" },
        { "pull", "use" },
        { "interact with", "use" },

        // Movement
        { "go to", "go" },
        { "move to", "go" },
        { "walk to", "go" },
        { "run to", "go" },
        { "head to", "go" },
        { "travel to", "go" },
        { "enter", "go" },

        // Inventory
        { "what am i carrying", "inventory" },
        { "what do i have", "inventory" },
        { "check inventory", "inventory" },
        { "check items", "inventory" },
        { "show inventory", "inventory" },
        { "whats in my inventory", "inventory" },
        { "what's in my inventory", "inventory" },

        // Help
        { "help me", "help" },
        { "what can i do", "help" },
        { "what are my options", "help" },
        { "show commands", "help" },
        { "im stuck", "help" },
        { "i'm stuck", "help" },

        // Special combinations
        { "show me around", "look around" },
        { "where am i", "look around" },
        { "describe surroundings", "look around" },
        { "describe area", "look around" },
        { "check surroundings", "look around" }
    };

    private GameItem? _lastMentionedItem;

    public CommandProcessingResult ProcessInput(string input, GameState gameState)
    {
        // Handle pronouns
        input = ReplacePronounsWithContext(input);

        // Try to match natural phrases
        var normalizedInput = NormalizeInput(input);

        // Split into command and args
        var parts = normalizedInput.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var command = parts[0];
        var args = parts.Skip(1).ToArray();

        // Update context
        UpdateContext(args, gameState);

        return new CommandProcessingResult(command, args);
    }

    private string NormalizeInput(string input)
    {
        foreach (var phrase in _naturalPhrases)
            if (input.Contains(phrase.Key, StringComparison.OrdinalIgnoreCase))
                return input.Replace(phrase.Key, phrase.Value, StringComparison.OrdinalIgnoreCase);

        return input;
    }

    private string ReplacePronounsWithContext(string input)
    {
        if (_lastMentionedItem == null) return input;

        var pronouns = new[] { " it", " that", " this" };
        foreach (var pronoun in pronouns)
            if (input.Contains(pronoun, StringComparison.OrdinalIgnoreCase))
                return input.Replace(pronoun, $" {_lastMentionedItem.Name}", StringComparison.OrdinalIgnoreCase);

        return input;
    }

    private void UpdateContext(string[] args, GameState gameState)
    {
        if (args.Length == 0 || gameState.CurrentRoomId == null)
            return;

        var itemName = string.Join(" ", args);

        // Check inventory
        _lastMentionedItem = gameState.Inventory
            .FirstOrDefault(i => i.Name.Contains(itemName, StringComparison.OrdinalIgnoreCase));

        // Check current room if not found in inventory
        if (_lastMentionedItem == null && gameState.Rooms.TryGetValue(gameState.CurrentRoomId, out var currentRoom))
            _lastMentionedItem = currentRoom.Items
                .FirstOrDefault(i => i.Name.Contains(itemName, StringComparison.OrdinalIgnoreCase));
    }

    public string GetSuggestion(string input, GameState gameState)
    {
        if (gameState.CurrentRoomId == null ||
            !gameState.Rooms.TryGetValue(gameState.CurrentRoomId, out var currentRoom))
            return "Try 'help' to see available commands.";

        var containsItem = currentRoom.Items.Any(i =>
            input.Contains(i.Name, StringComparison.OrdinalIgnoreCase));

        if (containsItem) return "Try 'examine [item]' or 'take [item]' to interact with objects.";

        // Check if trying to move
        var movementWords = new[] { "go", "move", "walk", "run" };
        if (movementWords.Any(w => input.Contains(w, StringComparison.OrdinalIgnoreCase)))
        {
            var exits = string.Join(", ", currentRoom.Exits.Keys);
            return currentRoom.Exits.Any()
                ? $"To move, try 'go [direction]'. Available exits: {exits}"
                : "There are no visible exits in this room.";
        }

        return "Try 'help' to see available commands or 'look around' to examine your surroundings.";
    }
}