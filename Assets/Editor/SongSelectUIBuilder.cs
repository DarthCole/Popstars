using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Builds the Song Select screen in one click.
/// Menu: PopstarHub ▶ Build Song Select UI
///
/// Layout: full-screen panel with same dark-purple aesthetic as the karaoke screen.
/// Contains a 3×2 grid of song cards. The SongSelectUI runtime script populates
/// them at play-time from the SongData[] array wired by Karaoke ▶ Wire Song Select.
/// </summary>
public class SongSelectUIBuilder : EditorWindow
{
    // ── Palette (matches KaraokeUIBuilder / MainHubUIBuilder) ─────────────────
    static readonly Color BG          = new Color(0.039f, 0.016f, 0.075f, 1.00f);
    static readonly Color CARD_BG     = new Color(0.072f, 0.032f, 0.145f, 0.92f);
    static readonly Color CARD_BORDER = new Color(0.340f, 0.150f, 0.530f, 0.65f);
    static readonly Color PILL_BG     = new Color(0.090f, 0.040f, 0.185f, 0.90f);
    static readonly Color PILL_BORDER = new Color(0.500f, 0.270f, 0.780f, 0.80f);
    static readonly Color ACCENT      = new Color(0.659f, 0.333f, 0.969f, 1.00f);
    static readonly Color W100        = Color.white;
    static readonly Color W65         = new Color(1f, 1f, 1f, 0.65f);
    static readonly Color W35         = new Color(1f, 1f, 1f, 0.35f);
    static readonly Color STAR_COL    = new Color(1f, 1f, 1f, 0.70f);
    static readonly Color WARN        = new Color(1f, 0.4f, 0.4f, 0.80f);

    [MenuItem("PopstarHub/Build Song Select UI")]
    static void Build()
    {
        var canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null) { Debug.LogError("[SongSelectUIBuilder] No Canvas found."); return; }

        var scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;

        var old = GameObject.Find("SongSelectPanel");
        if (old != null) Object.DestroyImmediate(old);

        var root  = canvas.GetComponent<RectTransform>();
        var panel = E(root, "SongSelectPanel");
        Stretch(panel);

        BuildBackground(panel);
        BuildBackButton(panel);
        BuildHeader(panel);
        BuildSongGrid(panel);

        EditorUtility.SetDirty(canvas.gameObject);
        Debug.Log("[SongSelectUIBuilder] Song Select UI built. Run 'Karaoke ► Wire Song Select' next.");
    }

    // ── Sections ──────────────────────────────────────────────────────────────

    static void BuildBackground(RectTransform p)
    {
        var bg = Img(p, "Background", BG);
        Stretch(bg);

        // Identical scattered star field to the karaoke and hub screens
        var stars = E(p, "StarField");
        Stretch(stars);

        float[] sx = { 0.09f, 0.15f, 0.22f, 0.31f, 0.04f, 0.48f, 0.57f, 0.66f,
                       0.73f, 0.83f, 0.90f, 0.96f, 0.87f, 0.77f, 0.60f, 0.38f,
                       0.13f, 0.44f, 0.70f, 0.03f, 0.52f, 0.28f, 0.93f, 0.19f };
        float[] sy = { 0.92f, 0.74f, 0.55f, 0.88f, 0.37f, 0.96f, 0.80f, 0.91f,
                       0.70f, 0.94f, 0.60f, 0.38f, 0.20f, 0.11f, 0.08f, 0.05f,
                       0.16f, 0.28f, 0.44f, 0.62f, 0.50f, 0.76f, 0.82f, 0.32f };
        float[] ss = { 3f, 2f, 4f, 3f, 2f, 2f, 3f, 2f, 4f, 3f, 3f, 2f,
                       2f, 3f, 2f, 3f, 4f, 2f, 3f, 2f, 2f, 3f, 2f, 3f };

        for (int i = 0; i < sx.Length; i++)
        {
            var dot = new GameObject("Star", typeof(RectTransform), typeof(Image));
            dot.transform.SetParent(stars, false);
            var rt = dot.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(sx[i], sy[i]);
            rt.pivot     = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = Vector2.one * ss[i];
            rt.anchoredPosition = Vector2.zero;
            dot.GetComponent<Image>().color = STAR_COL;
        }
    }

    static void BuildBackButton(RectTransform p)
    {
        var btnGO = new GameObject("BackButton", typeof(RectTransform), typeof(Image), typeof(Button));
        btnGO.transform.SetParent(p, false);
        var rt = btnGO.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot     = new Vector2(0f, 1f);
        rt.sizeDelta = new Vector2(70f, 70f);
        rt.anchoredPosition = new Vector2(32f, -24f);
        btnGO.GetComponent<Image>().color = new Color(0, 0, 0, 0); // invisible hit area

        var arrow = Tmp(rt, "ArrowLabel", "\u2190", 32f, W100, TextAlignmentOptions.Center, false);
        Stretch(arrow);
    }

    static void BuildHeader(RectTransform p)
    {
        var uiSpr = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");

        // "NOW SELECTING" pill  ────────────────────────────────────────────────
        var outerGO = new GameObject("PillBorder", typeof(RectTransform), typeof(Image));
        outerGO.transform.SetParent(p, false);
        var outerRT = outerGO.GetComponent<RectTransform>();
        outerRT.anchorMin = outerRT.anchorMax = new Vector2(0.5f, 1f);
        outerRT.pivot     = new Vector2(0.5f, 1f);
        outerRT.sizeDelta = new Vector2(262f, 50f);
        outerRT.anchoredPosition = new Vector2(0f, -68f);
        var outerImg = outerGO.GetComponent<Image>();
        outerImg.sprite = uiSpr; outerImg.color = PILL_BORDER; outerImg.type = Image.Type.Sliced;

        var innerGO = new GameObject("Pill", typeof(RectTransform), typeof(Image));
        innerGO.transform.SetParent(p, false);
        var innerRT = innerGO.GetComponent<RectTransform>();
        innerRT.anchorMin = innerRT.anchorMax = new Vector2(0.5f, 1f);
        innerRT.pivot     = new Vector2(0.5f, 1f);
        innerRT.sizeDelta = new Vector2(254f, 42f);
        innerRT.anchoredPosition = new Vector2(0f, -72f);
        var innerImg = innerGO.GetComponent<Image>();
        innerImg.sprite = uiSpr; innerImg.color = PILL_BG; innerImg.type = Image.Type.Sliced;

        var pill = Tmp(innerRT, "PillLabel", "\u2666 NOW SELECTING", 13f,
                       W100, TextAlignmentOptions.Center, false);
        Stretch(pill);
        pill.GetComponent<TextMeshProUGUI>().characterSpacing = 2.5f;

        // Main title ──────────────────────────────────────────────────────────
        var titleRT = Tmp(p, "TitleText",
                          "Choose Your <color=#EB6BA6>Song</color>",
                          72f, W100, TextAlignmentOptions.Center, false);
        titleRT.anchorMin = titleRT.anchorMax = new Vector2(0.5f, 1f);
        titleRT.pivot     = new Vector2(0.5f, 1f);
        titleRT.sizeDelta = new Vector2(900f, 100f);
        titleRT.anchoredPosition = new Vector2(0f, -140f);

        var titleTmp  = titleRT.GetComponent<TextMeshProUGUI>();
        titleTmp.richText = true;
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Popstar SDF.asset");
        if (font != null) titleTmp.font = font;

        // Subtitle ────────────────────────────────────────────────────────────
        var subRT = Tmp(p, "SubtitleText",
                        "Pick your track and hit the stage.",
                        21f, W65, TextAlignmentOptions.Center, false);
        subRT.anchorMin = subRT.anchorMax = new Vector2(0.5f, 1f);
        subRT.pivot     = new Vector2(0.5f, 1f);
        subRT.sizeDelta = new Vector2(700f, 36f);
        subRT.anchoredPosition = new Vector2(0f, -254f);
    }

    static void BuildSongGrid(RectTransform p)
    {
        // 3 cols × 2 rows  |  cell 380×210  |  gap 22
        // total: 1186 × 442
        var gridGO = new GameObject("SongGrid",
                                    typeof(RectTransform),
                                    typeof(GridLayoutGroup));
        gridGO.transform.SetParent(p, false);

        var gridRT = gridGO.GetComponent<RectTransform>();
        gridRT.anchorMin = gridRT.anchorMax = new Vector2(0.5f, 0.5f);
        gridRT.pivot     = new Vector2(0.5f, 0.5f);
        gridRT.sizeDelta = new Vector2(1186f, 442f);
        gridRT.anchoredPosition = new Vector2(0f, -20f); // slight downward offset under title

        var glg = gridGO.GetComponent<GridLayoutGroup>();
        glg.cellSize          = new Vector2(380f, 210f);
        glg.spacing           = new Vector2(22f, 22f);
        glg.constraint        = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount   = 3;
        glg.childAlignment    = TextAnchor.UpperCenter;
        glg.startCorner       = GridLayoutGroup.Corner.UpperLeft;
        glg.startAxis         = GridLayoutGroup.Axis.Horizontal;

        // 6 placeholder cards — populated at runtime by SongSelectUI
        string[] placeholderSongs   = { "Song Title", "Song Title", "Song Title",
                                        "Song Title", "Song Title", "Song Title" };
        string[] placeholderArtists = { "Artist", "Artist", "Artist",
                                        "Artist", "Artist", "Artist" };
        string[] icons              = { "\U0001F3A4", "\U0001F3B5", "\U0001F3B6",
                                        "\U0001F3B8", "\U0001F3B9", "\U0001F3BC" };

        for (int i = 0; i < 6; i++)
            BuildSongCard(gridRT, i, icons[i],
                          placeholderSongs[i], placeholderArtists[i]);
    }

    static void BuildSongCard(RectTransform parent, int index,
                               string icon, string songName, string artist)
    {
        var uiSpr = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");

        // Outer container — direct child of GridLayoutGroup
        var outer = new GameObject($"SongCard_{index}",
                                   typeof(RectTransform), typeof(Image), typeof(Button));
        outer.transform.SetParent(parent, false);
        var outerImg = outer.GetComponent<Image>();
        outerImg.sprite = uiSpr;
        outerImg.color  = CARD_BORDER;
        outerImg.type   = Image.Type.Sliced;

        // Inner fill (inset 2 px — gives "border" appearance)
        var innerGO = new GameObject("CardInner", typeof(RectTransform), typeof(Image));
        innerGO.transform.SetParent(outer.transform, false);
        var innerRT = innerGO.GetComponent<RectTransform>();
        innerRT.anchorMin = Vector2.zero;
        innerRT.anchorMax = Vector2.one;
        innerRT.offsetMin = new Vector2(2f, 2f);
        innerRT.offsetMax = new Vector2(-2f, -2f);
        var innerImg = innerGO.GetComponent<Image>();
        innerImg.sprite = uiSpr;
        innerImg.color  = CARD_BG;
        innerImg.type   = Image.Type.Sliced;

        // Cover art image (top area)
        var coverGO = new GameObject("CoverArt", typeof(RectTransform), typeof(Image));
        coverGO.transform.SetParent(innerRT, false);
        var iconRT = coverGO.GetComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(0.05f, 0.55f);
        iconRT.anchorMax = new Vector2(0.95f, 0.97f);
        iconRT.offsetMin = iconRT.offsetMax = Vector2.zero;
        var coverImg = coverGO.GetComponent<Image>();
        coverImg.color           = new Color(0.12f, 0.05f, 0.27f, 1f); // placeholder until sprite assigned
        coverImg.preserveAspect  = true;

        // Song name (middle 32 %)
        var nameRT = Tmp(innerRT, "SongName", songName, 23f,
                         W100, TextAlignmentOptions.Center, true);
        nameRT.anchorMin = new Vector2(0.05f, 0.30f);
        nameRT.anchorMax = new Vector2(0.95f, 0.60f);
        nameRT.offsetMin = nameRT.offsetMax = Vector2.zero;
        nameRT.GetComponent<TextMeshProUGUI>().enableWordWrapping = true;

        // Artist (lower 22 %)
        var artistRT = Tmp(innerRT, "Artist", artist, 16f,
                           W65, TextAlignmentOptions.Center, false);
        artistRT.anchorMin = new Vector2(0.05f, 0.10f);
        artistRT.anchorMax = new Vector2(0.95f, 0.32f);
        artistRT.offsetMin = artistRT.offsetMax = Vector2.zero;

        // AudioStatus — shown at runtime when no audio clip is assigned
        var warnRT = Tmp(innerRT, "AudioStatus", "", 13f,
                         WARN, TextAlignmentOptions.Center, false);
        warnRT.anchorMin = new Vector2(0.05f, 0.00f);
        warnRT.anchorMax = new Vector2(0.95f, 0.13f);
        warnRT.offsetMin = warnRT.offsetMax = Vector2.zero;
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
        var go  = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var tmp           = go.GetComponent<TextMeshProUGUI>();
        tmp.text          = text;
        tmp.fontSize      = size;
        tmp.color         = color;
        tmp.alignment     = align;
        tmp.fontStyle     = bold ? FontStyles.Bold : FontStyles.Normal;
        tmp.richText      = true;
        tmp.overflowMode  = TextOverflowModes.Overflow;
        tmp.enableWordWrapping = false;
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
