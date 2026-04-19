using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Runtime script that builds and drives the outfit card scroll panel.
/// Attach to the OutfitPanel GameObject created by AvatarSceneBuilder.
/// Ownership is driven by OutfitOwnershipManager (singleton).
/// </summary>
public class AvatarUIPanel : MonoBehaviour
{
    [Header("References")]
    public AvatarOutfitManager outfitManager;
    public RectTransform       cardContainer;

    [Header("Card Appearance")]
    public float cardWidth   = 220f;
    public float cardHeight  = 260f;
    public float cardSpacing = 18f;
    public float topPadding  = 20f;

    [Header("Testing")]
    [Tooltip("Tick this to treat all outfits as owned during testing.")]
    public bool unlockAllForTesting = false;

    // ── Colours (match the builder palette) ──────────────────────────────────
    static readonly Color CARD_BG      = new Color(0.120f, 0.058f, 0.255f, 1.00f);
    static readonly Color CARD_BORDER  = new Color(0.230f, 0.105f, 0.450f, 0.90f);
    static readonly Color EQUIP_BG     = new Color(0.255f, 0.118f, 0.530f, 1.00f);
    static readonly Color LOCKED_BG    = new Color(0.090f, 0.042f, 0.190f, 1.00f);
    static readonly Color SELECTED_BG  = new Color(0.360f, 0.160f, 0.720f, 1.00f);
    static readonly Color W100         = Color.white;
    static readonly Color W50          = new Color(1f, 1f, 1f, 0.50f);
    static readonly Color PRICE_COL    = new Color(0.88f, 0.82f, 0.24f, 1.00f);

    void Start()
    {
        if (outfitManager == null)
            outfitManager = FindObjectOfType<AvatarOutfitManager>();

        // Subscribe to ownership changes so cards refresh after a shop purchase
        if (OutfitOwnershipManager.Instance != null)
            OutfitOwnershipManager.Instance.OnOwnershipChanged += Refresh;

        if (cardContainer != null && cardContainer.childCount == 0)
            BuildCards();
    }

    void OnDestroy()
    {
        if (OutfitOwnershipManager.Instance != null)
            OutfitOwnershipManager.Instance.OnOwnershipChanged -= Refresh;
    }

    /// <summary>Call this after a gender switch or after a shop purchase unlocks an outfit.</summary>
    public void Refresh()
    {
        // Clear old cards
        foreach (Transform child in cardContainer)
            Destroy(child.gameObject);

        BuildCards();
    }

    // ── Card construction ─────────────────────────────────────────────────────

    void BuildCards()
    {
        if (outfitManager == null || cardContainer == null) return;

        var outfits = outfitManager.outfits;
        if (outfits == null || outfits.Length == 0) return;

        // Content height: one column of cards
        float totalH = topPadding + outfits.Length * (cardHeight + cardSpacing);
        cardContainer.sizeDelta = new Vector2(cardContainer.sizeDelta.x, totalH);

        var uiSpr = GetRoundedSprite();

        for (int i = 0; i < outfits.Length; i++)
        {
            var outfit = outfits[i];
            if (outfit == null) continue;

            bool owned = unlockAllForTesting || outfit.isDefault || outfit.price <= 0 || IsOwned(outfit.outfitID);
            CreateCard(i, outfit, owned, uiSpr);
        }
    }

    void CreateCard(int index, OutfitData outfit, bool owned, Sprite spr)
    {
        float yPos = -(topPadding + index * (cardHeight + cardSpacing));

        // ── Border ────────────────────────────────────────────────────────────
        var borderGO = new GameObject(outfit.outfitName + "_Border",
                                       typeof(RectTransform), typeof(Image));
        borderGO.transform.SetParent(cardContainer, false);
        var borderRT  = borderGO.GetComponent<RectTransform>();
        borderRT.anchorMin = borderRT.anchorMax = new Vector2(0.5f, 1f);
        borderRT.pivot     = new Vector2(0.5f, 1f);
        borderRT.sizeDelta = new Vector2(cardWidth + 4f, cardHeight + 4f);
        borderRT.anchoredPosition = new Vector2(0f, yPos);
        borderGO.GetComponent<Image>().color = CARD_BORDER;

        // ── Card face ─────────────────────────────────────────────────────────
        var cardGO = new GameObject(outfit.outfitName, typeof(RectTransform), typeof(Image));
        cardGO.transform.SetParent(borderRT, false);
        var cardRT  = cardGO.GetComponent<RectTransform>();
        cardRT.anchorMin = Vector2.zero;
        cardRT.anchorMax = Vector2.one;
        cardRT.offsetMin = new Vector2(2f, 2f);
        cardRT.offsetMax = new Vector2(-2f, -2f);
        cardGO.GetComponent<Image>().color = CARD_BG;

        // ── Outfit icon ───────────────────────────────────────────────────────
        if (outfit.previewIcon != null)
        {
            var iconGO = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGO.transform.SetParent(cardRT, false);
            var iconRT  = iconGO.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.1f, 0.38f);
            iconRT.anchorMax = new Vector2(0.9f, 0.92f);
            iconRT.offsetMin = iconRT.offsetMax = Vector2.zero;
            var iconImg  = iconGO.GetComponent<Image>();
            iconImg.sprite                  = outfit.previewIcon;
            iconImg.preserveAspect          = true;
            iconImg.color                   = owned ? W100 : W50;
        }

        // ── Outfit name ───────────────────────────────────────────────────────
        var nameGO  = new GameObject("Name", typeof(RectTransform), typeof(TextMeshProUGUI));
        nameGO.transform.SetParent(cardRT, false);
        var nameRT  = nameGO.GetComponent<RectTransform>();
        nameRT.anchorMin = new Vector2(0.05f, 0.24f);
        nameRT.anchorMax = new Vector2(0.95f, 0.40f);
        nameRT.offsetMin = nameRT.offsetMax = Vector2.zero;
        var nameTmp  = nameGO.GetComponent<TextMeshProUGUI>();
        nameTmp.text               = outfit.outfitName;
        nameTmp.fontSize           = 13f;
        nameTmp.color              = W100;
        nameTmp.alignment          = TextAlignmentOptions.Center;
        nameTmp.enableWordWrapping = false;
        nameTmp.overflowMode       = TextOverflowModes.Ellipsis;

        // ── Equip / Locked button ─────────────────────────────────────────────
        var btnGO = new GameObject("EquipButton",
                                   typeof(RectTransform), typeof(Image), typeof(Button));
        btnGO.transform.SetParent(cardRT, false);
        var btnRT  = btnGO.GetComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(0.08f, 0.04f);
        btnRT.anchorMax = new Vector2(0.92f, 0.23f);
        btnRT.offsetMin = btnRT.offsetMax = Vector2.zero;
        var btnImg  = btnGO.GetComponent<Image>();
        btnImg.color = owned ? EQUIP_BG : LOCKED_BG;

        var btnTxtGO  = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        btnTxtGO.transform.SetParent(btnGO.transform, false);
        var btnTxtRT  = btnTxtGO.GetComponent<RectTransform>();
        btnTxtRT.anchorMin = Vector2.zero;
        btnTxtRT.anchorMax = Vector2.one;
        btnTxtRT.offsetMin = btnTxtRT.offsetMax = Vector2.zero;
        var btnTmp  = btnTxtGO.GetComponent<TextMeshProUGUI>();
        btnTmp.text      = owned ? "EQUIP" : "\U0001F512 LOCKED";
        btnTmp.fontSize  = 12f;
        btnTmp.color     = owned ? W100 : W50;
        btnTmp.alignment = TextAlignmentOptions.Center;
        btnTmp.enableWordWrapping = false;
        btnTmp.fontStyle = FontStyles.Bold;

        if (owned)
        {
            int captured = index;
            btnGO.GetComponent<Button>().onClick.AddListener(() =>
            {
                outfitManager.EquipOutfit(captured);
                HighlightSelected(captured);
            });
            btnGO.AddComponent<ButtonHoverEffect>();
        }
        else
        {
            btnGO.GetComponent<Button>().interactable = false;
        }
    }

    void HighlightSelected(int selectedIndex)
    {
        // Update card backgrounds to show which is selected
        int i = 0;
        foreach (Transform border in cardContainer)
        {
            var card = border.GetChild(0); // card face inside border
            var img  = card?.GetComponent<Image>();
            if (img != null)
                img.color = (i == selectedIndex) ? SELECTED_BG : CARD_BG;
            i++;
        }
    }

    bool IsOwned(int id)
    {
        if (unlockAllForTesting) return true;
        if (OutfitOwnershipManager.Instance != null)
            return OutfitOwnershipManager.Instance.IsOwned(id);
        // Fallback if singleton not present
        return false;
    }

    static Sprite GetRoundedSprite() =>
        Resources.Load<Sprite>("UI/Skin/UISprite") ??
        Sprite.Create(Texture2D.whiteTexture,
                      new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f));
}
