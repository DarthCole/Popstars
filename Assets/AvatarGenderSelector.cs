using UnityEngine;

/// <summary>
/// 2D version — swaps the outfit catalogue on AvatarOutfitManager
/// and refreshes the AvatarUIPanel when the player picks Male or Female.
/// Wire the Female / Male UI buttons to SelectFemale() and SelectMale().
/// The last chosen gender is saved in PlayerPrefs and restored on next launch.
/// </summary>
public class AvatarGenderSelector : MonoBehaviour
{
    [Header("Outfit Manager")]
    public AvatarOutfitManager outfitManager;

    [Header("Outfit Catalogues")]
    public OutfitData[] femaleOutfits;
    public OutfitData[] maleOutfits;

    [Header("UI Panel (optional — refreshed on gender switch)")]
    public AvatarUIPanel uiPanel;

    [Header("Gender Button Images (wired by builder)")]
    public UnityEngine.UI.Image femaleBtnImage;
    public UnityEngine.UI.Image maleBtnImage;

    static readonly Color GENDER_ACTIVE = new Color(0.255f, 0.118f, 0.530f, 1.00f);
    static readonly Color GENDER_IDLE   = new Color(0.090f, 0.042f, 0.190f, 1.00f);

    const string PREF_KEY = "PopstarHub_SelectedGender"; // 0 = Female, 1 = Male

    void Awake()
    {
        ApplyGender(PlayerPrefs.GetInt(PREF_KEY, 0));
    }

    // ── Public — wire these to UI buttons ────────────────────────────────────

    public void SelectFemale() => ApplyGender(0);
    public void SelectMale()   => ApplyGender(1);

    // ── Internal ──────────────────────────────────────────────────────────────

    void ApplyGender(int gender)
    {
        if (femaleBtnImage != null) femaleBtnImage.color = gender == 0 ? GENDER_ACTIVE : GENDER_IDLE;
        if (maleBtnImage   != null) maleBtnImage.color   = gender == 1 ? GENDER_ACTIVE : GENDER_IDLE;

        if (outfitManager != null)
        {
            var catalogue = gender == 1 ? maleOutfits : femaleOutfits;
            outfitManager.SetOutfitCatalogue(catalogue);
        }

        if (uiPanel != null)
            uiPanel.Refresh();

        PlayerPrefs.SetInt(PREF_KEY, gender);
    }
}
