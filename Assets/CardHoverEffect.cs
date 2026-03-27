using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class CardHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TMP_Text cardLabel;
    private Vector3 originalScale;
    public float hoverScale = 1.15f;
    public float fadeDuration = 0.2f;
    private bool isHovering = false;
    private float fadeTimer = 0f;

    void Start()
    {
        originalScale = transform.localScale;
        if (cardLabel == null)
            cardLabel = GetComponentInChildren<TMP_Text>(true);

        if (cardLabel != null)
        {
            Color c = cardLabel.color;
            c.a = 0f;
            cardLabel.color = c;
        }
    }

    void Update()
    {
        if (cardLabel == null) return;

        // Move timer toward 1 when hovering, toward 0 when not
        if (isHovering)
            fadeTimer = Mathf.MoveTowards(fadeTimer, 1f, Time.deltaTime / fadeDuration);
        else
            fadeTimer = Mathf.MoveTowards(fadeTimer, 0f, Time.deltaTime / fadeDuration);

        // Apply alpha
        Color col = cardLabel.color;
        col.a = fadeTimer;
        cardLabel.color = col;

        // Scale
        transform.localScale = Vector3.Lerp(originalScale, originalScale * hoverScale, fadeTimer);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
    }
}