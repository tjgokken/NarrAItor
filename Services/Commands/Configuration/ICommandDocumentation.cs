using NarrAItor.Shared.Models;

namespace NarrAItor.Services.Commands.Configuration;

public interface ICommandDocumentation
{
    CommandDocumentation? GetDocumentation(string command);
    IEnumerable<string> GetAllCommands();
    IEnumerable<CommandSuggestion> GetSuggestions(GameState gameState);
    IEnumerable<CommandDocumentation> GetCommandsByCategory(CommandCategory category);
}