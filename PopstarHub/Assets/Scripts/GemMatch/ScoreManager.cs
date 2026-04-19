// File: Assets/Scripts/GemMatch/ScoreManager.cs
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private TextMeshProUGUI levelHeaderText;
    [SerializeField] private ReplayIncentiveUI replayIncentive;

    private int score;
    private int comboMultiplier;
    private int highScore;
    private int targetScore;
    private LevelData currentLevel;

    private void Awake()
    {
        EnsureScoreTexts();
    }

    private void Start()
    {
        // Wait for ConfigureForLevel()
    }

    public void ConfigureForLevel(LevelData level)
    {
        currentLevel = level;
        score = 0;
        comboMultiplier = 1;
        targetScore = level != null ? level.targetScore : 0;
        highScore = PlayerPrefs.GetInt("HighScore", 0);

        if (scoreText != null)
            scoreText.gameObject.SetActive(true);

        if (levelHeaderText != null)
        {
            if (level != null)
            {
                levelHeaderText.text = $"Level {level.levelNumber}";
                levelHeaderText.gameObject.SetActive(true);
            }
            else
            {
                levelHeaderText.gameObject.SetActive(false);
            }
        }

        if (highScoreText != null)
        {
            if (level != null)
                highScoreText.text = $"Target: {targetScore}";
            else
                highScoreText.text = $"High Score: {highScore}";
            highScoreText.gameObject.SetActive(true);
        }

        // Initialize replay incentive
        EnsureReplayIncentive();
        if (replayIncentive != null)
            replayIncentive.Initialize(level);

        UpdateScoreDisplay();
    }

    public void SetVisible(bool visible)
    {
        if (scoreText != null) scoreText.gameObject.SetActive(visible);
        if (highScoreText != null) highScoreText.gameObject.SetActive(visible);
        if (levelHeaderText != null) levelHeaderText.gameObject.SetActive(visible);
        if (replayIncentive != null) replayIncentive.SetVisible(visible);
    }

    public void AddMatchScore(int gemsMatched)
    {
        int basePoints;

        if (gemsMatched <= 3)
            basePoints = 50;
        else if (gemsMatched == 4)
            basePoints = 100;
        else
            basePoints = 200;

        int earnedPoints = basePoints * comboMultiplier;
        score += earnedPoints;

        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }

        Debug.Log($"Matched {gemsMatched} gems! +{earnedPoints} pts (x{comboMultiplier} combo)");

        comboMultiplier++;
        UpdateScoreDisplay();

        // Update replay incentive
        if (replayIncentive != null)
            replayIncentive.UpdateScore(score);
    }

    public void ResetCombo()
    {
        comboMultiplier = 1;
    }

    public int GetScore()
    {
        return score;
    }

    public int GetComboMultiplier()
    {
        return comboMultiplier;
    }

    public bool HasReachedTarget()
    {
        return score >= targetScore;
    }

    public int GetStarRating()
    {
        if (currentLevel == null) return 0;
        return currentLevel.GetStarRating(score);
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            if (currentLevel != null)
                scoreText.text = $"Score: {score} / {targetScore}";
            else
                scoreText.text = $"Score: {score}";
        }

        if (highScoreText != null && currentLevel == null)
        {
            highScoreText.text = $"High Score: {highScore}";
        }
    }

    private void EnsureReplayIncentive()
    {
        if (replayIncentive == null)
            replayIncentive = FindFirstObjectByType<ReplayIncentiveUI>();
        if (replayIncentive == null)
        {
            GameObject obj = new GameObject("ReplayIncentiveUI", typeof(ReplayIncentiveUI));
            replayIncentive = obj.GetComponent<ReplayIncentiveUI>();
        }
    }

    private void EnsureScoreTexts()
    {
        Canvas canvas = UIHierarchyBuilder.GetOrCreateCanvas("UICanvas");

        levelHeaderText = UIHierarchyBuilder.GetOrCreateTMPText(
            canvas.transform,
            "LevelHeaderText",
            "",
            new Vector2(-700f, 500f),
            new Vector2(420f, 60f),
            36,
            TMPro.TextAlignmentOptions.Left,
            new Color(0.88f, 0.70f, 1f, 1f));
        levelHeaderText.gameObject.SetActive(false);

        scoreText = UIHierarchyBuilder.GetOrCreateTMPText(
            canvas.transform,
            "ScoreText",
            "Score: 0",
            new Vector2(-700f, 460f),
            new Vector2(420f, 80f),
            42,
            TMPro.TextAlignmentOptions.Left,
            Color.white);
        scoreText.gameObject.SetActive(false);

        highScoreText = UIHierarchyBuilder.GetOrCreateTMPText(
            canvas.transform,
            "HighScoreText",
            "Target: 0",
            new Vector2(-700f, 405f),
            new Vector2(420f, 70f),
            30,
            TMPro.TextAlignmentOptions.Left,
            new Color(1f, 0.92f, 0.1f, 1f));
        highScoreText.gameObject.SetActive(false);
    }
}