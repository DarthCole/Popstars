using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton that carries avatar data from the Character Customisation scene
/// into the Stage scene. Lives across scene loads (DontDestroyOnLoad).
///
/// Usage in Avatar scene:
///   StageSessionData.Instance.SetFromOutfit(outfit);
///   SceneManager.LoadScene("StageScene");
///
/// Usage in Stage scene (handled automatically by StagePlayerController):
///   var data = StageSessionData.Instance;
/// </summary>
public class StageSessionData : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────────────────────────
    public static StageSessionData Instance { get; private set; }

    // ── Payload ───────────────────────────────────────────────────────────────
    [HideInInspector] public Sprite                    characterSprite;
    [HideInInspector] public Sprite                    idleSprite;
    [HideInInspector] public Sprite                    backgroundSprite;  // stage background from StageData
    [HideInInspector] public RuntimeAnimatorController animatorController;
    [HideInInspector] public int                       gender;   // 0 = Female, 1 = Male
    [HideInInspector] public string                    outfitName;

    // ════════════════════════════════════════════════════════════════════════
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// Call this before loading StageScene. Pass the currently equipped OutfitData.
    public void SetFromOutfit(OutfitData outfit, int selectedGender = 0)
    {
        if (outfit == null) return;
        characterSprite    = outfit.characterSprite;
        animatorController = outfit.animatorController;
        outfitName         = outfit.outfitName;
        gender             = selectedGender;
    }

    /// Convenience overload — reads directly from an AvatarOutfitManager.
    public void SetFromManager(AvatarOutfitManager manager, int selectedGender = 0)
    {
        if (manager == null) return;
        var outfit = manager.GetOutfit(manager.CurrentIndex);
        SetFromOutfit(outfit, selectedGender);
    }

    /// Navigate to the Stage scene after storing data.
    public static void GoToStage(AvatarOutfitManager manager, int selectedGender = 0)
    {
        // Ensure the singleton exists in the scene
        if (Instance == null)
        {
            var go = new GameObject("StageSessionData");
            go.AddComponent<StageSessionData>();
        }
        Instance.SetFromManager(manager, selectedGender);
        SceneManager.LoadScene("StageScene");
    }
}
