using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Builds the complete Shop UI in one editor click.
/// Menu: PopstarHub ▶ Build Shop UI
///
/// What gets built:
///   ShopPanel (ShopPageNavigator component attached)
///   ├── LandingPage   – dark bg, 3 category cards (OUTFITS / SONGS / STAGES)
///   ├── OutfitsPage   – title, tab bar, scrollable 4-column item grid (12 items)
///   ├── SongsPage     – title, tab bar, scrollable 4-column item grid (6 tracks)
///   └── StagesPage    – title, tab bar, scrollable 4-column item grid (4 venues)
///
/// All buttons are wired to ShopPageNavigator automatically.
/// Reference resolution: 1920 × 1080.
/// </summary>
public class ShopUIBuilder : EditorWindow
{
    // ── Palette ───────────────────────────────────────────────────────────────
    static readonly Color BG               = new Color(0.039f, 0.016f, 0.075f, 1.00f);
    static readonly Color LANDING_CARD_BG  = new Color(0.100f, 0.048f, 0.208f, 1.00f);
    static readonly Color LANDING_BORDER   = new Color(0.280f, 0.130f, 0.540f, 0.90f);
    static readonly Color ITEM_BG          = new Color(0.120f, 0.058f, 0.255f, 1.00f);
    static readonly Color ITEM_BORDER      = new Color(0.230f, 0.105f, 0.450f, 0.90f);
    static readonly Color ICON_AREA        = new Color(0.090f, 0.042f, 0.190f, 1.00f);
    static readonly Color ICON_OUTFITS     = new Color(0.190f, 0.130f, 0.460f, 1.00f);
    static readonly Color ICON_SONGS       = new Color(0.095f, 0.130f, 0.370f, 1.00f);
    static readonly Color ICON_STAGES      = new Color(0.210f, 0.080f, 0.220f, 1.00f);
    static readonly Color TITLE_YEL        = new Color(0.912f, 0.851f, 0.282f, 1.00f);
    static readonly Color SUBTITLE_COL     = new Color(1f, 1f, 1f, 0.55f);
    static readonly Color COIN_BG          = new Color(0.168f, 0.078f, 0.355f, 1.00f);
    static readonly Color COIN_BORDER      = new Color(0.418f, 0.220f, 0.758f, 1.00f);
    static readonly Color TAB_ACTIVE       = new Color(0.255f, 0.118f, 0.530f, 1.00f);
    static readonly Color BUY_BG           = new Color(0.245f, 0.118f, 0.490f, 1.00f);
    static readonly Color OWNED_BG         = new Color(0.098f, 0.046f, 0.215f, 1.00f);
    static readonly Color COUNT_PILL       = new Color(0.215f, 0.098f, 0.455f, 1.00f);
    static readonly Color BACK_BTN         = new Color(0.125f, 0.060f, 0.268f, 1.00f);
    static readonly Color PRICE_COL        = new Color(0.88f, 0.82f, 0.24f, 1.00f);
    static readonly Color STAR_COL         = new Color(1f, 1f, 1f, 0.65f);
    static readonly Color W100             = Color.white;
    static readonly Color W65              = new Color(1f, 1f, 1f, 0.65f);
    static readonly Color W40              = new Color(1f, 1f, 1f, 0.40f);

    // ── Item catalogue ────────────────────────────────────────────────────────
    struct ShopItem { public string Name; public int Price; public bool Owned; public string Icon; }

    static readonly ShopItem[] OUTFITS =
    {
        new ShopItem { Name = "Neon Pop",     Price = 100, Owned = true,  Icon = "\U0001F457" },
        new ShopItem { Name = "Street Star",  Price = 150, Owned = false, Icon = "\U0001F9E5" },
        new ShopItem { Name = "K-Pop Dream",  Price = 200, Owned = false, Icon = "\U0001F458" },
        new ShopItem { Name = "Glam Diva",    Price = 250, Owned = false, Icon = "\U0001FA71" },
        new ShopItem { Name = "Top Hat Jazz", Price = 120, Owned = false, Icon = "\U0001F3A9" },
        new ShopItem { Name = "White Coat",   Price = 180, Owned = false, Icon = "\U0001FA7A" },
        new ShopItem { Name = "Festival Fit", Price = 160, Owned = false, Icon = "\U0001F9BA" },
        new ShopItem { Name = "Luna Robe",    Price = 300, Owned = false, Icon = "\U0001F97B" },
        new ShopItem { Name = "Cyber Suit",   Price = 220, Owned = false, Icon = "\U0001F977" },
        new ShopItem { Name = "Rock Star",    Price = 175, Owned = false, Icon = "\U0001F3B8" },
        new ShopItem { Name = "Golden Gown",  Price = 350, Owned = false, Icon = "\u2728"     },
        new ShopItem { Name = "Retro Vibe",   Price = 130, Owned = false, Icon = "\U0001F57A" },
    };

    static readonly ShopItem[] SONGS =
    {
        new ShopItem { Name = "Neon Lights",     Price = 300, Owned = false, Icon = "\U0001F3B5" },
        new ShopItem { Name = "Dance Monkey",    Price = 250, Owned = false, Icon = "\U0001F3B5" },
        new ShopItem { Name = "How Far I'll Go", Price = 350, Owned = false, Icon = "\U0001F3B5" },
        new ShopItem { Name = "Pop Queen",       Price = 200, Owned = false, Icon = "\U0001F3B5" },
        new ShopItem { Name = "Galaxy Beat",     Price = 400, Owned = false, Icon = "\U0001F3B5" },
        new ShopItem { Name = "Night Drive",     Price = 280, Owned = false, Icon = "\U0001F3B5" },
    };

    static readonly ShopItem[] STAGES =
    {
        new ShopItem { Name = "Neon Arena",     Price = 500, Owned = false, Icon = "\U0001F3DF" },
        new ShopItem { Name = "Beach Festival", Price = 400, Owned = false, Icon = "\U0001F3AA" },
        new ShopItem { Name = "Rooftop Stage",  Price = 600, Owned = false, Icon = "\U0001F306" },
        new ShopItem { Name = "Tokyo Dome",     Price = 800, Owned = false, Icon = "\U0001F3AD" },
    };

    // ── Entry point ───────────────────────────────────────────────────────────
    [MenuItem("PopstarHub/Build Shop UI")]
    static void Build()
    {
        var canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[ShopUIBuilder] No Canvas found in scene. Add one first.");
            return;
        }

        // Enforce CanvasScaler
        var scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight  = 0.5f;

        // Destroy previous build
        var old = GameObject.Find("ShopPanel");
        if (old != null) Object.DestroyImmediate(old);

        var canvasRT = canvas.GetComponent<RectTransform>();
        var panel    = MakeRect(canvasRT, "ShopPanel");
        Stretch(panel);

        var nav = panel.gameObject.AddComponent<ShopPageNavigator>();

        // Load all OutfitData and StageData assets
        var outfitAssets = LoadAllOutfitData();
        var stageAssets  = LoadAllStageData();

        var songAssets = LoadAllSongData();

        var landing = BuildLandingPage(panel, outfitAssets.Length, stageAssets.Length);
        var outfits = BuildOutfitsPageFromData(panel, outfitAssets);
        var songs   = BuildSongsPageFromData(panel, songAssets);
        var stages  = BuildStagesPageFromData(panel, stageAssets);

        outfits.SetActive(false);
        songs.SetActive(false);
        stages.SetActive(false);

        var so = new SerializedObject(nav);
        so.FindProperty("landingPage").objectReferenceValue = landing;
        so.FindProperty("outfitsPage").objectReferenceValue = outfits;
        so.FindProperty("songsPage").objectReferenceValue   = songs;
        so.FindProperty("stagesPage").objectReferenceValue  = stages;
        so.ApplyModifiedProperties();

        WireLandingCards(landing, nav);
        WireAllBackButtons(panel.gameObject, nav);
        WireAllTabBars(outfits, songs, stages, nav);

        EditorUtility.SetDirty(canvas.gameObject);
        Debug.Log($"[ShopUIBuilder] Shop built — {outfitAssets.Length} outfits, {songAssets.Length} songs, {stageAssets.Length} stages.");
    }

    static OutfitData[] LoadAllOutfitData()
    {
        var guids = AssetDatabase.FindAssets("t:OutfitData");
        var list  = new System.Collections.Generic.List<OutfitData>();
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var data = AssetDatabase.LoadAssetAtPath<OutfitData>(path);
            if (data != null) list.Add(data);
        }
        // Sort: defaults first, then by price ascending
        list.Sort((a, b) =>
        {
            if (a.isDefault != b.isDefault) return a.isDefault ? -1 : 1;
            return a.price.CompareTo(b.price);
        });
        return list.ToArray();
    }

    static SongData[] LoadAllSongData()
    {
        var guids = AssetDatabase.FindAssets("t:SongData");
        var list  = new System.Collections.Generic.List<SongData>();
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var data = AssetDatabase.LoadAssetAtPath<SongData>(path);
            if (data != null) list.Add(data);
        }
        // Defaults first, then by price ascending
        list.Sort((a, b) =>
        {
            if (a.isDefault != b.isDefault) return a.isDefault ? -1 : 1;
            return a.price.CompareTo(b.price);
        });
        return list.ToArray();
    }

    static StageData[] LoadAllStageData()
    {
        var guids = AssetDatabase.FindAssets("t:StageData");
        var list  = new System.Collections.Generic.List<StageData>();
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var data = AssetDatabase.LoadAssetAtPath<StageData>(path);
            if (data != null) list.Add(data);
        }
        list.Sort((a, b) =>
        {
            if (a.isDefault != b.isDefault) return a.isDefault ? -1 : 1;
            return a.price.CompareTo(b.price);
        });
        return list.ToArray();
    }

    // ── Songs page (data-driven from SongData assets) ────────────────────────
    static GameObject BuildSongsPageFromData(RectTransform panel, SongData[] songAssets)
    {
        var page = MakeRect(panel, "SongsPage");
        Stretch(page);
        BuildBackground(page);
        BuildTopBar(page);
        BuildCategoryTitle(page, "SONGS");
        BuildTabBar(page, 1);
        BuildSongScrollView(page, songAssets);
        return page.gameObject;
    }

    static void BuildSongScrollView(RectTransform page, SongData[] songAssets)
    {
        var scrollGO = new GameObject("ScrollView", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
        scrollGO.transform.SetParent(page, false);
        var scrollRT  = scrollGO.GetComponent<RectTransform>();
        scrollRT.anchorMin = new Vector2(0f, 0f);
        scrollRT.anchorMax = new Vector2(1f, 1f);
        scrollRT.offsetMin = new Vector2(40f, 60f);
        scrollRT.offsetMax = new Vector2(-40f, -296f);
        scrollGO.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
        var scrollRect  = scrollGO.GetComponent<ScrollRect>();
        scrollRect.horizontal        = false;
        scrollRect.scrollSensitivity = 30f;
        scrollRect.movementType      = ScrollRect.MovementType.Elastic;

        var vpGO = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        vpGO.transform.SetParent(scrollRT, false);
        var vpRT  = vpGO.GetComponent<RectTransform>();
        Stretch(vpRT);
        vpGO.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.01f);
        vpGO.GetComponent<Mask>().showMaskGraphic = false;
        scrollRect.viewport = vpRT;

        var contentGO = new GameObject("Content", typeof(RectTransform));
        contentGO.transform.SetParent(vpRT, false);
        var contentRT  = contentGO.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0f, 1f);
        contentRT.anchorMax = new Vector2(1f, 1f);
        contentRT.pivot     = new Vector2(0f, 1f);
        contentRT.sizeDelta = new Vector2(0f, 600f);
        contentRT.anchoredPosition = Vector2.zero;
        scrollRect.content = contentRT;

        var grid = contentGO.AddComponent<DynamicShopGridLayout>();
        grid.content     = contentRT;
        grid.viewport    = vpRT;
        grid.columns     = 4;
        grid.cellSize    = new Vector2(380f, 290f);
        grid.horizontalSpacing = 24f;
        grid.verticalSpacing   = 24f;
        grid.leftPadding  = 50f;
        grid.rightPadding = 50f;
        grid.topPadding   = 24f;
        grid.bottomPadding = 40f;
        grid.controlChildSize       = true;
        grid.clampContentToViewport = true;

        foreach (var song in songAssets)
            BuildSongDataCard(contentRT, song);

        grid.RebuildNow();
        BuildScrollArrow(page);
    }

    static void BuildSongDataCard(RectTransform content, SongData song)
    {
        var spr  = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        var knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

        string safeName = song.songName.Replace(" ", "").Replace("'", "");
        bool isFree = song.isDefault || song.price <= 0;

        var borderGO = new GameObject(safeName + "_Border", typeof(RectTransform), typeof(Image));
        borderGO.transform.SetParent(content, false);
        var borderRT  = borderGO.GetComponent<RectTransform>();
        borderRT.anchorMin = borderRT.anchorMax = new Vector2(0f, 1f);
        borderRT.pivot     = new Vector2(0f, 1f);
        borderRT.sizeDelta = new Vector2(380f, 290f);
        var bImg = borderGO.GetComponent<Image>();
        bImg.sprite = spr; bImg.color = ITEM_BORDER; bImg.type = Image.Type.Sliced;

        var cardGO = new GameObject(safeName, typeof(RectTransform), typeof(Image));
        cardGO.transform.SetParent(borderRT, false);
        var cardRT  = cardGO.GetComponent<RectTransform>();
        cardRT.anchorMin = Vector2.zero; cardRT.anchorMax = Vector2.one;
        cardRT.offsetMin = new Vector2(2f, 2f); cardRT.offsetMax = new Vector2(-2f, -2f);
        var cImg = cardGO.GetComponent<Image>();
        cImg.sprite = spr; cImg.color = ITEM_BG; cImg.type = Image.Type.Sliced;

        // Cover art area (top ~58%)
        var iconAreaGO = new GameObject("IconArea", typeof(RectTransform), typeof(Image));
        iconAreaGO.transform.SetParent(cardRT, false);
        var iconAreaRT  = iconAreaGO.GetComponent<RectTransform>();
        iconAreaRT.anchorMin = new Vector2(0.08f, 0.38f);
        iconAreaRT.anchorMax = new Vector2(0.92f, 0.94f);
        iconAreaRT.offsetMin = iconAreaRT.offsetMax = Vector2.zero;
        var iconAreaImg  = iconAreaGO.GetComponent<Image>();
        iconAreaImg.sprite = knob; iconAreaImg.color = ICON_SONGS;

        Image coverImg = null;
        if (song.coverArt != null)
        {
            var coverGO = new GameObject("CoverArt", typeof(RectTransform), typeof(Image));
            coverGO.transform.SetParent(iconAreaRT, false);
            Stretch(coverGO.GetComponent<RectTransform>());
            coverImg = coverGO.GetComponent<Image>();
            coverImg.sprite         = song.coverArt;
            coverImg.preserveAspect = true;
            coverImg.color          = isFree ? W100 : new Color(1f, 1f, 1f, 0.55f);
        }

        // Song name
        var nameRT = MakeTMP(cardRT, "ItemName", song.songName, 14f, W100, TextAlignmentOptions.Center, true);
        nameRT.anchorMin = new Vector2(0.05f, 0.24f);
        nameRT.anchorMax = new Vector2(0.95f, 0.38f);
        nameRT.offsetMin = nameRT.offsetMax = Vector2.zero;
        nameRT.GetComponent<TextMeshProUGUI>().enableWordWrapping = false;

        // Action button (BUY / OWNED — handled at runtime by ShopSongButton)
        var btnGO = new GameObject("ActionButton", typeof(RectTransform), typeof(Image), typeof(Button));
        btnGO.transform.SetParent(cardRT, false);
        var btnRT  = btnGO.GetComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(0.06f, 0.04f);
        btnRT.anchorMax = new Vector2(0.94f, 0.24f);
        btnRT.offsetMin = btnRT.offsetMax = Vector2.zero;
        var btnImg  = btnGO.GetComponent<Image>();
        btnImg.sprite = spr;
        btnImg.color  = isFree ? new Color(0.047f, 0.180f, 0.090f, 1f) : BUY_BG;
        btnImg.type   = Image.Type.Sliced;
        btnGO.AddComponent<ButtonHoverEffect>();

        string initialLabel = isFree ? "OWNED" : $"\u2605 {song.price}";
        var btnTxtRT = MakeTMP(btnRT, "Label", initialLabel, 13f, isFree ? W65 : W100, TextAlignmentOptions.Center, true);
        Stretch(btnTxtRT);

        var shopBtn = btnGO.AddComponent<ShopSongButton>();
        shopBtn.song        = song;
        shopBtn.buttonImage = btnImg;
        shopBtn.buttonLabel = btnTxtRT.GetComponent<TextMeshProUGUI>();
        shopBtn.coverImage  = coverImg;

        var btn = btnGO.GetComponent<Button>();
        UnityEventTools.AddPersistentListener(btn.onClick, shopBtn.OnActionPressed);
    }

    // ── Stages page (data-driven from StageData assets) ───────────────────────
    static GameObject BuildStagesPageFromData(RectTransform panel, StageData[] stageAssets)
    {
        var page = MakeRect(panel, "StagesPage");
        Stretch(page);
        BuildBackground(page);
        BuildTopBar(page);
        BuildCategoryTitle(page, "STAGES");
        BuildTabBar(page, 2);
        BuildStageScrollView(page, stageAssets);
        return page.gameObject;
    }

    static void BuildStageScrollView(RectTransform page, StageData[] stageAssets)
    {
        var scrollGO = new GameObject("ScrollView", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
        scrollGO.transform.SetParent(page, false);
        var scrollRT  = scrollGO.GetComponent<RectTransform>();
        scrollRT.anchorMin = new Vector2(0f, 0f);
        scrollRT.anchorMax = new Vector2(1f, 1f);
        scrollRT.offsetMin = new Vector2(40f, 60f);
        scrollRT.offsetMax = new Vector2(-40f, -296f);
        scrollGO.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
        var scrollRect  = scrollGO.GetComponent<ScrollRect>();
        scrollRect.horizontal        = false;
        scrollRect.scrollSensitivity = 30f;
        scrollRect.movementType      = ScrollRect.MovementType.Elastic;

        var vpGO = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        vpGO.transform.SetParent(scrollRT, false);
        var vpRT  = vpGO.GetComponent<RectTransform>();
        Stretch(vpRT);
        vpGO.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.01f);
        vpGO.GetComponent<Mask>().showMaskGraphic = false;
        scrollRect.viewport = vpRT;

        var contentGO = new GameObject("Content", typeof(RectTransform));
        contentGO.transform.SetParent(vpRT, false);
        var contentRT  = contentGO.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0f, 1f);
        contentRT.anchorMax = new Vector2(1f, 1f);
        contentRT.pivot     = new Vector2(0f, 1f);
        contentRT.sizeDelta = new Vector2(0f, 600f);
        contentRT.anchoredPosition = Vector2.zero;
        scrollRect.content = contentRT;

        var grid = contentGO.AddComponent<DynamicShopGridLayout>();
        grid.content     = contentRT;
        grid.viewport    = vpRT;
        grid.columns     = 4;
        grid.cellSize    = new Vector2(380f, 290f);
        grid.horizontalSpacing = 24f;
        grid.verticalSpacing   = 24f;
        grid.leftPadding  = 50f;
        grid.rightPadding = 50f;
        grid.topPadding   = 24f;
        grid.bottomPadding = 40f;
        grid.controlChildSize       = true;
        grid.clampContentToViewport = true;

        foreach (var stage in stageAssets)
            BuildStageDataCard(contentRT, stage);

        grid.RebuildNow();
        BuildScrollArrow(page);
    }

    static void BuildStageDataCard(RectTransform content, StageData stage)
    {
        var spr  = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        var knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

        string safeName = stage.stageName.Replace(" ", "").Replace("'", "");
        bool isFree = stage.isDefault || stage.price <= 0;

        var borderGO = new GameObject(safeName + "_Border", typeof(RectTransform), typeof(Image));
        borderGO.transform.SetParent(content, false);
        var borderRT  = borderGO.GetComponent<RectTransform>();
        borderRT.anchorMin = borderRT.anchorMax = new Vector2(0f, 1f);
        borderRT.pivot     = new Vector2(0f, 1f);
        borderRT.sizeDelta = new Vector2(380f, 290f);
        var bImg = borderGO.GetComponent<Image>();
        bImg.sprite = spr; bImg.color = ITEM_BORDER; bImg.type = Image.Type.Sliced;

        var cardGO = new GameObject(safeName, typeof(RectTransform), typeof(Image));
        cardGO.transform.SetParent(borderRT, false);
        var cardRT  = cardGO.GetComponent<RectTransform>();
        cardRT.anchorMin = Vector2.zero; cardRT.anchorMax = Vector2.one;
        cardRT.offsetMin = new Vector2(2f, 2f); cardRT.offsetMax = new Vector2(-2f, -2f);
        var cImg = cardGO.GetComponent<Image>();
        cImg.sprite = spr; cImg.color = ITEM_BG; cImg.type = Image.Type.Sliced;

        // Preview icon area (top ~58%)
        var iconAreaGO = new GameObject("IconArea", typeof(RectTransform), typeof(Image));
        iconAreaGO.transform.SetParent(cardRT, false);
        var iconAreaRT  = iconAreaGO.GetComponent<RectTransform>();
        iconAreaRT.anchorMin = new Vector2(0.08f, 0.38f);
        iconAreaRT.anchorMax = new Vector2(0.92f, 0.94f);
        iconAreaRT.offsetMin = iconAreaRT.offsetMax = Vector2.zero;
        var iconAreaImg  = iconAreaGO.GetComponent<Image>();
        iconAreaImg.sprite = knob; iconAreaImg.color = ICON_AREA;

        Image previewImg = null;
        if (stage.previewIcon != null)
        {
            var previewGO = new GameObject("PreviewIcon", typeof(RectTransform), typeof(Image));
            previewGO.transform.SetParent(iconAreaRT, false);
            Stretch(previewGO.GetComponent<RectTransform>());
            previewImg = previewGO.GetComponent<Image>();
            previewImg.sprite         = stage.previewIcon;
            previewImg.preserveAspect = true;
            previewImg.color          = isFree ? W100 : new Color(1f, 1f, 1f, 0.55f);
        }

        // Name label
        var nameRT = MakeTMP(cardRT, "ItemName", stage.stageName, 14f, W100, TextAlignmentOptions.Center, true);
        nameRT.anchorMin = new Vector2(0.05f, 0.24f);
        nameRT.anchorMax = new Vector2(0.95f, 0.38f);
        nameRT.offsetMin = nameRT.offsetMax = Vector2.zero;
        nameRT.GetComponent<TextMeshProUGUI>().enableWordWrapping = false;

        // Action button (BUY / EQUIP / EQUIPPED — handled at runtime by ShopStageButton)
        var btnGO = new GameObject("ActionButton", typeof(RectTransform), typeof(Image), typeof(Button));
        btnGO.transform.SetParent(cardRT, false);
        var btnRT  = btnGO.GetComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(0.06f, 0.04f);
        btnRT.anchorMax = new Vector2(0.94f, 0.24f);
        btnRT.offsetMin = btnRT.offsetMax = Vector2.zero;
        var btnImg  = btnGO.GetComponent<Image>();
        btnImg.sprite = spr;
        btnImg.color  = isFree ? new Color(0.047f, 0.180f, 0.090f, 1f) : BUY_BG;
        btnImg.type   = Image.Type.Sliced;
        btnGO.AddComponent<ButtonHoverEffect>();

        string initialLabel = isFree ? "EQUIP" : $"\u2605 {stage.price}";
        var btnTxtRT = MakeTMP(btnRT, "Label", initialLabel, 13f, isFree ? W65 : W100, TextAlignmentOptions.Center, true);
        Stretch(btnTxtRT);

        var shopBtn = btnGO.AddComponent<ShopStageButton>();
        shopBtn.stage       = stage;
        shopBtn.buttonImage = btnImg;
        shopBtn.buttonLabel = btnTxtRT.GetComponent<TextMeshProUGUI>();
        shopBtn.iconImage   = previewImg;

        var btn = btnGO.GetComponent<Button>();
        UnityEventTools.AddPersistentListener(btn.onClick, shopBtn.OnActionPressed);
    }

    static GameObject BuildLandingPage(RectTransform panel, int outfitCount, int stageCount)
    {
        var page = MakeRect(panel, "LandingPage");
        Stretch(page);
        BuildBackground(page);
        BuildTopBar(page);
        // Title: "SHOP"
        var titleRT = MakeTMP(page, "TitleText", "SHOP", 96f, TITLE_YEL, TextAlignmentOptions.Center, true);
        titleRT.anchorMin = titleRT.anchorMax = new Vector2(0.5f, 1f);
        titleRT.pivot     = new Vector2(0.5f, 1f);
        titleRT.sizeDelta = new Vector2(500f, 120f);
        titleRT.anchoredPosition = new Vector2(0f, -130f);
        var titleTMP = titleRT.GetComponent<TextMeshProUGUI>();
        titleTMP.characterSpacing  = 6f;
        titleTMP.enableWordWrapping = false;
        ApplyBungeeFont(titleTMP);
        // Subtitle
        var subRT = MakeTMP(page, "SubtitleText", "SPEND YOUR STARCOINS", 22f, SUBTITLE_COL, TextAlignmentOptions.Center, false);
        subRT.anchorMin = subRT.anchorMax = new Vector2(0.5f, 1f);
        subRT.pivot     = new Vector2(0.5f, 1f);
        subRT.sizeDelta = new Vector2(620f, 38f);
        subRT.anchoredPosition = new Vector2(0f, -262f);
        subRT.GetComponent<TextMeshProUGUI>().characterSpacing = 4f;
        // Three category cards
        BuildLandingCard(page, "OutfitsCard", "OUTFITS", "Dress your popstar",     $"{outfitCount} items",  "\U0001F457", ICON_OUTFITS, -360f);
        BuildLandingCard(page, "SongsCard",   "SONGS",   "Unlock new tracks",      "6 tracks",             "\U0001F3B5", ICON_SONGS,     0f);
        BuildLandingCard(page, "StagesCard",  "STAGES",  "New performance venues", $"{stageCount} venues", "\U0001F3DF", ICON_STAGES,  360f);
        return page.gameObject;
    }

    static void BuildLandingCard(RectTransform page, string id, string title, string desc,
                                  string countStr, string icon, Color iconCircleCol, float xPos)
    {
        var spr  = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        var knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        const float W = 294f, H = 330f;

        // ── Outer border ──────────────────────────────────────────────────────
        var borderGO = new GameObject(id + "_Border", typeof(RectTransform), typeof(Image));
        borderGO.transform.SetParent(page, false);
        var borderRT  = borderGO.GetComponent<RectTransform>();
        borderRT.anchorMin = borderRT.anchorMax = new Vector2(0.5f, 0.5f);
        borderRT.pivot     = new Vector2(0.5f, 0.5f);
        borderRT.sizeDelta = new Vector2(W + 4f, H + 4f);
        borderRT.anchoredPosition = new Vector2(xPos, 40f);
        var borderImg  = borderGO.GetComponent<Image>();
        borderImg.sprite = spr;
        borderImg.color  = LANDING_BORDER;
        borderImg.type   = Image.Type.Sliced;

        // ── Card face + click target ──────────────────────────────────────────
        var cardGO = new GameObject(id, typeof(RectTransform), typeof(Image), typeof(Button));
        cardGO.transform.SetParent(borderRT, false);
        var cardRT  = cardGO.GetComponent<RectTransform>();
        cardRT.anchorMin = Vector2.zero;
        cardRT.anchorMax = Vector2.one;
        cardRT.offsetMin = new Vector2(2f, 2f);
        cardRT.offsetMax = new Vector2(-2f, -2f);
        var cardImg  = cardGO.GetComponent<Image>();
        cardImg.sprite = spr;
        cardImg.color  = LANDING_CARD_BG;
        cardImg.type   = Image.Type.Sliced;
        cardGO.AddComponent<CardHoverEffect>();

        // ── Icon circle ───────────────────────────────────────────────────────
        var circleGO = new GameObject("IconCircle", typeof(RectTransform), typeof(Image));
        circleGO.transform.SetParent(cardRT, false);
        var circleRT  = circleGO.GetComponent<RectTransform>();
        circleRT.anchorMin = circleRT.anchorMax = new Vector2(0.5f, 1f);
        circleRT.pivot     = new Vector2(0.5f, 1f);
        circleRT.sizeDelta = new Vector2(130f, 130f);
        circleRT.anchoredPosition = new Vector2(0f, -34f);
        var circleImg  = circleGO.GetComponent<Image>();
        circleImg.sprite = knob;
        circleImg.color  = iconCircleCol;

        var iconRT = MakeTMP(circleRT, "Icon", icon, 50f, W100, TextAlignmentOptions.Center, false);
        Stretch(iconRT);

        // ── Category name ─────────────────────────────────────────────────────
        var nameRT  = MakeTMP(cardRT, "CategoryName", title, 26f, W100, TextAlignmentOptions.Center, true);
        nameRT.anchorMin = new Vector2(0.05f, 0.44f);
        nameRT.anchorMax = new Vector2(0.95f, 0.60f);
        nameRT.offsetMin = nameRT.offsetMax = Vector2.zero;
        ApplyBungeeFont(nameRT.GetComponent<TextMeshProUGUI>());

        // ── Description ───────────────────────────────────────────────────────
        var descRT = MakeTMP(cardRT, "Desc", desc, 16f, W65, TextAlignmentOptions.Center, false);
        descRT.anchorMin = new Vector2(0.05f, 0.31f);
        descRT.anchorMax = new Vector2(0.95f, 0.44f);
        descRT.offsetMin = descRT.offsetMax = Vector2.zero;

        // ── Count pill (bottom) ───────────────────────────────────────────────
        var pillGO = new GameObject("CountPill", typeof(RectTransform), typeof(Image));
        pillGO.transform.SetParent(cardRT, false);
        var pillRT  = pillGO.GetComponent<RectTransform>();
        pillRT.anchorMin = pillRT.anchorMax = new Vector2(0.5f, 0f);
        pillRT.pivot     = new Vector2(0.5f, 0f);
        pillRT.sizeDelta = new Vector2(130f, 34f);
        pillRT.anchoredPosition = new Vector2(0f, 18f);
        var pillImg  = pillGO.GetComponent<Image>();
        pillImg.sprite = spr;
        pillImg.color  = COUNT_PILL;
        pillImg.type   = Image.Type.Sliced;
        var countTxtRT = MakeTMP(pillRT, "CountText", countStr, 15f, W100, TextAlignmentOptions.Center, false);
        Stretch(countTxtRT);
    }

    // ── Outfits page built from real OutfitData assets ────────────────────────
    static GameObject BuildOutfitsPageFromData(RectTransform panel, OutfitData[] outfitAssets)
    {
        var page = MakeRect(panel, "OutfitsPage");
        Stretch(page);
        BuildBackground(page);
        BuildTopBar(page);
        BuildCategoryTitle(page, "OUTFITS");
        BuildTabBar(page, 0);
        BuildScrollViewFromData(page, outfitAssets);
        return page.gameObject;
    }

    static void BuildScrollViewFromData(RectTransform page, OutfitData[] outfitAssets)
    {
        var scrollGO = new GameObject("ScrollView", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
        scrollGO.transform.SetParent(page, false);
        var scrollRT  = scrollGO.GetComponent<RectTransform>();
        scrollRT.anchorMin = new Vector2(0f, 0f);
        scrollRT.anchorMax = new Vector2(1f, 1f);
        scrollRT.offsetMin = new Vector2(40f, 60f);
        scrollRT.offsetMax = new Vector2(-40f, -296f);
        scrollGO.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
        var scrollRect  = scrollGO.GetComponent<ScrollRect>();
        scrollRect.horizontal        = false;
        scrollRect.scrollSensitivity = 30f;
        scrollRect.movementType      = ScrollRect.MovementType.Elastic;

        var vpGO = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        vpGO.transform.SetParent(scrollRT, false);
        var vpRT  = vpGO.GetComponent<RectTransform>();
        Stretch(vpRT);
        vpGO.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.01f);
        vpGO.GetComponent<Mask>().showMaskGraphic = false;
        scrollRect.viewport = vpRT;

        var contentGO = new GameObject("Content", typeof(RectTransform));
        contentGO.transform.SetParent(vpRT, false);
        var contentRT  = contentGO.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0f, 1f);
        contentRT.anchorMax = new Vector2(1f, 1f);
        contentRT.pivot     = new Vector2(0f, 1f);
        contentRT.sizeDelta = new Vector2(0f, 600f);
        contentRT.anchoredPosition = Vector2.zero;
        scrollRect.content = contentRT;

        var grid = contentGO.AddComponent<DynamicShopGridLayout>();
        grid.content     = contentRT;
        grid.viewport    = vpRT;
        grid.columns     = 4;
        grid.cellSize    = new Vector2(380f, 290f);
        grid.horizontalSpacing = 24f;
        grid.verticalSpacing   = 24f;
        grid.leftPadding  = 50f;
        grid.rightPadding = 50f;
        grid.topPadding   = 24f;
        grid.bottomPadding = 40f;
        grid.controlChildSize       = true;
        grid.clampContentToViewport = true;

        foreach (var outfit in outfitAssets)
            BuildOutfitDataCard(contentRT, outfit);

        grid.RebuildNow();
        BuildScrollArrow(page);
    }

    static void BuildOutfitDataCard(RectTransform content, OutfitData outfit)
    {
        var spr  = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        var knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

        string safeName = outfit.outfitName.Replace(" ", "").Replace("'", "");
        bool isFree = outfit.isDefault || outfit.price <= 0;

        var borderGO = new GameObject(safeName + "_Border", typeof(RectTransform), typeof(Image));
        borderGO.transform.SetParent(content, false);
        var borderRT  = borderGO.GetComponent<RectTransform>();
        borderRT.anchorMin = borderRT.anchorMax = new Vector2(0f, 1f);
        borderRT.pivot     = new Vector2(0f, 1f);
        borderRT.sizeDelta = new Vector2(380f, 290f);
        borderGO.GetComponent<Image>().color = ITEM_BORDER;
        var bImg = borderGO.GetComponent<Image>();
        bImg.sprite = spr; bImg.color = ITEM_BORDER; bImg.type = Image.Type.Sliced;

        var cardGO = new GameObject(safeName, typeof(RectTransform), typeof(Image));
        cardGO.transform.SetParent(borderRT, false);
        var cardRT  = cardGO.GetComponent<RectTransform>();
        cardRT.anchorMin = Vector2.zero; cardRT.anchorMax = Vector2.one;
        cardRT.offsetMin = new Vector2(2f, 2f); cardRT.offsetMax = new Vector2(-2f, -2f);
        var cImg = cardGO.GetComponent<Image>();
        cImg.sprite = spr; cImg.color = ITEM_BG; cImg.type = Image.Type.Sliced;

        // Preview icon (outfit-only PNG)
        var iconAreaGO = new GameObject("IconArea", typeof(RectTransform), typeof(Image));
        iconAreaGO.transform.SetParent(cardRT, false);
        var iconAreaRT  = iconAreaGO.GetComponent<RectTransform>();
        iconAreaRT.anchorMin = new Vector2(0.08f, 0.38f);
        iconAreaRT.anchorMax = new Vector2(0.92f, 0.94f);
        iconAreaRT.offsetMin = iconAreaRT.offsetMax = Vector2.zero;
        var iconAreaImg  = iconAreaGO.GetComponent<Image>();
        iconAreaImg.sprite = knob; iconAreaImg.color = ICON_AREA;

        if (outfit.previewIcon != null)
        {
            var previewGO = new GameObject("PreviewIcon", typeof(RectTransform), typeof(Image));
            previewGO.transform.SetParent(iconAreaRT, false);
            var previewRT  = previewGO.GetComponent<RectTransform>();
            Stretch(previewRT);
            var previewImg  = previewGO.GetComponent<Image>();
            previewImg.sprite         = outfit.previewIcon;
            previewImg.preserveAspect = true;
            previewImg.color          = isFree ? W100 : new Color(1f, 1f, 1f, 0.55f);
        }

        // Name
        var nameRT = MakeTMP(cardRT, "ItemName", outfit.outfitName, 14f, W100, TextAlignmentOptions.Center, true);
        nameRT.anchorMin = new Vector2(0.05f, 0.24f);
        nameRT.anchorMax = new Vector2(0.95f, 0.38f);
        nameRT.offsetMin = nameRT.offsetMax = Vector2.zero;
        nameRT.GetComponent<TextMeshProUGUI>().enableWordWrapping = false;

        // Price
        string priceStr = isFree ? "FREE" : ("\u2605 " + outfit.price);
        var priceRT = MakeTMP(cardRT, "Price", priceStr, 14f, isFree ? W65 : PRICE_COL, TextAlignmentOptions.Left, false);
        priceRT.anchorMin = new Vector2(0.06f, 0.04f);
        priceRT.anchorMax = new Vector2(0.50f, 0.24f);
        priceRT.offsetMin = priceRT.offsetMax = Vector2.zero;
        priceRT.GetComponent<TextMeshProUGUI>().enableWordWrapping = false;

        // BUY / OWNED button
        var btnGO = new GameObject("BuyButton", typeof(RectTransform), typeof(Image), typeof(Button));
        btnGO.transform.SetParent(cardRT, false);
        var btnRT  = btnGO.GetComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(0.50f, 0.04f);
        btnRT.anchorMax = new Vector2(0.94f, 0.24f);
        btnRT.offsetMin = btnRT.offsetMax = Vector2.zero;
        var btnImg  = btnGO.GetComponent<Image>();
        btnImg.sprite = spr; btnImg.color = isFree ? OWNED_BG : BUY_BG; btnImg.type = Image.Type.Sliced;
        btnGO.AddComponent<ButtonHoverEffect>();

        var btnTxtRT = MakeTMP(btnRT, "Label", isFree ? "OWNED" : "BUY",
                                13f, isFree ? W65 : W100, TextAlignmentOptions.Center, true);
        Stretch(btnTxtRT);

        // ShopOutfitButton runtime component
        var shopBtn = btnGO.AddComponent<ShopOutfitButton>();
        shopBtn.outfit      = outfit;
        shopBtn.buttonImage = btnImg;
        shopBtn.buttonLabel = btnTxtRT.GetComponent<TextMeshProUGUI>();
        shopBtn.iconImage   = (outfit.previewIcon != null)
            ? iconAreaRT.Find("PreviewIcon")?.GetComponent<Image>() : null;

        // Wire button click
        var btn = btnGO.GetComponent<Button>();
        UnityEventTools.AddPersistentListener(btn.onClick, shopBtn.OnBuyPressed);
        if (isFree) btn.interactable = false;
    }

    // ── Category page (Songs / Stages — still hardcoded) ─────────────────────
    static GameObject BuildCategoryPage(RectTransform panel, string pageId,
                                         string pageTitle, ShopItem[] items, int activeTab)
    {
        var page = MakeRect(panel, pageId);
        Stretch(page);
        BuildBackground(page);
        BuildTopBar(page);
        BuildCategoryTitle(page, pageTitle);
        BuildTabBar(page, activeTab);
        BuildScrollView(page, items);
        return page.gameObject;
    }

    static void BuildCategoryTitle(RectTransform page, string title)
    {
        var rt = MakeTMP(page, "TitleText", title, 74f, TITLE_YEL, TextAlignmentOptions.Center, true);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot     = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(700f, 100f);
        rt.anchoredPosition = new Vector2(0f, -110f);
        var t = rt.GetComponent<TextMeshProUGUI>();
        t.characterSpacing  = 5f;
        t.enableWordWrapping = false;
        ApplyBungeeFont(t);
    }

    static void BuildTabBar(RectTransform page, int activeTabIndex)
    {
        var spr = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        var tabBarGO = new GameObject("TabBar", typeof(RectTransform));
        tabBarGO.transform.SetParent(page, false);
        var tbRT = tabBarGO.GetComponent<RectTransform>();
        tbRT.anchorMin = tbRT.anchorMax = new Vector2(0.5f, 1f);
        tbRT.pivot     = new Vector2(0.5f, 1f);
        tbRT.sizeDelta = new Vector2(500f, 52f);
        tbRT.anchoredPosition = new Vector2(0f, -226f);

        string[] labels = { "OUTFITS", "SONGS", "STAGES" };
        float[]  xPos   = { -160f, 0f, 160f };

        for (int i = 0; i < 3; i++)
        {
            bool active = i == activeTabIndex;
            var tabGO = new GameObject("Tab_" + labels[i], typeof(RectTransform), typeof(Image), typeof(Button));
            tabGO.transform.SetParent(tbRT, false);
            var tRT  = tabGO.GetComponent<RectTransform>();
            tRT.anchorMin = tRT.anchorMax = new Vector2(0.5f, 0.5f);
            tRT.pivot     = new Vector2(0.5f, 0.5f);
            tRT.sizeDelta = new Vector2(145f, 44f);
            tRT.anchoredPosition = new Vector2(xPos[i], 0f);
            var tImg  = tabGO.GetComponent<Image>();
            tImg.sprite = spr;
            tImg.color  = active ? TAB_ACTIVE : new Color(0f, 0f, 0f, 0f);
            tImg.type   = Image.Type.Sliced;
            var tTxtRT = MakeTMP(tRT, "Label", labels[i], 15f,
                                  active ? W100 : W40, TextAlignmentOptions.Center, active);
            Stretch(tTxtRT);
            tTxtRT.GetComponent<TextMeshProUGUI>().enableWordWrapping = false;
            tabGO.AddComponent<ButtonHoverEffect>();
        }
    }

    static void BuildScrollView(RectTransform page, ShopItem[] items)
    {
        // ScrollRect container — fills from below the tab bar to above a bottom margin
        var scrollGO = new GameObject("ScrollView", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
        scrollGO.transform.SetParent(page, false);
        var scrollRT  = scrollGO.GetComponent<RectTransform>();
        scrollRT.anchorMin = new Vector2(0f, 0f);
        scrollRT.anchorMax = new Vector2(1f, 1f);
        scrollRT.offsetMin = new Vector2(40f, 60f);    // 40 left, 60 from bottom
        scrollRT.offsetMax = new Vector2(-40f, -296f); // 40 right, 296 from top (below tab bar)
        var scrollImg  = scrollGO.GetComponent<Image>();
        scrollImg.color = new Color(0f, 0f, 0f, 0f);  // transparent shell
        var scrollRect  = scrollGO.GetComponent<ScrollRect>();
        scrollRect.horizontal       = false;
        scrollRect.scrollSensitivity = 30f;
        scrollRect.movementType     = ScrollRect.MovementType.Elastic;

        // Viewport
        var vpGO = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        vpGO.transform.SetParent(scrollRT, false);
        var vpRT  = vpGO.GetComponent<RectTransform>();
        Stretch(vpRT);
        var vpImg  = vpGO.GetComponent<Image>();
        vpImg.color = new Color(1f, 1f, 1f, 0.01f); // near-invisible for mask clipping
        vpGO.GetComponent<Mask>().showMaskGraphic = false;
        scrollRect.viewport = vpRT;

        // Content (DynamicShopGridLayout will control its height)
        var contentGO = new GameObject("Content", typeof(RectTransform));
        contentGO.transform.SetParent(vpRT, false);
        var contentRT  = contentGO.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0f, 1f);
        contentRT.anchorMax = new Vector2(1f, 1f);
        contentRT.pivot     = new Vector2(0f, 1f);
        contentRT.sizeDelta = new Vector2(0f, 600f); // height set by grid
        contentRT.anchoredPosition = Vector2.zero;
        scrollRect.content = contentRT;

        // Dynamic grid layout
        var grid = contentGO.AddComponent<DynamicShopGridLayout>();
        grid.content     = contentRT;
        grid.viewport    = vpRT;
        grid.columns     = 4;
        grid.cellSize    = new Vector2(380f, 290f);
        grid.horizontalSpacing = 24f;
        grid.verticalSpacing   = 24f;
        grid.leftPadding  = 50f;
        grid.rightPadding = 50f;
        grid.topPadding   = 24f;
        grid.bottomPadding = 40f;
        grid.controlChildSize      = true;
        grid.clampContentToViewport = true;

        // Populate item cards
        for (int i = 0; i < items.Length; i++)
            BuildItemCard(contentRT, items[i]);

        grid.RebuildNow();

        // Scroll-down arrow hint at bottom of page
        BuildScrollArrow(page);
    }

    static void BuildItemCard(RectTransform content, ShopItem item)
    {
        var spr  = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        var knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

        string safeName = item.Name.Replace(" ", "").Replace("'", "");

        // ── Border (direct child of grid content) ─────────────────────────────
        var borderGO = new GameObject(safeName + "_Border", typeof(RectTransform), typeof(Image));
        borderGO.transform.SetParent(content, false);
        var borderRT  = borderGO.GetComponent<RectTransform>();
        borderRT.anchorMin = borderRT.anchorMax = new Vector2(0f, 1f);
        borderRT.pivot     = new Vector2(0f, 1f);
        borderRT.sizeDelta = new Vector2(380f, 290f); // overridden by grid
        var borderImg  = borderGO.GetComponent<Image>();
        borderImg.sprite = spr;
        borderImg.color  = ITEM_BORDER;
        borderImg.type   = Image.Type.Sliced;

        // ── Card face ─────────────────────────────────────────────────────────
        var cardGO = new GameObject(safeName, typeof(RectTransform), typeof(Image));
        cardGO.transform.SetParent(borderRT, false);
        var cardRT  = cardGO.GetComponent<RectTransform>();
        cardRT.anchorMin = Vector2.zero;
        cardRT.anchorMax = Vector2.one;
        cardRT.offsetMin = new Vector2(2f, 2f);
        cardRT.offsetMax = new Vector2(-2f, -2f);
        var cardImg  = cardGO.GetComponent<Image>();
        cardImg.sprite = spr;
        cardImg.color  = ITEM_BG;
        cardImg.type   = Image.Type.Sliced;

        // ── Icon area (top ~58%) ──────────────────────────────────────────────
        var iconAreaGO = new GameObject("IconArea", typeof(RectTransform), typeof(Image));
        iconAreaGO.transform.SetParent(cardRT, false);
        var iconAreaRT  = iconAreaGO.GetComponent<RectTransform>();
        iconAreaRT.anchorMin = new Vector2(0.08f, 0.40f);
        iconAreaRT.anchorMax = new Vector2(0.92f, 0.94f);
        iconAreaRT.offsetMin = iconAreaRT.offsetMax = Vector2.zero;
        var iconAreaImg  = iconAreaGO.GetComponent<Image>();
        iconAreaImg.sprite = knob;
        iconAreaImg.color  = ICON_AREA;

        var iconRT = MakeTMP(iconAreaRT, "Icon", item.Icon, 42f, W100, TextAlignmentOptions.Center, false);
        Stretch(iconRT);
        iconRT.GetComponent<TextMeshProUGUI>().enableWordWrapping = false;

        // ── Item name ─────────────────────────────────────────────────────────
        var nameRT = MakeTMP(cardRT, "ItemName", item.Name, 14f, W100, TextAlignmentOptions.Center, true);
        nameRT.anchorMin = new Vector2(0.05f, 0.24f);
        nameRT.anchorMax = new Vector2(0.95f, 0.40f);
        nameRT.offsetMin = nameRT.offsetMax = Vector2.zero;
        nameRT.GetComponent<TextMeshProUGUI>().enableWordWrapping = false;

        // ── Price (bottom-left) ───────────────────────────────────────────────
        var priceRT = MakeTMP(cardRT, "Price", "\u2605 " + item.Price, 14f, PRICE_COL, TextAlignmentOptions.Left, false);
        priceRT.anchorMin = new Vector2(0.06f, 0.04f);
        priceRT.anchorMax = new Vector2(0.50f, 0.24f);
        priceRT.offsetMin = priceRT.offsetMax = Vector2.zero;
        priceRT.GetComponent<TextMeshProUGUI>().enableWordWrapping = false;

        // ── BUY / OWNED button (bottom-right) ─────────────────────────────────
        var btnGO = new GameObject("BuyButton", typeof(RectTransform), typeof(Image), typeof(Button));
        btnGO.transform.SetParent(cardRT, false);
        var btnRT  = btnGO.GetComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(0.50f, 0.04f);
        btnRT.anchorMax = new Vector2(0.94f, 0.24f);
        btnRT.offsetMin = btnRT.offsetMax = Vector2.zero;
        var btnImg  = btnGO.GetComponent<Image>();
        btnImg.sprite = spr;
        btnImg.color  = item.Owned ? OWNED_BG : BUY_BG;
        btnImg.type   = Image.Type.Sliced;
        btnGO.AddComponent<ButtonHoverEffect>();

        var btnTxtRT = MakeTMP(btnRT, "Label", item.Owned ? "OWNED" : "BUY",
                                13f, item.Owned ? W65 : W100, TextAlignmentOptions.Center, true);
        Stretch(btnTxtRT);
    }

    static void BuildScrollArrow(RectTransform page)
    {
        var spr = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        var arrowGO = new GameObject("ScrollArrow", typeof(RectTransform), typeof(Image));
        arrowGO.transform.SetParent(page, false);
        var arrowRT  = arrowGO.GetComponent<RectTransform>();
        arrowRT.anchorMin = arrowRT.anchorMax = new Vector2(0.5f, 0f);
        arrowRT.pivot     = new Vector2(0.5f, 0f);
        arrowRT.sizeDelta = new Vector2(52f, 52f);
        arrowRT.anchoredPosition = new Vector2(0f, 8f);
        var arrowImg  = arrowGO.GetComponent<Image>();
        arrowImg.sprite = spr;
        arrowImg.color  = new Color(0.24f, 0.11f, 0.48f, 0.85f);
        arrowImg.type   = Image.Type.Sliced;
        var arrowTxt = MakeTMP(arrowRT, "Label", "\u2193", 22f, W100, TextAlignmentOptions.Center, false);
        Stretch(arrowTxt);
    }

    // ── Shared page helpers ───────────────────────────────────────────────────
    static void BuildBackground(RectTransform page)
    {
        var bg = MakeImg(page, "Background", BG);
        Stretch(bg);
        BuildStarField(page);
    }

    static void BuildStarField(RectTransform page)
    {
        var stars = MakeRect(page, "StarField");
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

    static void BuildTopBar(RectTransform page)
    {
        var spr = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        // ── Back button (top-left) ────────────────────────────────────────────
        var backGO = new GameObject("BackButton", typeof(RectTransform), typeof(Image), typeof(Button));
        backGO.transform.SetParent(page, false);
        var backRT  = backGO.GetComponent<RectTransform>();
        backRT.anchorMin = backRT.anchorMax = new Vector2(0f, 1f);
        backRT.pivot     = new Vector2(0f, 1f);
        backRT.sizeDelta = new Vector2(70f, 70f);
        backRT.anchoredPosition = new Vector2(32f, -26f);
        var backImg  = backGO.GetComponent<Image>();
        backImg.sprite = spr;
        backImg.color  = BACK_BTN;
        backImg.type   = Image.Type.Sliced;
        backGO.AddComponent<ButtonHoverEffect>();
        var backTxtRT = MakeTMP(backRT, "Label", "\u2190", 28f, W100, TextAlignmentOptions.Center, true);
        Stretch(backTxtRT);

        // ── Starcoins badge (top-right) ───────────────────────────────────────
        // Border pill
        var borderGO = new GameObject("StarcoinsDisplay", typeof(RectTransform), typeof(Image));
        borderGO.transform.SetParent(page, false);
        var borderRT  = borderGO.GetComponent<RectTransform>();
        borderRT.anchorMin = borderRT.anchorMax = new Vector2(1f, 1f);
        borderRT.pivot     = new Vector2(1f, 1f);
        borderRT.sizeDelta = new Vector2(242f, 52f);
        borderRT.anchoredPosition = new Vector2(-28f, -26f);
        var borderImg  = borderGO.GetComponent<Image>();
        borderImg.sprite = spr;
        borderImg.color  = COIN_BORDER;
        borderImg.type   = Image.Type.Sliced;
        // Inner fill
        var fillGO = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillGO.transform.SetParent(borderRT, false);
        var fillRT  = fillGO.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = new Vector2(2f, 2f);
        fillRT.offsetMax = new Vector2(-2f, -2f);
        var fillImg  = fillGO.GetComponent<Image>();
        fillImg.sprite = spr;
        fillImg.color  = COIN_BG;
        fillImg.type   = Image.Type.Sliced;
        var coinTxtRT = MakeTMP(fillRT, "CoinsText", "\u2605 1,200 STARCOINS", 17f, W100, TextAlignmentOptions.Center, true);
        Stretch(coinTxtRT);
    }

    // ── Button wiring ─────────────────────────────────────────────────────────
    static void WireLandingCards(GameObject landingPage, ShopPageNavigator nav)
    {
        var outfitsBtn = landingPage.transform.Find("OutfitsCard_Border/OutfitsCard")?.GetComponent<Button>();
        var songsBtn   = landingPage.transform.Find("SongsCard_Border/SongsCard")?.GetComponent<Button>();
        var stagesBtn  = landingPage.transform.Find("StagesCard_Border/StagesCard")?.GetComponent<Button>();

        if (outfitsBtn != null) UnityEventTools.AddPersistentListener(outfitsBtn.onClick, nav.OpenOutfitsPage);
        else Debug.LogWarning("[ShopUIBuilder] OutfitsCard button not found.");
        if (songsBtn   != null) UnityEventTools.AddPersistentListener(songsBtn.onClick,   nav.OpenSongsPage);
        else Debug.LogWarning("[ShopUIBuilder] SongsCard button not found.");
        if (stagesBtn  != null) UnityEventTools.AddPersistentListener(stagesBtn.onClick,  nav.OpenStagesPage);
        else Debug.LogWarning("[ShopUIBuilder] StagesCard button not found.");
    }

    static void WireAllBackButtons(GameObject shopPanel, ShopPageNavigator nav)
    {
        var allButtons = shopPanel.GetComponentsInChildren<Button>(includeInactive: true);
        foreach (var btn in allButtons)
        {
            if (btn.gameObject.name == "BackButton")
                UnityEventTools.AddPersistentListener(btn.onClick, nav.OnBackPressed);
        }
    }

    static void WireAllTabBars(GameObject outfitsPage, GameObject songsPage,
                                GameObject stagesPage, ShopPageNavigator nav)
    {
        WireTabsInPage(outfitsPage, nav);
        WireTabsInPage(songsPage,   nav);
        WireTabsInPage(stagesPage,  nav);
    }

    static void WireTabsInPage(GameObject page, ShopPageNavigator nav)
    {
        var tabBar = page.transform.Find("TabBar");
        if (tabBar == null) return;

        var outfitsTab = tabBar.Find("Tab_OUTFITS")?.GetComponent<Button>();
        var songsTab   = tabBar.Find("Tab_SONGS")?.GetComponent<Button>();
        var stagesTab  = tabBar.Find("Tab_STAGES")?.GetComponent<Button>();

        if (outfitsTab != null) UnityEventTools.AddPersistentListener(outfitsTab.onClick, nav.OpenOutfitsPage);
        if (songsTab   != null) UnityEventTools.AddPersistentListener(songsTab.onClick,   nav.OpenSongsPage);
        if (stagesTab  != null) UnityEventTools.AddPersistentListener(stagesTab.onClick,  nav.OpenStagesPage);
    }

    // ── Creation helpers ──────────────────────────────────────────────────────
    static RectTransform MakeRect(RectTransform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }

    static RectTransform MakeImg(RectTransform parent, string name, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = color;
        return go.GetComponent<RectTransform>();
    }

    static RectTransform MakeTMP(RectTransform parent, string name, string text,
                                   float size, Color color, TextAlignmentOptions align, bool bold)
    {
        var go  = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text               = text;
        tmp.fontSize           = size;
        tmp.color              = color;
        tmp.alignment          = align;
        tmp.fontStyle          = bold ? FontStyles.Bold : FontStyles.Normal;
        tmp.enableWordWrapping = false;
        tmp.richText           = true;
        tmp.overflowMode       = TextOverflowModes.Overflow;
        return go.GetComponent<RectTransform>();
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static void ApplyBungeeFont(TextMeshProUGUI t)
    {
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Bungee-Regular SDF.asset");
        if (font != null) t.font = font;
    }
}
