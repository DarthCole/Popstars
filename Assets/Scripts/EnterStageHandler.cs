using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach to any GameObject in the Avatar customisation scene.
/// Wire the "Enter Stage" button onClick to GoToStage().
/// AvatarSceneBuilder does this automatically.
/// </summary>
public class EnterStageHandler : MonoBehaviour
{
    [Header("References (wired by AvatarSceneBuilder)")]
    public AvatarOutfitManager  outfitManager;
    public AvatarGenderSelector genderSelector;

    [Header("Animator Controllers per gender")]
    [Tooltip("Drag DefaultWoman_Walk controller here")]
    public RuntimeAnimatorController femaleController;
    [Tooltip("Drag DefaultMan_Walk controller here")]
    public RuntimeAnimatorController maleController;

    [Header("Idle Sprites per gender")]
    [Tooltip("Default woman idle PNG")]
    public Sprite femaleIdleSprite;
    [Tooltip("Default man idle PNG")]
    public Sprite maleIdleSprite;

    // ── Called by Enter Stage button ─────────────────────────────────────────
    public void GoToStage()
    {
        // Read saved gender (0 = Female, 1 = Male)
        int gender = PlayerPrefs.GetInt("PopstarHub_SelectedGender", 0);

        // Ensure singleton exists
        if (StageSessionData.Instance == null)
        {
            var go = new GameObject("StageSessionData");
            go.AddComponent<StageSessionData>();
        }

        var data = StageSessionData.Instance;

        // Store outfit from currently equipped character
        data.SetFromManager(outfitManager, gender);

        // Override animator controller with the gender-specific walk controller
        // (the OutfitData controller is the avatar screen one — stage needs the walk one)
        data.animatorController = gender == 1 ? maleController : femaleController;

        // Store idle sprite so StagePlayerController can show it at rest
        data.idleSprite = gender == 1 ? maleIdleSprite : femaleIdleSprite;

        // Load equipped stage background (falls back to default stage if none equipped)
        if (StageOwnershipManager.Instance != null)
        {
            var equippedStage = StageOwnershipManager.Instance.GetEquippedStage();
            if (equippedStage != null)
                data.backgroundSprite = equippedStage.backgroundSprite;
        }

        SceneManager.LoadScene("StageScene");
    }
}
