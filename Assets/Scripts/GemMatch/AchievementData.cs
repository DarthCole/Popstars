// File: Assets/Scripts/GemMatch/AchievementData.cs
using System.Collections.Generic;

/// <summary>
/// Defines a single achievement.
/// </summary>
public class AchievementData
{
    public string id;
    public string name;
    public string description;
    public int coinReward;

    public AchievementData(string id, string name, string description, int coinReward)
    {
        this.id = id;
        this.name = name;
        this.description = description;
        this.coinReward = coinReward;
    }

    /// <summary>
    /// Returns all achievement definitions.
    /// </summary>
    public static List<AchievementData> GetAllAchievements()
    {
        return new List<AchievementData>
        {
            new AchievementData("first_clear", "First Clear",
                "Complete any level", 5),

            new AchievementData("first_3star", "Perfect Score",
                "Get 3 stars on any level", 15),

            new AchievementData("chain_master", "Chain Master",
                "Trigger a 5+ combo chain", 20),

            new AchievementData("speed_demon", "Speed Demon",
                "Finish a timed level with 20s remaining", 20),

            new AchievementData("ice_breaker", "Ice Breaker",
                "Break 50 total ice blocks", 15),

            new AchievementData("color_collector", "Color Collector",
                "Clear 100 gems of one color in a single game", 10),

            new AchievementData("all_clear_5", "Halfway Hero",
                "Complete levels 1 through 5", 30),

            new AchievementData("all_clear_10", "Champion",
                "Complete all 10 levels", 50),

            new AchievementData("star_hoarder", "Star Hoarder",
                "Earn 25 total stars", 40),

            new AchievementData("survivor", "Survivor",
                "Reach wave 10 in Challenge Mode", 30),
        };
    }
}
