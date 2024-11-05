using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using NarrAItor.Shared.Configuration;

namespace NarrAItor.Api.Services;

public class OpenAIService
{
    private const string API_URL = "https://api.openai.com/v1/chat/completions";
    private const int DEFAULT_TIMEOUT_SECONDS = 45;

    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private readonly OpenAIOptions _options;

    public OpenAIService(IConfiguration configuration, ILogger<OpenAIService> logger)
    {
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
                     ?? configuration["OpenAI:ApiKey"]
                     ?? throw new InvalidOperationException("OpenAI API key not found");

        _httpClient = InitializeHttpClient(apiKey);
        _logger = logger;

        _options = new OpenAIOptions
        {
            ApiKey = apiKey,
            Model = "gpt-4o-mini",
            Temperature = 0.7,
            MaxTokens = 1500,
            ResponseFormat = "json_object"
        };
    }

    private static HttpClient InitializeHttpClient(string apiKey)
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);
        client.Timeout = TimeSpan.FromSeconds(DEFAULT_TIMEOUT_SECONDS);
        return client;
    }

    public async Task<string> GenerateCompletion(string prompt, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(prompt, nameof(prompt));

        try
        {
            var request = new CompletionRequest
            {
                Model = _options.Model,
                Messages = new List<Message>
                {
                    new()
                    {
                        Role = "system",
                        Content =
                            "You are NarrAItor, an expert in creating focused text adventures. " +
                            "Generate only valid JSON, optimized for quick response. Prioritize concise, impactful descriptions."
                    },
                    new()
                    {
                        Role = "user",
                        Content = prompt
                    }
                },
                Temperature = _options.Temperature,
                MaxTokens = _options.MaxTokens,
                ResponseFormat = new ResponseFormat { Type = _options.ResponseFormat }
            };

            var response = await _httpClient.PostAsJsonAsync(API_URL, request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("OpenAI API error: {StatusCode} - {Error}",
                    response.StatusCode, errorContent);
                throw new OpenAIException($"API request failed with status {response.StatusCode}");
            }

            var result = await response.Content.ReadFromJsonAsync<CompletionResponse>(
                cancellationToken);

            if (result?.Choices == null || result.Choices.Count == 0)
                throw new OpenAIException("No completion choices returned from API");

            var completionText = result.Choices[0].Message?.Content;
            if (string.IsNullOrEmpty(completionText))
                throw new OpenAIException("Empty completion received from API");

            return completionText;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request to OpenAI failed");
            throw new OpenAIException("Failed to connect to OpenAI API", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse OpenAI response");
            throw new OpenAIException("Invalid response format from OpenAI", ex);
        }
        catch (Exception ex) when (ex is not OpenAIException)
        {
            _logger.LogError(ex, "Unexpected error during OpenAI request");
            throw new OpenAIException("Unexpected error during API request", ex);
        }
    }
}

public class CompletionRequest
{
    [JsonPropertyName("model")] public string Model { get; set; } = string.Empty;

    [JsonPropertyName("messages")] public List<Message> Messages { get; set; } = new();

    [JsonPropertyName("temperature")] public double Temperature { get; set; }

    [JsonPropertyName("max_tokens")] public int MaxTokens { get; set; }

    [JsonPropertyName("response_format")] public ResponseFormat? ResponseFormat { get; set; }
}

public class Message
{
    [JsonPropertyName("role")] public string Role { get; set; } = string.Empty;

    [JsonPropertyName("content")] public string Content { get; set; } = string.Empty;
}

public class ResponseFormat
{
    [JsonPropertyName("type")] public string Type { get; set; } = string.Empty;
}

public class CompletionResponse
{
    [JsonPropertyName("choices")] public List<Choice> Choices { get; set; } = new();
}

public class Choice
{
    [JsonPropertyName("message")] public Message? Message { get; set; }
}

public class OpenAIException : Exception
{
    public OpenAIException(string message) : base(message)
    {
    }

    public OpenAIException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

public class OpenAITimeoutException(string message) : OpenAIException(message);