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

    private void Awake()
    {
        EnsureReferences();
    }

    public void ShowPopup(Vector2 position, string text, Color color)
    {
        EnsureReferences();
        if (popupPrefab == null || popupParent == null)
            return;

        GameObject popup = Instantiate(popupPrefab, popupParent);
        RectTransform rect = popup.GetComponent<RectTransform>();
        if (rect == null)
        {
            Destroy(popup);
            return;
        }

        rect.anchoredPosition = position;

        TextMeshProUGUI tmpText = popup.GetComponent<TextMeshProUGUI>();
        if (tmpText == null)
        {
            Destroy(popup);
            return;
        }

        tmpText.text = text;
        TMP_FontAsset preferred = UIHierarchyBuilder.GetPreferredFont();
        if (preferred != null)
            tmpText.font = preferred;

        tmpText.fontSize = Mathf.Max(tmpText.fontSize, 42f);
        tmpText.fontStyle = FontStyles.Bold;
        tmpText.color = color;
        tmpText.raycastTarget = false;

        StartCoroutine(AnimatePopup(rect, tmpText));
    }

    private void EnsureReferences()
    {
        Canvas canvas = UIHierarchyBuilder.GetOrCreateCanvas("UICanvas");

        if (popupParent == null || popupParent.GetComponent<RectTransform>() == null)
        {
            Transform boardContainer = canvas.transform.Find("GemBoardContainer");
            popupParent = boardContainer != null ? boardContainer : canvas.transform;
        }

        if (popupPrefab == null)
        {
            GameObject fallback = new GameObject("ScorePopupRuntime", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            RectTransform rect = fallback.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(300f, 60f);
            TextMeshProUGUI tmp = fallback.GetComponent<TextMeshProUGUI>();
            TMP_FontAsset preferred = UIHierarchyBuilder.GetPreferredFont();
            if (preferred != null)
                tmp.font = preferred;
            else if (TMP_Settings.defaultFontAsset != null)
                tmp.font = TMP_Settings.defaultFontAsset;
            tmp.fontSize = 42;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.yellow;
            popupPrefab = fallback;
            fallback.SetActive(false);
        }
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