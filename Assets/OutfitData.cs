using UnityEngine;

/// <summary>
/// One outfit item shared by both the Shop and the Avatar Customiser.
/// Create via: Assets ▶ Create ▶ PopstarHub ▶ Outfit Data
/// </summary>
[CreateAssetMenu(fileName = "NewOutfit", menuName = "PopstarHub/Outfit Data")]
public class OutfitData : ScriptableObject
{
    [Header("Identity")]
    public int    outfitID;
    public string outfitName;

    [Header("Visuals")]
    public Sprite previewIcon;                       // outfit-only image shown in the card panel
    public Sprite characterSprite;                   // full character wearing this outfit (shown in AvatarZone)
    public RuntimeAnimatorController animatorController; // idle animation — leave null if no spritesheet

    [Header("Shop")]
    public int  price;      // StarCoins cost
    public bool isDefault;  // owned from the start, no purchase required
}
