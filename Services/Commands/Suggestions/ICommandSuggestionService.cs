using NarrAItor.Services.Commands.Configuration;
using NarrAItor.Shared.Models;

namespace NarrAItor.Services.Commands.Suggestions;

public interface ICommandSuggestionService
{
    IEnumerable<CommandSuggestion> GetSuggestions(GameState gameState);
}