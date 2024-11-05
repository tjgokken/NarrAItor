namespace NarrAItor.Services.Commands.Models;

public record CommandProcessingResult(
    string Command,
    string[] Args
);