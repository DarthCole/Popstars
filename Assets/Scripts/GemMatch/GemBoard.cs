// File: Assets/Scripts/GemMatch/GemBoard.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private MoveCounter moveCounter;
    [SerializeField] private ObjectiveTracker objectiveTracker;
    [SerializeField] private ComboAnnouncer comboAnnouncer;
    [SerializeField] private VFXManager vfxManager;

    private Gem[,] board;
    private bool isProcessing;
    private int gemTypeCount = 5;
    private int comboCount;
    private bool[,] stoneMap;  // true = stone at this cell
    private List<ObstaclePlacement> currentObstacles;
    public Gem[,] GetBoard() { return board; }

    // Track which gems were swapped so power-ups spawn on the right gem
    private Gem lastSwappedGem;

    public int Rows => rows;
    public int Columns => columns;
    public float CellSize => cellSize;
    public bool IsProcessing => isProcessing;

    private bool boardInitialized = false;

    // Star gem spawning for CollectStars objective
    private bool collectStarsActive;
    private int starGemsCollected;
    private float starGemSpawnTimer;
    private float starGemSpawnInterval = 8f; // seconds between star gem spawns

    private void Awake()
    {
        EnsureReferences();
    }

    private void Start()
    {
        // Don't auto-init; wait for GameManager.StartLevel()
    }

    private void Update()
    {
        // Star gem spawning logic
        if (collectStarsActive && boardInitialized && !isProcessing)
        {
            starGemSpawnTimer += Time.deltaTime;
            if (starGemSpawnTimer >= starGemSpawnInterval)
            {
                starGemSpawnTimer = 0f;
                SpawnStarGem();
            }
        }
    }

    public void ConfigureFromLevel(LevelData level)
    {
        if (level == null) return;

        rows = level.rows;
        columns = level.columns;
        gemTypeCount = level.gemTypeCount;
        currentObstacles = level.obstacles;

        // Check if this level has CollectStars objective
        collectStarsActive = level.objective != null &&
                            level.objective.objectiveType == ObjectiveType.CollectStars;
        starGemSpawnTimer = 5f; // first star gem spawns sooner
    }

    public void InitializeBoard()
    {
        if (boardInitialized)
            ClearBoard();

        EnsureBoardVisual();
        board = new Gem[rows, columns];
        stoneMap = new bool[rows, columns];

        // Place stone obstacles first (they occupy entire cells)
        if (currentObstacles != null)
        {
            foreach (var obs in currentObstacles)
            {
                if (obs.type == ObstacleType.Stone && obs.row < rows && obs.col < columns)
                {
                    stoneMap[obs.row, obs.col] = true;
                    SpawnStoneAt(obs.row, obs.col);
                }
            }
        }

        SpawnBoard();
        ClearStartingMatches();

        // Place ice and chain obstacles on existing gems
        if (currentObstacles != null)
        {
            foreach (var obs in currentObstacles)
            {
                if (obs.type != ObstacleType.Stone && obs.row < rows && obs.col < columns)
                {
                    Gem gem = board[obs.row, obs.col];
                    if (gem != null)
                        gem.SetObstacle(obs.type);
                }
            }
        }

        boardInitialized = true;
    }

    public void ClearBoard()
    {
        if (board != null)
        {
            for (int r = 0; r < board.GetLength(0); r++)
            {
                for (int c = 0; c < board.GetLength(1); c++)
                {
                    if (board[r, c] != null)
                    {
                        Destroy(board[r, c].gameObject);
                        board[r, c] = null;
                    }
                }
            }
        }

        // Clear stone visual objects
        if (boardParent != null)
        {
            foreach (Transform child in boardParent)
            {
                if (child.name == "StoneBlock")
                    Destroy(child.gameObject);
            }
        }

        board = null;
        stoneMap = null;
        boardInitialized = false;
        collectStarsActive = false;
    }

    public void SetVisible(bool visible)
    {
        if (boardParent != null)
            boardParent.gameObject.SetActive(visible);
    }

    private void EnsureReferences()
    {
        if (gemPrefab == null)
            Debug.LogError("GemBoard is missing gemPrefab reference.");

        if (inputHandler == null)
            inputHandler = FindFirstObjectByType<GemInputHandler>();
        if (matchDetector == null)
            matchDetector = FindFirstObjectByType<MatchDetector>();
        if (scoreManager == null)
            scoreManager = FindFirstObjectByType<ScoreManager>();
        if (gemAnimator == null)
            gemAnimator = FindFirstObjectByType<GemAnimator>();
        if (soundManager == null)
            soundManager = FindFirstObjectByType<SoundManager>();
        if (powerUpHandler == null)
            powerUpHandler = FindFirstObjectByType<PowerUpHandler>();
        if (hintSystem == null)
            hintSystem = FindFirstObjectByType<HintSystem>();
        if (scorePopup == null)
            scorePopup = FindFirstObjectByType<ScorePopup>();
        if (moveCounter == null)
            moveCounter = FindFirstObjectByType<MoveCounter>();
        if (objectiveTracker == null)
            objectiveTracker = FindFirstObjectByType<ObjectiveTracker>();
        if (comboAnnouncer == null)
            comboAnnouncer = FindFirstObjectByType<ComboAnnouncer>();
        if (vfxManager == null)
            vfxManager = FindFirstObjectByType<VFXManager>();

        Canvas canvas = UIHierarchyBuilder.GetOrCreateCanvas("UICanvas");
        if (boardParent == null || boardParent.GetComponentInParent<Canvas>() == null)
        {
            Transform container = canvas.transform.Find("GemBoardContainer");
            if (container == null)
            {
                GameObject containerObject = new GameObject("GemBoardContainer", typeof(RectTransform));
                containerObject.transform.SetParent(canvas.transform, false);
                RectTransform containerRect = containerObject.GetComponent<RectTransform>();
                containerRect.anchorMin = new Vector2(0.5f, 0.5f);
                containerRect.anchorMax = new Vector2(0.5f, 0.5f);
                containerRect.pivot = new Vector2(0.5f, 0.5f);
                containerRect.anchoredPosition = Vector2.zero;
                container = containerRect;
            }

            boardParent = container;
        }

        EnsureBoardVisual();
    }

    private void EnsureBoardVisual()
    {
        if (boardParent == null)
            return;

        RectTransform boardRect = boardParent as RectTransform;
        if (boardRect == null)
            return;

        boardRect.anchorMin = new Vector2(0.5f, 0.5f);
        boardRect.anchorMax = new Vector2(0.5f, 0.5f);
        boardRect.pivot = new Vector2(0.5f, 0.5f);
        boardRect.anchoredPosition = Vector2.zero;

        Transform shadow = boardParent.Find("BoardShadow");
        GameObject shadowObject;

        if (shadow == null)
        {
            shadowObject = new GameObject("BoardShadow", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            shadowObject.transform.SetParent(boardParent, false);
            shadowObject.transform.SetAsFirstSibling();
        }
        else
        {
            shadowObject = shadow.gameObject;
            shadowObject.transform.SetAsFirstSibling();
        }

        Transform surface = boardParent.Find("BoardSurface");
        GameObject surfaceObject;

        if (surface == null)
        {
            surfaceObject = new GameObject("BoardSurface", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            surfaceObject.transform.SetParent(boardParent, false);
            surfaceObject.transform.SetAsFirstSibling();
        }
        else
        {
            surfaceObject = surface.gameObject;
            surfaceObject.transform.SetAsFirstSibling();
        }

        float width = columns * cellSize + 170f;
        float height = rows * cellSize + 220f;

        RectTransform shadowRect = shadowObject.GetComponent<RectTransform>();
        shadowRect.anchorMin = new Vector2(0.5f, 0.5f);
        shadowRect.anchorMax = new Vector2(0.5f, 0.5f);
        shadowRect.pivot = new Vector2(0.5f, 0.5f);
        shadowRect.anchoredPosition = new Vector2(0f, -12f);
        shadowRect.sizeDelta = new Vector2(width + 8f, height + 8f);

        Image shadowImage = shadowObject.GetComponent<Image>();
        shadowImage.color = new Color(0.06f, 0.03f, 0.12f, 0.55f);
        shadowObject.layer = 5;

        RectTransform surfaceRect = surfaceObject.GetComponent<RectTransform>();
        surfaceRect.anchorMin = new Vector2(0.5f, 0.5f);
        surfaceRect.anchorMax = new Vector2(0.5f, 0.5f);
        surfaceRect.pivot = new Vector2(0.5f, 0.5f);
        surfaceRect.anchoredPosition = Vector2.zero;
        surfaceRect.sizeDelta = new Vector2(width, height);

        Image surfaceImage = surfaceObject.GetComponent<Image>();
        surfaceImage.color = new Color(0.52f, 0.1f, 0.86f, 0.88f);
        surfaceObject.layer = 5;

        Transform topBand = boardParent.Find("BoardTopBand");
        GameObject topBandObject;

        if (topBand == null)
        {
            topBandObject = new GameObject("BoardTopBand", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            topBandObject.transform.SetParent(boardParent, false);
            topBandObject.transform.SetAsFirstSibling();
        }
        else
        {
            topBandObject = topBand.gameObject;
            topBandObject.transform.SetAsFirstSibling();
        }

        RectTransform topBandRect = topBandObject.GetComponent<RectTransform>();
        topBandRect.anchorMin = new Vector2(0.5f, 0.5f);
        topBandRect.anchorMax = new Vector2(0.5f, 0.5f);
        topBandRect.pivot = new Vector2(0.5f, 1f);
        topBandRect.anchoredPosition = new Vector2(0f, height * 0.5f - 12f);
        topBandRect.sizeDelta = new Vector2(width - 18f, 80f);

        Image topBandImage = topBandObject.GetComponent<Image>();
        topBandImage.color = new Color(0.64f, 0.2f, 0.95f, 0.55f);
        topBandObject.layer = 5;
    }

    private void SpawnBoard()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                if (stoneMap != null && stoneMap[row, col]) continue; // skip stone cells
                SpawnGemAt(row, col);
            }
        }
    }

    private Gem SpawnGemAt(int row, int col)
    {
        if (stoneMap != null && stoneMap[row, col]) return null; // stone cell

        float startX = -(columns - 1) * cellSize / 2f;
        float startY = -(rows - 1) * cellSize / 2f;

        GameObject gemObject = Instantiate(gemPrefab, boardParent);
        gemObject.name = "GemPanel";

        RectTransform rect = gemObject.GetComponent<RectTransform>();
        if (rect == null)
        {
            Debug.LogError("Gem prefab is missing RectTransform.");
            Destroy(gemObject);
            return null;
        }

        rect.anchoredPosition = new Vector2(
            startX + col * cellSize,
            startY + row * cellSize
        );

        Gem.GemType randomType = (Gem.GemType)Random.Range(0, gemTypeCount);

        Gem gem = gemObject.GetComponent<Gem>();
        if (gem == null)
        {
            Debug.LogError("Gem prefab is missing Gem component.");
            Destroy(gemObject);
            return null;
        }

        gem.Initialize(randomType, row, col);
        gem.SetInputHandler(inputHandler);

        board[row, col] = gem;
        return gem;
    }

    private void SpawnStoneAt(int row, int col)
    {
        float startX = -(columns - 1) * cellSize / 2f;
        float startY = -(rows - 1) * cellSize / 2f;

        GameObject stoneObj = new GameObject("StoneBlock", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        stoneObj.transform.SetParent(boardParent, false);

        RectTransform rect = stoneObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(cellSize * 0.85f, cellSize * 0.85f);
        rect.anchoredPosition = new Vector2(
            startX + col * cellSize,
            startY + row * cellSize
        );

        Image stoneImage = stoneObj.GetComponent<Image>();
        stoneImage.color = new Color(0.4f, 0.35f, 0.45f, 0.9f);
        stoneImage.raycastTarget = false;
    }

    private void SpawnStarGem()
    {
        // Find a random gem and mark it as a star gem
        List<Gem> candidates = new List<Gem>();
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                Gem g = board[r, c];
                if (g != null && !g.IsStarGem && g.CurrentObstacle == ObstacleType.None)
                    candidates.Add(g);
            }
        }

        if (candidates.Count > 0)
        {
            Gem chosen = candidates[Random.Range(0, candidates.Count)];
            chosen.SetAsStarGem();
        }
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
        comboCount = 0;

        soundManager.PlaySwap();
        RectTransform rectA = gemA.GetComponent<RectTransform>();
        RectTransform rectB = gemB.GetComponent<RectTransform>();
        yield return StartCoroutine(gemAnimator.AnimateSwap(rectA, rectB));

        SwapGemData(gemA, gemB);
        hintSystem?.ResetTimer();

        if (moveCounter != null)
            moveCounter.UseMove();

        Gem bomb = gemA.CurrentPowerUp == PowerUpType.ColorBomb ? gemA : gemB;
        Gem other = bomb == gemA ? gemB : gemA;

        List<Gem> targets = powerUpHandler.ActivateColorBomb(bomb, other, board, rows, columns);

        soundManager.PlayCombo();
        scoreManager.ResetCombo();
        scoreManager.AddMatchScore(targets.Count);

        // Notify objective tracker for each cleared gem
        foreach (Gem gem in targets)
            NotifyGemCleared(gem);

        List<Coroutine> clearAnimations = new List<Coroutine>();
        foreach (Gem gem in targets)
        {
            SpawnMatchVFX(gem);
            clearAnimations.Add(StartCoroutine(gemAnimator.AnimateClear(gem)));
        }
        foreach (Coroutine c in clearAnimations) yield return c;

        foreach (Gem gem in targets)
        {
            // Handle obstacles on adjacent gems (ice break)
            HitAdjacentIce(gem.Row, gem.Column);
            board[gem.Row, gem.Column] = null;
            Destroy(gem.gameObject);
        }

        yield return StartCoroutine(AnimateCollapseColumns());
        CheckStarGemCollection();
        RefillEmptySlots();
        yield return new WaitForSeconds(0.1f);

        List<Gem> newMatches = matchDetector.FindMatches(board, rows, columns);
        if (newMatches.Count > 0)
        {
            yield return StartCoroutine(ProcessMatches(newMatches));
        }

        CheckForValidMoves();
        isProcessing = false;
    }

    private IEnumerator ProcessSwap(Gem gemA, Gem gemB)
    {
        isProcessing = true;
        lastSwappedGem = gemA;
        comboCount = 0;

        soundManager.PlaySwap();
        RectTransform rectA = gemA.GetComponent<RectTransform>();
        RectTransform rectB = gemB.GetComponent<RectTransform>();
        yield return StartCoroutine(gemAnimator.AnimateSwap(rectA, rectB));

        SwapGemData(gemA, gemB);
        hintSystem?.ResetTimer();

        List<Gem> matches = matchDetector.FindMatches(board, rows, columns);

        if (matches.Count > 0)
        {
            if (moveCounter != null)
                moveCounter.UseMove();

            scoreManager.ResetCombo();
            yield return StartCoroutine(ProcessMatches(matches));
        }
        else
        {
            soundManager.PlaySwap();
            yield return StartCoroutine(gemAnimator.AnimateSwap(rectA, rectB));
            SwapGemData(gemA, gemB);
        }

        CheckForValidMoves();
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
        comboCount++;

        // Announce combo
        if (comboAnnouncer != null && comboCount >= 2)
            comboAnnouncer.ShowCombo(comboCount);

        // Report combo to achievement manager
        if (AchievementManager.Instance != null)
            AchievementManager.Instance.RecordCombo(comboCount);

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

        if (scorePopup != null && allMatches.Count > 0)
        {
            RectTransform popupRect = allMatches[0].GetComponent<RectTransform>();
            if (popupRect != null)
            {
                Vector2 popupPos = popupRect.anchoredPosition;
                int points = allMatches.Count <= 3 ? 50 : allMatches.Count == 4 ? 100 : 200;
                string popupText = $"+{points}";

                if (allMatches.Count >= 4)
                    popupText += " COMBO!";

                scorePopup.ShowPopup(popupPos, popupText, Color.yellow);
            }
        }

        // Notify objective tracker for each cleared gem
        foreach (Gem gem in allMatches)
            NotifyGemCleared(gem);

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

        // Handle obstacles: chain gems survive first hit, ice on adjacent cells breaks
        List<Gem> survivingChainGems = new List<Gem>();
        foreach (Gem gem in new List<Gem>(finalTargets))
        {
            if (gem.CurrentObstacle == ObstacleType.Chain)
            {
                gem.HitObstacle(); // removes chain, gem survives
                finalTargets.Remove(gem);
                survivingChainGems.Add(gem);
                if (objectiveTracker != null)
                    objectiveTracker.OnIceBroken(); // chains count toward break objectives too
                if (AchievementManager.Instance != null)
                    AchievementManager.Instance.RecordIceBroken();
            }
        }

        // Hit ice on cells adjacent to cleared gems
        foreach (Gem gem in finalTargets)
        {
            HitAdjacentIce(gem.Row, gem.Column);
        }

        // Animate clearing
        List<Coroutine> clearAnimations = new List<Coroutine>();
        foreach (Gem gem in finalTargets)
        {
            SpawnMatchVFX(gem);
            clearAnimations.Add(StartCoroutine(gemAnimator.AnimateClear(gem)));
        }
        foreach (Coroutine c in clearAnimations) yield return c;

        // Screen shake on big combos
        if (comboCount >= 4 && vfxManager != null)
            vfxManager.TriggerScreenShake(6f + comboCount * 2f);

        // Check if any stones were destroyed by power-ups
        foreach (Gem gem in finalTargets)
        {
            CheckStoneDestruction(gem.Row, gem.Column);
        }

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
        CheckStarGemCollection();
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

    /// <summary>
    /// Hit ice blocks adjacent to a cleared position.
    /// </summary>
    private void HitAdjacentIce(int row, int col)
    {
        int[][] neighbors = { new[] {0,1}, new[] {0,-1}, new[] {1,0}, new[] {-1,0} };
        foreach (int[] dir in neighbors)
        {
            int nr = row + dir[0];
            int nc = col + dir[1];
            if (nr < 0 || nr >= rows || nc < 0 || nc >= columns) continue;

            Gem neighbor = board[nr, nc];
            if (neighbor != null && neighbor.CurrentObstacle == ObstacleType.Ice)
            {
                neighbor.HitObstacle();
                if (objectiveTracker != null)
                    objectiveTracker.OnIceBroken();
                if (AchievementManager.Instance != null)
                    AchievementManager.Instance.RecordIceBroken();
                if (soundManager != null)
                    soundManager.PlayIceBreak();
            }
        }
    }

    /// <summary>
    /// Check if power-ups destroyed a stone at given position.
    /// </summary>
    private void CheckStoneDestruction(int row, int col)
    {
        // Power-ups that hit stone cells should destroy stone
        int[][] neighbors = { new[] {0,1}, new[] {0,-1}, new[] {1,0}, new[] {-1,0} };
        foreach (int[] dir in neighbors)
        {
            int nr = row + dir[0];
            int nc = col + dir[1];
            if (nr < 0 || nr >= rows || nc < 0 || nc >= columns) continue;

            if (stoneMap != null && stoneMap[nr, nc])
            {
                // Destroy stone
                stoneMap[nr, nc] = false;

                // Remove stone visual
                float startX = -(columns - 1) * cellSize / 2f;
                float startY = -(rows - 1) * cellSize / 2f;
                Vector2 stonePos = new Vector2(startX + nc * cellSize, startY + nr * cellSize);

                foreach (Transform child in boardParent)
                {
                    if (child.name == "StoneBlock")
                    {
                        RectTransform rt = child.GetComponent<RectTransform>();
                        if (rt != null && Vector2.Distance(rt.anchoredPosition, stonePos) < 5f)
                        {
                            Destroy(child.gameObject);
                            break;
                        }
                    }
                }

                if (objectiveTracker != null)
                    objectiveTracker.OnStoneDestroyed();

                Debug.Log($"Stone destroyed at ({nr},{nc})!");
            }
        }
    }

    /// <summary>
    /// Check if any star gems reached the bottom row (row 0).
    /// </summary>
    private void CheckStarGemCollection()
    {
        if (!collectStarsActive || objectiveTracker == null) return;

        for (int col = 0; col < columns; col++)
        {
            Gem gem = board[0, col];
            if (gem != null && gem.IsStarGem)
            {
                objectiveTracker.OnStarCollected();
                // Mark it as no longer a star gem (collected)
                // The gem stays but loses its star status
            }
        }
    }

    private void NotifyGemCleared(Gem gem)
    {
        if (gem == null) return;

        // Notify objective tracker
        if (objectiveTracker != null)
            objectiveTracker.OnGemCleared(gem.Type);

        // Notify achievement manager
        if (AchievementManager.Instance != null)
            AchievementManager.Instance.RecordColorClear(gem.Type);
    }

    private void SpawnMatchVFX(Gem gem)
    {
        if (vfxManager == null || gem == null) return;

        RectTransform rect = gem.GetComponent<RectTransform>();
        if (rect != null)
        {
            int idx = Mathf.Clamp((int)gem.Type, 0, 6);
            Color[] palette = {
                new Color(1f, 0.2f, 0.42f),
                new Color(0.12f, 0.76f, 1f),
                new Color(0.18f, 1f, 0.58f),
                new Color(1f, 0.9f, 0.1f),
                new Color(0.84f, 0.42f, 1f),
                new Color(1f, 0.6f, 0.15f),
                new Color(0.15f, 0.92f, 0.92f)
            };
            vfxManager.SpawnMatchBurst(rect.anchoredPosition, palette[idx]);
        }
    }

    /// <summary>
    /// Check if valid moves exist. If not, shuffle the board.
    /// </summary>
    private void CheckForValidMoves()
    {
        if (board == null) return;

        bool hasValidMove = false;

        for (int r = 0; r < rows && !hasValidMove; r++)
        {
            for (int c = 0; c < columns && !hasValidMove; c++)
            {
                Gem gem = board[r, c];
                if (gem == null || gem.IsSwapBlocked) continue;
                if (stoneMap != null && stoneMap[r, c]) continue;

                // Check right neighbor
                if (c + 1 < columns && board[r, c + 1] != null && !board[r, c + 1].IsSwapBlocked)
                {
                    SwapGemData(gem, board[r, c + 1]);
                    if (matchDetector.FindMatches(board, rows, columns).Count > 0)
                        hasValidMove = true;
                    SwapGemData(board[r, c], board[r, c + 1]); // swap back
                }

                // Check upper neighbor
                if (!hasValidMove && r + 1 < rows && board[r + 1, c] != null && !board[r + 1, c].IsSwapBlocked)
                {
                    SwapGemData(gem, board[r + 1, c]);
                    if (matchDetector.FindMatches(board, rows, columns).Count > 0)
                        hasValidMove = true;
                    SwapGemData(board[r, c], board[r + 1, c]); // swap back
                }
            }
        }

        if (!hasValidMove)
        {
            Debug.Log("No valid moves! Shuffling board...");
            StartCoroutine(ShuffleBoard());
        }
    }

    private IEnumerator ShuffleBoard()
    {
        if (soundManager != null)
            soundManager.PlayShuffle();

        // Animate gems shrinking
        List<Gem> allGems = new List<Gem>();
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                if (board[r, c] != null)
                    allGems.Add(board[r, c]);
            }
        }

        // Scale down
        float shrinkTime = 0.25f;
        float elapsed = 0f;
        while (elapsed < shrinkTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / shrinkTime;
            float scale = Mathf.Lerp(1f, 0.3f, t);
            foreach (Gem g in allGems)
            {
                if (g != null)
                    g.transform.localScale = Vector3.one * scale;
            }
            yield return null;
        }

        // Randomize gem types
        foreach (Gem g in allGems)
        {
            if (g != null)
            {
                Gem.GemType newType = (Gem.GemType)Random.Range(0, gemTypeCount);
                g.Initialize(newType, g.Row, g.Column);
            }
        }

        // Clear any instant matches
        ClearStartingMatches();

        // Scale back up
        elapsed = 0f;
        float growTime = 0.25f;
        // re-gather gems after clearing matches may have changed them
        allGems.Clear();
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                if (board[r, c] != null)
                    allGems.Add(board[r, c]);
            }
        }

        while (elapsed < growTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / growTime;
            float scale = Mathf.Lerp(0.3f, 1f, t);
            foreach (Gem g in allGems)
            {
                if (g != null)
                    g.transform.localScale = Vector3.one * scale;
            }
            yield return null;
        }

        foreach (Gem g in allGems)
        {
            if (g != null)
                g.transform.localScale = Vector3.one;
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
                // Skip stone cells
                if (stoneMap != null && stoneMap[row, col])
                {
                    emptyRow = -1; // reset — gems can't fall through stones
                    continue;
                }

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
                    if (rect != null)
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
                if (stoneMap != null && stoneMap[row, col]) continue; // skip stone cells
                if (board[row, col] == null)
                {
                    SpawnGemAt(row, col);
                }
            }
        }
    }

    private struct PowerUpSpawn
    {
        public int row;
        public int column;
        public PowerUpType type;
        public Gem.GemType gemType;
    }
}