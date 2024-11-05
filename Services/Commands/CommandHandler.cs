using NarrAItor.Shared.Models;

namespace NarrAItor.Services.Commands;

public class CommandHandler(ILogger<CommandHandler> logger,
    IEnumerable<ICommand> commandStrategies)
{
    private readonly NaturalLanguageHandler _naturalLanguageHandler = new();

    public async Task HandleCommand(string input, GameState gameState)
    {
        try
        {
            var result = _naturalLanguageHandler.ProcessInput(input, gameState);
            var context = new CommandContext(gameState, result.Command, result.Args);

            var strategyFound = false;
            foreach (var strategy in commandStrategies)
            {
                if (strategy.CanHandle(context))
                {
                    await strategy.ExecuteAsync(context);
                    gameState.UpdateLastAction();
                    strategyFound = true;
                    break;
                }
            }

            if (!strategyFound)
            {
                var suggestion = _naturalLanguageHandler.GetSuggestion(input, gameState);
                gameState.AddToLog($"I don't understand that command. {suggestion}");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling command: {Command}", input);
            gameState.AddToLog("Something went wrong processing that command.");
        }
    }
}