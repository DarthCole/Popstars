using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

public static class UIHierarchyBuilder
{
    private static TMP_FontAsset cachedThemeFont;

    public static Canvas GetOrCreateCanvas(string preferredName = "UICanvas")
    {
        Canvas canvas = null;

        GameObject preferred = GameObject.Find(preferredName);
        if (preferred != null)
            canvas = preferred.GetComponent<Canvas>();

        if (canvas == null)
        {
            GameObject canvasObject = new GameObject(preferredName,
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));

            canvas = canvasObject.GetComponent<Canvas>();
        }

        if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        canvas.sortingOrder = 10;

        CanvasScaler scalerComponent = canvas.GetComponent<CanvasScaler>();
        if (scalerComponent == null)
            scalerComponent = canvas.gameObject.AddComponent<CanvasScaler>();

        scalerComponent.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scalerComponent.referenceResolution = new Vector2(1920f, 1080f);
        scalerComponent.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scalerComponent.matchWidthOrHeight = 0.5f;

        if (canvas.GetComponent<GraphicRaycaster>() == null)
            canvas.gameObject.AddComponent<GraphicRaycaster>();

        if (canvas.gameObject.layer != 5)
            canvas.gameObject.layer = 5;

        RectTransform rootRect = canvas.GetComponent<RectTransform>();
        if (rootRect != null)
        {
            if (rootRect.localScale == Vector3.zero)
                rootRect.localScale = Vector3.one;

            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;
        }

        EnsureEventSystem();

        return canvas;
    }

    private static void EnsureEventSystem()
    {
        EventSystem existing = Object.FindFirstObjectByType<EventSystem>();
        if (existing == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem));
            eventSystemObject.layer = 5;
            existing = eventSystemObject.GetComponent<EventSystem>();
        }

#if ENABLE_INPUT_SYSTEM
        if (existing.GetComponent<InputSystemUIInputModule>() == null)
            existing.gameObject.AddComponent<InputSystemUIInputModule>();

        StandaloneInputModule legacyModule = existing.GetComponent<StandaloneInputModule>();
        if (legacyModule != null)
            legacyModule.enabled = false;
#else
        if (existing.GetComponent<StandaloneInputModule>() == null)
            existing.gameObject.AddComponent<StandaloneInputModule>();
#endif
    }

    public static RectTransform GetOrCreatePanel(
        Transform parent,
        string name,
        Vector2 anchoredPosition,
        Vector2 size,
        bool visible = true)
    {
        Transform existing = parent.Find(name);
        GameObject panelObject;

        if (existing != null)
        {
            panelObject = existing.gameObject;
        }
        else
        {
            panelObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panelObject.transform.SetParent(parent, false);
        }

        RectTransform rect = panelObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Image image = panelObject.GetComponent<Image>();
        if (image != null)
        {
            image.color = new Color(0f, 0f, 0f, 0.65f);
        }

        panelObject.SetActive(visible);
        return rect;
    }

    public static TextMeshProUGUI GetOrCreateTMPText(
        Transform parent,
        string name,
        string defaultText,
        Vector2 anchoredPosition,
        Vector2 size,
        int fontSize,
        TextAlignmentOptions alignment,
        Color color)
    {
        Transform existing = parent.Find(name);
        GameObject textObject;

        if (existing != null)
        {
            textObject = existing.gameObject;
        }
        else
        {
            textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);
        }

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        TextMeshProUGUI tmp = textObject.GetComponent<TextMeshProUGUI>();
        TMP_FontAsset themedFont = GetThemeFont();
        if (themedFont != null)
            tmp.font = themedFont;
        else if (tmp.font == null && TMP_Settings.defaultFontAsset != null)
            tmp.font = TMP_Settings.defaultFontAsset;

        if (tmp.spriteAsset == null && TMP_Settings.defaultSpriteAsset != null)
            tmp.spriteAsset = TMP_Settings.defaultSpriteAsset;

        tmp.text = defaultText;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = color;
        tmp.raycastTarget = false;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;

        if (textObject.layer != 5)
            textObject.layer = 5;

        return tmp;
    }

    public static TMP_FontAsset GetPreferredFont()
    {
        return GetThemeFont();
    }

    private static TMP_FontAsset GetThemeFont()
    {
        if (cachedThemeFont != null)
            return cachedThemeFont;

        cachedThemeFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/Bangers SDF");
        if (cachedThemeFont == null)
            cachedThemeFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/Anton SDF");
        if (cachedThemeFont == null)
            cachedThemeFont = TMP_Settings.defaultFontAsset;

        return cachedThemeFont;
    }
}