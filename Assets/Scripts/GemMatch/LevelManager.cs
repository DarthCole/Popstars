// File: Assets/Scripts/GemMatch/LevelManager.cs
using UnityEngine;

/// <summary>
/// Singleton that persists across scene loads.
/// Manages which level is currently selected, unlock state, and star progress.
/// </summary>
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    /// <summary>
    /// The level data for the currently selected level.
    /// Set this before loading the gameplay scene.
    /// </summary>
    public LevelData CurrentLevel { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Default to level 1 if nothing selected yet
        if (CurrentLevel == null)
            CurrentLevel = LevelData.GetLevel(1);

        // Level 1 is always unlocked
        if (!IsLevelUnlocked(1))
            UnlockLevel(1);
    }

    /// <summary>
    /// Select a level to play. Returns false if the level is locked or invalid.
    /// </summary>
    public bool SelectLevel(int levelNumber)
    {
        if (!IsLevelUnlocked(levelNumber)) return false;

        LevelData level = LevelData.GetLevel(levelNumber);
        if (level == null) return false;

        CurrentLevel = level;
        return true;
    }

    /// <summary>
    /// Returns true if the given level number is unlocked.
    /// Also checks star gate requirements.
    /// </summary>
    public bool IsLevelUnlocked(int levelNumber)
    {
        if (levelNumber == 1) return true;

        // Check star gate first
        if (!IsStarGateMet(levelNumber)) return false;

        return PlayerPrefs.GetInt($"LevelUnlocked_{levelNumber}", 0) == 1;
    }

    /// <summary>
    /// Returns the star gate requirement for a level.
    /// 0 means no gate, otherwise player needs this many total stars.
    /// </summary>
    public static int GetStarGateRequirement(int levelNumber)
    {
        if (levelNumber >= 4 && levelNumber <= 6) return 4;  // Gate 1
        if (levelNumber >= 7 && levelNumber <= 10) return 10; // Gate 2
        return 0;
    }

    /// <summary>
    /// Returns true if the star gate for a level is met.
    /// </summary>
    public bool IsStarGateMet(int levelNumber)
    {
        int required = GetStarGateRequirement(levelNumber);
        if (required == 0) return true;
        return GetTotalStars() >= required;
    }

    /// <summary>
    /// Unlocks a specific level.
    /// </summary>
    public void UnlockLevel(int levelNumber)
    {
        PlayerPrefs.SetInt($"LevelUnlocked_{levelNumber}", 1);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Unlocks the next level after the current one.
    /// </summary>
    public void UnlockNextLevel()
    {
        if (CurrentLevel == null) return;

        int next = CurrentLevel.levelNumber + 1;
        if (next <= 10)
            UnlockLevel(next);
    }

    /// <summary>
    /// Returns the best star rating achieved for a level (0–3).
    /// </summary>
    public int GetStarsForLevel(int levelNumber)
    {
        return PlayerPrefs.GetInt($"LevelStars_{levelNumber}", 0);
    }

    /// <summary>
    /// Saves stars for the current level. Only updates if the new rating is higher.
    /// Returns the coins earned (only awards new coins for improvement).
    /// </summary>
    public int SaveStars(int newStars)
    {
        if (CurrentLevel == null) return 0;

        int levelNumber = CurrentLevel.levelNumber;
        int previousStars = GetStarsForLevel(levelNumber);

        if (newStars > previousStars)
        {
            PlayerPrefs.SetInt($"LevelStars_{levelNumber}", newStars);
            PlayerPrefs.Save();

            // Award the difference in coins
            int previousCoins = LevelData.GetStarCoins(previousStars);
            int newCoins = LevelData.GetStarCoins(newStars);
            int earned = newCoins - previousCoins;

            // Add to total star coins
            int totalCoins = GetTotalStarCoins();
            PlayerPrefs.SetInt("TotalStarCoins", totalCoins + earned);
            PlayerPrefs.Save();

            return earned;
        }

        return 0;
    }

    /// <summary>
    /// Returns total accumulated StarCoins across all levels.
    /// </summary>
    public int GetTotalStarCoins()
    {
        return PlayerPrefs.GetInt("TotalStarCoins", 0);
    }

    /// <summary>
    /// Returns the total number of stars earned across all levels.
    /// </summary>
    public int GetTotalStars()
    {
        int total = 0;
        for (int i = 1; i <= 10; i++)
            total += GetStarsForLevel(i);
        return total;
    }

    /// <summary>
    /// Returns the highest unlocked level number.
    /// </summary>
    public int GetHighestUnlockedLevel()
    {
        int highest = 1;
        for (int i = 2; i <= 10; i++)
        {
            if (IsLevelUnlocked(i))
                highest = i;
        }
        return highest;
    }
}
