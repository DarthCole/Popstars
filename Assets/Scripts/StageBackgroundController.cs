using UnityEngine;

/// <summary>
/// Attach to the StageBackground GameObject in the Stage scene.
/// On Start it reads the equipped stage's background sprite from StageSessionData
/// and applies it to the SpriteRenderer on this GameObject.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class StageBackgroundController : MonoBehaviour
{
    void Start()
    {
        if (StageSessionData.Instance == null) return;

        var bg = StageSessionData.Instance.backgroundSprite;
        if (bg != null)
            GetComponent<SpriteRenderer>().sprite = bg;
    }
}
