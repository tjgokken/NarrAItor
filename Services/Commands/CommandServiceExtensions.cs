using NarrAItor.Services.Commands.Configuration;

namespace NarrAItor.Services.Commands;

public static class CommandServiceExtensions
{
    public static IServiceCollection AddGameCommandDocumentation(this IServiceCollection services)
    {
        var commandDocs = new Dictionary<string, CommandDocumentation>
        {
            {
                "look", new CommandDocumentation(
                    "Look at your surroundings or examine specific items",
                    new[]
                    {
                        "look around - Get detailed room description",
                        "look [item] - Examine specific item",
                        "examine [item] - Same as look",
                        "look at [item] - Detailed examination"
                    },
                    new[] { "look", "examine", "inspect", "check", "read" },
                    CommandCategory.Information,
                    new[] { "items" }
                )
            },
            // Add more comprehensive documentation for other commands...
        };

        services.AddSingleton<ICommandDocumentation>(new CommandDocumentationService(commandDocs));
        return services;
    }
}