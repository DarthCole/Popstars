// File: Assets/Scripts/GemMatch/AchievementPopup.cs
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Animated achievement notification banner that slides in from the top.
/// </summary>
public class AchievementPopup : MonoBehaviour
{
    private RectTransform bannerRect;
    private TextMeshProUGUI bannerText;
    private Image bannerBg;
    private Coroutine currentAnimation;

    private void Awake()
    {
        EnsureBanner();
    }

    public void ShowAchievement(string achievementName, int coinReward)
    {
        EnsureBanner();

        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        bannerText.text = $"Achievement: {achievementName}  +{coinReward} coins";
        bannerRect.gameObject.SetActive(true);

        currentAnimation = StartCoroutine(AnimateBanner());
    }

    private IEnumerator AnimateBanner()
    {
        float showY = 420f;
        float hideY = 560f;
        float slideTime = 0.4f;
        float holdTime = 3f;

        // Slide in from top
        float elapsed = 0f;
        while (elapsed < slideTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / slideTime;
            t = t * t * (3f - 2f * t); // smoothstep
            bannerRect.anchoredPosition = new Vector2(0f, Mathf.Lerp(hideY, showY, t));
            yield return null;
        }
        bannerRect.anchoredPosition = new Vector2(0f, showY);

        // Hold
        yield return new WaitForSeconds(holdTime);

        // Slide out
        elapsed = 0f;
        while (elapsed < slideTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / slideTime;
            t = t * t * (3f - 2f * t);
            bannerRect.anchoredPosition = new Vector2(0f, Mathf.Lerp(showY, hideY, t));
            yield return null;
        }

        bannerRect.gameObject.SetActive(false);
        currentAnimation = null;
    }

    private void EnsureBanner()
    {
        if (bannerRect != null) return;

        Canvas canvas = UIHierarchyBuilder.GetOrCreateCanvas("UICanvas");

        // Banner background
        Transform existing = canvas.transform.Find("AchievementBanner");
        GameObject bannerObj;
        if (existing != null)
        {
            bannerObj = existing.gameObject;
        }
        else
        {
            bannerObj = new GameObject("AchievementBanner", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bannerObj.transform.SetParent(canvas.transform, false);
        }

        bannerRect = bannerObj.GetComponent<RectTransform>();
        bannerRect.anchorMin = new Vector2(0.5f, 0.5f);
        bannerRect.anchorMax = new Vector2(0.5f, 0.5f);
        bannerRect.pivot = new Vector2(0.5f, 0.5f);
        bannerRect.sizeDelta = new Vector2(700f, 70f);
        bannerRect.anchoredPosition = new Vector2(0f, 560f); // start offscreen

        bannerBg = bannerObj.GetComponent<Image>();
        bannerBg.color = new Color(0.15f, 0.08f, 0.30f, 0.95f);
        bannerBg.raycastTarget = false;

        // Text
        Transform textT = bannerObj.transform.Find("AchievementText");
        GameObject textObj;
        if (textT != null)
        {
            textObj = textT.gameObject;
        }
        else
        {
            textObj = new GameObject("AchievementText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(bannerObj.transform, false);
        }

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(20f, 0f);
        textRect.offsetMax = new Vector2(-20f, 0f);

        bannerText = textObj.GetComponent<TextMeshProUGUI>();
        TMP_FontAsset font = UIHierarchyBuilder.GetPreferredFont();
        if (font != null) bannerText.font = font;
        else if (TMP_Settings.defaultFontAsset != null) bannerText.font = TMP_Settings.defaultFontAsset;
        bannerText.fontSize = 28;
        bannerText.alignment = TextAlignmentOptions.Center;
        bannerText.color = new Color(1f, 0.85f, 0.1f, 1f);
        bannerText.raycastTarget = false;

        bannerObj.SetActive(false);
    }
}
