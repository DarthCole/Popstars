using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Placed on every shop outfit card's BUY button at build time.
/// Calls OutfitOwnershipManager.TryPurchase at runtime and refreshes the button state.
/// </summary>
public class ShopOutfitButton : MonoBehaviour
{
    public OutfitData outfit;

    // Cached refs set by ShopUIBuilder
    public Image            buttonImage;
    public TextMeshProUGUI  buttonLabel;
    public Image            iconImage;     // the preview icon Image on the card

    static readonly Color BUY_BG   = new Color(0.245f, 0.118f, 0.490f, 1.00f);
    static readonly Color OWNED_BG  = new Color(0.098f, 0.046f, 0.215f, 1.00f);
    static readonly Color W100      = Color.white;
    static readonly Color W65       = new Color(1f, 1f, 1f, 0.65f);

    void Start()
    {
        RefreshState();

        // Subscribe so card updates if ownership changes from another source
        if (OutfitOwnershipManager.Instance != null)
            OutfitOwnershipManager.Instance.OnOwnershipChanged += RefreshState;
    }

    void OnDestroy()
    {
        if (OutfitOwnershipManager.Instance != null)
            OutfitOwnershipManager.Instance.OnOwnershipChanged -= RefreshState;
    }

    public void OnBuyPressed()
    {
        if (outfit == null) return;
        if (OutfitOwnershipManager.Instance == null) return;

        bool success = OutfitOwnershipManager.Instance.TryPurchase(outfit);
        if (!success && !OutfitOwnershipManager.Instance.IsOwned(outfit.outfitID))
        {
            Debug.Log($"[Shop] Not enough StarCoins to buy {outfit.outfitName}.");
        }
        // RefreshState is called automatically via OnOwnershipChanged event
    }

    void RefreshState()
    {
        if (outfit == null) return;

        bool owned = outfit.isDefault || outfit.price <= 0
                     || (OutfitOwnershipManager.Instance != null
                         && OutfitOwnershipManager.Instance.IsOwned(outfit.outfitID));

        if (buttonImage != null)
            buttonImage.color = owned ? OWNED_BG : BUY_BG;

        if (buttonLabel != null)
        {
            buttonLabel.text  = owned ? "OWNED" : "BUY";
            buttonLabel.color = owned ? W65 : W100;
        }

        if (iconImage != null)
            iconImage.color = owned ? W100 : new Color(1f, 1f, 1f, 0.5f);

        var btn = GetComponent<Button>();
        if (btn != null) btn.interactable = !owned;
    }
}
