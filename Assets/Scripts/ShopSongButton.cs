using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Placed on each song shop card's action button.
/// Two states:
///   BUY   — player doesn't own this song → click purchases it
///   OWNED — player already owns it       → button disabled
/// </summary>
public class ShopSongButton : MonoBehaviour
{
    [Header("Data")]
    public SongData song;

    [Header("UI References")]
    public Image           buttonImage;
    public TextMeshProUGUI buttonLabel;
    public Image           coverImage;   // card cover art (dimmed if not owned)

    // ── Palette ───────────────────────────────────────────────────────────────
    static readonly Color BUY_BG   = new Color(0.245f, 0.118f, 0.490f, 1f);
    static readonly Color OWNED_BG = new Color(0.047f, 0.180f, 0.090f, 1f);
    static readonly Color W100     = Color.white;
    static readonly Color W65      = new Color(1f, 1f, 1f, 0.65f);

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Start()
    {
        RefreshState();
        if (SongOwnershipManager.Instance != null)
            SongOwnershipManager.Instance.OnOwnershipChanged += RefreshState;
    }

    void OnDestroy()
    {
        if (SongOwnershipManager.Instance != null)
            SongOwnershipManager.Instance.OnOwnershipChanged -= RefreshState;
    }

    // ── Button handler ────────────────────────────────────────────────────────
    public void OnActionPressed()
    {
        if (song == null || SongOwnershipManager.Instance == null) return;
        if (SongOwnershipManager.Instance.IsOwned(song.songID)) return;

        bool bought = SongOwnershipManager.Instance.TryPurchase(song);
        if (!bought)
            Debug.Log($"[Shop] Not enough StarCoins to buy {song.songName}.");
    }

    // ── Visual refresh ────────────────────────────────────────────────────────
    void RefreshState()
    {
        if (song == null) return;

        bool owned = song.isDefault || song.price <= 0
                     || (SongOwnershipManager.Instance != null
                         && SongOwnershipManager.Instance.IsOwned(song.songID));

        if (buttonImage != null)
            buttonImage.color = owned ? OWNED_BG : BUY_BG;

        if (buttonLabel != null)
        {
            buttonLabel.text  = owned ? "OWNED" : $"\u2605 {song.price}";
            buttonLabel.color = owned ? W65 : W100;
        }

        if (coverImage != null)
            coverImage.color = owned ? W100 : W65;

        var btn = GetComponent<Button>();
        if (btn != null) btn.interactable = !owned;
    }
}
