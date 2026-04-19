// File: Assets/Scripts/GemMatch/PowerUpHandler.cs
using System.Collections.Generic;
using UnityEngine;

public enum PowerUpType
{
    None,
    StripedHorizontal,
    StripedVertical,
    Wrapped,
    ColorBomb
}

public class PowerUpHandler : MonoBehaviour
{
    // Checks if matched gems should produce a power-up
    public PowerUpType DetectPowerUp(List<Gem> matchedGems, Gem[,] board, int rows, int columns)
    {
        if (matchedGems.Count >= 5)
        {
            // Check for L or T shape first
            if (IsLOrTShape(matchedGems))
                return PowerUpType.Wrapped;

            // 5 in a straight line = Color Bomb
            return PowerUpType.ColorBomb;
        }

        if (matchedGems.Count == 4)
        {
            // Determine if horizontal or vertical match
            if (IsHorizontalLine(matchedGems))
                return PowerUpType.StripedVertical;

            if (IsVerticalLine(matchedGems))
                return PowerUpType.StripedHorizontal;
        }

        return PowerUpType.None;
    }

    // Find the best gem to place the power-up on (center of the match)
    public Gem FindPowerUpGem(List<Gem> matchedGems, Gem swappedGem)
    {
        // Place power-up where the player swapped, if it's part of the match
        if (swappedGem != null && matchedGems.Contains(swappedGem))
            return swappedGem;

        // Otherwise use the middle gem
        return matchedGems[matchedGems.Count / 2];
    }

    // Get all gems that a power-up destroys when activated
    public List<Gem> GetPowerUpTargets(Gem powerUpGem, Gem[,] board, int rows, int columns)
    {
        List<Gem> targets = new List<Gem>();

        switch (powerUpGem.CurrentPowerUp)
        {
            case PowerUpType.StripedHorizontal:
                // Destroy entire row
                for (int col = 0; col < columns; col++)
                {
                    if (board[powerUpGem.Row, col] != null)
                        targets.Add(board[powerUpGem.Row, col]);
                }
                break;

            case PowerUpType.StripedVertical:
                // Destroy entire column
                for (int row = 0; row < rows; row++)
                {
                    if (board[row, powerUpGem.Column] != null)
                        targets.Add(board[row, powerUpGem.Column]);
                }
                break;

            case PowerUpType.Wrapped:
                // Destroy 3x3 area
                for (int row = powerUpGem.Row - 1; row <= powerUpGem.Row + 1; row++)
                {
                    for (int col = powerUpGem.Column - 1; col <= powerUpGem.Column + 1; col++)
                    {
                        if (row >= 0 && row < rows && col >= 0 && col < columns)
                        {
                            if (board[row, col] != null)
                                targets.Add(board[row, col]);
                        }
                    }
                }
                break;

            case PowerUpType.ColorBomb:
                // Destroy all gems of the same color as the power-up gem's type
                Gem.GemType targetType = powerUpGem.Type;
                for (int row = 0; row < rows; row++)
                {
                    for (int col = 0; col < columns; col++)
                    {
                        if (board[row, col] != null && board[row, col].Type == targetType)
                            targets.Add(board[row, col]);
                    }
                }
                break;
        }

        return targets;
    }

    // Activates a Color Bomb using the type of the gem it was swapped with
    public List<Gem> ActivateColorBomb(Gem colorBomb, Gem swappedWith, Gem[,] board, int rows, int columns)
    {
        List<Gem> targets = new List<Gem>();
        Gem.GemType targetType = swappedWith.Type;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                if (board[row, col] != null && board[row, col].Type == targetType)
                    targets.Add(board[row, col]);
            }
        }

        // Also destroy the bomb itself
        if (!targets.Contains(colorBomb))
            targets.Add(colorBomb);

        return targets;
    }

    private bool IsHorizontalLine(List<Gem> gems)
    {
        int row = gems[0].Row;
        foreach (Gem gem in gems)
        {
            if (gem.Row != row) return false;
        }
        return true;
    }

    private bool IsVerticalLine(List<Gem> gems)
    {
        int col = gems[0].Column;
        foreach (Gem gem in gems)
        {
            if (gem.Column != col) return false;
        }
        return true;
    }

    private bool IsLOrTShape(List<Gem> gems)
    {
        HashSet<int> uniqueRows = new HashSet<int>();
        HashSet<int> uniqueCols = new HashSet<int>();

        foreach (Gem gem in gems)
        {
            uniqueRows.Add(gem.Row);
            uniqueCols.Add(gem.Column);
        }

        // L or T shapes span at least 2 rows AND 2 columns
        return uniqueRows.Count >= 2 && uniqueCols.Count >= 2;
    }
}