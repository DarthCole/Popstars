using UnityEngine;
using TMPro;

public class ShopItemCard : MonoBehaviour
{
    [Header("Item Info")]
    public string itemName;
    public int price;
    public string itemID;

    [Header("UI References")]
    public TMP_Text priceText;
    public GameObject purchasedIndicator;

    private bool isPurchased = false;

    void Start()
    {
        isPurchased = PlayerPrefs.GetInt("Purchased_" + itemID, 0) == 1;

        if (priceText != null)
            priceText.text = price.ToString();

        UpdateVisual();
    }

    public void OnBuyButtonClicked()
    {
        if (isPurchased) return;

        if (StarCoinManager.Instance.SpendCoins(price))
        {
            isPurchased = true;
            PlayerPrefs.SetInt("Purchased_" + itemID, 1);
            PlayerPrefs.Save();
            UpdateVisual();
            Debug.Log("Purchased: " + itemName + " for " + price + " StarCoins");
        }
    }

    private void UpdateVisual()
    {
        if (isPurchased)
        {
            if (priceText != null)
                priceText.text = "OWNED";

            if (purchasedIndicator != null)
                purchasedIndicator.SetActive(true);
        }
    }
}