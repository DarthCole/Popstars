// File: Assets/Scripts/GemMatch/LevelObjective.cs

/// <summary>
/// Types of special objectives that levels can require.
/// </summary>
public enum ObjectiveType
{
    /// <summary>Reach the target score. Default objective for all levels.</summary>
    Score,
    /// <summary>Clear a specific number of gems of a given color.</summary>
    ClearColor,
    /// <summary>Break all ice blocks on the board.</summary>
    BreakIce,
    /// <summary>Collect star gems that fall to the bottom row.</summary>
    CollectStars,
    /// <summary>Destroy all stone blocks using power-ups.</summary>
    ClearStone
}

/// <summary>
/// Defines and tracks a level objective.
/// </summary>
public class LevelObjective
{
    public ObjectiveType objectiveType;
    public Gem.GemType targetGemType;  // used only for ClearColor
    public int targetCount;
    public int currentCount;

    public LevelObjective(ObjectiveType type, int targetCount, Gem.GemType targetGemType = Gem.GemType.Red)
    {
        this.objectiveType = type;
        this.targetCount = targetCount;
        this.targetGemType = targetGemType;
        this.currentCount = 0;
    }

    /// <summary>
    /// Create a copy for use in gameplay (so static LevelData originals aren't mutated).
    /// </summary>
    public LevelObjective Clone()
    {
        return new LevelObjective(objectiveType, targetCount, targetGemType);
    }

    public bool IsComplete()
    {
        return currentCount >= targetCount;
    }

    public void RecordProgress(int amount = 1)
    {
        currentCount += amount;
        if (currentCount > targetCount)
            currentCount = targetCount;
    }

    public string GetDisplayText()
    {
        switch (objectiveType)
        {
            case ObjectiveType.Score:
                return $"Score Target";
            case ObjectiveType.ClearColor:
                return $"{targetGemType}: {currentCount}/{targetCount}";
            case ObjectiveType.BreakIce:
                return $"Ice: {currentCount}/{targetCount}";
            case ObjectiveType.CollectStars:
                return $"Stars: {currentCount}/{targetCount}";
            case ObjectiveType.ClearStone:
                return $"Stone: {currentCount}/{targetCount}";
            default:
                return "";
        }
    }

    public string GetBriefDescription()
    {
        switch (objectiveType)
        {
            case ObjectiveType.Score:
                return "Reach the target score";
            case ObjectiveType.ClearColor:
                return $"Clear {targetCount} {targetGemType} gems";
            case ObjectiveType.BreakIce:
                return $"Break all {targetCount} ice blocks";
            case ObjectiveType.CollectStars:
                return $"Collect {targetCount} falling stars";
            case ObjectiveType.ClearStone:
                return $"Destroy all {targetCount} stones";
            default:
                return "";
        }
    }
}
