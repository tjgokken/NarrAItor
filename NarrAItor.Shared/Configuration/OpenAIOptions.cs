namespace NarrAItor.Shared.Configuration;

public class OpenAIOptions
{
    public required string ApiKey { get; set; }
    public string Model { get; set; } = "gpt-4-turbo-preview";
    public double Temperature { get; set; } = 0.7;
    public int MaxTokens { get; set; } = 1500;
    public string ResponseFormat { get; set; } = "json_object";
}