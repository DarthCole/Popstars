using UnityEngine;
using UnityEngine.InputSystem;

public class CoinSystemTest : MonoBehaviour
{
    void Start()
    {
        if (StarCoinManager.Instance == null)
        {
            Debug.LogError("StarCoinManager not found in scene!");
            return;
        }

        // Subscribe to events
        StarCoinManager.Instance.OnCoinsEarned += OnCoinsEarned;
        StarCoinManager.Instance.OnCoinsSpent += OnCoinsSpent;
        StarCoinManager.Instance.OnPurchaseFailed += OnPurchaseFailed;

        Debug.Log("=== CoinSystemTest Ready ===");
        Debug.Log("Press G = +100 coins");
        Debug.Log("Press H = spend 150 coins");
        Debug.Log("Press R = reset coins");
        Debug.Log("Press B = check balance");
    }

    void OnDestroy()
    {
        if (StarCoinManager.Instance == null) return;

        // Unsubscribe to prevent memory issues
        StarCoinManager.Instance.OnCoinsEarned -= OnCoinsEarned;
        StarCoinManager.Instance.OnCoinsSpent -= OnCoinsSpent;
        StarCoinManager.Instance.OnPurchaseFailed -= OnPurchaseFailed;
    }

    void Update()
    {
        if (StarCoinManager.Instance == null) return;

        if (Keyboard.current.gKey.wasPressedThisFrame)
            StarCoinManager.Instance.AddCoins(100);

        if (Keyboard.current.hKey.wasPressedThisFrame)
            StarCoinManager.Instance.SpendCoins(150);

        if (Keyboard.current.rKey.wasPressedThisFrame)
            StarCoinManager.Instance.ResetCoins();

        if (Keyboard.current.bKey.wasPressedThisFrame)
            Debug.Log("Balance: " + StarCoinManager.Instance.GetBalance());
    }

    // Event handlers
    void OnCoinsEarned(int amount, int newBal)
    {
        Debug.Log("EVENT: Earned " + amount + " | Balance: " + newBal);
    }

    void OnCoinsSpent(int amount, int newBal)
    {
        Debug.Log("EVENT: Spent " + amount + " | Balance: " + newBal);
    }

    void OnPurchaseFailed(int needed)
    {
        Debug.Log("EVENT: Purchase failed! Needed " + needed);
    }
}