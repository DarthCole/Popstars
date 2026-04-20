using UnityEngine;

/// <summary>
/// One purchasable stage item — used by both the Shop and the Stage scene.
/// Create via: Assets ▶ Create ▶ PopstarHub ▶ Stage Data
/// </summary>
[CreateAssetMenu(fileName = "NewStage", menuName = "PopstarHub/Stage Data")]
public class StageData : ScriptableObject
{
    [Header("Identity")]
    public int    stageID;
    public string stageName;

    [Header("Visuals")]
    public Sprite previewIcon;       // thumbnail shown in the shop card
    public Sprite backgroundSprite;  // full background image loaded in the stage scene

    [Header("Shop")]
    public int  price;
    public bool isDefault;           // owned from the start (base stage — no purchase needed)
}
