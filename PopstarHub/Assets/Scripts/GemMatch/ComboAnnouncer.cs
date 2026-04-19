// File: Assets/Scripts/GemMatch/ComboAnnouncer.cs
using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Shows escalating animated combo text during chain reactions.
/// </summary>
public class ComboAnnouncer : MonoBehaviour
{
    private TextMeshProUGUI comboText;
    private RectTransform comboRect;
    private Coroutine currentAnim;

    private static readonly string[] comboMessages =
    {
        "",          // 0
        "",          // 1 (no announcement)
        "Good!",     // 2
        "Great!",    // 3
        "Amazing!",  // 4
        "Incredible!", // 5
        "UNSTOPPABLE!" // 6+
    };

    private static readonly Color[] comboColors =
    {
        Color.white,
        Color.white,
        Color.white,                              // 2
        new Color(1f, 0.95f, 0.4f),               // 3 - yellow
        new Color(1f, 0.7f, 0.2f),                // 4 - orange
        new Color(1f, 0.35f, 0.2f),               // 5 - red-orange
        new Color(1f, 0.2f, 0.6f),                // 6+ - hot pink
    };

    private void Awake()
    {
        EnsureUI();
    }

    /// <summary>
    /// Show combo announcement for the given combo level.
    /// </summary>
    public void ShowCombo(int comboLevel)
    {
        if (comboLevel < 2) return;

        EnsureUI();

        if (currentAnim != null)
            StopCoroutine(currentAnim);

        int index = Mathf.Min(comboLevel, comboMessages.Length - 1);
        string message = comboMessages[index];
        Color color = comboColors[Mathf.Min(index, comboColors.Length - 1)];

        comboText.text = message;
        comboText.color = color;
        comboText.fontSize = 50 + (comboLevel - 2) * 6; // gets bigger with combo
        comboRect.gameObject.SetActive(true);

        currentAnim = StartCoroutine(AnimateCombo());
    }

    private IEnumerator AnimateCombo()
    {
        float duration = 1.4f;
        float elapsed = 0f;

        // Scale up from 0 → 1.4 → 1.0 in first 0.3s
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / 0.3f;
            float scale = t < 0.6f
                ? Mathf.Lerp(0f, 1.4f, t / 0.6f)
                : Mathf.Lerp(1.4f, 1f, (t - 0.6f) / 0.4f);
            comboRect.localScale = Vector3.one * scale;
            yield return null;
        }
        comboRect.localScale = Vector3.one;

        // Hold briefly
        yield return new WaitForSeconds(0.5f);

        // Fade out
        float fadeStart = 0.8f;
        elapsed = 0f;
        Color startColor = comboText.color;
        while (elapsed < fadeStart)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeStart;
            comboText.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t);
            comboRect.anchoredPosition = new Vector2(0f, 180f + t * 40f); // drift up
            yield return null;
        }

        comboRect.gameObject.SetActive(false);
        comboRect.anchoredPosition = new Vector2(0f, 180f);
        currentAnim = null;
    }

    private void EnsureUI()
    {
        if (comboText != null) return;

        Canvas canvas = UIHierarchyBuilder.GetOrCreateCanvas("UICanvas");

        Transform existing = canvas.transform.Find("ComboAnnouncerText");
        GameObject obj;
        if (existing != null)
        {
            obj = existing.gameObject;
        }
        else
        {
            obj = new GameObject("ComboAnnouncerText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            obj.transform.SetParent(canvas.transform, false);
        }

        comboRect = obj.GetComponent<RectTransform>();
        comboRect.anchorMin = new Vector2(0.5f, 0.5f);
        comboRect.anchorMax = new Vector2(0.5f, 0.5f);
        comboRect.pivot = new Vector2(0.5f, 0.5f);
        comboRect.anchoredPosition = new Vector2(0f, 180f);
        comboRect.sizeDelta = new Vector2(600f, 100f);

        comboText = obj.GetComponent<TextMeshProUGUI>();
        TMP_FontAsset font = UIHierarchyBuilder.GetPreferredFont();
        if (font != null) comboText.font = font;
        else if (TMP_Settings.defaultFontAsset != null) comboText.font = TMP_Settings.defaultFontAsset;
        comboText.fontSize = 56;
        comboText.alignment = TextAlignmentOptions.Center;
        comboText.fontStyle = FontStyles.Bold;
        comboText.color = Color.white;
        comboText.raycastTarget = false;
        comboText.outlineWidth = 0.25f;
        comboText.outlineColor = new Color(0.15f, 0.05f, 0.25f, 0.9f);

        obj.SetActive(false);
    }
}
