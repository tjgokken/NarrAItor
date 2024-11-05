namespace NarrAItor.Services.Commands.Configuration;

public enum CommandCategory
{
    Information
}

public record CommandDocumentation(
    string Description,
    string[] Examples,
    string[] Aliases,
    CommandCategory Category,
    string[] Context = null!
);
