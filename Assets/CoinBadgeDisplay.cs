using TMPro;
using UnityEngine;

/// <summary>
/// Attach this to the CoinsText TMP in the TopBar.
/// It subscribes to CoinManager.OnBalanceChanged and keeps the label up to date.
/// </summary>
public class CoinBadgeDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coinLabel;
    [SerializeField] private string prefix = "★ ";
    [SerializeField] private string suffix = " STARCOINS";

    private void Awake()
    {
        if (coinLabel == null)
            coinLabel = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.OnBalanceChanged += RefreshDisplay;
            RefreshDisplay(CoinManager.Instance.Balance);
        }
        else
        {
            // CoinManager may not be ready yet — listen for first update
            CoinManager.OnAnyInstanceReady += OnManagerReady;
        }
    }

    private void OnDisable()
    {
        if (CoinManager.Instance != null)
            CoinManager.Instance.OnBalanceChanged -= RefreshDisplay;

        CoinManager.OnAnyInstanceReady -= OnManagerReady;
    }

    private void OnManagerReady(CoinManager manager)
    {
        CoinManager.OnAnyInstanceReady -= OnManagerReady;
        manager.OnBalanceChanged += RefreshDisplay;
        RefreshDisplay(manager.Balance);
    }

    private void RefreshDisplay(int balance)
    {
        if (coinLabel != null)
            coinLabel.text = prefix + balance.ToString("N0") + suffix;
    }
}
