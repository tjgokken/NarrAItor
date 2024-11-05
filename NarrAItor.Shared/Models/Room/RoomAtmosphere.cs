namespace NarrAItor.Shared.Models;

public class RoomAtmosphere(string? sights = null, string? sounds = null, string? smells = null)
{
    public string? Sights { get; set; } = sights;
    public string? Sounds { get; set; } = sounds;
    public string? Smells { get; set; } = smells;
}