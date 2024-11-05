using System.Text.Json;
using System.Text.Json.Serialization;

namespace NarrAItor.Shared.Models;

public class GameItemConverter : JsonConverter<GameItem>
{
    public override GameItem Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException("Expected StartObject token");

        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;

        var name = root.GetProperty("name").GetString() ?? "Unknown Item";
        var id = root.TryGetProperty("id", out var idElement) ? idElement.GetString() : null;

        var item = new GameItem(name, id);

        if (root.TryGetProperty("description", out var descElement))
            item.Description = descElement.GetString() ?? string.Empty;

        if (root.TryGetProperty("interactions", out var interactionsElement))
            foreach (var interaction in interactionsElement.EnumerateArray())
            {
                var interactionStr = interaction.GetString();
                if (interactionStr != null) item.Interactions.Add(interactionStr);
            }

        if (root.TryGetProperty("properties", out var propsElement))
            foreach (var prop in propsElement.EnumerateObject())
                item.Properties[prop.Name] = prop.Value.GetString() ?? string.Empty;

        return item;
    }

    public override void Write(Utf8JsonWriter writer, GameItem value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString("id", value.Id);
        writer.WriteString("name", value.Name);
        writer.WriteString("description", value.Description);

        writer.WriteStartArray("interactions");
        foreach (var interaction in value.Interactions) writer.WriteStringValue(interaction);
        writer.WriteEndArray();

        writer.WriteStartObject("properties");
        foreach (var prop in value.Properties) writer.WriteString(prop.Key, prop.Value);
        writer.WriteEndObject();

        writer.WriteEndObject();
    }
}