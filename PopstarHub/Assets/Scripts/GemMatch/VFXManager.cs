// File: Assets/Scripts/GemMatch/VFXManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Procedurally generated visual effects: match bursts, screen shake, confetti, star bursts.
/// All particles are UI Image objects — no external assets needed.
/// </summary>
public class VFXManager : MonoBehaviour
{
    private Transform vfxParent;
    private RectTransform boardContainer;
    private static Sprite particleSprite;

    private void Awake()
    {
        EnsureReferences();
    }

    /// <summary>
    /// Spawn a burst of colored particles at a position (anchored coordinates).
    /// </summary>
    public void SpawnMatchBurst(Vector2 position, Color gemColor)
    {
        EnsureReferences();
        int particleCount = Random.Range(6, 10);
        for (int i = 0; i < particleCount; i++)
        {
            StartCoroutine(AnimateParticle(position, gemColor, 0.5f, 120f));
        }
    }

    /// <summary>
    /// Shake the board container to create a screen shake effect.
    /// </summary>
    public void TriggerScreenShake(float intensity = 8f)
    {
        EnsureReferences();
        if (boardContainer != null)
            StartCoroutine(ShakeCoroutine(boardContainer, intensity, 0.3f));
    }

    /// <summary>
    /// Spawn confetti particles falling from above on level complete.
    /// </summary>
    public void SpawnConfetti()
    {
        EnsureReferences();
        for (int i = 0; i < 35; i++)
        {
            Color confettiColor = Random.ColorHSV(0f, 1f, 0.7f, 1f, 0.8f, 1f);
            Vector2 startPos = new Vector2(Random.Range(-500f, 500f), 600f);
            StartCoroutine(AnimateConfettiParticle(startPos, confettiColor));
        }
    }

    /// <summary>
    /// Spawn golden star-shaped burst for earning stars.
    /// </summary>
    public void SpawnStarBurst(Vector2 position)
    {
        EnsureReferences();
        Color starGold = new Color(1f, 0.85f, 0.1f);
        for (int i = 0; i < 12; i++)
        {
            StartCoroutine(AnimateParticle(position, starGold, 0.7f, 180f));
        }
    }

    private IEnumerator AnimateParticle(Vector2 origin, Color color, float lifetime, float speed)
    {
        GameObject particle = CreateParticleObject(origin, color, Random.Range(8f, 16f));
        RectTransform rect = particle.GetComponent<RectTransform>();
        Image img = particle.GetComponent<Image>();

        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float spd = Random.Range(speed * 0.5f, speed);
        Vector2 velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * spd;

        float elapsed = 0f;
        while (elapsed < lifetime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / lifetime;

            velocity.y -= 200f * Time.deltaTime; // gravity
            rect.anchoredPosition += velocity * Time.deltaTime;

            float alpha = 1f - t;
            float scale = Mathf.Lerp(1f, 0.2f, t);
            img.color = new Color(color.r, color.g, color.b, alpha);
            rect.localScale = Vector3.one * scale;

            yield return null;
        }

        Destroy(particle);
    }

    private IEnumerator AnimateConfettiParticle(Vector2 startPos, Color color)
    {
        float size = Random.Range(10f, 20f);
        GameObject particle = CreateParticleObject(startPos, color, size);
        RectTransform rect = particle.GetComponent<RectTransform>();
        Image img = particle.GetComponent<Image>();

        float fallSpeed = Random.Range(150f, 350f);
        float swaySpeed = Random.Range(2f, 5f);
        float swayAmount = Random.Range(30f, 80f);
        float startDelay = Random.Range(0f, 0.5f);
        float lifetime = 3f;

        yield return new WaitForSeconds(startDelay);

        float elapsed = 0f;
        while (elapsed < lifetime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / lifetime;

            float x = startPos.x + Mathf.Sin(elapsed * swaySpeed) * swayAmount;
            float y = startPos.y - fallSpeed * elapsed;
            rect.anchoredPosition = new Vector2(x, y);

            float alpha = t > 0.7f ? Mathf.Lerp(1f, 0f, (t - 0.7f) / 0.3f) : 1f;
            img.color = new Color(color.r, color.g, color.b, alpha);

            // Rotate
            rect.localEulerAngles = new Vector3(0f, 0f, elapsed * 180f);

            yield return null;
        }

        Destroy(particle);
    }

    private IEnumerator ShakeCoroutine(RectTransform target, float intensity, float duration)
    {
        Vector2 originalPos = target.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float dampener = 1f - (elapsed / duration);
            float offsetX = Random.Range(-intensity, intensity) * dampener;
            float offsetY = Random.Range(-intensity, intensity) * dampener;
            target.anchoredPosition = originalPos + new Vector2(offsetX, offsetY);
            yield return null;
        }

        target.anchoredPosition = originalPos;
    }

    private GameObject CreateParticleObject(Vector2 position, Color color, float size)
    {
        if (particleSprite == null)
            particleSprite = CreateParticleSprite();

        GameObject obj = new GameObject("VFXParticle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        obj.transform.SetParent(vfxParent, false);

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(size, size);

        Image img = obj.GetComponent<Image>();
        img.sprite = particleSprite;
        img.color = color;
        img.raycastTarget = false;

        return obj;
    }

    private static Sprite CreateParticleSprite()
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = (size - 2) * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center) / radius;
                float alpha = Mathf.Clamp01(1f - dist);
                alpha *= alpha; // softer falloff
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();

        Sprite s = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        s.name = "VFXParticle";
        return s;
    }

    private void EnsureReferences()
    {
        if (vfxParent != null) return;

        Canvas canvas = UIHierarchyBuilder.GetOrCreateCanvas("UICanvas");

        Transform existing = canvas.transform.Find("VFXContainer");
        if (existing != null)
        {
            vfxParent = existing;
        }
        else
        {
            GameObject container = new GameObject("VFXContainer", typeof(RectTransform));
            container.transform.SetParent(canvas.transform, false);
            RectTransform rect = container.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            vfxParent = rect;
        }

        // Also try to find the board container for screen shake
        boardContainer = canvas.transform.Find("GemBoardContainer") as RectTransform;
    }
}
