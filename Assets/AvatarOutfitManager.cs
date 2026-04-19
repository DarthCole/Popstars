using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach to the avatar GameObject (AvatarDisplay inside AvatarZone).
/// Manages outfit swapping — changing the Image sprite and optional
/// AnimatorController to match the selected OutfitData.
/// </summary>
public class AvatarOutfitManager : MonoBehaviour
{
    [Header("Avatar Components")]
    public Image          avatarImage;      // visible UI Image on AvatarDisplay
    public SpriteRenderer animationSource; // hidden child SR driven by Animator
    public Animator       avatarAnimator;  // on the hidden AnimationSource child

    [Header("Outfit Catalogue")]
    public OutfitData[] outfits;

    int _currentIndex = -1;
    public int CurrentIndex => _currentIndex;
    bool _animating = false;

    void Update()
    {
        // While animation is running, copy SR sprite directly to the UI Image
        if (_animating && animationSource != null && avatarImage != null
            && animationSource.sprite != null)
        {
            avatarImage.sprite = animationSource.sprite;
        }
}

    void Start()
    {
        // Equip the default outfit, or fall back to index 0
        for (int i = 0; i < outfits.Length; i++)
        {
            if (outfits[i] != null && outfits[i].isDefault)
            {
                EquipOutfit(i);
                return;
            }
        }
        if (outfits.Length > 0) EquipOutfit(0);
    }

    /// <summary>Swap to outfit at index. Called by AvatarUIPanel buttons.</summary>
    public void EquipOutfit(int index)
    {
        if (index < 0 || index >= outfits.Length || outfits[index] == null) return;

        _currentIndex = index;
        OutfitData outfit = outfits[index];

        // Show the static character sprite; Animator stays OFF until button is pressed
        if (avatarImage != null && outfit.characterSprite != null)
            avatarImage.sprite = outfit.characterSprite;

        // Clear animationSource so the bridge can't leak stale frames from a previous outfit
        if (animationSource != null)
            animationSource.sprite = null;

        // Load the controller but keep the Animator disabled
        if (avatarAnimator != null)
        {
            if (outfit.animatorController != null)
                avatarAnimator.runtimeAnimatorController = outfit.animatorController;
            avatarAnimator.enabled = false;
        }
    }

    /// <summary>Replace the entire outfit list (called by AvatarGenderSelector on gender switch).</summary>
    public void SetOutfitCatalogue(OutfitData[] newOutfits)
    {
        outfits = newOutfits;
        _currentIndex = -1;

        // Auto-equip default in the new set
        for (int i = 0; i < outfits.Length; i++)
        {
            if (outfits[i] != null && outfits[i].isDefault)
            {
                EquipOutfit(i);
                return;
            }
        }
        if (outfits.Length > 0) EquipOutfit(0);
    }

    public OutfitData GetOutfit(int index) =>
        (index >= 0 && index < outfits.Length) ? outfits[index] : null;

    /// <summary>
    /// Plays the current outfit's animation once then returns to the static sprite.
    /// Wire to the PLAY ANIMATION button's onClick.
    /// </summary>
    public void PlayAnimation()
    {
        if (avatarAnimator == null) return;
        if (avatarAnimator.runtimeAnimatorController == null) return;

        StopAllCoroutines();
        StartCoroutine(PlayOnce());
    }

    IEnumerator PlayOnce()
    {
        _animating = true;
        if (animationSource != null) animationSource.enabled = true;
        avatarAnimator.enabled = true;

        // Wait two frames for Animator to initialise
        yield return null;
        yield return null;

        var info = avatarAnimator.GetCurrentAnimatorStateInfo(0);
        float clipLength = info.length > 0f ? info.length : 1f;

        yield return new WaitForSeconds(clipLength);

        // Done — stop animating and restore static sprite
        _animating = false;
        avatarAnimator.enabled = false;
        if (animationSource != null) animationSource.enabled = false;

        if (avatarImage != null)
        {
            var outfit = GetOutfit(_currentIndex);
            if (outfit != null && outfit.characterSprite != null)
                avatarImage.sprite = outfit.characterSprite;
        }
    }
}
