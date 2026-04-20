using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Placed on every stage shop card's action button at build time.
/// Cycles through three visual states:
///   BUY      — player doesn't own this stage yet     → click purchases &amp; equips
///   EQUIP    — player owns but hasn't equipped it    → click equips
///   EQUIPPED — currently equipped                    → button disabled
/// </summary>
public class ShopStageButton : MonoBehaviour
{
    [Header("Data")]
    public StageData stage;

    [Header("UI References (set by ShopUIBuilder)")]
    public Image           buttonImage;
    public TextMeshProUGUI buttonLabel;
    public Image           iconImage;   // the preview icon on the card (dimmed if not owned)

    // ── Palette ───────────────────────────────────────────────────────────────
    static readonly Color BUY_BG      = new Color(0.245f, 0.118f, 0.490f, 1f);
    static readonly Color EQUIP_BG    = new Color(0.098f, 0.310f, 0.165f, 1f);
    static readonly Color EQUIPPED_BG = new Color(0.047f, 0.180f, 0.090f, 1f);
    static readonly Color W100        = Color.white;
    static readonly Color W65         = new Color(1f, 1f, 1f, 0.65f);

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Start()
    {
        RefreshState();
        if (StageOwnershipManager.Instance != null)
        {
            StageOwnershipManager.Instance.OnOwnershipChanged += RefreshState;
            StageOwnershipManager.Instance.OnEquipChanged     += RefreshState;
        }
    }

    void OnDestroy()
    {
        if (StageOwnershipManager.Instance != null)
        {
            StageOwnershipManager.Instance.OnOwnershipChanged -= RefreshState;
            StageOwnershipManager.Instance.OnEquipChanged     -= RefreshState;
        }
    }

    // ── Button handler ────────────────────────────────────────────────────────
    public void OnActionPressed()
    {
        if (stage == null || StageOwnershipManager.Instance == null) return;

        bool owned = StageOwnershipManager.Instance.IsOwned(stage.stageID);

        if (!owned)
        {
            // Try to purchase; auto-equip on success
            bool bought = StageOwnershipManager.Instance.TryPurchase(stage);
            if (bought)
                StageOwnershipManager.Instance.EquipStage(stage.stageID);
            else
                Debug.Log($"[Shop] Not enough StarCoins to buy {stage.stageName}.");
        }
        else
        {
            // Already owned — just equip
            StageOwnershipManager.Instance.EquipStage(stage.stageID);
        }
    }

    // ── Visual refresh ────────────────────────────────────────────────────────
    void RefreshState()
    {
        if (stage == null) return;

        bool owned    = stage.isDefault || stage.price <= 0
                        || (StageOwnershipManager.Instance != null
                            && StageOwnershipManager.Instance.IsOwned(stage.stageID));
        bool equipped = StageOwnershipManager.Instance != null
                        && StageOwnershipManager.Instance.IsEquipped(stage.stageID);

        if (buttonImage != null)
            buttonImage.color = equipped ? EQUIPPED_BG : (owned ? EQUIP_BG : BUY_BG);

        if (buttonLabel != null)
        {
            buttonLabel.text  = equipped ? "EQUIPPED" : (owned ? "EQUIP" : $"\u2605 {stage.price}");
            buttonLabel.color = (owned && !equipped) ? W100 : W65;
        }

        if (iconImage != null)
            iconImage.color = owned ? W100 : W65;

        var btn = GetComponent<Button>();
        if (btn != null) btn.interactable = !equipped;
    }
}
