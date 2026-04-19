// File: Assets/Scripts/GemMatch/LevelSelectUI.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Builds and manages the level-select screen with 10 level buttons,
/// star displays, lock/unlock states, and total StarCoin display.
/// </summary>
public class LevelSelectUI : MonoBehaviour
{
    private GameObject levelSelectPanel;
    private Button[] levelButtons = new Button[10];

    // Purple/dark theme colors matching the existing game style
    private static readonly Color panelColor = new Color(0.10f, 0.06f, 0.20f, 0.97f);
    private static readonly Color unlockedButtonColor = new Color(0.60f, 0.20f, 0.90f, 0.95f);
    private static readonly Color lockedButtonColor = new Color(0.25f, 0.18f, 0.32f, 0.80f);
    private static readonly Color highlightButtonColor = new Color(0.75f, 0.35f, 1.0f, 1.0f);
    private static readonly Color starFilledColor = new Color(1f, 0.85f, 0.1f, 1f);
    private static readonly Color starEmptyColor = new Color(0.4f, 0.3f, 0.5f, 0.6f);

    private void Awake()
    {
        EnsureLevelManager();
    }

    private void Start()
    {
        BuildLevelSelectUI();
        Show();
    }

    /// <summary>
    /// Ensures a LevelManager instance exists in the scene.
    /// </summary>
    private void EnsureLevelManager()
    {
        if (LevelManager.Instance == null)
        {
            GameObject managerObj = new GameObject("LevelManager", typeof(LevelManager));
            // LevelManager.Awake() will handle DontDestroyOnLoad
        }
    }

    /// <summary>
    /// Shows the level select screen and refreshes button states.
    /// </summary>
    public void Show()
    {
        if (levelSelectPanel != null)
        {
            RefreshButtons();
            levelSelectPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Hides the level select screen.
    /// </summary>
    public void Hide()
    {
        if (levelSelectPanel != null)
            levelSelectPanel.SetActive(false);
    }

    public bool IsVisible()
    {
        return levelSelectPanel != null && levelSelectPanel.activeSelf;
    }

    private void BuildLevelSelectUI()
    {
        Canvas canvas = UIHierarchyBuilder.GetOrCreateCanvas("UICanvas");

        // Main panel — fullscreen overlay
        Transform existingPanel = canvas.transform.Find("LevelSelectPanel");
        if (existingPanel != null)
        {
            levelSelectPanel = existingPanel.gameObject;
            // Buttons already exist, just grab references
            for (int i = 0; i < 10; i++)
            {
                Transform btnTransform = levelSelectPanel.transform.Find($"LevelButton_{i + 1}");
                if (btnTransform != null)
                    levelButtons[i] = btnTransform.GetComponent<Button>();
            }
            return;
        }

        GameObject panelObj = new GameObject("LevelSelectPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panelObj.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image panelImage = panelObj.GetComponent<Image>();
        panelImage.color = panelColor;
        panelImage.raycastTarget = true;

        levelSelectPanel = panelObj;

        // Title
        UIHierarchyBuilder.GetOrCreateTMPText(
            panelRect,
            "LevelSelectTitle",
            "SELECT LEVEL",
            new Vector2(0f, 340f),
            new Vector2(600f, 100f),
            64,
            TextAlignmentOptions.Center,
            new Color(0.90f, 0.75f, 1f, 1f));

        // Decorative subtitle
        UIHierarchyBuilder.GetOrCreateTMPText(
            panelRect,
            "LevelSelectSubtitle",
            "Match gems to earn StarCoins!",
            new Vector2(0f, 280f),
            new Vector2(600f, 60f),
            28,
            TextAlignmentOptions.Center,
            new Color(0.75f, 0.60f, 0.90f, 0.80f));

        // Total stars display
        int totalStars = LevelManager.Instance != null ? LevelManager.Instance.GetTotalStars() : 0;
        UIHierarchyBuilder.GetOrCreateTMPText(
            panelRect,
            "TotalStarsText",
            $"Total Stars: {totalStars} / 30",
            new Vector2(0f, 230f),
            new Vector2(400f, 50f),
            26,
            TextAlignmentOptions.Center,
            starFilledColor);

        // Total coins display
        int totalCoins = LevelManager.Instance != null ? LevelManager.Instance.GetTotalStarCoins() : 0;
        UIHierarchyBuilder.GetOrCreateTMPText(
            panelRect,
            "TotalCoinsText",
            $"StarCoins: {totalCoins}",
            new Vector2(0f, 195f),
            new Vector2(400f, 40f),
            22,
            TextAlignmentOptions.Center,
            new Color(1f, 0.92f, 0.1f, 0.9f));

        // Build 10 level buttons in a 2x5 grid
        float buttonWidth = 160f;
        float buttonHeight = 160f;
        float spacingX = 180f;
        float spacingY = 195f;
        float startX = -2f * spacingX;
        float startY = 80f;

        for (int i = 0; i < 10; i++)
        {
            int levelNumber = i + 1;
            int gridRow = i / 5;
            int gridCol = i % 5;

            float x = startX + gridCol * spacingX;
            float y = startY - gridRow * spacingY;

            levelButtons[i] = CreateLevelButton(panelRect, levelNumber, new Vector2(x, y), new Vector2(buttonWidth, buttonHeight));
        }

        // Star gate dividers
        CreateStarGateLabel(panelRect, "StarGate1",
            new Vector2(startX - 30f, startY - spacingY * 0.40f),
            "4 Stars Required", 4);

        CreateStarGateLabel(panelRect, "StarGate2",
            new Vector2(startX - 30f, startY - spacingY * 1.40f),
            "10 Stars Required", 10);

        // Challenge Mode button
        Button challengeBtn = CreateSimpleButton(panelRect, "ChallengeModeBtn", "Challenge Mode",
            new Vector2(0f, -260f), new Vector2(280f, 60f));
        challengeBtn.onClick.AddListener(() =>
        {
            if (ChallengeMode.IsUnlocked())
            {
                Hide();
                GameManager gm = FindFirstObjectByType<GameManager>();
                if (gm != null)
                    gm.StartChallengeMode();
            }
            else
            {
                Debug.Log("Challenge Mode locked — complete all 10 levels first!");
            }
        });

        // Back button
        Button backBtn = CreateSimpleButton(panelRect, "BackToHubBtn", "Back to Hub",
            new Vector2(0f, -340f), new Vector2(240f, 60f));
        backBtn.onClick.AddListener(() =>
        {
            Debug.Log("Returning to hub from level select");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        });
    }

    private Button CreateLevelButton(RectTransform parent, int levelNumber, Vector2 position, Vector2 size)
    {
        string objName = $"LevelButton_{levelNumber}";

        GameObject btnObj = new GameObject(objName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(parent, false);

        RectTransform rect = btnObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image bg = btnObj.GetComponent<Image>();
        bg.color = lockedButtonColor;

        Button btn = btnObj.GetComponent<Button>();

        // Level number text
        GameObject numObj = new GameObject("LevelNum", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        numObj.transform.SetParent(btnObj.transform, false);
        RectTransform numRect = numObj.GetComponent<RectTransform>();
        numRect.anchorMin = new Vector2(0.5f, 0.5f);
        numRect.anchorMax = new Vector2(0.5f, 0.5f);
        numRect.pivot = new Vector2(0.5f, 0.5f);
        numRect.anchoredPosition = new Vector2(0f, 15f);
        numRect.sizeDelta = new Vector2(120f, 70f);

        TextMeshProUGUI numText = numObj.GetComponent<TextMeshProUGUI>();
        TMP_FontAsset font = UIHierarchyBuilder.GetPreferredFont();
        if (font != null) numText.font = font;
        else if (TMP_Settings.defaultFontAsset != null) numText.font = TMP_Settings.defaultFontAsset;
        numText.text = levelNumber.ToString();
        numText.fontSize = 48;
        numText.alignment = TextAlignmentOptions.Center;
        numText.color = Color.white;
        numText.raycastTarget = false;

        // Star display (3 stars below the number)
        GameObject starsObj = new GameObject("Stars", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        starsObj.transform.SetParent(btnObj.transform, false);
        RectTransform starsRect = starsObj.GetComponent<RectTransform>();
        starsRect.anchorMin = new Vector2(0.5f, 0.5f);
        starsRect.anchorMax = new Vector2(0.5f, 0.5f);
        starsRect.pivot = new Vector2(0.5f, 0.5f);
        starsRect.anchoredPosition = new Vector2(0f, -40f);
        starsRect.sizeDelta = new Vector2(120f, 40f);

        TextMeshProUGUI starsText = starsObj.GetComponent<TextMeshProUGUI>();
        if (font != null) starsText.font = font;
        else if (TMP_Settings.defaultFontAsset != null) starsText.font = TMP_Settings.defaultFontAsset;
        starsText.fontSize = 24;
        starsText.alignment = TextAlignmentOptions.Center;
        starsText.raycastTarget = false;
        starsText.enableVertexGradient = false;

        // Constraint label (e.g. "60s" or "30 moves")
        LevelData data = LevelData.GetLevel(levelNumber);
        string constraintLabel = "";
        if (data != null)
        {
            constraintLabel = data.constraintType == LevelConstraintType.Time
                ? $"{(int)data.timeLimit}s"
                : $"{data.moveLimit} moves";
        }

        GameObject constraintObj = new GameObject("Constraint", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        constraintObj.transform.SetParent(btnObj.transform, false);
        RectTransform constraintRect = constraintObj.GetComponent<RectTransform>();
        constraintRect.anchorMin = new Vector2(0.5f, 0.5f);
        constraintRect.anchorMax = new Vector2(0.5f, 0.5f);
        constraintRect.pivot = new Vector2(0.5f, 0.5f);
        constraintRect.anchoredPosition = new Vector2(0f, -62f);
        constraintRect.sizeDelta = new Vector2(140f, 30f);

        TextMeshProUGUI constraintText = constraintObj.GetComponent<TextMeshProUGUI>();
        if (font != null) constraintText.font = font;
        else if (TMP_Settings.defaultFontAsset != null) constraintText.font = TMP_Settings.defaultFontAsset;
        constraintText.text = constraintLabel;
        constraintText.fontSize = 18;
        constraintText.alignment = TextAlignmentOptions.Center;
        constraintText.color = new Color(0.80f, 0.70f, 0.95f, 0.75f);
        constraintText.raycastTarget = false;

        // Lock icon (shown when locked)
        GameObject lockObj = new GameObject("LockIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        lockObj.transform.SetParent(btnObj.transform, false);
        RectTransform lockRect = lockObj.GetComponent<RectTransform>();
        lockRect.anchorMin = new Vector2(0.5f, 0.5f);
        lockRect.anchorMax = new Vector2(0.5f, 0.5f);
        lockRect.pivot = new Vector2(0.5f, 0.5f);
        lockRect.anchoredPosition = Vector2.zero;
        lockRect.sizeDelta = new Vector2(80f, 80f);

        TextMeshProUGUI lockText = lockObj.GetComponent<TextMeshProUGUI>();
        if (font != null) lockText.font = font;
        else if (TMP_Settings.defaultFontAsset != null) lockText.font = TMP_Settings.defaultFontAsset;
        lockText.text = "LOCKED";
        lockText.fontSize = 20;
        lockText.alignment = TextAlignmentOptions.Center;
        lockText.color = new Color(0.6f, 0.5f, 0.7f, 0.7f);
        lockText.raycastTarget = false;

        // Button click handler
        int capturedLevel = levelNumber;
        btn.onClick.AddListener(() => OnLevelButtonClicked(capturedLevel));

        // Configure button colors
        ColorBlock colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.95f, 1f, 1f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        btn.colors = colors;

        return btn;
    }

    private Button CreateSimpleButton(RectTransform parent, string name, string label, Vector2 position, Vector2 size)
    {
        GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(parent, false);

        RectTransform rect = btnObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image bg = btnObj.GetComponent<Image>();
        bg.color = new Color(0.50f, 0.18f, 0.75f, 0.90f);

        Button btn = btnObj.GetComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.95f, 1f, 1f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        btn.colors = colors;

        // Label
        GameObject labelObj = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        labelObj.transform.SetParent(btnObj.transform, false);
        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        TextMeshProUGUI labelText = labelObj.GetComponent<TextMeshProUGUI>();
        TMP_FontAsset font = UIHierarchyBuilder.GetPreferredFont();
        if (font != null) labelText.font = font;
        else if (TMP_Settings.defaultFontAsset != null) labelText.font = TMP_Settings.defaultFontAsset;
        labelText.text = label;
        labelText.fontSize = 26;
        labelText.alignment = TextAlignmentOptions.Center;
        labelText.color = Color.white;
        labelText.raycastTarget = false;

        return btn;
    }

    private void RefreshButtons()
    {
        for (int i = 0; i < 10; i++)
        {
            int levelNumber = i + 1;
            if (levelButtons[i] == null) continue;

            bool unlocked = LevelManager.Instance != null && LevelManager.Instance.IsLevelUnlocked(levelNumber);
            int stars = LevelManager.Instance != null ? LevelManager.Instance.GetStarsForLevel(levelNumber) : 0;

            Image bg = levelButtons[i].GetComponent<Image>();
            levelButtons[i].interactable = unlocked;

            if (unlocked)
            {
                bg.color = unlockedButtonColor;

                // Show level number and stars, hide lock
                Transform numT = levelButtons[i].transform.Find("LevelNum");
                if (numT != null) numT.gameObject.SetActive(true);

                Transform starsT = levelButtons[i].transform.Find("Stars");
                if (starsT != null)
                {
                    starsT.gameObject.SetActive(true);
                    TextMeshProUGUI starsText = starsT.GetComponent<TextMeshProUGUI>();
                    if (starsText != null)
                    {
                        string starDisplay = "";
                        for (int s = 0; s < 3; s++)
                        {
                            starDisplay += s < stars ? "<color=#FFD91A>*</color>" : "<color=#5A4670>*</color>";
                            if (s < 2) starDisplay += " ";
                        }
                        starsText.text = starDisplay;
                        starsText.richText = true;
                    }
                }

                Transform constraintT = levelButtons[i].transform.Find("Constraint");
                if (constraintT != null) constraintT.gameObject.SetActive(true);

                Transform lockT = levelButtons[i].transform.Find("LockIcon");
                if (lockT != null) lockT.gameObject.SetActive(false);
            }
            else
            {
                bg.color = lockedButtonColor;

                Transform numT = levelButtons[i].transform.Find("LevelNum");
                if (numT != null) numT.gameObject.SetActive(false);

                Transform starsT = levelButtons[i].transform.Find("Stars");
                if (starsT != null) starsT.gameObject.SetActive(false);

                Transform constraintT = levelButtons[i].transform.Find("Constraint");
                if (constraintT != null) constraintT.gameObject.SetActive(false);

                // Show lock text — use star gate info if applicable
                Transform lockT = levelButtons[i].transform.Find("LockIcon");
                if (lockT != null)
                {
                    lockT.gameObject.SetActive(true);
                    TextMeshProUGUI lockText = lockT.GetComponent<TextMeshProUGUI>();
                    int gateReq = LevelManager.GetStarGateRequirement(levelNumber);
                    if (gateReq > 0 && LevelManager.Instance != null && !LevelManager.Instance.IsStarGateMet(levelNumber))
                        lockText.text = $"Need\n{gateReq} Stars";
                    else
                        lockText.text = "LOCKED";
                }
            }
        }

        // Update total stars/coins display
        if (levelSelectPanel != null)
        {
            Transform totalStarsT = levelSelectPanel.transform.Find("TotalStarsText");
            if (totalStarsT != null)
            {
                TextMeshProUGUI t = totalStarsT.GetComponent<TextMeshProUGUI>();
                int totalStars = LevelManager.Instance != null ? LevelManager.Instance.GetTotalStars() : 0;
                t.text = $"Total Stars: {totalStars} / 30";
            }

            Transform totalCoinsT = levelSelectPanel.transform.Find("TotalCoinsText");
            if (totalCoinsT != null)
            {
                TextMeshProUGUI t = totalCoinsT.GetComponent<TextMeshProUGUI>();
                int totalCoins = LevelManager.Instance != null ? LevelManager.Instance.GetTotalStarCoins() : 0;
                t.text = $"StarCoins: {totalCoins}";
            }
        }

        // Update challenge mode button
        if (levelSelectPanel != null)
        {
            Transform challengeT = levelSelectPanel.transform.Find("ChallengeModeBtn");
            if (challengeT != null)
            {
                Image challengeBg = challengeT.GetComponent<Image>();
                bool unlocked = ChallengeMode.IsUnlocked();
                challengeBg.color = unlocked
                    ? new Color(0.20f, 0.75f, 0.45f, 0.95f)
                    : new Color(0.25f, 0.18f, 0.32f, 0.80f);

                Transform labelT = challengeT.Find("Label");
                if (labelT != null)
                {
                    TextMeshProUGUI labelText = labelT.GetComponent<TextMeshProUGUI>();
                    if (labelText != null)
                    {
                        int bestWave = ChallengeMode.GetBestWave();
                        labelText.text = unlocked
                            ? (bestWave > 0 ? $"Challenge (Best: Wave {bestWave})" : "Challenge Mode")
                            : "Challenge (Locked)";
                    }
                }
            }

            // Update star gate labels
            UpdateStarGateLabel("StarGate1", 4);
            UpdateStarGateLabel("StarGate2", 10);
        }
    }

    private void OnLevelButtonClicked(int levelNumber)
    {
        if (LevelManager.Instance == null) return;

        if (LevelManager.Instance.SelectLevel(levelNumber))
        {
            Debug.Log($"Starting Level {levelNumber}: {LevelManager.Instance.CurrentLevel.GetObjectiveText()}");
            Hide();

            // Notify the GameManager to start the level
            GameManager gm = FindFirstObjectByType<GameManager>();
            if (gm != null)
                gm.StartLevel();
        }
    }

    private void CreateStarGateLabel(RectTransform parent, string name, Vector2 position, string text, int requiredStars)
    {
        Transform existing = parent.Find(name);
        if (existing != null) return;

        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(950f, 30f);

        TextMeshProUGUI label = obj.GetComponent<TextMeshProUGUI>();
        TMP_FontAsset font = UIHierarchyBuilder.GetPreferredFont();
        if (font != null) label.font = font;
        else if (TMP_Settings.defaultFontAsset != null) label.font = TMP_Settings.defaultFontAsset;
        label.text = text;
        label.fontSize = 18;
        label.alignment = TextAlignmentOptions.Left;
        label.color = new Color(1f, 0.85f, 0.1f, 0.6f);
        label.raycastTarget = false;
        label.fontStyle = FontStyles.Italic;
    }

    private void UpdateStarGateLabel(string name, int requiredStars)
    {
        if (levelSelectPanel == null) return;
        Transform t = levelSelectPanel.transform.Find(name);
        if (t == null) return;

        TextMeshProUGUI label = t.GetComponent<TextMeshProUGUI>();
        if (label == null) return;

        int totalStars = LevelManager.Instance != null ? LevelManager.Instance.GetTotalStars() : 0;
        bool met = totalStars >= requiredStars;

        label.color = met
            ? new Color(0.3f, 1f, 0.5f, 0.5f)
            : new Color(1f, 0.85f, 0.1f, 0.6f);
        label.text = met
            ? $"{requiredStars} Stars — Unlocked!"
            : $"{requiredStars} Stars Required ({totalStars}/{requiredStars})";
    }
}
