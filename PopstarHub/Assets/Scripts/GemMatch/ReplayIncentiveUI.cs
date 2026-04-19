// File: Assets/Scripts/GemMatch/ReplayIncentiveUI.cs
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Shows "Best: XXXX" and a star progress bar during gameplay.
/// Flashes "NEW BEST!" when the player beats their previous record.
/// </summary>
public class ReplayIncentiveUI : MonoBehaviour
{
    private TextMeshProUGUI bestScoreText;
    private TextMeshProUGUI newBestText;
    private Image progressBarBg;
    private Image progressBarFill;
    private Image star1Marker;
    private Image star2Marker;
    private Image star3Marker;

    private int previousBest;
    private bool hasShownNewBest;
    private LevelData currentLevel;
    private Coroutine newBestAnim;

    private void Awake()
    {
        EnsureUI();
    }

    /// <summary>
    /// Initialize for a new level.
    /// </summary>
    public void Initialize(LevelData level)
    {
        EnsureUI();
        currentLevel = level;
        hasShownNewBest = false;

        if (level == null)
        {
            SetVisible(false);
            return;
        }

        // Load previous best for this level
        previousBest = PlayerPrefs.GetInt($"LevelBestScore_{level.levelNumber}", 0);

        if (bestScoreText != null)
        {
            bestScoreText.text = previousBest > 0 ? $"Best: {previousBest}" : "Best: ---";
            bestScoreText.gameObject.SetActive(true);
        }

        if (newBestText != null)
            newBestText.gameObject.SetActive(false);

        UpdateProgressBar(0);
        SetVisible(true);
    }

    /// <summary>
    /// Update with current score. Called by ScoreManager after each scoring event.
    /// </summary>
    public void UpdateScore(int currentScore)
    {
        if (currentLevel == null) return;

        UpdateProgressBar(currentScore);

        // Check for new best
        if (currentScore > previousBest && !hasShownNewBest && previousBest > 0)
        {
            hasShownNewBest = true;
            ShowNewBestFlash();

            // Save new best
            PlayerPrefs.SetInt($"LevelBestScore_{currentLevel.levelNumber}", currentScore);
            PlayerPrefs.Save();
        }
        else if (currentScore > previousBest)
        {
            // Silently save new best even if no flash (first play)
            PlayerPrefs.SetInt($"LevelBestScore_{currentLevel.levelNumber}", currentScore);
            PlayerPrefs.Save();
        }
    }

    public void SetVisible(bool visible)
    {
        if (bestScoreText != null) bestScoreText.gameObject.SetActive(visible);
        if (progressBarBg != null) progressBarBg.gameObject.SetActive(visible);
        if (progressBarFill != null) progressBarFill.gameObject.SetActive(visible);
        if (star1Marker != null) star1Marker.gameObject.SetActive(visible);
        if (star2Marker != null) star2Marker.gameObject.SetActive(visible);
        if (star3Marker != null) star3Marker.gameObject.SetActive(visible);
        if (!visible && newBestText != null) newBestText.gameObject.SetActive(false);
    }

    private void UpdateProgressBar(int score)
    {
        if (currentLevel == null || progressBarFill == null) return;

        float maxScore = currentLevel.threeStarScore;
        float fill = Mathf.Clamp01(score / maxScore);

        RectTransform fillRect = progressBarFill.GetComponent<RectTransform>();
        // Adjust width relative to background
        RectTransform bgRect = progressBarBg.GetComponent<RectTransform>();
        float maxWidth = bgRect.sizeDelta.x - 4f;
        fillRect.sizeDelta = new Vector2(maxWidth * fill, fillRect.sizeDelta.y);

        // Color shifts from red → yellow → green as you progress
        if (fill < 0.33f)
            progressBarFill.color = Color.Lerp(new Color(1f, 0.3f, 0.3f), new Color(1f, 0.8f, 0.2f), fill / 0.33f);
        else if (fill < 0.66f)
            progressBarFill.color = Color.Lerp(new Color(1f, 0.8f, 0.2f), new Color(0.3f, 1f, 0.5f), (fill - 0.33f) / 0.33f);
        else
            progressBarFill.color = new Color(0.3f, 1f, 0.5f);

        // Update star marker colors
        UpdateStarMarker(star1Marker, score >= currentLevel.targetScore);
        UpdateStarMarker(star2Marker, score >= currentLevel.twoStarScore);
        UpdateStarMarker(star3Marker, score >= currentLevel.threeStarScore);
    }

    private void UpdateStarMarker(Image marker, bool achieved)
    {
        if (marker == null) return;
        marker.color = achieved
            ? new Color(1f, 0.85f, 0.1f, 1f)
            : new Color(0.4f, 0.3f, 0.5f, 0.5f);
    }

    private void ShowNewBestFlash()
    {
        if (newBestText == null) return;

        if (newBestAnim != null)
            StopCoroutine(newBestAnim);

        newBestText.text = "NEW BEST!";
        newBestText.gameObject.SetActive(true);
        newBestAnim = StartCoroutine(AnimateNewBest());
    }

    private IEnumerator AnimateNewBest()
    {
        RectTransform rect = newBestText.GetComponent<RectTransform>();
        float duration = 2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Scale bounce
            float scale = t < 0.2f
                ? Mathf.Lerp(0f, 1.3f, t / 0.2f)
                : t < 0.35f
                    ? Mathf.Lerp(1.3f, 1f, (t - 0.2f) / 0.15f)
                    : 1f;
            rect.localScale = Vector3.one * scale;

            // Fade out in last 0.5s
            float alpha = t > 0.75f ? Mathf.Lerp(1f, 0f, (t - 0.75f) / 0.25f) : 1f;
            newBestText.color = new Color(1f, 0.9f, 0.1f, alpha);

            yield return null;
        }

        newBestText.gameObject.SetActive(false);
        newBestAnim = null;
    }

    private void EnsureUI()
    {
        if (bestScoreText != null) return;

        Canvas canvas = UIHierarchyBuilder.GetOrCreateCanvas("UICanvas");

        // Best score text
        bestScoreText = UIHierarchyBuilder.GetOrCreateTMPText(
            canvas.transform,
            "BestScoreText",
            "Best: ---",
            new Vector2(-700f, 365f),
            new Vector2(420f, 40f),
            24,
            TMPro.TextAlignmentOptions.Left,
            new Color(0.7f, 0.6f, 0.85f, 0.8f));
        bestScoreText.gameObject.SetActive(false);

        // New Best flash text
        newBestText = UIHierarchyBuilder.GetOrCreateTMPText(
            canvas.transform,
            "NewBestText",
            "NEW BEST!",
            new Vector2(0f, 250f),
            new Vector2(400f, 80f),
            52,
            TMPro.TextAlignmentOptions.Center,
            new Color(1f, 0.9f, 0.1f, 1f));
        newBestText.gameObject.SetActive(false);

        // Progress bar background
        Transform barBgT = canvas.transform.Find("StarProgressBarBg");
        GameObject barBgObj;
        if (barBgT != null) barBgObj = barBgT.gameObject;
        else
        {
            barBgObj = new GameObject("StarProgressBarBg", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            barBgObj.transform.SetParent(canvas.transform, false);
        }
        RectTransform barBgRect = barBgObj.GetComponent<RectTransform>();
        barBgRect.anchorMin = new Vector2(0.5f, 0.5f);
        barBgRect.anchorMax = new Vector2(0.5f, 0.5f);
        barBgRect.pivot = new Vector2(0f, 0.5f);
        barBgRect.anchoredPosition = new Vector2(-910f, 335f);
        barBgRect.sizeDelta = new Vector2(380f, 16f);
        progressBarBg = barBgObj.GetComponent<Image>();
        progressBarBg.color = new Color(0.2f, 0.12f, 0.3f, 0.7f);
        progressBarBg.raycastTarget = false;
        barBgObj.SetActive(false);

        // Progress bar fill
        Transform barFillT = barBgObj.transform.Find("StarProgressBarFill");
        GameObject barFillObj;
        if (barFillT != null) barFillObj = barFillT.gameObject;
        else
        {
            barFillObj = new GameObject("StarProgressBarFill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            barFillObj.transform.SetParent(barBgObj.transform, false);
        }
        RectTransform barFillRect = barFillObj.GetComponent<RectTransform>();
        barFillRect.anchorMin = new Vector2(0f, 0.5f);
        barFillRect.anchorMax = new Vector2(0f, 0.5f);
        barFillRect.pivot = new Vector2(0f, 0.5f);
        barFillRect.anchoredPosition = new Vector2(2f, 0f);
        barFillRect.sizeDelta = new Vector2(0f, 12f);
        progressBarFill = barFillObj.GetComponent<Image>();
        progressBarFill.color = new Color(0.3f, 1f, 0.5f, 1f);
        progressBarFill.raycastTarget = false;

        // Star markers (small dots at star thresholds)
        star1Marker = CreateStarMarker(barBgObj.transform, "Star1Marker", 0.33f, barBgRect.sizeDelta.x);
        star2Marker = CreateStarMarker(barBgObj.transform, "Star2Marker", 0.55f, barBgRect.sizeDelta.x);
        star3Marker = CreateStarMarker(barBgObj.transform, "Star3Marker", 1.0f, barBgRect.sizeDelta.x);
    }

    private Image CreateStarMarker(Transform parent, string name, float normalizedX, float barWidth)
    {
        Transform existing = parent.Find(name);
        GameObject obj;
        if (existing != null) obj = existing.gameObject;
        else
        {
            obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            obj.transform.SetParent(parent, false);
        }
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0.5f);
        rect.anchorMax = new Vector2(0f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(normalizedX * barWidth, 0f);
        rect.sizeDelta = new Vector2(8f, 20f);

        Image img = obj.GetComponent<Image>();
        img.color = new Color(0.4f, 0.3f, 0.5f, 0.5f);
        img.raycastTarget = false;
        return img;
    }
}
