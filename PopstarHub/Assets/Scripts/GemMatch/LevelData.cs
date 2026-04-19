// File: Assets/Scripts/GemMatch/LevelData.cs
using System.Collections.Generic;

public enum LevelConstraintType
{
    Time,
    Moves
}

public class LevelData
{
    public int levelNumber;
    public int rows;
    public int columns;
    public int gemTypeCount;
    public LevelConstraintType constraintType;
    public float timeLimit;
    public int moveLimit;
    public int targetScore;
    public int twoStarScore;
    public int threeStarScore;
    public List<ObstaclePlacement> obstacles;
    public LevelObjective objective;

    public LevelData(int levelNumber, int rows, int columns, int gemTypeCount,
                     LevelConstraintType constraintType, float timeLimit, int moveLimit,
                     int targetScore, int twoStarScore, int threeStarScore,
                     List<ObstaclePlacement> obstacles = null,
                     LevelObjective objective = null)
    {
        this.levelNumber = levelNumber;
        this.rows = rows;
        this.columns = columns;
        this.gemTypeCount = gemTypeCount;
        this.constraintType = constraintType;
        this.timeLimit = timeLimit;
        this.moveLimit = moveLimit;
        this.targetScore = targetScore;
        this.twoStarScore = twoStarScore;
        this.threeStarScore = threeStarScore;
        this.obstacles = obstacles ?? new List<ObstaclePlacement>();
        this.objective = objective;
    }

    public int GetStarRating(int score)
    {
        if (score >= threeStarScore) return 3;
        if (score >= twoStarScore) return 2;
        if (score >= targetScore) return 1;
        return 0;
    }

    public static int GetStarCoins(int stars)
    {
        switch (stars)
        {
            case 1: return 10;
            case 2: return 25;
            case 3: return 50;
            default: return 0;
        }
    }

    public string GetObjectiveText()
    {
        string baseText;
        if (constraintType == LevelConstraintType.Time)
            baseText = $"Score {targetScore} in {(int)timeLimit}s";
        else
            baseText = $"Score {targetScore} in {moveLimit} moves";

        if (objective != null && objective.objectiveType != ObjectiveType.Score)
            baseText += $"\n{objective.GetBriefDescription()}";

        return baseText;
    }

    public static LevelData GetLevel(int levelNumber)
    {
        List<LevelData> all = GetAllLevels();
        if (levelNumber < 1 || levelNumber > all.Count) return null;
        return all[levelNumber - 1];
    }

    public static List<LevelData> GetAllLevels()
    {
        return new List<LevelData>
        {
            // Level 1 — Tutorial easy: 7x7, 4 gems, 60s, pure score
            new LevelData(1, 7, 7, 4,
                LevelConstraintType.Time, 60f, 0,
                500, 800, 1200,
                null,
                new LevelObjective(ObjectiveType.Score, 500)),

            // Level 2 — Full palette: 7x7, 5 gems, 55s, pure score
            new LevelData(2, 7, 7, 5,
                LevelConstraintType.Time, 55f, 0,
                800, 1200, 1800,
                null,
                new LevelObjective(ObjectiveType.Score, 800)),

            // Level 3 — Ice intro: 7x7, 5 gems, 50s, clear 15 blue gems + ice blocks
            new LevelData(3, 7, 7, 5,
                LevelConstraintType.Time, 50f, 0,
                1000, 1500, 2200,
                new List<ObstaclePlacement>
                {
                    new ObstaclePlacement(1, 1, ObstacleType.Ice),
                    new ObstaclePlacement(1, 5, ObstacleType.Ice),
                    new ObstaclePlacement(3, 3, ObstacleType.Ice),
                    new ObstaclePlacement(5, 1, ObstacleType.Ice),
                    new ObstaclePlacement(5, 5, ObstacleType.Ice),
                },
                new LevelObjective(ObjectiveType.ClearColor, 15, Gem.GemType.Blue)),

            // Level 4 — Move-limit + ice: 7x7, 5 gems, 30 moves, break all ice
            new LevelData(4, 7, 7, 5,
                LevelConstraintType.Moves, 0f, 30,
                1200, 1800, 2600,
                new List<ObstaclePlacement>
                {
                    new ObstaclePlacement(0, 0, ObstacleType.Ice),
                    new ObstaclePlacement(0, 6, ObstacleType.Ice),
                    new ObstaclePlacement(2, 2, ObstacleType.Ice),
                    new ObstaclePlacement(2, 4, ObstacleType.Ice),
                    new ObstaclePlacement(4, 2, ObstacleType.Ice),
                    new ObstaclePlacement(4, 4, ObstacleType.Ice),
                    new ObstaclePlacement(6, 0, ObstacleType.Ice),
                    new ObstaclePlacement(6, 6, ObstacleType.Ice),
                },
                new LevelObjective(ObjectiveType.BreakIce, 8)),

            // Level 5 — Bigger board + chains: 8x8, 5 gems, 45s, collect 3 stars
            new LevelData(5, 8, 8, 5,
                LevelConstraintType.Time, 45f, 0,
                1500, 2200, 3200,
                new List<ObstaclePlacement>
                {
                    new ObstaclePlacement(2, 2, ObstacleType.Chain),
                    new ObstaclePlacement(2, 5, ObstacleType.Chain),
                    new ObstaclePlacement(5, 2, ObstacleType.Chain),
                    new ObstaclePlacement(5, 5, ObstacleType.Chain),
                    new ObstaclePlacement(1, 3, ObstacleType.Ice),
                    new ObstaclePlacement(6, 4, ObstacleType.Ice),
                },
                new LevelObjective(ObjectiveType.CollectStars, 3)),

            // Level 6 — 6th gem + moves: 8x8, 6 gems, 25 moves, break all ice
            new LevelData(6, 8, 8, 6,
                LevelConstraintType.Moves, 0f, 25,
                1800, 2600, 3800,
                new List<ObstaclePlacement>
                {
                    new ObstaclePlacement(1, 1, ObstacleType.Ice),
                    new ObstaclePlacement(1, 6, ObstacleType.Ice),
                    new ObstaclePlacement(3, 3, ObstacleType.Ice),
                    new ObstaclePlacement(3, 4, ObstacleType.Ice),
                    new ObstaclePlacement(4, 3, ObstacleType.Ice),
                    new ObstaclePlacement(4, 4, ObstacleType.Ice),
                    new ObstaclePlacement(6, 1, ObstacleType.Ice),
                    new ObstaclePlacement(6, 6, ObstacleType.Ice),
                    new ObstaclePlacement(2, 3, ObstacleType.Chain),
                    new ObstaclePlacement(5, 4, ObstacleType.Chain),
                },
                new LevelObjective(ObjectiveType.BreakIce, 8)),

            // Level 7 — Stone intro: 8x8, 6 gems, 40s, clear 20 green gems
            new LevelData(7, 8, 8, 6,
                LevelConstraintType.Time, 40f, 0,
                2200, 3200, 4500,
                new List<ObstaclePlacement>
                {
                    new ObstaclePlacement(3, 0, ObstacleType.Stone),
                    new ObstaclePlacement(4, 0, ObstacleType.Stone),
                    new ObstaclePlacement(3, 7, ObstacleType.Stone),
                    new ObstaclePlacement(4, 7, ObstacleType.Stone),
                    new ObstaclePlacement(1, 3, ObstacleType.Ice),
                    new ObstaclePlacement(1, 4, ObstacleType.Ice),
                    new ObstaclePlacement(6, 3, ObstacleType.Ice),
                    new ObstaclePlacement(6, 4, ObstacleType.Ice),
                    new ObstaclePlacement(2, 2, ObstacleType.Chain),
                    new ObstaclePlacement(5, 5, ObstacleType.Chain),
                },
                new LevelObjective(ObjectiveType.ClearColor, 20, Gem.GemType.Green)),

            // Level 8 — Large board few moves: 9x9, 6 gems, 20 moves, collect 4 stars
            new LevelData(8, 9, 9, 6,
                LevelConstraintType.Moves, 0f, 20,
                2500, 3600, 5000,
                new List<ObstaclePlacement>
                {
                    new ObstaclePlacement(4, 4, ObstacleType.Stone),
                    new ObstaclePlacement(0, 4, ObstacleType.Ice),
                    new ObstaclePlacement(8, 4, ObstacleType.Ice),
                    new ObstaclePlacement(4, 0, ObstacleType.Ice),
                    new ObstaclePlacement(4, 8, ObstacleType.Ice),
                    new ObstaclePlacement(2, 2, ObstacleType.Chain),
                    new ObstaclePlacement(2, 6, ObstacleType.Chain),
                    new ObstaclePlacement(6, 2, ObstacleType.Chain),
                    new ObstaclePlacement(6, 6, ObstacleType.Chain),
                },
                new LevelObjective(ObjectiveType.CollectStars, 4)),

            // Level 9 — Time crunch: 9x9, 6 gems, 35s, pure score (hard)
            new LevelData(9, 9, 9, 6,
                LevelConstraintType.Time, 35f, 0,
                3000, 4200, 5800,
                new List<ObstaclePlacement>
                {
                    new ObstaclePlacement(0, 0, ObstacleType.Stone),
                    new ObstaclePlacement(0, 8, ObstacleType.Stone),
                    new ObstaclePlacement(8, 0, ObstacleType.Stone),
                    new ObstaclePlacement(8, 8, ObstacleType.Stone),
                    new ObstaclePlacement(2, 4, ObstacleType.Ice),
                    new ObstaclePlacement(4, 2, ObstacleType.Ice),
                    new ObstaclePlacement(4, 6, ObstacleType.Ice),
                    new ObstaclePlacement(6, 4, ObstacleType.Ice),
                    new ObstaclePlacement(3, 3, ObstacleType.Chain),
                    new ObstaclePlacement(3, 5, ObstacleType.Chain),
                    new ObstaclePlacement(5, 3, ObstacleType.Chain),
                    new ObstaclePlacement(5, 5, ObstacleType.Chain),
                },
                new LevelObjective(ObjectiveType.Score, 3000)),

            // Level 10 — Max difficulty: 9x9, 7 gems, 18 moves, destroy all stones
            new LevelData(10, 9, 9, 7,
                LevelConstraintType.Moves, 0f, 18,
                3500, 5000, 7000,
                new List<ObstaclePlacement>
                {
                    new ObstaclePlacement(1, 1, ObstacleType.Stone),
                    new ObstaclePlacement(1, 7, ObstacleType.Stone),
                    new ObstaclePlacement(4, 4, ObstacleType.Stone),
                    new ObstaclePlacement(7, 1, ObstacleType.Stone),
                    new ObstaclePlacement(7, 7, ObstacleType.Stone),
                    new ObstaclePlacement(0, 4, ObstacleType.Ice),
                    new ObstaclePlacement(4, 0, ObstacleType.Ice),
                    new ObstaclePlacement(4, 8, ObstacleType.Ice),
                    new ObstaclePlacement(8, 4, ObstacleType.Ice),
                    new ObstaclePlacement(2, 3, ObstacleType.Chain),
                    new ObstaclePlacement(2, 5, ObstacleType.Chain),
                    new ObstaclePlacement(6, 3, ObstacleType.Chain),
                    new ObstaclePlacement(6, 5, ObstacleType.Chain),
                },
                new LevelObjective(ObjectiveType.ClearStone, 5)),
        };
    }
}
