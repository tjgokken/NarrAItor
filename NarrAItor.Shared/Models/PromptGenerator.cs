namespace NarrAItor.Shared.Models;

public class PromptGenerator
{
    public string GenerateGamePrompt(string theme)
    {
        return $@"Create an interactive text adventure game with the theme: {theme}.
Keep descriptions concise but evocative. Generate exactly 4 interconnected rooms, ensuring every room connects to at least one other room. **No room should lack exits.**

CRITICAL REQUIREMENTS:
1. Every room MUST have at least one exit. If any room does not have an exit, it should not be considered a valid game setup.
2. Exits must clearly connect to other rooms. Use directions such as ""north"", ""south"", ""east"", and ""west"" for each exit, and ensure each direction connects logically to another room.
3. Ensure there are NO isolated rooms. Each room must have at least one exit that leads to another room, creating a connected path between all rooms.
4. Each item MUST have:
   - A unique description
   - At least one meaningful interaction that progresses the game
   - Clear purpose in the game world
5. Each room should contain 1-3 interactive items
6. Items should provide clues or enable progress
7. Maintain consistent narrative flow

Use this exact JSON structure:

{{
    ""title"": ""short_title"",
    ""introduction"": {{
        ""opening"": ""brief_opening"",
        ""objective"": ""clear_player_goal""
    }},
    ""rooms"": [
        {{
            ""id"": ""room_1"",
            ""name"": ""evocative_name"",
            ""description"": {{
                ""initial"": ""first_impression (under 50 words)"",
                ""detailed"": ""deeper_examination (under 100 words)""
            }},
            ""exits"": {{
                ""north"": {{
                    ""targetId"": ""room_2"",
                    ""description"": ""The door leads to another hallway"",
                    ""condition"": ""none""
                }}
            }},
            ""items"": [
                {{
                    ""name"": ""item_name"",
                    ""description"": ""item_details"",
                    ""interactions"": [
                        ""Using this item causes: specific_effect"",
                        ""Examining closely reveals: additional_detail""
                    ],
                    ""properties"": {{
                        ""usableWith"": ""target_item_or_location"",
                        ""effect"": ""what_happens_when_used"",
                        ""consumed"": ""true_if_single_use""
                    }}
                }}
            ]
        }}
    ]
}}

REQUIREMENTS FOR EACH ROOM:
1. **At least one clear exit to another room.** No room can be isolated or lack an exit. Validate the structure to ensure all rooms are interconnected. 
2. Exits must be assigned to real directions (e.g., north, south, east, west) and must point to a valid room ID.
3. All rooms must be interconnected. Ensure there is a clear, logical path between all rooms.
4. Items must provide useful interactions.
5. Maintain a consistent theme and narrative flow throughout.


IMPORTANT: Ensure that the output includes a valid set of exits for each room, clearly specifying the room they lead to. This is a critical aspect and must not be omitted.
At the end of the JSON, provide a summary:
- Number of rooms generated: {{room_count}}
- Number of rooms without exits: {{rooms_without_exits}}

If any rooms do not have exits, regenerate the rooms until all rooms are interconnected.
";
    }
}