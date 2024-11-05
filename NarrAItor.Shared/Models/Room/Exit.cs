namespace NarrAItor.Shared.Models;

public class Exit(string targetId, string? description = null, string? condition = null)
{
    public string TargetId { get; set; } = targetId;
    public string Description { get; set; } = description ?? "You move to another location.";
    public string? Condition { get; set; } = condition;
}