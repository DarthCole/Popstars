using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 originalScale;
    public float hoverScale = 1.2f;
    public float fadeDuration = 0.15f;
    private float fadeTimer = 0f;
    private bool isHovering = false;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        if (isHovering)
            fadeTimer = Mathf.MoveTowards(fadeTimer, 1f, Time.deltaTime / fadeDuration);
        else
            fadeTimer = Mathf.MoveTowards(fadeTimer, 0f, Time.deltaTime / fadeDuration);

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