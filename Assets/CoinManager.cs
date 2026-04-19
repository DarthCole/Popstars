using System;
using UnityEngine;

/// <summary>
/// Singleton — persists across scenes.
/// Manages the player's StarCoin balance using PlayerPrefs.
/// </summary>
public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    /// <summary>Fired once when the singleton initialises. Useful for UI that loads before the manager.</summary>
    public static event Action<CoinManager> OnAnyInstanceReady;

    const string PREFS_KEY = "PopstarHub_StarCoins";
    const int    START_COINS = 500; // coins given on first launch

    public event Action<int> OnBalanceChanged;

    public int Balance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Balance = PlayerPrefs.GetInt(PREFS_KEY, START_COINS);
        OnAnyInstanceReady?.Invoke(this);
    }

    /// <summary>Add coins and save.</summary>
    public void AddCoins(int amount)
    {
        if (amount <= 0) return;
        Balance += amount;
        Save();
    }

    /// <summary>Deduct coins if balance is sufficient. Returns true on success.</summary>
    public bool TrySpend(int amount)
    {
        if (amount <= 0) return true;
        if (Balance < amount) return false;
        Balance -= amount;
        Save();
        return true;
    }

    void Save()
    {
        PlayerPrefs.SetInt(PREFS_KEY, Balance);
        PlayerPrefs.Save();
        OnBalanceChanged?.Invoke(Balance);
    }

    /// <summary>Reset to starting coins (useful for testing).</summary>
    [ContextMenu("Reset Coins to 500")]
    public void ResetCoins()
    {
        Balance = START_COINS;
        Save();
    }
}
