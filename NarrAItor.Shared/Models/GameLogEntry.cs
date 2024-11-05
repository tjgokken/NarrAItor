namespace NarrAItor.Shared.Models;

public class GameLogEntry(string content, MessageType type = MessageType.System)
{
    public string Content { get; set; } = content;
    public MessageType Type { get; set; } = type;
}

public enum MessageType
{
    System,
    Input,
    Error,
    Success
}