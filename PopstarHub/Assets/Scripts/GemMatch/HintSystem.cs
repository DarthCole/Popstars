// File: Assets/Scripts/GemMatch/HintSystem.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HintSystem : MonoBehaviour
{
    [SerializeField] private GemBoard gemBoard;
    [SerializeField] private float hintDelay = 5f;
    [SerializeField] private float pulseSpeed = 3f;

    private float idleTimer;
    private List<Gem> hintedGems = new List<Gem>();
    private bool isHinting;
    private Coroutine pulseCoroutine;

    private void Update()
    {
        if (gemBoard == null || gemBoard.GetBoard() == null)
            return;

        if (gemBoard.IsProcessing)
        {
            ResetHint();
            return;
        }

        idleTimer += Time.deltaTime;

        if (idleTimer >= hintDelay && !isHinting)
        {
            ShowHint();
        }
    }

    // Called externally when the player makes any move
    public void ResetTimer()
    {
        idleTimer = 0f;
        ResetHint();
    }

    private void ShowHint()
    {
        Gem gemA;
        Gem gemB;

        bool foundMove = FindValidMove(out gemA, out gemB);

        if (!foundMove)
        {
            Debug.Log("No valid moves available!");
            return;
        }

        isHinting = true;
        hintedGems.Add(gemA);
        hintedGems.Add(gemB);

        pulseCoroutine = StartCoroutine(PulseGems());
    }

    private void ResetHint()
    {
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }

        // Restore original scale
        foreach (Gem gem in hintedGems)
        {
            if (gem != null)
            {
                RectTransform rect = gem.GetComponent<RectTransform>();
                if (rect != null)
                    rect.localScale = Vector3.one;
            }
        }

        hintedGems.Clear();
        isHinting = false;
        idleTimer = 0f;
    }

    private IEnumerator PulseGems()
    {
        while (isHinting)
        {
            float scale = 1f + Mathf.Sin(Time.time * pulseSpeed) * 0.15f;

            foreach (Gem gem in hintedGems)
            {
                if (gem != null)
                {
                    RectTransform rect = gem.GetComponent<RectTransform>();
                    if (rect != null)
                        rect.localScale = Vector3.one * scale;
                }
            }

            yield return null;
        }
    }

    // Brute-force check every possible swap for a valid move
    private bool FindValidMove(out Gem gemA, out Gem gemB)
    {
        gemA = null;
        gemB = null;

        Gem[,] board = gemBoard.GetBoard();
        int rows = gemBoard.Rows;
        int columns = gemBoard.Columns;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Gem current = board[row, col];
                if (current == null) continue;

                // Try swapping right
                if (col + 1 < columns)
                {
                    Gem right = board[row, col + 1];
                    if (right != null && WouldCreateMatch(board, current, right, rows, columns))
                    {
                        gemA = current;
                        gemB = right;
                        return true;
                    }
                }

                // Try swapping up
                if (row + 1 < rows)
                {
                    Gem above = board[row + 1, col];
                    if (above != null && WouldCreateMatch(board, current, above, rows, columns))
                    {
                        gemA = current;
                        gemB = above;
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private bool WouldCreateMatch(Gem[,] board, Gem gemA, Gem gemB, int rows, int columns)
    {
        // Temporarily swap
        board[gemA.Row, gemA.Column] = gemB;
        board[gemB.Row, gemB.Column] = gemA;

        int tempRow = gemA.Row;
        int tempCol = gemA.Column;
        gemA.SetPosition(gemB.Row, gemB.Column);
        gemB.SetPosition(tempRow, tempCol);

        // Check for matches
        bool hasMatch = CheckForMatchAt(board, gemA, rows, columns)
                     || CheckForMatchAt(board, gemB, rows, columns);

        // Swap back
        board[gemA.Row, gemA.Column] = gemB;
        board[gemB.Row, gemB.Column] = gemA;

        int revertRow = gemA.Row;
        int revertCol = gemA.Column;
        gemA.SetPosition(gemB.Row, gemB.Column);
        gemB.SetPosition(revertRow, revertCol);

        return hasMatch;
    }

    // Checks if a specific gem is part of a 3-in-a-row
    private bool CheckForMatchAt(Gem[,] board, Gem gem, int rows, int columns)
    {
        Gem.GemType type = gem.Type;
        int row = gem.Row;
        int col = gem.Column;

        // Horizontal check
        int horizontalCount = 1;
        int left = col - 1;
        while (left >= 0 && board[row, left] != null && board[row, left].Type == type)
        {
            horizontalCount++;
            left--;
        }
        int right = col + 1;
        while (right < columns && board[row, right] != null && board[row, right].Type == type)
        {
            horizontalCount++;
            right++;
        }
        if (horizontalCount >= 3) return true;

        // Vertical check
        int verticalCount = 1;
        int down = row - 1;
        while (down >= 0 && board[down, col] != null && board[down, col].Type == type)
        {
            verticalCount++;
            down--;
        }
        int up = row + 1;
        while (up < rows && board[up, col] != null && board[up, col].Type == type)
        {
            verticalCount++;
            up++;
        }
        if (verticalCount >= 3) return true;

        return false;
    }
}