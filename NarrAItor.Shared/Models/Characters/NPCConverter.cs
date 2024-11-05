using System.Text.Json;
using System.Text.Json.Serialization;

namespace NarrAItor.Shared.Models;

public class NPCConverter : JsonConverter<NPC>
{
    public override NPC Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException("Expected StartObject token");

        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;

        var name = root.GetProperty("name").GetString() ?? "Unknown NPC";
        var id = root.TryGetProperty("id", out var idElement) ? idElement.GetString() : null;

        var npc = new NPC(name, id);

        if (root.TryGetProperty("description", out var descElement))
            npc.Description = descElement.GetString() ?? string.Empty;

        if (root.TryGetProperty("dialogue", out var dialogueElement))
            foreach (var dialogue in dialogueElement.EnumerateObject())
                npc.Dialogue[dialogue.Name] = dialogue.Value.GetString() ?? string.Empty;

        if (root.TryGetProperty("currentDialog", out var currentDialogElement))
            npc.CurrentDialog = currentDialogElement.GetString();

        if (root.TryGetProperty("inventory", out var inventoryElement))
            foreach (var itemElement in inventoryElement.EnumerateArray())
            {
                var item = JsonSerializer.Deserialize<GameItem>(itemElement.GetRawText(), options);
                if (item != null) npc.Inventory.Add(item);
            }

        return npc;
    }

    public override void Write(Utf8JsonWriter writer, NPC value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString("id", value.Id);
        writer.WriteString("name", value.Name);
        writer.WriteString("description", value.Description);

        writer.WriteStartObject("dialogue");
        foreach (var dialogue in value.Dialogue) writer.WriteString(dialogue.Key, dialogue.Value);
        writer.WriteEndObject();

        if (value.CurrentDialog != null) writer.WriteString("currentDialog", value.CurrentDialog);

        writer.WriteStartArray("inventory");
        foreach (var item in value.Inventory) JsonSerializer.Serialize(writer, item, options);
        writer.WriteEndArray();

        writer.WriteEndObject();
    }
}