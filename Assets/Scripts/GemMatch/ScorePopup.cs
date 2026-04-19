// File: Assets/Scripts/GemMatch/ScorePopup.cs
using System.Collections;
using UnityEngine;
using TMPro;

public class ScorePopup : MonoBehaviour
{
    [SerializeField] private GameObject popupPrefab;
    [SerializeField] private Transform popupParent;
    [SerializeField] private float floatDistance = 80f;
    [SerializeField] private float fadeDuration = 0.8f;

    public void ShowPopup(Vector2 position, string text, Color color)
    {
        GameObject popup = Instantiate(popupPrefab, popupParent);
        RectTransform rect = popup.GetComponent<RectTransform>();
        rect.anchoredPosition = position;

        TextMeshProUGUI tmpText = popup.GetComponent<TextMeshProUGUI>();
        tmpText.text = text;
        tmpText.color = color;

        StartCoroutine(AnimatePopup(rect, tmpText));
    }

    private IEnumerator AnimatePopup(RectTransform rect, TextMeshProUGUI text)
    {
        Vector2 startPos = rect.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(0f, floatDistance);
        Color startColor = text.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        float elapsed = 0f;

        // Start slightly bigger then shrink to normal
        rect.localScale = Vector3.one * 1.5f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            text.color = Color.Lerp(startColor, endColor, t);

            // Scale down from 1.5 to 1.0 in the first half
            if (t < 0.5f)
            {
                float scaleT = t / 0.5f;
                rect.localScale = Vector3.one * Mathf.Lerp(1.5f, 1f, scaleT);
            }

            yield return null;
        }

        Destroy(rect.gameObject);
    }
}