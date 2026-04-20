using System.Collections.Generic;
using UnityEngine;

namespace Popstars.Characters {
    /// <summary>
    /// Defines the 6 character archetypes in Popstars with their associated properties.
    /// Each archetype has a unique color, represented artists, and gameplay focus.
    /// </summary>
    public enum CharacterArchetype {
        Vocalist = 0,
        Rockstar = 1,
        Lyricist = 2,
        Performer = 3,
        Trendsetter = 4,
        Afrogiant = 5
    }

    /// <summary>
    /// Static utility class providing archetype metadata and color associations.
    /// Maintains harmony with the game's purple (#6B4C9A) and yellow (#F0D23E) UI theme.
    /// </summary>
    public static class ArchetypeUtility {
        private static readonly Dictionary<CharacterArchetype, ArchetypeData> ArchetypeDatabase = new() {
            { CharacterArchetype.Vocalist, new ArchetypeData {
                Archetype = CharacterArchetype.Vocalist,
                DisplayName = "Vocalist",
                HexColor = "#BA2104",
                RGBColor = new Color(0.729f, 0.129f, 0.016f),
                ArchetypeColor = new Color(0.929f, 0.431f, 0.216f), // Warmer orange-red for UI
                UIAccentColor = new Color(1f, 0.686f, 0.2f), // Gold accent for UI elements
                RepresentedArtists = new List<string> { "Sabrina Carpenter", "Ariana Grande", "SZA", "Freddie Mercury", "Giveon", "Rihanna", "Naomi Sharon" },
                Description = "Soulful and expressive. Vocalists bring emotional depth to every performance.",
                GameplayFocus = "Precision singing and emotional expression",
                StatsModifier = new CharacterStatsModifier { VocalStrength = 1.2f, RhythmAccuracy = 0.95f, PerformanceStyle = 1.0f }
            }},
            { CharacterArchetype.Rockstar, new ArchetypeData {
                Archetype = CharacterArchetype.Rockstar,
                DisplayName = "Rockstar",
                HexColor = "#F0D23E",
                RGBColor = new Color(0.941f, 0.824f, 0.243f),
                ArchetypeColor = new Color(1f, 0.922f, 0.561f), // Lighter yellow-gold
                UIAccentColor = new Color(0.737f, 0.102f, 0.102f), // Deep red accent
                RepresentedArtists = new List<string> { "Billie Eilish", "Playboi Carti", "Destroy Lonely", "Yeat", "Ken Carson", "Lana Del Ray" },
                Description = "Bold and energetic. Rockstars command the stage with attitude and power.",
                GameplayFocus = "High-energy performances and rhythm",
                StatsModifier = new CharacterStatsModifier { VocalStrength = 0.9f, RhythmAccuracy = 1.1f, PerformanceStyle = 1.2f }
            }},
            { CharacterArchetype.Lyricist, new ArchetypeData {
                Archetype = CharacterArchetype.Lyricist,
                DisplayName = "Lyricist",
                HexColor = "#38F277",
                RGBColor = new Color(0.220f, 0.949f, 0.467f),
                ArchetypeColor = new Color(0.486f, 1f, 0.655f), // Bright green-cyan
                UIAccentColor = new Color(0.435f, 0.176f, 0.953f), // Purple accent
                RepresentedArtists = new List<string> { "Eminem", "Jay-Z", "Dave", "Kendrick Lamar", "Bktherula", "Drake" },
                Description = "Clever and articulate. Lyricists captivate with wordplay and storytelling.",
                GameplayFocus = "Lyrical accuracy and flow",
                StatsModifier = new CharacterStatsModifier { VocalStrength = 0.85f, RhythmAccuracy = 1.15f, PerformanceStyle = 1.05f }
            }},
            { CharacterArchetype.Performer, new ArchetypeData {
                Archetype = CharacterArchetype.Performer,
                DisplayName = "Performer",
                HexColor = "#4CBEF5",
                RGBColor = new Color(0.298f, 0.745f, 0.961f),
                ArchetypeColor = new Color(0.565f, 0.859f, 1f), // Bright sky blue
                UIAccentColor = new Color(1f, 0.329f, 0.329f), // Red accent
                RepresentedArtists = new List<string> { "Chris Brown", "Beyoncé", "Usher", "Uncle Waffles", "Karol G", "Bad Bunny" },
                Description = "Dynamic and expressive. Performers dazzle with movement and charisma.",
                GameplayFocus = "Stage presence and choreography",
                StatsModifier = new CharacterStatsModifier { VocalStrength = 1.0f, RhythmAccuracy = 1.0f, PerformanceStyle = 1.25f }
            }},
            { CharacterArchetype.Trendsetter, new ArchetypeData {
                Archetype = CharacterArchetype.Trendsetter,
                DisplayName = "Trendsetter",
                HexColor = "#9F52FF",
                RGBColor = new Color(0.624f, 0.322f, 1f),
                ArchetypeColor = new Color(0.792f, 0.565f, 1f), // Bright purple
                UIAccentColor = new Color(1f, 0.941f, 0.2f), // Yellow accent
                RepresentedArtists = new List<string> { "Ye", "Ice Spice", "Travis Scott", "Lil Yachty", "Cardi B", "Latto" },
                Description = "Innovative and fashion-forward. Trendsetters push boundaries and set the stage.",
                GameplayFocus = "Innovation and style",
                StatsModifier = new CharacterStatsModifier { VocalStrength = 1.05f, RhythmAccuracy = 1.05f, PerformanceStyle = 1.15f }
            }},
            { CharacterArchetype.Afrogiant, new ArchetypeData {
                Archetype = CharacterArchetype.Afrogiant,
                DisplayName = "Afrogiant",
                HexColor = "#FFB23D",
                RGBColor = new Color(1f, 0.698f, 0.239f),
                ArchetypeColor = new Color(1f, 0.808f, 0.569f), // Warm orange
                UIAccentColor = new Color(0.149f, 0.451f, 0.949f), // Blue accent
                RepresentedArtists = new List<string> { "Tems", "Burna Boy", "Davido", "Wizkid", "Sarkodie", "Tyla", "Ayra Starr" },
                Description = "Powerful and influential. Afrogiants bring continental flair and unmatched charisma.",
                GameplayFocus = "Global rhythm and cultural authenticity",
                StatsModifier = new CharacterStatsModifier { VocalStrength = 1.1f, RhythmAccuracy = 1.1f, PerformanceStyle = 1.1f }
            }}
        };

        /// <summary>
        /// Gets the metadata for a specific archetype.
        /// </summary>
        public static ArchetypeData GetArchetypeData(CharacterArchetype archetype) {
            return ArchetypeDatabase.TryGetValue(archetype, out var data) ? data : null;
        }

        /// <summary>
        /// Gets the color for a specific archetype, compatible with the UI theme.
        /// </summary>
        public static Color GetArchetypeColor(CharacterArchetype archetype) {
            var data = GetArchetypeData(archetype);
            return data != null ? data.ArchetypeColor : Color.white;
        }

        /// <summary>
        /// Gets the UI accent color for a specific archetype.
        /// </summary>
        public static Color GetUIAccentColor(CharacterArchetype archetype) {
            var data = GetArchetypeData(archetype);
            return data != null ? data.UIAccentColor : Color.white;
        }

        /// <summary>
        /// Gets all archetypes in the game.
        /// </summary>
        public static CharacterArchetype[] GetAllArchetypes() {
            return new[] { CharacterArchetype.Vocalist, CharacterArchetype.Rockstar, CharacterArchetype.Lyricist, CharacterArchetype.Performer, CharacterArchetype.Trendsetter, CharacterArchetype.Afrogiant };
        }
    }

    /// <summary>
    /// Data container for archetype metadata.
    /// </summary>
    public class ArchetypeData {
        public CharacterArchetype Archetype;
        public string DisplayName;
        public string HexColor;
        public Color RGBColor;
        public Color ArchetypeColor;
        public Color UIAccentColor;
        public List<string> RepresentedArtists;
        public string Description;
        public string GameplayFocus;
        public CharacterStatsModifier StatsModifier;
    }

    /// <summary>
    /// Modifier applied to base character stats based on archetype.
    /// </summary>
    [System.Serializable]
    public class CharacterStatsModifier {
        [Range(0.5f, 1.5f)] public float VocalStrength = 1.0f;
        [Range(0.5f, 1.5f)] public float RhythmAccuracy = 1.0f;
        [Range(0.5f, 1.5f)] public float PerformanceStyle = 1.0f;
    }
}