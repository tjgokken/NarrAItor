namespace NarrAItor.Shared.Models;

public class ThemeDefinition
{
    public static Dictionary<string, ThemeDefinition> PresetThemes = new()
    {
        ["Cyberpunk"] = new ThemeDefinition
        {
            Name= "Cyberpunk",
            PrimaryColor = "#00ff9f",
            SecondaryColor = "#ff00ff",
            BackgroundColor = "#0a0a1f",
            FontFamily = "'Share Tech Mono', monospace",
            AccentColor = "#00ffff",
            Suggestions = new List<string>
            {
                "neon-lit streets",
                "corporate intrigue",
                "digital consciousness",
                "underground hackers"
            },
            Description = "High tech, low life. A world of neon and chrome."
        },
        ["Fantasy"] = new ThemeDefinition
        {
            Name = "Fantasy",
            PrimaryColor = "#ffd700",
            SecondaryColor = "#8b4513",
            BackgroundColor = "#2a0a1f",
            FontFamily = "'MedievalSharp', cursive",
            AccentColor = "#ff4500",
            Suggestions = new List<string>
            {
                "ancient dungeons",
                "mystical forests",
                "dragon's lair",
                "enchanted castle"
            },
            Description = "A realm of magic, monsters, and medieval mystery."
        },
        ["SciFi"] = new ThemeDefinition
        {
            Name = "SciFi",
            PrimaryColor = "#7fff00",
            SecondaryColor = "#4169e1",
            BackgroundColor = "#000033",
            FontFamily = "'Orbitron', sans-serif",
            AccentColor = "#00ffff",
            Suggestions = new List<string>
            {
                "space station",
                "alien artifacts",
                "quantum realm",
                "martian colony"
            },
            Description = "The final frontier awaits exploration."
        },
        ["Horror"] = new ThemeDefinition
        {
            Name = "Horror",
            PrimaryColor = "#8b0000",
            SecondaryColor = "#483d8b",
            BackgroundColor = "#000000",
            FontFamily = "'Creepster', cursive",
            AccentColor = "#800080",
            Suggestions = new List<string>
            {
                "abandoned asylum",
                "haunted mansion",
                "cursed artifacts",
                "cosmic horror"
            },
            Description = "Face your fears in realms of darkness."
        },
        ["Steampunk"] = new ThemeDefinition
        {
            Name = "Steampunk",
            PrimaryColor = "#b8860b",
            SecondaryColor = "#8b4513",
            BackgroundColor = "#2f1810",
            FontFamily = "'UnifrakturMaguntia', cursive",
            AccentColor = "#cd853f",
            Suggestions = new List<string>
            {
                "clockwork city",
                "airship pirates",
                "mechanical wonders",
                "Victorian mysteries"
            },
            Description = "Brass, steam, and Victorian dreams."
        },

        ["Western"] = new ThemeDefinition
        {
            Name = "Western",
            PrimaryColor = "#d2691e", // Saddle Brown color to represent the Western feel
            SecondaryColor = "#8b0000", // Dark Red for intensity and ruggedness
            BackgroundColor = "#deb887", // BurlyWood color to evoke dusty landscapes
            FontFamily = "'Special Elite', cursive", // A classic old western typewriter feel
            AccentColor = "#ffdead", // NavajoWhite for a softer, complementary accent
            Suggestions = new List<string>
            {
                "deserted saloon",
                "gold mine hideout",
                "dusty town showdown",
                "rattlesnake canyon"
            },
            Description = "Saddle up for dusty trails, old saloons, and high-noon showdowns."
        }
    };

    public required string Name { get; set; }
    public required string PrimaryColor { get; set; }
    public required string SecondaryColor { get; set; }
    public required string BackgroundColor { get; set; }
    public required string FontFamily { get; set; }
    public required string AccentColor { get; set; }
    public List<string> Suggestions { get; set; } = new();
    public required string Description { get; set; }
}