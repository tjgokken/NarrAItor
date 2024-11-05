namespace NarrAItor.Services.Commands.Configuration;

public record CommandSuggestion(
    string Command,
    string Reason,
    int Priority
);