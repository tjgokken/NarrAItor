namespace NarrAItor.Services.Commands;

public interface ICommand
{
    bool CanHandle(CommandContext context);
    Task ExecuteAsync(CommandContext context);
}