// File: Assets/Scripts/GemMatch/ScoreManager.cs
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;

    private int score;
    private int comboMultiplier;

    private void Start()
    {
        score = 0;
        comboMultiplier = 1;
        UpdateScoreDisplay();
    }

    public void AddMatchScore(int gemsMatched)
    {
        // 3 gems = 50 pts, 4 gems = 100 pts, 5+ = 200 pts, multiplied by combo
        int basePoints;

        if (gemsMatched <= 3)
            basePoints = 50;
        else if (gemsMatched == 4)
            basePoints = 100;
        else
            basePoints = 200;

        int earnedPoints = basePoints * comboMultiplier;
        score += earnedPoints;

        Debug.Log($"Matched {gemsMatched} gems! +{earnedPoints} pts (x{comboMultiplier} combo)");

        comboMultiplier++;
        UpdateScoreDisplay();
    }

    public void ResetCombo()
    {
        comboMultiplier = 1;
    }

    public int GetScore()
    {
        return score;
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }
}