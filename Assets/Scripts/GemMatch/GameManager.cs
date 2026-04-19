// File: Assets/Scripts/GemMatch/GameManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameTimer gameTimer;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private GemInputHandler inputHandler;
    [SerializeField] private GemBoard gemBoard;
    [SerializeField] private MoveCounter moveCounter;
    [SerializeField] private LevelSelectUI levelSelectUI;
    [SerializeField] private ObjectiveTracker objectiveTracker;
    [SerializeField] private VFXManager vfxManager;
    [SerializeField] private ComboAnnouncer comboAnnouncer;
    [SerializeField] private SoundManager soundManager;
    [SerializeField] private ChallengeMode challengeMode;
    [SerializeField] private ReplayIncentiveUI replayIncentive;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI starCoinsEarnedText;
    [SerializeField] private TextMeshProUGUI levelResultTitle;
    [SerializeField] private TextMeshProUGUI starsDisplayText;
    [SerializeField] private TextMeshProUGUI objectiveBannerText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button levelSelectButton;
    [SerializeField] private Button backToHubButton;

    private int lastEarnedCoins;
    private int lastStarRating;
    private bool isChallengeMode;

    private void Awake()
    {
        EnsureReferences();
    }

    private void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        HideGameElements();

        if (levelSelectUI != null)
            levelSelectUI.Show();
    }

    /// <summary>
    /// Called by LevelSelectUI when a level is selected.
    /// </summary>
    public void StartLevel()
    {
        isChallengeMode = false;
        LevelData level = LevelManager.Instance != null ? LevelManager.Instance.CurrentLevel : null;
        if (level == null)
        {
            level = LevelData.GetLevel(1);
            Debug.LogWarning("No level selected, defaulting to Level 1.");
        }

        StartLevelInternal(level);
    }

    /// <summary>
    /// Start Challenge Mode (endless waves).
    /// </summary>
    public void StartChallengeMode()
    {
        isChallengeMode = true;

        // Ensure ChallengeMode exists
        if (challengeMode == null)
        {
            GameObject cmObj = new GameObject("ChallengeMode", typeof(ChallengeMode));
            challengeMode = cmObj.GetComponent<ChallengeMode>();
        }

        challengeMode.StartChallenge();
        LevelData waveData = challengeMode.GetCurrentWaveData();

        StartLevelInternal(waveData);
    }

    private void StartLevelInternal(LevelData level)
    {
        // Hide level select, show game elements
        if (levelSelectUI != null)
            levelSelectUI.Hide();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Reset achievement session stats
        if (AchievementManager.Instance != null)
            AchievementManager.Instance.ResetSessionStats();

        // Configure board
        if (gemBoard != null)
        {
            gemBoard.ConfigureFromLevel(level);
            gemBoard.SetVisible(true);
            gemBoard.InitializeBoard();
        }

        // Configure timer or move counter
        if (level.constraintType == LevelConstraintType.Time)
        {
            if (gameTimer != null)
                gameTimer.ConfigureAndStart(level.timeLimit);
            if (moveCounter != null)
                moveCounter.SetInactive();
        }
        else
        {
            if (gameTimer != null)
                gameTimer.SetDisabled();
            if (moveCounter != null)
                moveCounter.Initialize(level.moveLimit);
        }

        // Configure score
        if (scoreManager != null)
        {
            scoreManager.ConfigureForLevel(level);
            scoreManager.SetVisible(true);
        }

        // Configure objective tracker
        if (objectiveTracker != null)
        {
            objectiveTracker.Initialize(level.objective);
            objectiveTracker.SetVisible(level.objective != null &&
                                        level.objective.objectiveType != ObjectiveType.Score);
        }

        // Enable input
        if (inputHandler != null)
            inputHandler.SetInteractable(true);

        // Show objective banner
        ShowObjectiveBanner(level);

        // Start background music
        if (soundManager != null)
            soundManager.PlayLevelMusic(level.levelNumber);

        // Subscribe to events
        if (gameTimer != null)
            gameTimer.OnTimeUp += HandleLevelEnd;
        if (moveCounter != null)
            moveCounter.OnMovesExhausted += HandleLevelEnd;

        string modeLabel = isChallengeMode ? $"Challenge Wave {challengeMode.CurrentWave}" : $"Level {level.levelNumber}";
        Debug.Log($"Starting {modeLabel}: {level.GetObjectiveText()}");
    }

    private void HandleLevelEnd()
    {
        // Unsubscribe events
        if (gameTimer != null)
            gameTimer.OnTimeUp -= HandleLevelEnd;
        if (moveCounter != null)
            moveCounter.OnMovesExhausted -= HandleLevelEnd;

        if (gameTimer != null)
            gameTimer.StopTimer();

        if (inputHandler != null)
            inputHandler.SetInteractable(false);

        // Stop music
        if (soundManager != null)
            soundManager.StopMusic();

        int finalScore = scoreManager != null ? scoreManager.GetScore() : 0;
        lastStarRating = scoreManager != null ? scoreManager.GetStarRating() : 0;
        float timeRemaining = gameTimer != null ? gameTimer.GetTimeRemaining() : 0f;

        // Check objective completion
        bool objectiveMet = objectiveTracker != null ? objectiveTracker.IsObjectiveMet() : true;

        // Level passes only if star rating >= 1 AND objective is met
        bool levelPassed = lastStarRating >= 1 && objectiveMet;

        // Handle challenge mode
        if (isChallengeMode)
        {
            HandleChallengeModeEnd(levelPassed, finalScore);
            return;
        }

        // Save progress for normal levels
        if (levelPassed && LevelManager.Instance != null)
        {
            lastEarnedCoins = LevelManager.Instance.SaveStars(lastStarRating);
            LevelManager.Instance.UnlockNextLevel();

            if (soundManager != null)
                soundManager.PlayLevelComplete();

            if (vfxManager != null)
                vfxManager.SpawnConfetti();
        }
        else
        {
            lastEarnedCoins = 0;
            if (soundManager != null)
                soundManager.PlayGameOver();
        }

        // Check achievements
        int challengeWave = challengeMode != null ? challengeMode.CurrentWave : 0;
        if (AchievementManager.Instance != null)
            AchievementManager.Instance.CheckAllConditions(lastStarRating, timeRemaining, challengeWave);

        // Update game over panel
        LevelData level = LevelManager.Instance != null ? LevelManager.Instance.CurrentLevel : null;
        int levelNum = level != null ? level.levelNumber : 0;

        if (levelResultTitle != null)
        {
            if (levelPassed)
            {
                levelResultTitle.text = $"Level {levelNum} Complete!";
                levelResultTitle.color = new Color(0.3f, 1f, 0.5f, 1f);
            }
            else
            {
                string failReason = !objectiveMet ? " (Objective Incomplete)" : "";
                levelResultTitle.text = $"Level {levelNum} Failed{failReason}";
                levelResultTitle.color = new Color(1f, 0.35f, 0.35f, 1f);
            }
        }

        if (finalScoreText != null)
            finalScoreText.text = $"Score: {finalScore}";

        if (starsDisplayText != null)
        {
            string starStr = "";
            for (int i = 0; i < 3; i++)
            {
                if (i < lastStarRating)
                    starStr += "<color=#FFD91A><size=64>*</size></color>";
                else
                    starStr += "<color=#5A4670><size=64>*</size></color>";
                if (i < 2) starStr += "  ";
            }
            starsDisplayText.text = starStr;
            starsDisplayText.richText = true;
        }

        if (starCoinsEarnedText != null)
        {
            if (lastEarnedCoins > 0)
                starCoinsEarnedText.text = $"StarCoins Earned: +{lastEarnedCoins}";
            else if (levelPassed)
                starCoinsEarnedText.text = "Already earned max StarCoins!";
            else
                starCoinsEarnedText.text = "No StarCoins earned";
        }

        if (nextLevelButton != null)
        {
            bool canGoNext = levelPassed && levelNum < 10;
            nextLevelButton.gameObject.SetActive(canGoNext);
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Debug.Log($"Level {levelNum} ended! Score: {finalScore} | Stars: {lastStarRating} | Coins: {lastEarnedCoins}");
    }

    private void HandleChallengeModeEnd(bool wavePassed, int finalScore)
    {
        if (wavePassed && challengeMode != null)
        {
            // Advance to next wave
            int nextWave = challengeMode.AdvanceWave();

            if (soundManager != null)
                soundManager.PlayLevelComplete();

            if (vfxManager != null)
                vfxManager.SpawnConfetti();

            // Check achievement
            if (AchievementManager.Instance != null)
                AchievementManager.Instance.CheckAllConditions(0, 0, nextWave);

            // Show brief wave transition, then start next wave
            StartCoroutine(ChallengeWaveTransition(nextWave));
        }
        else
        {
            // Challenge failed
            if (challengeMode != null)
                challengeMode.EndChallenge();

            if (soundManager != null)
                soundManager.PlayGameOver();

            if (AchievementManager.Instance != null)
                AchievementManager.Instance.CheckAllConditions(0, 0, challengeMode != null ? challengeMode.CurrentWave : 0);

            // Show game over with challenge stats
            if (levelResultTitle != null)
            {
                int wave = challengeMode != null ? challengeMode.CurrentWave : 0;
                levelResultTitle.text = $"Challenge Over! Wave {wave}";
                levelResultTitle.color = new Color(1f, 0.35f, 0.35f, 1f);
            }

            if (finalScoreText != null)
                finalScoreText.text = $"Score: {finalScore}";

            if (starsDisplayText != null)
                starsDisplayText.text = $"Best Wave: {ChallengeMode.GetBestWave()}";

            if (starCoinsEarnedText != null)
                starCoinsEarnedText.text = "";

            if (nextLevelButton != null)
                nextLevelButton.gameObject.SetActive(false);

            if (gameOverPanel != null)
                gameOverPanel.SetActive(true);
        }
    }

    private IEnumerator ChallengeWaveTransition(int nextWave)
    {
        // Show wave banner
        if (objectiveBannerText != null)
        {
            objectiveBannerText.text = $"Wave {nextWave}!";
            objectiveBannerText.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(2f);

        if (objectiveBannerText != null)
            objectiveBannerText.gameObject.SetActive(false);

        // Start next wave
        if (gemBoard != null)
            gemBoard.ClearBoard();

        LevelData waveData = challengeMode.GetCurrentWaveData();
        StartLevelInternal(waveData);
    }

    private void ShowObjectiveBanner(LevelData level)
    {
        if (objectiveBannerText == null) return;

        string label = isChallengeMode
            ? $"Wave {challengeMode.CurrentWave} — {level.GetObjectiveText()}"
            : $"Level {level.levelNumber} — {level.GetObjectiveText()}";

        objectiveBannerText.text = label;
        objectiveBannerText.gameObject.SetActive(true);

        StartCoroutine(HideBannerAfterDelay(3f));
    }

    private IEnumerator HideBannerAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (objectiveBannerText != null)
            objectiveBannerText.gameObject.SetActive(false);
    }

    private void HideGameElements()
    {
        if (gemBoard != null)
            gemBoard.SetVisible(false);
        if (scoreManager != null)
            scoreManager.SetVisible(false);
        if (gameTimer != null)
            gameTimer.SetDisabled();
        if (moveCounter != null)
            moveCounter.SetInactive();
        if (objectiveBannerText != null)
            objectiveBannerText.gameObject.SetActive(false);
        if (objectiveTracker != null)
            objectiveTracker.SetVisible(false);
        if (replayIncentive != null)
            replayIncentive.SetVisible(false);
    }

    private void EnsureReferences()
    {
        if (gameTimer == null)
            gameTimer = FindFirstObjectByType<GameTimer>();
        if (scoreManager == null)
            scoreManager = FindFirstObjectByType<ScoreManager>();
        if (inputHandler == null)
            inputHandler = FindFirstObjectByType<GemInputHandler>();
        if (gemBoard == null)
            gemBoard = FindFirstObjectByType<GemBoard>();
        if (moveCounter == null)
            moveCounter = FindFirstObjectByType<MoveCounter>();
        if (soundManager == null)
            soundManager = FindFirstObjectByType<SoundManager>();

        // Ensure singletons
        if (LevelManager.Instance == null)
            new GameObject("LevelManager", typeof(LevelManager));
        if (AchievementManager.Instance == null)
            new GameObject("AchievementManager", typeof(AchievementManager));

        // Ensure VFX, ComboAnnouncer, ObjectiveTracker
        if (vfxManager == null)
            vfxManager = FindFirstObjectByType<VFXManager>();
        if (vfxManager == null)
            vfxManager = new GameObject("VFXManager", typeof(VFXManager)).GetComponent<VFXManager>();

        if (comboAnnouncer == null)
            comboAnnouncer = FindFirstObjectByType<ComboAnnouncer>();
        if (comboAnnouncer == null)
            comboAnnouncer = new GameObject("ComboAnnouncer", typeof(ComboAnnouncer)).GetComponent<ComboAnnouncer>();

        if (objectiveTracker == null)
            objectiveTracker = FindFirstObjectByType<ObjectiveTracker>();
        if (objectiveTracker == null)
            objectiveTracker = new GameObject("ObjectiveTracker", typeof(ObjectiveTracker)).GetComponent<ObjectiveTracker>();

        if (replayIncentive == null)
            replayIncentive = FindFirstObjectByType<ReplayIncentiveUI>();

        // Level select
        if (levelSelectUI == null)
            levelSelectUI = FindFirstObjectByType<LevelSelectUI>();
        if (levelSelectUI == null)
        {
            GameObject lsObj = new GameObject("LevelSelectUI", typeof(LevelSelectUI));
            levelSelectUI = lsObj.GetComponent<LevelSelectUI>();
        }

        // Build Game Over Panel
        Canvas canvas = UIHierarchyBuilder.GetOrCreateCanvas("UICanvas");
        RectTransform panelRect = UIHierarchyBuilder.GetOrCreatePanel(
            canvas.transform,
            "GameOverPanel",
            Vector2.zero,
            new Vector2(720f, 520f),
            false);

        StyleGameOverPanel(panelRect);
        gameOverPanel = panelRect.gameObject;

        levelResultTitle = UIHierarchyBuilder.GetOrCreateTMPText(
            panelRect,
            "LevelResultTitle",
            "Level Complete!",
            new Vector2(0f, 170f),
            new Vector2(600f, 80f),
            48,
            TMPro.TextAlignmentOptions.Center,
            new Color(0.3f, 1f, 0.5f, 1f));

        starsDisplayText = UIHierarchyBuilder.GetOrCreateTMPText(
            panelRect,
            "StarsDisplayText",
            "* * *",
            new Vector2(0f, 100f),
            new Vector2(400f, 80f),
            60,
            TMPro.TextAlignmentOptions.Center,
            new Color(1f, 0.85f, 0.1f, 1f));

        finalScoreText = UIHierarchyBuilder.GetOrCreateTMPText(
            panelRect,
            "FinalScoreText",
            "Score: 0",
            new Vector2(0f, 40f),
            new Vector2(560f, 70f),
            42,
            TMPro.TextAlignmentOptions.Center,
            Color.white);

        starCoinsEarnedText = UIHierarchyBuilder.GetOrCreateTMPText(
            panelRect,
            "StarCoinsEarnedText",
            "StarCoins Earned: 0",
            new Vector2(0f, -20f),
            new Vector2(560f, 60f),
            32,
            TMPro.TextAlignmentOptions.Center,
            new Color(1f, 0.92f, 0.1f, 1f));

        retryButton = GetOrCreateButton(panelRect, "RetryButton", "Retry Level", new Vector2(-130f, -110f));
        nextLevelButton = GetOrCreateButton(panelRect, "NextLevelButton", "Next Level", new Vector2(130f, -110f));
        levelSelectButton = GetOrCreateButton(panelRect, "LevelSelectButton", "Level Select", new Vector2(-130f, -185f));
        backToHubButton = GetOrCreateButton(panelRect, "BackToHubButton", "Back To Hub", new Vector2(130f, -185f));

        if (retryButton != null)
        {
            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(RetryLevel);
        }

        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.RemoveAllListeners();
            nextLevelButton.onClick.AddListener(GoToNextLevel);
        }

        if (levelSelectButton != null)
        {
            levelSelectButton.onClick.RemoveAllListeners();
            levelSelectButton.onClick.AddListener(GoToLevelSelect);
        }

        if (backToHubButton != null)
        {
            backToHubButton.onClick.RemoveAllListeners();
            backToHubButton.onClick.AddListener(GoToHub);
        }

        objectiveBannerText = UIHierarchyBuilder.GetOrCreateTMPText(
            canvas.transform,
            "ObjectiveBannerText",
            "",
            new Vector2(0f, 350f),
            new Vector2(800f, 80f),
            36,
            TMPro.TextAlignmentOptions.Center,
            new Color(1f, 0.95f, 0.75f, 1f));
        objectiveBannerText.gameObject.SetActive(false);
    }

    private Button GetOrCreateButton(Transform parent, string objectName, string label, Vector2 anchoredPosition)
    {
        Transform existing = parent.Find(objectName);
        GameObject buttonObject;

        if (existing != null)
        {
            buttonObject = existing.gameObject;
        }
        else
        {
            buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
        }

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(220f, 55f);

        Image background = buttonObject.GetComponent<Image>();
        background.color = new Color(0.83f, 0.28f, 0.96f, 0.95f);

        Button button = buttonObject.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(1f, 1f, 1f, 1f);
        colors.highlightedColor = new Color(1f, 0.95f, 1f, 1f);
        colors.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.7f, 0.7f, 0.7f, 0.6f);
        button.colors = colors;

        Transform labelTransform = buttonObject.transform.Find("Label");
        TextMeshProUGUI buttonText;

        if (labelTransform != null)
        {
            buttonText = labelTransform.GetComponent<TextMeshProUGUI>();
        }
        else
        {
            GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(buttonObject.transform, false);
            buttonText = labelObject.GetComponent<TextMeshProUGUI>();
        }

        if (buttonText != null)
        {
            RectTransform textRect = buttonText.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            TMP_FontAsset preferredFont = UIHierarchyBuilder.GetPreferredFont();
            if (preferredFont != null)
                buttonText.font = preferredFont;
            else if (TMP_Settings.defaultFontAsset != null)
                buttonText.font = TMP_Settings.defaultFontAsset;

            buttonText.text = label;
            buttonText.fontSize = 24;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = Color.white;
            buttonText.raycastTarget = false;
            buttonText.textWrappingMode = TextWrappingModes.NoWrap;
        }

        return button;
    }

    private void StyleGameOverPanel(RectTransform panelRect)
    {
        if (panelRect == null) return;

        Image panelImage = panelRect.GetComponent<Image>();
        if (panelImage != null)
            panelImage.color = new Color(0.13f, 0.08f, 0.24f, 0.97f);

        Image shadow = GetOrCreateDecorLayer(panelRect, "PanelShadow", new Vector2(0f, -14f), new Vector2(746f, 546f), new Color(0.03f, 0.01f, 0.08f, 0.58f));
        if (shadow != null)
            shadow.transform.SetAsFirstSibling();

        Image glow = GetOrCreateDecorLayer(panelRect, "PanelGlow", new Vector2(0f, 4f), new Vector2(734f, 534f), new Color(0.48f, 0.22f, 0.86f, 0.38f));
        if (glow != null)
            glow.transform.SetAsFirstSibling();

        Image innerSurface = GetOrCreateDecorLayer(panelRect, "PanelInnerSurface", new Vector2(0f, -8f), new Vector2(682f, 406f), new Color(0.25f, 0.13f, 0.46f, 0.66f));
        if (innerSurface != null)
            innerSurface.transform.SetAsFirstSibling();

        Image topBand = GetOrCreateDecorLayer(panelRect, "PanelTopBand", new Vector2(0f, 206f), new Vector2(660f, 62f), new Color(0.80f, 0.34f, 0.98f, 0.58f));
        if (topBand != null)
            topBand.transform.SetAsFirstSibling();
    }

    private Image GetOrCreateDecorLayer(RectTransform parent, string objectName, Vector2 anchoredPosition, Vector2 size, Color color)
    {
        Transform existing = parent.Find(objectName);
        GameObject layerObject;

        if (existing != null)
        {
            layerObject = existing.gameObject;
        }
        else
        {
            layerObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            layerObject.transform.SetParent(parent, false);
        }

        RectTransform layerRect = layerObject.GetComponent<RectTransform>();
        layerRect.anchorMin = new Vector2(0.5f, 0.5f);
        layerRect.anchorMax = new Vector2(0.5f, 0.5f);
        layerRect.pivot = new Vector2(0.5f, 0.5f);
        layerRect.anchoredPosition = anchoredPosition;
        layerRect.sizeDelta = size;

        Image layerImage = layerObject.GetComponent<Image>();
        layerImage.color = color;
        layerImage.raycastTarget = false;
        return layerImage;
    }

    public void RetryLevel()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (gemBoard != null)
            gemBoard.ClearBoard();

        if (isChallengeMode)
            StartChallengeMode();
        else
            StartLevel();
    }

    public void GoToNextLevel()
    {
        if (LevelManager.Instance == null) return;

        int nextLevel = LevelManager.Instance.CurrentLevel.levelNumber + 1;
        if (nextLevel > 10)
        {
            GoToLevelSelect();
            return;
        }

        LevelManager.Instance.SelectLevel(nextLevel);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (gemBoard != null)
            gemBoard.ClearBoard();

        StartLevel();
    }

    public void GoToLevelSelect()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (gemBoard != null)
        {
            gemBoard.ClearBoard();
            gemBoard.SetVisible(false);
        }

        if (soundManager != null)
            soundManager.StopMusic();

        HideGameElements();

        if (levelSelectUI != null)
            levelSelectUI.Show();
    }

    public void GoToHub()
    {
        if (soundManager != null)
            soundManager.StopMusic();

        Debug.Log("Returning to Hub with " + lastEarnedCoins + " StarCoins");
        SceneManager.LoadScene("MainMenuScene");
    }

    private void OnDestroy()
    {
        if (gameTimer != null)
            gameTimer.OnTimeUp -= HandleLevelEnd;
        if (moveCounter != null)
            moveCounter.OnMovesExhausted -= HandleLevelEnd;
    }
}