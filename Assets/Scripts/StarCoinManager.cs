using UnityEngine;
using System;

public class StarCoinManager : MonoBehaviour
{
    // Singleton instance (accessible globally)
    public static StarCoinManager Instance;

    [Header("Debug")]
    [SerializeField] private bool debugMode = true;

    // Events for UI and other systems to subscribe to
    public event Action<int> OnBalanceChanged;
    public event Action<int, int> OnCoinsEarned;
    public event Action<int, int> OnCoinsSpent;
    public event Action<int> OnPurchaseFailed;
    public event Action OnCoinsReset;

    private int balance = 0;
    private const string SAVE_KEY = "StarCoins";

    void Awake()
    {
        // Ensure only one instance exists
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Persist across scenes (important for multi-panel or multi-scene setups)
        DontDestroyOnLoad(gameObject);

        LoadCoins();
    }

    // Load saved coins from PlayerPrefs
    private void LoadCoins()
    {
        balance = PlayerPrefs.GetInt(SAVE_KEY, 0);
    }

    // Save current balance
    private void SaveCoins()
    {
        PlayerPrefs.SetInt(SAVE_KEY, balance);
        PlayerPrefs.Save();
    }

    // Add coins to the balance
    public void AddCoins(int amount)
    {
        if (amount <= 0) return;

        int previousBalance = balance;

        // Clamp to avoid negative values
        balance = Mathf.Max(0, balance + amount);
        SaveCoins();

        if (debugMode)
            Debug.Log("[StarCoins] +" + amount + " | " + previousBalance + " -> " + balance);

        OnCoinsEarned?.Invoke(amount, balance);
        OnBalanceChanged?.Invoke(balance);
    }

    // Attempt to spend coins
    public bool SpendCoins(int amount)
    {
        if (amount <= 0) return false;

        if (balance >= amount)
        {
            int previousBalance = balance;

            balance -= amount;
            SaveCoins();

            if (debugMode)
                Debug.Log("[StarCoins] -" + amount + " | " + previousBalance + " -> " + balance);

            OnCoinsSpent?.Invoke(amount, balance);
            OnBalanceChanged?.Invoke(balance);
            return true;
        }

        // Not enough coins
        if (debugMode)
            Debug.Log("[StarCoins] FAILED: need " + amount + ", have " + balance);

        OnPurchaseFailed?.Invoke(amount);
        return false;
    }

    // Get current balance (used by UI on start)
    public int GetBalance()
    {
        return balance;
    }

    // Check if player has enough coins
    public bool HasEnough(int amount)
    {
        return balance >= amount;
    }

    // Reset coins to zero
    public void ResetCoins()
    {
        balance = 0;
        SaveCoins();

        if (debugMode)
            Debug.Log("[StarCoins] Reset to 0");

        OnCoinsReset?.Invoke();
        OnBalanceChanged?.Invoke(balance);
    }

    // Force set coins (useful for testing or admin tools)
    public void SetCoins(int amount)
    {
        int previousBalance = balance;

        balance = Mathf.Max(0, amount);
        SaveCoins();

        if (debugMode)
            Debug.Log("[StarCoins] Set: " + previousBalance + " -> " + balance);

        OnBalanceChanged?.Invoke(balance);
    }

    // Convert score into coins (used by mini-games)
    public void RewardFromScore(int score)
    {
        int coins = Mathf.Clamp(score / 10, 5, 100);
        AddCoins(coins);
    }

    // Apply multiplier (for combos, streaks, bonuses)
    public void AddCoinsWithMultiplier(int baseAmount, float multiplier)
    {
        int finalAmount = Mathf.RoundToInt(baseAmount * multiplier);
        AddCoins(finalAmount);
    }

    // Returns how many more coins are needed for a purchase
    public int GetDeficit(int amount)
    {
        if (HasEnough(amount)) return 0;
        return amount - balance;
    }
}