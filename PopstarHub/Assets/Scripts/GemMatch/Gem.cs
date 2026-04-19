// File: Assets/Scripts/GemMatch/Gem.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

public class Gem : MonoBehaviour, IPointerClickHandler
{
    public enum GemType
    {
        Red,
        Blue,
        Green,
        Yellow,
        Purple,
        Orange,
        Cyan
    }

    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI iconText;
    [SerializeField] private Image symbolImage;
    [SerializeField] private Image glossImage;
    [SerializeField] private Image rimImage;
    [SerializeField] private Image obstacleOverlay;

    private GemType type;
    private int row;
    private int column;
    private GemInputHandler inputHandler;
    private PowerUpType currentPowerUp = PowerUpType.None;
    private ObstacleType currentObstacle = ObstacleType.None;
    private bool isStarGem; // for CollectStars objective

    private static readonly Color[] gemColors = new Color[]
    {
        new Color(0.9f, 0.2f, 0.3f),
        new Color(0.2f, 0.4f, 0.9f),
        new Color(0.2f, 0.8f, 0.4f),
        new Color(1.0f, 0.8f, 0.1f),
        new Color(0.6f, 0.2f, 0.9f),
        new Color(1.0f, 0.55f, 0.1f),
        new Color(0.1f, 0.85f, 0.85f)
    };

    // Power-up display icons
    private static readonly string[] powerUpIcons = new string[]
    {
        "",            // None
        "↔",          // Striped horizontal
        "↕",          // Striped vertical
        "✸",          // Wrapped
        "✺"           // Color bomb
    };

    private static readonly string[] fallbackPowerUpIcons =
    {
        "",
        "H",
        "V",
        "W",
        "B"
    };

    private static readonly Color[] GemPalette =
    {
        new Color(1.00f, 0.20f, 0.42f),
        new Color(0.12f, 0.76f, 1.00f),
        new Color(0.18f, 1.00f, 0.58f),
        new Color(1.00f, 0.90f, 0.10f),
        new Color(0.84f, 0.42f, 1.00f),
        new Color(1.00f, 0.60f, 0.15f),
        new Color(0.15f, 0.92f, 0.92f)
    };

    private static Sprite defaultUiSprite;
    private static readonly Dictionary<GemType, Sprite> symbolSpriteCache = new Dictionary<GemType, Sprite>();
    public GemType Type => type;
    public int Row => row;
    public int Column => column;
    public PowerUpType CurrentPowerUp => currentPowerUp;
    public ObstacleType CurrentObstacle => currentObstacle;
    public bool IsStarGem => isStarGem;

    /// <summary>Returns true if this gem has an obstacle that prevents swapping.</summary>
    public bool IsSwapBlocked => currentObstacle == ObstacleType.Chain;

    private void Awake()
    {
        EnsureVisualReferences();
    }

    public void Initialize(GemType gemType, int row, int column)
    {
        EnsureVisualReferences();

        this.type = gemType;
        this.row = row;
        this.column = column;
        this.currentPowerUp = PowerUpType.None;
        this.currentObstacle = ObstacleType.None;
        this.isStarGem = false;

        image.color = GetBodyColor();
        image.type = Image.Type.Simple;
        image.raycastTarget = true;

        if (iconText != null)
        {
            iconText.text = string.Empty;
            iconText.gameObject.SetActive(false);
        }

        if (symbolImage != null)
        {
            symbolImage.sprite = GetOrCreateGemSymbol(gemType);
            symbolImage.color = new Color(1f, 1f, 1f, 0.98f);
            symbolImage.gameObject.SetActive(true);
        }

        if (obstacleOverlay != null)
            obstacleOverlay.gameObject.SetActive(false);

        ApplyLayerVisuals();
    }

    /// <summary>
    /// Mark this gem as a star gem (for CollectStars objective).
    /// </summary>
    public void SetAsStarGem()
    {
        isStarGem = true;
        // Add a golden border/glow to indicate star gem
        if (rimImage != null)
            rimImage.color = new Color(1f, 0.85f, 0.1f, 0.6f);
        if (glossImage != null)
            glossImage.color = new Color(1f, 0.9f, 0.3f, 0.4f);
    }

    /// <summary>
    /// Apply an obstacle to this gem.
    /// </summary>
    public void SetObstacle(ObstacleType obstacle)
    {
        currentObstacle = obstacle;
        UpdateObstacleVisual();
    }

    /// <summary>
    /// Hit the obstacle (e.g. when adjacent match happens for ice, or gem is matched for chain).
    /// Returns true if the obstacle is now removed and gem can be cleared.
    /// </summary>
    public bool HitObstacle()
    {
        switch (currentObstacle)
        {
            case ObstacleType.Ice:
                currentObstacle = ObstacleType.None;
                UpdateObstacleVisual();
                return true; // ice is removed, gem is freed but not cleared by this hit

            case ObstacleType.Chain:
                currentObstacle = ObstacleType.None;
                UpdateObstacleVisual();
                return false; // chain is removed, gem survives and can now be matched normally

            default:
                return true;
        }
    }

    private void UpdateObstacleVisual()
    {
        EnsureVisualReferences();

        if (obstacleOverlay == null) return;

        switch (currentObstacle)
        {
            case ObstacleType.Ice:
                obstacleOverlay.gameObject.SetActive(true);
                obstacleOverlay.color = new Color(0.7f, 0.9f, 1f, 0.55f);
                break;

            case ObstacleType.Chain:
                obstacleOverlay.gameObject.SetActive(true);
                obstacleOverlay.color = new Color(0.5f, 0.45f, 0.4f, 0.7f);
                break;

            default:
                obstacleOverlay.gameObject.SetActive(false);
                break;
        }
    }

    public void SetPowerUp(PowerUpType powerUp)
    {
        currentPowerUp = powerUp;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        EnsureVisualReferences();

        if (currentPowerUp == PowerUpType.None)
        {
            image.color = GetBodyColor();
            if (symbolImage != null)
            {
                symbolImage.sprite = GetOrCreateGemSymbol(type);
                symbolImage.color = new Color(1f, 1f, 1f, 0.98f);
                symbolImage.gameObject.SetActive(true);
            }

            if (iconText != null)
            {
                iconText.text = string.Empty;
                iconText.gameObject.SetActive(false);
            }

            ApplyLayerVisuals();
            return;
        }

        if (currentPowerUp == PowerUpType.ColorBomb)
        {
            image.color = new Color(0.21f, 0.10f, 0.30f, 0.98f);

            if (symbolImage != null)
                symbolImage.gameObject.SetActive(false);

            iconText.text = GetPowerUpIcon(currentPowerUp);
            iconText.fontSize = 50;
            iconText.color = new Color(1f, 0.88f, 0.25f, 1f);
            iconText.fontStyle = FontStyles.Bold;
            iconText.outlineWidth = 0.22f;
            iconText.outlineColor = new Color(0.36f, 0.06f, 0.46f, 0.95f);
            iconText.gameObject.SetActive(true);
            ApplyLayerVisuals();
            return;
        }

        if (symbolImage != null)
            symbolImage.gameObject.SetActive(false);

        iconText.text = GetPowerUpIcon(currentPowerUp);
        iconText.fontSize = 46;
        iconText.color = Color.Lerp(GetBaseColor(), Color.white, 0.35f);
        iconText.fontStyle = FontStyles.Bold;
        iconText.outlineWidth = 0.2f;
        iconText.outlineColor = new Color(0.24f, 0.08f, 0.33f, 0.95f);
        iconText.gameObject.SetActive(true);

        image.color = Color.Lerp(GetBodyColor(), Color.white, 0.18f);
        ApplyLayerVisuals();
    }

    private Color GetBaseColor()
    {
        int index = Mathf.Clamp((int)type, 0, GemPalette.Length - 1);
        Color baseColor = GemPalette[index];
        return currentPowerUp != PowerUpType.None ? Color.Lerp(baseColor, Color.white, 0.15f) : baseColor;
    }

    private Color GetBodyColor()
    {
        Color baseColor = GetBaseColor();
        return new Color(baseColor.r, baseColor.g, baseColor.b, 1f);
    }

    private void EnsureVisualReferences()
    {
        if (defaultUiSprite == null)
            defaultUiSprite = CreateCircularGemSprite();

        if (image == null)
            image = GetComponent<Image>();

        if (image == null)
        {
            GameObject imageObj = new GameObject("GemImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            imageObj.transform.SetParent(transform, false);
            RectTransform imageRect = imageObj.GetComponent<RectTransform>();
            imageRect.anchorMin = Vector2.zero;
            imageRect.anchorMax = Vector2.one;
            imageRect.offsetMin = Vector2.zero;
            imageRect.offsetMax = Vector2.zero;
            image = imageObj.GetComponent<Image>();
        }

        if (image.sprite == null && defaultUiSprite != null)
            image.sprite = defaultUiSprite;

        image.type = Image.Type.Simple;
        image.preserveAspect = false;

        if (iconText == null)
        {
            Transform iconChild = transform.Find("GemIconText");
            if (iconChild != null)
                iconText = iconChild.GetComponent<TextMeshProUGUI>();
        }

        if (iconText == null)
        {
            iconText = GetComponentInChildren<TextMeshProUGUI>();
            if (iconText != null)
                iconText.gameObject.name = "GemIconText";
        }

        if (iconText == null)
        {
            GameObject iconObj = new GameObject("GemIconText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            iconObj.transform.SetParent(transform, false);
            RectTransform textRect = iconObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            iconText = iconObj.GetComponent<TextMeshProUGUI>();
            iconText.alignment = TextAlignmentOptions.Center;
            iconText.fontSize = 40;
            iconText.color = Color.white;
            iconText.raycastTarget = false;

            if (TMP_Settings.defaultFontAsset != null)
                iconText.font = TMP_Settings.defaultFontAsset;
        }

        if (iconText != null)
        {
            if (TMP_Settings.defaultFontAsset != null)
                iconText.font = TMP_Settings.defaultFontAsset;

            iconText.raycastTarget = false;
            iconText.alignment = TextAlignmentOptions.Center;
            iconText.textWrappingMode = TextWrappingModes.NoWrap;
        }

        if (symbolImage == null)
            symbolImage = EnsureOverlay("GemSymbol", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(50f, 50f), Color.white);

        if (glossImage == null)
            glossImage = EnsureOverlay("GemGloss", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -2f), new Vector2(66f, 26f), new Color(1f, 1f, 1f, 0.23f));

        if (rimImage == null)
            rimImage = EnsureOverlay("GemRim", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(82f, 82f), new Color(1f, 1f, 1f, 0.13f));

        // Obstacle overlay — sits on top of everything
        if (obstacleOverlay == null)
            obstacleOverlay = EnsureOverlay("ObstacleOverlay", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(90f, 90f), Color.clear);

        if (obstacleOverlay != null)
        {
            obstacleOverlay.gameObject.SetActive(false);
            obstacleOverlay.transform.SetAsLastSibling();
        }
    }

    private Image EnsureOverlay(
        string objectName,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Vector2 size,
        Color color)
    {
        Transform existing = transform.Find(objectName);
        GameObject overlayObject;

        if (existing != null)
        {
            overlayObject = existing.gameObject;
        }
        else
        {
            overlayObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            overlayObject.transform.SetParent(transform, false);
            overlayObject.transform.SetAsLastSibling();
        }

        RectTransform rect = overlayObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Image overlay = overlayObject.GetComponent<Image>();
        if (overlay.sprite == null && defaultUiSprite != null)
            overlay.sprite = defaultUiSprite;

        overlay.type = Image.Type.Simple;
        overlay.color = color;
        overlay.raycastTarget = false;
        return overlay;
    }

    private string GetIconForType(GemType gemType)
    {
        return string.Empty;
    }

    private string GetPowerUpIcon(PowerUpType powerUpType)
    {
        int index = Mathf.Clamp((int)powerUpType, 0, powerUpIcons.Length - 1);
        string primary = powerUpIcons[index];
        if (IsGlyphSupported(primary))
            return primary;

        string fallback = fallbackPowerUpIcons[index];
        return IsGlyphSupported(fallback) ? fallback : "!";
    }

    private bool IsGlyphSupported(string icon)
    {
        if (iconText == null || iconText.font == null || string.IsNullOrEmpty(icon))
            return false;

        return iconText.font.HasCharacter(icon[0]);
    }

    private static Sprite CreateCircularGemSprite()
    {
        const int size = 128;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = (size - 2) * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                float normalized = dist / radius;

                if (normalized > 1f)
                {
                    texture.SetPixel(x, y, Color.clear);
                    continue;
                }

                float edgeFade = Mathf.SmoothStep(1f, 0f, Mathf.Clamp01((normalized - 0.86f) / 0.14f));
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, edgeFade));
            }
        }

        texture.Apply();

        Rect rect = new Rect(0f, 0f, size, size);
        Sprite sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), 100f);
        if (sprite == null)
            return null;

        sprite.name = "RuntimeGemCircle";
        return sprite;
    }

    private void ApplyDefaultIconStyle()
    {
        iconText.color = new Color(1f, 1f, 1f, 1f);
        iconText.fontSize = 44;
        iconText.fontStyle = FontStyles.Bold;
        iconText.outlineWidth = 0.2f;
        iconText.outlineColor = new Color(0.20f, 0.06f, 0.30f, 0.78f);
    }

    private Sprite GetOrCreateGemSymbol(GemType gemType)
    {
        if (symbolSpriteCache.TryGetValue(gemType, out Sprite cached) && cached != null)
            return cached;

        Sprite created = CreateGemSymbolSprite(gemType);
        symbolSpriteCache[gemType] = created;
        return created;
    }

    private static Sprite CreateGemSymbolSprite(GemType gemType)
    {
        const int size = 96;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float scale = size * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float px = (x - center.x) / scale;
                float py = (y - center.y) / scale;
                float d = SignedDistanceGemSymbol(gemType, px, py);
                float alpha = 1f - Mathf.Clamp01((d + 0.04f) / 0.08f);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
        sprite.name = "GemSymbol_" + gemType;
        return sprite;
    }

    private static float SignedDistanceGemSymbol(GemType gemType, float x, float y)
    {
        float circle = Mathf.Sqrt(x * x + y * y) - 0.85f;

        switch (gemType)
        {
            case GemType.Red:
            {
                float a = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y)) - 0.17f;
                float b = Mathf.Max(Mathf.Abs((x + y) * 0.7071067f), Mathf.Abs((x - y) * 0.7071067f)) - 0.17f;
                return Mathf.Max(Mathf.Min(a, b), circle);
            }
            case GemType.Blue:
                return Mathf.Abs(x) + Mathf.Abs(y) - 0.72f;
            case GemType.Green:
            {
                float c1 = Mathf.Sqrt((x - 0.28f) * (x - 0.28f) + y * y) - 0.23f;
                float c2 = Mathf.Sqrt((x + 0.28f) * (x + 0.28f) + y * y) - 0.23f;
                float c3 = Mathf.Sqrt(x * x + (y - 0.28f) * (y - 0.28f)) - 0.23f;
                float c4 = Mathf.Sqrt(x * x + (y + 0.28f) * (y + 0.28f)) - 0.23f;
                return Mathf.Min(Mathf.Min(c1, c2), Mathf.Min(c3, c4));
            }
            case GemType.Yellow:
            {
                float outer = Mathf.Sqrt(x * x + y * y) - 0.70f;
                float inner = 0.28f - Mathf.Sqrt(x * x + y * y);
                return Mathf.Max(outer, inner);
            }
            case GemType.Purple:
            {
                float hex = Mathf.Max(Mathf.Abs(x) * 0.8660254f + Mathf.Abs(y) * 0.5f, Mathf.Abs(y)) - 0.68f;
                return hex;
            }
            case GemType.Orange:
            {
                float tri = Mathf.Max(Mathf.Abs(x) * 0.866f + y * 0.5f, -y) - 0.52f;
                return tri;
            }
            case GemType.Cyan:
            default:
            {
                float angle1 = Mathf.Abs(x) * 0.9511f + y * 0.309f;
                float angle2 = Mathf.Abs(x) * 0.5878f - y * 0.809f;
                float pent = Mathf.Max(Mathf.Max(angle1, angle2), -y * 0.85f) - 0.55f;
                return pent;
            }
        }
    }

    private void ApplyLayerVisuals()
    {
        if (glossImage != null)
            glossImage.color = currentPowerUp == PowerUpType.None
                ? new Color(1f, 1f, 1f, 0.30f)
                : new Color(1f, 1f, 1f, 0.36f);

        if (rimImage != null && !isStarGem)
            rimImage.color = currentPowerUp == PowerUpType.None
                ? new Color(1f, 1f, 1f, 0.2f)
                : new Color(1f, 0.94f, 0.62f, 0.3f);
    }

    public void SetInputHandler(GemInputHandler handler)
    {
        inputHandler = handler;
    }

    public void SetPosition(int newRow, int newColumn)
    {
        row = newRow;
        column = newColumn;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (inputHandler != null)
        {
            inputHandler.OnGemClicked(this);
        }
    }
}