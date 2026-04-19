// File: Assets/Scripts/GemMatch/GemAnimator.cs
using System.Collections;
using UnityEngine;

public class GemAnimator : MonoBehaviour
{
    [SerializeField] private float swapDuration = 0.2f;
    [SerializeField] private float clearDuration = 0.15f;
    [SerializeField] private float collapseDuration = 0.2f;

    public IEnumerator AnimateSwap(RectTransform rectA, RectTransform rectB)
    {
        Vector2 startA = rectA.anchoredPosition;
        Vector2 startB = rectB.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < swapDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / swapDuration;

            // Smooth ease-in-out curve
            t = t * t * (3f - 2f * t);

            rectA.anchoredPosition = Vector2.Lerp(startA, startB, t);
            rectB.anchoredPosition = Vector2.Lerp(startB, startA, t);
            yield return null;
        }

        rectA.anchoredPosition = startB;
        rectB.anchoredPosition = startA;
    }

    public IEnumerator AnimateClear(Gem gem)
    {
        RectTransform rect = gem.GetComponent<RectTransform>();
        Vector3 startScale = rect.localScale;
        float elapsed = 0f;

        while (elapsed < clearDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / clearDuration;
            rect.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            yield return null;
        }

        rect.localScale = Vector3.zero;
    }

    public IEnumerator AnimateCollapse(RectTransform rect, Vector2 targetPosition)
    {
        Vector2 startPosition = rect.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < collapseDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / collapseDuration;

            // Slight bounce at the end
            t = t * t * (3f - 2f * t);

            rect.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        rect.anchoredPosition = targetPosition;
    }

    public float SwapDuration => swapDuration;
    public float ClearDuration => clearDuration;
    public float CollapseDuration => collapseDuration;
}