// File: Assets/Scripts/GemMatch/AchievementManager.cs
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton that manages achievement unlock state, checks conditions,
/// awards coins, and triggers popup notifications.
/// </summary>
public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    private List<AchievementData> allAchievements;
    private AchievementPopup popup;

    // Session stats tracked for condition checking
    private int sessionMaxCombo;
    private int sessionIceBroken;
    private Dictionary<Gem.GemType, int> sessionColorClears = new Dictionary<Gem.GemType, int>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        allAchievements = AchievementData.GetAllAchievements();
    }

    /// <summary>
    /// Reset session stats at the start of each level.
    /// </summary>
    public void ResetSessionStats()
    {
        sessionMaxCombo = 0;
        sessionIceBroken = 0;
        sessionColorClears.Clear();
    }

    /// <summary>
    /// Record a combo level during gameplay.
    /// </summary>
    public void RecordCombo(int comboLevel)
    {
        if (comboLevel > sessionMaxCombo)
            sessionMaxCombo = comboLevel;
    }

    /// <summary>
    /// Record an ice block being broken.
    /// </summary>
    public void RecordIceBroken()
    {
        sessionIceBroken++;

        // Accumulate lifetime ice broken
        int lifetime = PlayerPrefs.GetInt("LifetimeIceBroken", 0) + 1;
        PlayerPrefs.SetInt("LifetimeIceBroken", lifetime);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Record a gem color being cleared.
    /// </summary>
    public void RecordColorClear(Gem.GemType gemType)
    {
        if (!sessionColorClears.ContainsKey(gemType))
            sessionColorClears[gemType] = 0;
        sessionColorClears[gemType]++;
    }

    /// <summary>
    /// Check all achievement conditions — typically called at end of a level.
    /// </summary>
    public void CheckAllConditions(int starRating, float timeRemaining, int challengeWave)
    {
        if (LevelManager.Instance == null) return;

        // First Clear
        if (starRating >= 1)
            TryUnlock("first_clear");

        // Perfect Score
        if (starRating >= 3)
            TryUnlock("first_3star");

        // Chain Master
        if (sessionMaxCombo >= 5)
            TryUnlock("chain_master");

        // Speed Demon
        if (timeRemaining >= 20f && starRating >= 1)
            TryUnlock("speed_demon");

        // Ice Breaker (lifetime)
        int lifetimeIce = PlayerPrefs.GetInt("LifetimeIceBroken", 0);
        if (lifetimeIce >= 50)
            TryUnlock("ice_breaker");

        // Color Collector (session)
        foreach (var kvp in sessionColorClears)
        {
            if (kvp.Value >= 100)
            {
                TryUnlock("color_collector");
                break;
            }
        }

        // Halfway Hero
        bool allFirst5 = true;
        for (int i = 1; i <= 5; i++)
        {
            if (LevelManager.Instance.GetStarsForLevel(i) < 1)
            {
                allFirst5 = false;
                break;
            }
        }
        if (allFirst5) TryUnlock("all_clear_5");

        // Champion
        bool allComplete = true;
        for (int i = 1; i <= 10; i++)
        {
            if (LevelManager.Instance.GetStarsForLevel(i) < 1)
            {
                allComplete = false;
                break;
            }
        }
        if (allComplete) TryUnlock("all_clear_10");

        // Star Hoarder
        if (LevelManager.Instance.GetTotalStars() >= 25)
            TryUnlock("star_hoarder");

        // Survivor
        if (challengeWave >= 10)
            TryUnlock("survivor");
    }

    /// <summary>
    /// Try to unlock an achievement. Returns true if newly unlocked.
    /// </summary>
    public bool TryUnlock(string achievementId)
    {
        if (IsUnlocked(achievementId)) return false;

        AchievementData data = allAchievements.Find(a => a.id == achievementId);
        if (data == null) return false;

        // Mark as unlocked
        PlayerPrefs.SetInt($"Achievement_{achievementId}", 1);
        PlayerPrefs.Save();

        // Award coins
        int totalCoins = PlayerPrefs.GetInt("TotalStarCoins", 0) + data.coinReward;
        PlayerPrefs.SetInt("TotalStarCoins", totalCoins);
        PlayerPrefs.Save();

        // Show popup
        EnsurePopup();
        if (popup != null)
            popup.ShowAchievement(data.name, data.coinReward);

        Debug.Log($"Achievement Unlocked: {data.name} (+{data.coinReward} coins)");
        return true;
    }

    public bool IsUnlocked(string achievementId)
    {
        return PlayerPrefs.GetInt($"Achievement_{achievementId}", 0) == 1;
    }

    public int GetUnlockedCount()
    {
        int count = 0;
        foreach (var ach in allAchievements)
        {
            if (IsUnlocked(ach.id)) count++;
        }
        return count;
    }

    private void EnsurePopup()
    {
        if (popup == null)
            popup = FindFirstObjectByType<AchievementPopup>();
        if (popup == null)
        {
            GameObject obj = new GameObject("AchievementPopup", typeof(AchievementPopup));
            popup = obj.GetComponent<AchievementPopup>();
        }
    }
}
