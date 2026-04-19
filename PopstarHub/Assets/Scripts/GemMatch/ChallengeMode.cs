// File: Assets/Scripts/GemMatch/ChallengeMode.cs
using UnityEngine;

/// <summary>
/// Manages endless Challenge Mode waves.
/// Unlocked after completing all 10 levels.
/// Each wave gets harder: more gem types, higher target scores.
/// </summary>
public class ChallengeMode : MonoBehaviour
{
    public static ChallengeMode Instance { get; private set; }

    private int currentWave;
    private int waveScore;
    private bool isActive;

    public int CurrentWave => currentWave;
    public bool IsActive => isActive;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Check if challenge mode is unlocked (all 10 levels completed).
    /// </summary>
    public static bool IsUnlocked()
    {
        if (LevelManager.Instance == null) return false;
        for (int i = 1; i <= 10; i++)
        {
            if (LevelManager.Instance.GetStarsForLevel(i) < 1)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Start challenge mode from wave 1.
    /// </summary>
    public void StartChallenge()
    {
        currentWave = 1;
        waveScore = 0;
        isActive = true;
    }

    /// <summary>
    /// Generate a LevelData for the current wave.
    /// Difficulty ramps each wave.
    /// </summary>
    public LevelData GetCurrentWaveData()
    {
        int gemTypes = Mathf.Min(4 + (currentWave / 3), 7); // 4→7 over waves
        int rows = 8;
        int cols = 8;
        float timeLimit = Mathf.Max(60f - (currentWave - 1) * 3f, 30f); // 60s→30s
        int targetScore = 500 + (currentWave - 1) * 200; // ramps up 200 per wave
        int twoStar = (int)(targetScore * 1.5f);
        int threeStar = targetScore * 2;

        return new LevelData(
            100 + currentWave, // level numbers 101+ for challenge
            rows, cols, gemTypes,
            LevelConstraintType.Time, timeLimit, 0,
            targetScore, twoStar, threeStar
        );
    }

    /// <summary>
    /// Called when a challenge wave ends successfully. Advances to next wave.
    /// Returns the new wave number.
    /// </summary>
    public int AdvanceWave()
    {
        currentWave++;

        // Save best wave
        int best = GetBestWave();
        if (currentWave > best)
        {
            PlayerPrefs.SetInt("ChallengeBestWave", currentWave);
            PlayerPrefs.Save();
        }

        return currentWave;
    }

    /// <summary>
    /// Called when challenge mode ends (player failed a wave).
    /// </summary>
    public void EndChallenge()
    {
        isActive = false;

        int best = GetBestWave();
        if (currentWave > best)
        {
            PlayerPrefs.SetInt("ChallengeBestWave", currentWave);
            PlayerPrefs.Save();
        }

        Debug.Log($"Challenge Mode ended at Wave {currentWave}. Best: {GetBestWave()}");
    }

    /// <summary>
    /// Returns the highest wave reached.
    /// </summary>
    public static int GetBestWave()
    {
        return PlayerPrefs.GetInt("ChallengeBestWave", 0);
    }
}
