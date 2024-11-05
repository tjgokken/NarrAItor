using System.Text.Json;
using System.Text.Json.Serialization;

namespace NarrAItor.Shared.Models;

public class RoomConverter : JsonConverter<Room>
{
    public override Room Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected StartObject token");

        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;

        // Use TryGetProperty for required fields with default values if missing
        var name = root.TryGetProperty("name", out var nameElement)
            ? nameElement.GetString() ?? "Unknown Room"
            : "Unknown Room";

        // Handle potentially missing or malformed description property
        string description;
        if (root.TryGetProperty("description", out var descElement))
        {
            if (descElement.ValueKind == JsonValueKind.Object &&
                descElement.TryGetProperty("initial", out var initialElement))
                description = initialElement.GetString() ?? "An empty room.";
            else
                description = descElement.GetString() ?? "An empty room.";
        }
        else
        {
            description = "An empty room.";
        }

        var room = new Room(name, description);

        // Handle optional ID
        if (root.TryGetProperty("id", out var idElement)) room.Id = idElement.GetString() ?? room.Id;

        // Handle optional description properties
        if (root.TryGetProperty("description", out descElement) &&
            descElement.ValueKind == JsonValueKind.Object)
        {
            if (descElement.TryGetProperty("subsequent", out var subElement))
                room.Description.Subsequent = subElement.GetString();
            if (descElement.TryGetProperty("detailed", out var detElement))
                room.Description.Detailed = detElement.GetString();
        }

        // Handle optional exits
        if (root.TryGetProperty("exits", out var exitsElement))
            foreach (var exit in exitsElement.EnumerateObject())
                if (exit.Value.TryGetProperty("targetId", out var targetIdElement))
                {
                    var targetId = targetIdElement.GetString()!;
                    var exitDesc = exit.Value.TryGetProperty("description", out var descProp)
                        ? descProp.GetString()
                        : null;
                    var condition = exit.Value.TryGetProperty("condition", out var condProp)
                        ? condProp.GetString()
                        : null;

                    room.Exits[exit.Name] = new Exit(targetId, exitDesc, condition);
                }

        // Handle optional items
        if (root.TryGetProperty("items", out var itemsElement))
            foreach (var itemElement in itemsElement.EnumerateArray())
                try
                {
                    var item = JsonSerializer.Deserialize<GameItem>(
                        itemElement.GetRawText(),
                        options);
                    if (item != null) room.Items.Add(item);
                }
                catch (JsonException)
                {
                    // Skip malformed items
                }

        // Handle optional NPCs
        if (root.TryGetProperty("npcs", out var npcsElement))
            foreach (var npcElement in npcsElement.EnumerateArray())
                try
                {
                    var npc = JsonSerializer.Deserialize<NPC>(
                        npcElement.GetRawText(),
                        options);
                    if (npc != null) room.NPCs.Add(npc);
                }
                catch (JsonException)
                {
                    // Skip malformed NPCs
                }

        // Handle optional atmosphere
        if (root.TryGetProperty("atmosphere", out var atmElement))
            room.Atmosphere = new RoomAtmosphere(
                atmElement.TryGetProperty("sights", out var sightsProp) ? sightsProp.GetString() : null,
                atmElement.TryGetProperty("sounds", out var soundsProp) ? soundsProp.GetString() : null,
                atmElement.TryGetProperty("smells", out var smellsProp) ? smellsProp.GetString() : null
            );

        return room;
    }

    public override void Write(Utf8JsonWriter writer, Room value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString("id", value.Id);
        writer.WriteString("name", value.Name);

        writer.WriteStartObject("description");
        writer.WriteString("initial", value.Description.Initial);
        if (value.Description.Subsequent != null)
            writer.WriteString("subsequent", value.Description.Subsequent);
        if (value.Description.Detailed != null)
            writer.WriteString("detailed", value.Description.Detailed);
        writer.WriteEndObject();

        writer.WriteStartObject("exits");
        foreach (var exit in value.Exits)
        {
            writer.WritePropertyName(exit.Key);
            JsonSerializer.Serialize(writer, exit.Value, options);
        }

        writer.WriteEndObject();

        if (value.Items.Any())
        {
            writer.WriteStartArray("items");
            foreach (var item in value.Items) JsonSerializer.Serialize(writer, item, options);
            writer.WriteEndArray();
        }

        if (value.NPCs.Any())
        {
            writer.WriteStartArray("npcs");
            foreach (var npc in value.NPCs) JsonSerializer.Serialize(writer, npc, options);
            writer.WriteEndArray();
        }

        if (value.Atmosphere != null)
        {
            writer.WritePropertyName("atmosphere");
            JsonSerializer.Serialize(writer, value.Atmosphere, options);
        }

        writer.WriteEndObject();
    }
}