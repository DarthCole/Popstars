using UnityEngine;
using TMPro;

public class CoinDisplay : MonoBehaviour
{
    public TMP_Text coinText;

    void Start()
    {
        UpdateDisplay(StarCoinManager.Instance.GetBalance());
        StarCoinManager.Instance.OnBalanceChanged += UpdateDisplay;
    }

    void OnDestroy()
    {
        if (StarCoinManager.Instance != null)
            StarCoinManager.Instance.OnBalanceChanged -= UpdateDisplay;
    }

    private void UpdateDisplay(int newBalance)
    {
        if (coinText != null)
            coinText.text = newBalance.ToString();
    }
}
