// File: Assets/Scripts/GemMatch/ObstacleData.cs

/// <summary>
/// Types of obstacles that can occupy board cells.
/// </summary>
public enum ObstacleType
{
    None,
    /// <summary>Match adjacent gems to crack and remove the ice. Gem beneath is freed.</summary>
    Ice,
    /// <summary>Gem is locked. Must be matched once to remove chain, then again to clear.</summary>
    Chain,
    /// <summary>Solid block. Unmovable, unswappable. Only destroyed by power-ups.</summary>
    Stone
}

/// <summary>
/// Describes where an obstacle should be placed on the board.
/// </summary>
public struct ObstaclePlacement
{
    public int row;
    public int col;
    public ObstacleType type;

    public ObstaclePlacement(int row, int col, ObstacleType type)
    {
        this.row = row;
        this.col = col;
        this.type = type;
    }
}
