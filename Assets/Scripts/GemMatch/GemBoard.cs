// File: Assets/Scripts/GemMatch/GemBoard.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemBoard : MonoBehaviour
{
    [SerializeField] private int rows = 7;
    [SerializeField] private int columns = 7;
    [SerializeField] private float cellSize = 110f;
    [SerializeField] private GameObject gemPrefab;
    [SerializeField] private Transform boardParent;
    [SerializeField] private GemInputHandler inputHandler;
    [SerializeField] private MatchDetector matchDetector;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private GemAnimator gemAnimator;
    [SerializeField] private SoundManager soundManager;
    [SerializeField] private PowerUpHandler powerUpHandler;
    [SerializeField] private HintSystem hintSystem;
    [SerializeField] private ScorePopup scorePopup;

    private Gem[,] board;
    private bool isProcessing;
    public Gem[,] GetBoard() { return board; }

    // Track which gems were swapped so power-ups spawn on the right gem
    private Gem lastSwappedGem;

    public int Rows => rows;
    public int Columns => columns;
    public bool IsProcessing => isProcessing;

    private void Start()
    {
        board = new Gem[rows, columns];
        SpawnBoard();
        ClearStartingMatches();
    }

    private void SpawnBoard()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                SpawnGemAt(row, col);
            }
        }
    }

    private Gem SpawnGemAt(int row, int col)
    {
        float startX = -(columns - 1) * cellSize / 2f;
        float startY = -(rows - 1) * cellSize / 2f;

        GameObject gemObject = Instantiate(gemPrefab, boardParent);

        RectTransform rect = gemObject.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(
            startX + col * cellSize,
            startY + row * cellSize
        );

        Gem.GemType randomType = (Gem.GemType)Random.Range(0, 5);

        Gem gem = gemObject.GetComponent<Gem>();
        gem.Initialize(randomType, row, col);
        gem.SetInputHandler(inputHandler);

        board[row, col] = gem;
        return gem;
    }

    private void ClearStartingMatches()
    {
        List<Gem> matches = matchDetector.FindMatches(board, rows, columns);
        int safetyCounter = 0;

        while (matches.Count > 0 && safetyCounter < 100)
        {
            foreach (Gem gem in matches)
            {
                Destroy(gem.gameObject);
                board[gem.Row, gem.Column] = null;
            }

            RefillEmptySlots();
            matches = matchDetector.FindMatches(board, rows, columns);
            safetyCounter++;
        }
    }

    public void TrySwapGems(Gem gemA, Gem gemB)
    {
        if (isProcessing) return;

        // Special case: Color Bomb swap doesn't need a normal match
        if (gemA.CurrentPowerUp == PowerUpType.ColorBomb || gemB.CurrentPowerUp == PowerUpType.ColorBomb)
        {
            StartCoroutine(ProcessColorBombSwap(gemA, gemB));
            return;
        }

        StartCoroutine(ProcessSwap(gemA, gemB));
    }

    private IEnumerator ProcessColorBombSwap(Gem gemA, Gem gemB)
    {
        isProcessing = true;

        soundManager.PlaySwap();
        RectTransform rectA = gemA.GetComponent<RectTransform>();
        RectTransform rectB = gemB.GetComponent<RectTransform>();
        yield return StartCoroutine(gemAnimator.AnimateSwap(rectA, rectB));

        SwapGemData(gemA, gemB);
        hintSystem.ResetTimer();

        // Figure out which is the bomb and which determines the color
        Gem bomb = gemA.CurrentPowerUp == PowerUpType.ColorBomb ? gemA : gemB;
        Gem other = bomb == gemA ? gemB : gemA;

        List<Gem> targets = powerUpHandler.ActivateColorBomb(bomb, other, board, rows, columns);

        soundManager.PlayCombo();
        scoreManager.ResetCombo();
        scoreManager.AddMatchScore(targets.Count);

        // Animate all targets clearing
        List<Coroutine> clearAnimations = new List<Coroutine>();
        foreach (Gem gem in targets)
        {
            clearAnimations.Add(StartCoroutine(gemAnimator.AnimateClear(gem)));
        }
        foreach (Coroutine c in clearAnimations) yield return c;

        // Destroy targets
        foreach (Gem gem in targets)
        {
            board[gem.Row, gem.Column] = null;
            Destroy(gem.gameObject);
        }

        yield return StartCoroutine(AnimateCollapseColumns());
        RefillEmptySlots();
        yield return new WaitForSeconds(0.1f);

        // Check for chain reactions
        List<Gem> newMatches = matchDetector.FindMatches(board, rows, columns);
        if (newMatches.Count > 0)
        {
            yield return StartCoroutine(ProcessMatches(newMatches));
        }

        isProcessing = false;
    }

    private IEnumerator ProcessSwap(Gem gemA, Gem gemB)
    {
        isProcessing = true;
        lastSwappedGem = gemA;

        soundManager.PlaySwap();
        RectTransform rectA = gemA.GetComponent<RectTransform>();
        RectTransform rectB = gemB.GetComponent<RectTransform>();
        yield return StartCoroutine(gemAnimator.AnimateSwap(rectA, rectB));

        SwapGemData(gemA, gemB);
        hintSystem.ResetTimer();

        List<Gem> matches = matchDetector.FindMatches(board, rows, columns);

        if (matches.Count > 0)
        {
            scoreManager.ResetCombo();
            yield return StartCoroutine(ProcessMatches(matches));
        }
        else
        {
            soundManager.PlaySwap();
            yield return StartCoroutine(gemAnimator.AnimateSwap(rectA, rectB));
            SwapGemData(gemA, gemB);
        }

        isProcessing = false;
    }

    private void SwapGemData(Gem gemA, Gem gemB)
    {
        board[gemA.Row, gemA.Column] = gemB;
        board[gemB.Row, gemB.Column] = gemA;

        int tempRow = gemA.Row;
        int tempCol = gemA.Column;
        gemA.SetPosition(gemB.Row, gemB.Column);
        gemB.SetPosition(tempRow, tempCol);
    }

    private IEnumerator ProcessMatches(List<Gem> allMatches)
    {
        // Detect power-ups from match groups before clearing
        List<List<Gem>> matchGroups = matchDetector.FindMatchGroups(board, rows, columns);
        List<PowerUpSpawn> powerUpsToSpawn = new List<PowerUpSpawn>();

        foreach (List<Gem> group in matchGroups)
        {
            PowerUpType powerUp = powerUpHandler.DetectPowerUp(group, board, rows, columns);

            if (powerUp != PowerUpType.None)
            {
                Gem spawnGem = powerUpHandler.FindPowerUpGem(group, lastSwappedGem);
                powerUpsToSpawn.Add(new PowerUpSpawn
                {
                    row = spawnGem.Row,
                    column = spawnGem.Column,
                    type = powerUp,
                    gemType = spawnGem.Type
                });
            }
        }

        scoreManager.AddMatchScore(allMatches.Count);
        soundManager.PlayMatch();
        // Show floating score popup at the center of the matched gems
        if (scorePopup != null && allMatches.Count > 0)
        {
            Vector2 popupPos = allMatches[0].GetComponent<RectTransform>().anchoredPosition;
            int points = allMatches.Count <= 3 ? 50 : allMatches.Count == 4 ? 100 : 200;
            string popupText = $"+{points}";

            if (allMatches.Count >= 4)
                popupText += " COMBO!";

            scorePopup.ShowPopup(popupPos, popupText, Color.yellow);
        }

        // Collect power-up targets before clearing
        HashSet<Gem> allTargets = new HashSet<Gem>(allMatches);
        foreach (Gem gem in allMatches)
        {
            if (gem.CurrentPowerUp != PowerUpType.None)
            {
                List<Gem> powerUpTargets = powerUpHandler.GetPowerUpTargets(gem, board, rows, columns);
                foreach (Gem target in powerUpTargets)
                {
                    allTargets.Add(target);
                }
                soundManager.PlayCombo();
            }
        }

        List<Gem> finalTargets = new List<Gem>(allTargets);

        // Remove power-up spawn positions from destruction list
        foreach (PowerUpSpawn spawn in powerUpsToSpawn)
        {
            Gem gemAtSpawn = board[spawn.row, spawn.column];
            if (gemAtSpawn != null)
                finalTargets.Remove(gemAtSpawn);
        }

        // Animate clearing
        List<Coroutine> clearAnimations = new List<Coroutine>();
        foreach (Gem gem in finalTargets)
        {
            clearAnimations.Add(StartCoroutine(gemAnimator.AnimateClear(gem)));
        }
        foreach (Coroutine c in clearAnimations) yield return c;

        // Destroy cleared gems
        foreach (Gem gem in finalTargets)
        {
            board[gem.Row, gem.Column] = null;
            Destroy(gem.gameObject);
        }

        // Apply power-ups to surviving gems
        foreach (PowerUpSpawn spawn in powerUpsToSpawn)
        {
            Gem gemAtSpawn = board[spawn.row, spawn.column];
            if (gemAtSpawn != null)
            {
                gemAtSpawn.SetPowerUp(spawn.type);
                Debug.Log($"Power-up created: {spawn.type} at ({spawn.row},{spawn.column})");
            }
        }

        yield return StartCoroutine(AnimateCollapseColumns());
        RefillEmptySlots();
        yield return new WaitForSeconds(0.1f);

        // Check for chain reactions
        List<Gem> newMatches = matchDetector.FindMatches(board, rows, columns);
        if (newMatches.Count > 0)
        {
            soundManager.PlayCombo();
            Debug.Log($"Chain reaction! {newMatches.Count} more gems matched.");
            yield return StartCoroutine(ProcessMatches(newMatches));
        }
        else
        {
            scoreManager.ResetCombo();
        }
    }

    private IEnumerator AnimateCollapseColumns()
    {
        float startX = -(columns - 1) * cellSize / 2f;
        float startY = -(rows - 1) * cellSize / 2f;

        List<Coroutine> collapseAnimations = new List<Coroutine>();

        for (int col = 0; col < columns; col++)
        {
            int emptyRow = -1;

            for (int row = 0; row < rows; row++)
            {
                if (board[row, col] == null && emptyRow == -1)
                {
                    emptyRow = row;
                }
                else if (board[row, col] != null && emptyRow != -1)
                {
                    Gem gem = board[row, col];
                    board[emptyRow, col] = gem;
                    board[row, col] = null;
                    gem.SetPosition(emptyRow, col);

                    Vector2 targetPos = new Vector2(
                        startX + col * cellSize,
                        startY + emptyRow * cellSize
                    );

                    RectTransform rect = gem.GetComponent<RectTransform>();
                    collapseAnimations.Add(StartCoroutine(gemAnimator.AnimateCollapse(rect, targetPos)));

                    emptyRow++;
                }
            }
        }

        foreach (Coroutine c in collapseAnimations) yield return c;
    }

    private void RefillEmptySlots()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                if (board[row, col] == null)
                {
                    SpawnGemAt(row, col);
                }
            }
        }
    }

    // Helper struct to track where power-ups should spawn
    private struct PowerUpSpawn
    {
        public int row;
        public int column;
        public PowerUpType type;
        public Gem.GemType gemType;
    }
}