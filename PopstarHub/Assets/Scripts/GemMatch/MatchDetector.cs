// File: Assets/Scripts/GemMatch/MatchDetector.cs
using System.Collections.Generic;
using UnityEngine;

public class MatchDetector : MonoBehaviour
{
    // Returns all matched gems as a flat list
    public List<Gem> FindMatches(Gem[,] board, int rows, int columns)
    {
        HashSet<Gem> matchedGems = new HashSet<Gem>();

        foreach (List<Gem> group in FindMatchGroups(board, rows, columns))
        {
            foreach (Gem gem in group)
            {
                matchedGems.Add(gem);
            }
        }

        return new List<Gem>(matchedGems);
    }

    // Returns matches as separate groups so power-ups can be detected per group
    public List<List<Gem>> FindMatchGroups(Gem[,] board, int rows, int columns)
    {
        List<List<Gem>> matchGroups = new List<List<Gem>>();

        // Check horizontal matches
        for (int row = 0; row < rows; row++)
        {
            int col = 0;
            while (col < columns)
            {
                Gem startGem = board[row, col];
                if (startGem == null) { col++; continue; }

                List<Gem> run = new List<Gem> { startGem };
                int nextCol = col + 1;

                while (nextCol < columns && board[row, nextCol] != null
                    && board[row, nextCol].Type == startGem.Type)
                {
                    run.Add(board[row, nextCol]);
                    nextCol++;
                }

                if (run.Count >= 3)
                    matchGroups.Add(run);

                col = nextCol;
            }
        }

        // Check vertical matches
        for (int col = 0; col < columns; col++)
        {
            int row = 0;
            while (row < rows)
            {
                Gem startGem = board[row, col];
                if (startGem == null) { row++; continue; }

                List<Gem> run = new List<Gem> { startGem };
                int nextRow = row + 1;

                while (nextRow < rows && board[nextRow, col] != null
                    && board[nextRow, col].Type == startGem.Type)
                {
                    run.Add(board[nextRow, col]);
                    nextRow++;
                }

                if (run.Count >= 3)
                    matchGroups.Add(run);

                row = nextRow;
            }
        }

        return matchGroups;
    }
}