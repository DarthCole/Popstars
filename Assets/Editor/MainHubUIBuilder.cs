using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// PopstarHub — Main Hub screen builder.
/// Menu: PopstarHub ▶ Build Main Hub UI
/// Requires a Canvas in the scene. Destroys and rebuilds "MainHubPanel" each run.
/// Reference resolution: 1920 × 1080.
/// </summary>
public class MainHubUIBuilder : EditorWindow
{
    // ── Palette (sampled from concept art) ────────────────────────────────────
    static readonly Color BG          = new Color(0.039f, 0.016f, 0.075f, 1.00f); // near-black purple
    static readonly Color CARD_BG     = new Color(0.085f, 0.040f, 0.160f, 0.82f); // deep purple card fill
    static readonly Color CARD_BORDER = new Color(0.340f, 0.150f, 0.530f, 0.70f); // soft violet border
    static readonly Color PILL_BG     = new Color(0.090f, 0.040f, 0.185f, 0.90f); // "NOW PLAYING" pill fill
    static readonly Color PILL_BORDER = new Color(0.500f, 0.270f, 0.780f, 0.80f); // pill border glow
    static readonly Color HOT_BG      = new Color(0.180f, 0.110f, 0.045f, 0.96f); // "hot" badge fill
    static readonly Color HOT_TEXT    = new Color(0.900f, 0.730f, 0.250f, 1.00f); // "hot" badge text
    static readonly Color W100        = Color.white;
    static readonly Color W65         = new Color(1f, 1f, 1f, 0.65f);
    static readonly Color STAR_COL    = new Color(1f, 1f, 1f, 0.72f);

    // ── Entry point ───────────────────────────────────────────────────────────

    [MenuItem("PopstarHub/Build Main Hub UI")]
    static void Build()
    {
        var canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null) { Debug.LogError("[MainHubUIBuilder] No Canvas found in scene."); return; }

        var scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;

        var old = GameObject.Find("MainHubPanel");
        if (old != null) Object.DestroyImmediate(old);

        var root  = canvas.GetComponent<RectTransform>();
        var panel = E(root, "MainHubPanel"); Stretch(panel);

        BuildBackground(panel);
        BuildNowPlayingPill(panel);
        BuildTitle(panel);
        BuildSubtitle(panel);
        BuildCards(panel);

        EditorUtility.SetDirty(canvas.gameObject);
        Debug.Log("[MainHubUIBuilder] Main Hub UI built successfully.");
    }

    // ── Section builders ──────────────────────────────────────────────────────

    static void BuildBackground(RectTransform p)
    {
        var bg = Img(p, "Background", BG);
        Stretch(bg);
        PlaceStarField(p);
    }

    static void PlaceStarField(RectTransform p)
    {
        // Scattered white dots matching the concept art placement.
        // Positions are normalised (0-1 from bottom-left), sizes in px.
        var root = E(p, "StarField");
        Stretch(root);

        float[] sx = { 0.14f, 0.17f, 0.07f, 0.12f, 0.21f, 0.30f, 0.42f, 0.54f,
                       0.60f, 0.71f, 0.78f, 0.86f, 0.91f, 0.88f, 0.83f, 0.93f,
                       0.76f, 0.50f, 0.36f, 0.25f, 0.08f, 0.47f, 0.64f, 0.03f };
        float[] sy = { 0.91f, 0.71f, 0.51f, 0.23f, 0.08f, 0.87f, 0.96f, 0.88f,
                       0.79f, 0.96f, 0.74f, 0.91f, 0.67f, 0.46f, 0.30f, 0.19f,
                       0.12f, 0.10f, 0.05f, 0.36f, 0.80f, 0.75f, 0.56f, 0.38f };
        float[] ss = { 4f, 3f, 3f, 3f, 2f, 3f, 2f, 2f,
                       4f, 3f, 3f, 2f, 3f, 2f, 4f, 3f,
                       2f, 3f, 2f, 3f, 2f, 2f, 2f, 3f };

        for (int i = 0; i < sx.Length; i++)
        {
            var dot = new GameObject("Star", typeof(RectTransform), typeof(Image));
            dot.transform.SetParent(root, false);
            var rt = dot.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(sx[i], sy[i]);
            rt.pivot     = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = Vector2.one * ss[i];
            rt.anchoredPosition = Vector2.zero;
            dot.GetComponent<Image>().color = STAR_COL;
        }
    }

    static void BuildNowPlayingPill(RectTransform p)
    {
        var spr = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        // Outer border ring
        var outerGO = new GameObject("NowPlayingPill_Border", typeof(RectTransform), typeof(Image));
        outerGO.transform.SetParent(p, false);
        var outerRT = outerGO.GetComponent<RectTransform>();
        outerRT.anchorMin = outerRT.anchorMax = new Vector2(0.5f, 1f);
        outerRT.pivot     = new Vector2(0.5f, 1f);
        outerRT.sizeDelta = new Vector2(228f, 50f);
        outerRT.anchoredPosition = new Vector2(0f, -90f);
        var outerImg = outerGO.GetComponent<Image>();
        outerImg.sprite = spr;
        outerImg.color  = PILL_BORDER;
        outerImg.type   = Image.Type.Sliced;

        // Inner fill
        var innerGO = new GameObject("NowPlayingPill", typeof(RectTransform), typeof(Image));
        innerGO.transform.SetParent(p, false);
        var innerRT = innerGO.GetComponent<RectTransform>();
        innerRT.anchorMin = innerRT.anchorMax = new Vector2(0.5f, 1f);
        innerRT.pivot     = new Vector2(0.5f, 1f);
        innerRT.sizeDelta = new Vector2(220f, 42f);
        innerRT.anchoredPosition = new Vector2(0f, -94f);
        var innerImg = innerGO.GetComponent<Image>();
        innerImg.sprite = spr;
        innerImg.color  = PILL_BG;
        innerImg.type   = Image.Type.Sliced;

        // Label — \u2666 = ♦
        var lbl = Tmp(innerRT, "Label", "\u2666 NOW PLAYING", 13f, W100, TextAlignmentOptions.Center, false);
        Stretch(lbl);
        lbl.GetComponent<TextMeshProUGUI>().characterSpacing = 2.5f;
    }

    static void BuildTitle(RectTransform p)
    {
        // "Popstar" in white, "Hub" in pink via rich text.
        var rt = Tmp(p, "TitleText", "Popstar <color=#EB6BA6>Hub</color>",
                     80f, W100, TextAlignmentOptions.Center, false);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot     = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(760f, 108f);
        rt.anchoredPosition = new Vector2(0f, -157f);

        var t = rt.GetComponent<TextMeshProUGUI>();
        t.richText = true;
        t.enableWordWrapping = false;

        // Use project's custom Popstar font if present.
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Popstar SDF.asset");
        if (font != null) t.font = font;
    }

    static void BuildSubtitle(RectTransform p)
    {
        var rt = Tmp(p, "SubtitleText", "Choose your stage. Own the spotlight.",
                     22f, W65, TextAlignmentOptions.Center, false);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot     = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(700f, 38f);
        rt.anchoredPosition = new Vector2(0f, -272f);
    }

    static void BuildCards(RectTransform p)
    {
        // 4 cards × 352 px + 3 gaps × 26 px = 1486 px total
        // Centers relative to screen center (x): -564, -188, 188, 564
        const float Y = -72f;
        BuildCard(p, "KaraokeCard",  "\U0001F3A4", "Karaoke",      "Sing your heart out",    false, -564f, Y);
        BuildCard(p, "DanceCard",    "\U0001F483", "Dance Battle", "Challenge the crowd",    true,  -188f, Y);
        BuildCard(p, "TalentCard",   "\u2728",     "Talent Show",  "Show off your skills",   false,  188f, Y);
        BuildCard(p, "ShopCard",     "\U0001F6CD", "Shop",         "Browse outfits & more",  false,  564f, Y);
    }

    static void BuildCard(RectTransform p, string id, string emoji,
                          string title, string desc, bool hotBadge, float x, float y)
    {
        var uiSpr  = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        var knobSpr = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

        // ── Outer border ──────────────────────────────────────────────────────
        var borderGO = new GameObject(id + "_Border", typeof(RectTransform), typeof(Image));
        borderGO.transform.SetParent(p, false);
        var borderRT = borderGO.GetComponent<RectTransform>();
        borderRT.anchorMin = borderRT.anchorMax = new Vector2(0.5f, 0.5f);
        borderRT.pivot     = new Vector2(0.5f, 0.5f);
        borderRT.sizeDelta = new Vector2(356f, 268f);
        borderRT.anchoredPosition = new Vector2(x, y);
        var borderImg = borderGO.GetComponent<Image>();
        borderImg.sprite = uiSpr;
        borderImg.color  = CARD_BORDER;
        borderImg.type   = Image.Type.Sliced;

        // ── Card face (child of border, inset 2 px) ───────────────────────────
        var cardGO = new GameObject(id, typeof(RectTransform), typeof(Image), typeof(Button));
        cardGO.transform.SetParent(borderRT, false);
        var cardRT = cardGO.GetComponent<RectTransform>();
        cardRT.anchorMin = Vector2.zero;
        cardRT.anchorMax = Vector2.one;
        cardRT.offsetMin = new Vector2(2f, 2f);
        cardRT.offsetMax = new Vector2(-2f, -2f);
        var cardImg = cardGO.GetComponent<Image>();
        cardImg.sprite = uiSpr;
        cardImg.color  = CARD_BG;
        cardImg.type   = Image.Type.Sliced;

        // ── Emoji icon ────────────────────────────────────────────────────────
        var iconRT = Tmp(cardRT, "Icon", emoji, 50f, W100, TextAlignmentOptions.Center, false);
        iconRT.anchorMin = new Vector2(0.10f, 0.54f);
        iconRT.anchorMax = new Vector2(0.90f, 0.93f);
        iconRT.offsetMin = iconRT.offsetMax = Vector2.zero;

        // ── Title ─────────────────────────────────────────────────────────────
        var titleRT = Tmp(cardRT, "Title", title, 26f, W100, TextAlignmentOptions.Center, true);
        titleRT.anchorMin = new Vector2(0.05f, 0.31f);
        titleRT.anchorMax = new Vector2(0.95f, 0.57f);
        titleRT.offsetMin = titleRT.offsetMax = Vector2.zero;

        // ── Description ───────────────────────────────────────────────────────
        var descRT = Tmp(cardRT, "Desc", desc, 17f, W65, TextAlignmentOptions.Center, false);
        descRT.anchorMin = new Vector2(0.05f, 0.07f);
        descRT.anchorMax = new Vector2(0.95f, 0.34f);
        descRT.offsetMin = descRT.offsetMax = Vector2.zero;
        descRT.GetComponent<TextMeshProUGUI>().enableWordWrapping = true;

        // ── "hot" badge (anchored to top-centre of border rect) ───────────────
        if (hotBadge)
        {
            var badgeGO = new GameObject("HotBadge", typeof(RectTransform), typeof(Image));
            badgeGO.transform.SetParent(borderRT, false);
            var badgeRT = badgeGO.GetComponent<RectTransform>();
            badgeRT.anchorMin = badgeRT.anchorMax = new Vector2(0.5f, 1f);
            badgeRT.pivot     = new Vector2(0.5f, 0.5f);
            badgeRT.sizeDelta = new Vector2(60f, 26f);
            badgeRT.anchoredPosition = new Vector2(0f, 6f);
            var badgeImg = badgeGO.GetComponent<Image>();
            badgeImg.sprite = knobSpr;
            badgeImg.color  = HOT_BG;
            badgeImg.type   = Image.Type.Simple;

            var hotTxt = Tmp(badgeRT, "HotText", "hot", 14f, HOT_TEXT, TextAlignmentOptions.Center, false);
            Stretch(hotTxt);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static RectTransform E(RectTransform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }

    static RectTransform Img(RectTransform parent, string name, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = color;
        return go.GetComponent<RectTransform>();
    }

    static RectTransform Tmp(RectTransform parent, string name, string text,
                              float size, Color color, TextAlignmentOptions align, bool bold)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var tmp          = go.GetComponent<TextMeshProUGUI>();
        tmp.text         = text;
        tmp.fontSize     = size;
        tmp.color        = color;
        tmp.alignment    = align;
        tmp.fontStyle    = bold ? FontStyles.Bold : FontStyles.Normal;
        tmp.enableWordWrapping = false;
        tmp.richText     = true;
        tmp.overflowMode = TextOverflowModes.Overflow;
        return go.GetComponent<RectTransform>();
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
