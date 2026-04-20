using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Builds the 2D Character Customisation screen in one click.
/// Menu: PopstarHub ? Build Avatar Scene UI
///
/// Requires: A Canvas in the scene.
///
/// Layout (1920 � 1080):
///   AvatarUIRoot
///   +-- Background         � full-screen dark purple + star field
///   +-- TopBar             � back button | "MY AVATAR" title | starcoins badge
///   +-- GenderBar          � FEMALE | MALE toggle
///   +-- AvatarZone         � centre display area with spotlight glow
///   +-- OutfitPanel        � right-side scroll panel (AvatarUIPanel populates cards at runtime)
///
/// Components wired:
///   AvatarOutfitManager  ? on AvatarDisplay child of AvatarZone
///   AvatarIdleTimer      ? on AvatarDisplay
///   AvatarGenderSelector ? on AvatarUIRoot, buttons wired to SelectFemale/SelectMale
///   AvatarUIPanel        ? on OutfitPanel, linked to manager
/// </summary>
public class AvatarSceneBuilder : EditorWindow
{
    // -- Palette ---------------------------------------------------------------
    static readonly Color BG            = new Color(0.039f, 0.016f, 0.075f, 1.00f);
    static readonly Color TITLE_YEL     = new Color(0.912f, 0.851f, 0.282f, 1.00f);
    static readonly Color BACK_BTN      = new Color(0.125f, 0.060f, 0.268f, 1.00f);
    static readonly Color GENDER_ACTIVE = new Color(0.255f, 0.118f, 0.530f, 1.00f);
    static readonly Color GENDER_IDLE   = new Color(0.090f, 0.042f, 0.190f, 1.00f);
    static readonly Color BORDER_COL    = new Color(0.340f, 0.150f, 0.530f, 0.85f);
    static readonly Color COIN_BG       = new Color(0.168f, 0.078f, 0.355f, 1.00f);
    static readonly Color COIN_BORDER   = new Color(0.418f, 0.220f, 0.758f, 1.00f);
    static readonly Color PANEL_BG      = new Color(0.072f, 0.032f, 0.145f, 1.00f);
    static readonly Color PANEL_BORDER  = new Color(0.230f, 0.105f, 0.450f, 0.90f);
    static readonly Color STAR_COL      = new Color(1f, 1f, 1f, 0.65f);
    static readonly Color W100          = Color.white;
    static readonly Color W65           = new Color(1f, 1f, 1f, 0.65f);
    static readonly Color W25           = new Color(1f, 1f, 1f, 0.25f);

    // -- Entry point -----------------------------------------------------------
    [MenuItem("PopstarHub/Build Avatar Scene UI")]
    static void Build()
    {
        var canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[AvatarSceneBuilder] No Canvas found. Add one to the scene first.");
            return;
        }

        // Ensure EventSystem exists — without it no buttons or hover effects fire
        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<StandaloneInputModule>();
            Debug.Log("[AvatarSceneBuilder] Created missing EventSystem.");
        }

        // Canvas scaler
        var scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight  = 0.5f;

        // Destroy old build
        var old = GameObject.Find("AvatarUIRoot");
        if (old != null) Object.DestroyImmediate(old);

        var canvasRT = canvas.GetComponent<RectTransform>();
        var root     = MakeRect(canvasRT, "AvatarUIRoot");
        Stretch(root);

        BuildBackground(root);
        BuildTopBar(root);
        BuildGenderBar(root, out var femaleBtn, out var maleBtn);
        var avatarDisplayRT = BuildAvatarArea(root);
        var outfitPanelRT   = BuildOutfitPanel(root);

        // Add visible UI Image to AvatarDisplay
        var avatarImg = avatarDisplayRT.gameObject.AddComponent<Image>();
        avatarImg.color           = Color.white;
        avatarImg.preserveAspect  = true;
        avatarImg.raycastTarget   = false;

        // Hidden child: SpriteRenderer + Animator — animation clips target this SR
        var animSrcGO = new GameObject("AnimationSource");
        animSrcGO.transform.SetParent(avatarDisplayRT, false);
        var animSR   = animSrcGO.AddComponent<SpriteRenderer>();
        animSR.enabled = false; // invisible — only used as animation driver
        var avatarAnim = animSrcGO.AddComponent<Animator>();
        avatarAnim.enabled = false;

        // Bridge component still attached for scene compatibility — logic is now in AvatarOutfitManager.Update
        avatarDisplayRT.gameObject.AddComponent<AvatarAnimationBridge>();

        // AvatarOutfitManager + AvatarIdleTimer on avatar display
        var manager          = avatarDisplayRT.gameObject.AddComponent<AvatarOutfitManager>();
        manager.avatarImage      = avatarImg;
        manager.animationSource  = animSR;
        manager.avatarAnimator   = avatarAnim;
        var idleTimer = avatarDisplayRT.gameObject.AddComponent<AvatarIdleTimer>();
        idleTimer.avatarAnimator = avatarAnim;

        // AvatarUIPanel on outfit panel
        var scrollContent = outfitPanelRT.transform
                                .Find("ScrollView/Viewport/Content")
                                ?.GetComponent<RectTransform>();
        var uiPanel = outfitPanelRT.gameObject.AddComponent<AvatarUIPanel>();
        uiPanel.outfitManager = manager;
        uiPanel.cardContainer = scrollContent;

        // AvatarGenderSelector on root
        var selector = root.gameObject.AddComponent<AvatarGenderSelector>();
        var femaleBtnImg = root.transform
            .Find("GenderBar/GenderBorder_FEMALE/GenderBtn_FEMALE")
            ?.GetComponent<Image>();
        var maleBtnImg = root.transform
            .Find("GenderBar/GenderBorder_MALE/GenderBtn_MALE")
            ?.GetComponent<Image>();
        var so = new SerializedObject(selector);
        so.FindProperty("outfitManager").objectReferenceValue    = manager;
        so.FindProperty("uiPanel").objectReferenceValue          = uiPanel;
        so.FindProperty("femaleBtnImage").objectReferenceValue   = femaleBtnImg;
        so.FindProperty("maleBtnImage").objectReferenceValue     = maleBtnImg;
        so.ApplyModifiedProperties();

        // Wire gender buttons
        if (femaleBtn != null)
            UnityEventTools.AddPersistentListener(femaleBtn.onClick, selector.SelectFemale);
        if (maleBtn != null)
            UnityEventTools.AddPersistentListener(maleBtn.onClick, selector.SelectMale);

        // Wire PlayAnimation button
        var playAnimBtnGO = root.transform.Find("AvatarZone/PlayAnimBtn_Border/PlayAnimBtn");
        if (playAnimBtnGO != null)
            WirePlayAnimButton(playAnimBtnGO.GetComponent<Button>(), manager);

        // ── Enter Stage button + EnterStageHandler ────────────────────────────
        var enterStageBtn = BuildEnterStageButton(root);
        var handler = root.gameObject.AddComponent<EnterStageHandler>();
        var hso = new SerializedObject(handler);
        hso.FindProperty("outfitManager").objectReferenceValue  = manager;
        hso.FindProperty("genderSelector").objectReferenceValue = selector;
        hso.ApplyModifiedProperties();
        if (enterStageBtn != null)
            UnityEventTools.AddPersistentListener(enterStageBtn.onClick, handler.GoToStage);

        EditorUtility.SetDirty(canvas.gameObject);
        Debug.Log("[AvatarSceneBuilder] 2D Avatar Scene UI built. Create OutfitData assets (Assets > Create > PopstarHub > Outfit Data), assign characterSprite + optional animatorController, then populate femaleOutfits / maleOutfits on AvatarGenderSelector.");
    }

    // -- Sections --------------------------------------------------------------

    static void BuildBackground(RectTransform p)
    {
        var bg = MakeImg(p, "Background", BG);
        Stretch(bg);

        var stars = MakeRect(p, "StarField");
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

    static void BuildTopBar(RectTransform p)
    {
        var spr = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        // Back button
        var backGO = new GameObject("BackButton",
                                    typeof(RectTransform), typeof(Image), typeof(Button));
        backGO.transform.SetParent(p, false);
        var backRT  = backGO.GetComponent<RectTransform>();
        backRT.anchorMin = backRT.anchorMax = new Vector2(0f, 1f);
        backRT.pivot     = new Vector2(0f, 1f);
        backRT.sizeDelta = new Vector2(70f, 70f);
        backRT.anchoredPosition = new Vector2(32f, -26f);
        var backImg  = backGO.GetComponent<Image>();
        backImg.sprite = spr; backImg.color = BACK_BTN; backImg.type = Image.Type.Sliced;
        backGO.AddComponent<ButtonHoverEffect>();
        var backLbl = MakeTMP(backRT, "Label", "\u2190", 28f, W100, TextAlignmentOptions.Center, true);
        Stretch(backLbl);

        // Title
        var titleRT = MakeTMP(p, "TitleText", "MY AVATAR", 72f, TITLE_YEL,
                               TextAlignmentOptions.Center, true);
        titleRT.anchorMin = titleRT.anchorMax = new Vector2(0.5f, 1f);
        titleRT.pivot     = new Vector2(0.5f, 1f);
        titleRT.sizeDelta = new Vector2(600f, 96f);
        titleRT.anchoredPosition = new Vector2(0f, -36f);
        var titleTMP = titleRT.GetComponent<TextMeshProUGUI>();
        titleTMP.characterSpacing   = 5f;
        titleTMP.enableWordWrapping = false;
        var bungee = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Bungee-Regular SDF.asset");
        if (bungee != null) titleTMP.font = bungee;

        // Starcoins badge
        var coinBorderGO = new GameObject("StarcoinsDisplay", typeof(RectTransform), typeof(Image));
        coinBorderGO.transform.SetParent(p, false);
        var coinBorderRT  = coinBorderGO.GetComponent<RectTransform>();
        coinBorderRT.anchorMin = coinBorderRT.anchorMax = new Vector2(1f, 1f);
        coinBorderRT.pivot     = new Vector2(1f, 1f);
        coinBorderRT.sizeDelta = new Vector2(242f, 52f);
        coinBorderRT.anchoredPosition = new Vector2(-28f, -26f);
        var coinBorderImg  = coinBorderGO.GetComponent<Image>();
        coinBorderImg.sprite = spr; coinBorderImg.color = COIN_BORDER; coinBorderImg.type = Image.Type.Sliced;
        var coinFillGO = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        coinFillGO.transform.SetParent(coinBorderRT, false);
        var coinFillRT  = coinFillGO.GetComponent<RectTransform>();
        coinFillRT.anchorMin = Vector2.zero; coinFillRT.anchorMax = Vector2.one;
        coinFillRT.offsetMin = new Vector2(2f, 2f); coinFillRT.offsetMax = new Vector2(-2f, -2f);
        var coinFillImg  = coinFillGO.GetComponent<Image>();
        coinFillImg.sprite = spr; coinFillImg.color = COIN_BG; coinFillImg.type = Image.Type.Sliced;
        var coinLbl = MakeTMP(coinFillRT, "CoinsText", "\u2605 1,200 STARCOINS",
                               17f, W100, TextAlignmentOptions.Center, true);
        Stretch(coinLbl);
    }

    static void BuildGenderBar(RectTransform p, out Button femaleBtn, out Button maleBtn)
    {
        femaleBtn = maleBtn = null;
        var spr = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        var barGO = new GameObject("GenderBar", typeof(RectTransform));
        barGO.transform.SetParent(p, false);
        var barRT  = barGO.GetComponent<RectTransform>();
        barRT.anchorMin = barRT.anchorMax = new Vector2(0.5f, 1f);
        barRT.pivot     = new Vector2(0.5f, 1f);
        barRT.sizeDelta = new Vector2(360f, 58f);
        barRT.anchoredPosition = new Vector2(0f, -152f);

        string[] labels = { "FEMALE", "MALE" };
        float[]  xPos   = { -90f, 90f };
        bool[]   active = { true, false };

        for (int i = 0; i < 2; i++)
        {
            var brdGO = new GameObject("GenderBorder_" + labels[i], typeof(RectTransform), typeof(Image));
            brdGO.transform.SetParent(barRT, false);
            var bRT  = brdGO.GetComponent<RectTransform>();
            bRT.anchorMin = bRT.anchorMax = new Vector2(0.5f, 0.5f);
            bRT.pivot     = new Vector2(0.5f, 0.5f);
            bRT.sizeDelta = new Vector2(164f, 54f);
            bRT.anchoredPosition = new Vector2(xPos[i], 0f);
            var bImg  = brdGO.GetComponent<Image>();
            bImg.sprite = spr; bImg.color = BORDER_COL; bImg.type = Image.Type.Sliced;

            var btnGO = new GameObject("GenderBtn_" + labels[i],
                                        typeof(RectTransform), typeof(Image), typeof(Button));
            btnGO.transform.SetParent(brdGO.transform, false);
            var btnRT  = btnGO.GetComponent<RectTransform>();
            btnRT.anchorMin = Vector2.zero; btnRT.anchorMax = Vector2.one;
            btnRT.offsetMin = new Vector2(2f, 2f); btnRT.offsetMax = new Vector2(-2f, -2f);
            var btnImg  = btnGO.GetComponent<Image>();
            btnImg.sprite = spr;
            btnImg.color  = active[i] ? GENDER_ACTIVE : GENDER_IDLE;
            btnImg.type   = Image.Type.Sliced;
            btnGO.AddComponent<ButtonHoverEffect>();

            // Fix Button ColorBlock so tint doesn't make it black
            var btn = btnGO.GetComponent<Button>();
            var cb  = ColorBlock.defaultColorBlock;
            cb.normalColor      = Color.white;
            cb.highlightedColor = new Color(1f, 1f, 1f, 0.85f);
            cb.pressedColor     = new Color(0.8f, 0.8f, 0.8f, 1f);
            cb.selectedColor    = Color.white;
            btn.colors          = cb;

            var lblRT = MakeTMP(btnRT, "Label", labels[i], 15f,
                                 active[i] ? W100 : W65, TextAlignmentOptions.Center, active[i]);
            Stretch(lblRT);
            lblRT.GetComponent<TextMeshProUGUI>().characterSpacing = 2f;

            if (i == 0) femaleBtn = btn; else maleBtn = btn;
        }
    }

    static RectTransform BuildAvatarArea(RectTransform p)
    {
        var knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

        // Zone (left 65%, below top bar + gender bar)
        var zoneGO = new GameObject("AvatarZone", typeof(RectTransform));
        zoneGO.transform.SetParent(p, false);
        var zoneRT  = zoneGO.GetComponent<RectTransform>();
        zoneRT.anchorMin = new Vector2(0f, 0f);
        zoneRT.anchorMax = new Vector2(0.65f, 1f);
        zoneRT.offsetMin = new Vector2(0f, 30f);
        zoneRT.offsetMax = new Vector2(0f, -222f);

        // Spotlight glow
        var glowGO = new GameObject("SpotlightGlow", typeof(RectTransform), typeof(Image));
        glowGO.transform.SetParent(zoneRT, false);
        var glowRT  = glowGO.GetComponent<RectTransform>();
        glowRT.anchorMin = new Vector2(0.15f, 0.05f);
        glowRT.anchorMax = new Vector2(0.85f, 0.85f);
        glowRT.offsetMin = glowRT.offsetMax = Vector2.zero;
        var glowImg  = glowGO.GetComponent<Image>();
        glowImg.sprite = knob;
        glowImg.color  = new Color(0.28f, 0.10f, 0.55f, 0.28f);

        // Stage floor line
        var floorGO = new GameObject("StageFloor", typeof(RectTransform), typeof(Image));
        floorGO.transform.SetParent(zoneRT, false);
        var floorRT  = floorGO.GetComponent<RectTransform>();
        floorRT.anchorMin = new Vector2(0.05f, 0f);
        floorRT.anchorMax = new Vector2(0.95f, 0f);
        floorRT.pivot     = new Vector2(0.5f, 0f);
        floorRT.sizeDelta = new Vector2(0f, 3f);
        floorRT.anchoredPosition = new Vector2(0f, 38f);
        floorGO.GetComponent<Image>().color = new Color(0.45f, 0.20f, 0.80f, 0.50f);

        // AvatarDisplay � user adds SpriteRenderer + Animator here
        var displayGO = new GameObject("AvatarDisplay", typeof(RectTransform));
        displayGO.transform.SetParent(zoneRT, false);
        var displayRT  = displayGO.GetComponent<RectTransform>();
        displayRT.anchorMin = new Vector2(0.15f, 0.08f);
        displayRT.anchorMax = new Vector2(0.85f, 0.96f);
        displayRT.offsetMin = displayRT.offsetMax = Vector2.zero;

        // Faint placeholder text visible before first outfit is assigned in Inspector
        var hintRT = MakeTMP(displayRT, "Placeholder",
                              "Assign OutfitData\nassets to\nAvatarGenderSelector",
                              14f, W25, TextAlignmentOptions.Center, false);
        Stretch(hintRT);
        hintRT.GetComponent<TextMeshProUGUI>().enableWordWrapping = true;

        // PLAY ANIMATION button — sits at the bottom of the AvatarZone
        var playBorderGO = new GameObject("PlayAnimBtn_Border", typeof(RectTransform), typeof(Image));
        playBorderGO.transform.SetParent(zoneRT, false);
        var playBorderRT  = playBorderGO.GetComponent<RectTransform>();
        playBorderRT.anchorMin = new Vector2(0.5f, 0f);
        playBorderRT.anchorMax = new Vector2(0.5f, 0f);
        playBorderRT.pivot     = new Vector2(0.5f, 0f);
        playBorderRT.sizeDelta = new Vector2(262f, 58f);
        playBorderRT.anchoredPosition = new Vector2(0f, 48f);
        var playBorderImg  = playBorderGO.GetComponent<Image>();
        var uiSpr = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        playBorderImg.sprite = uiSpr; playBorderImg.color = BORDER_COL; playBorderImg.type = Image.Type.Sliced;

        var playBtnGO = new GameObject("PlayAnimBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        playBtnGO.transform.SetParent(playBorderGO.transform, false);
        var playBtnRT  = playBtnGO.GetComponent<RectTransform>();
        playBtnRT.anchorMin = Vector2.zero; playBtnRT.anchorMax = Vector2.one;
        playBtnRT.offsetMin = new Vector2(2f, 2f); playBtnRT.offsetMax = new Vector2(-2f, -2f);
        var playBtnImg  = playBtnGO.GetComponent<Image>();
        playBtnImg.sprite = uiSpr; playBtnImg.color = GENDER_ACTIVE; playBtnImg.type = Image.Type.Sliced;
        playBtnGO.AddComponent<ButtonHoverEffect>();
        var playCb = ColorBlock.defaultColorBlock;
        playCb.normalColor = Color.white; playCb.highlightedColor = new Color(1f,1f,1f,0.85f);
        playCb.pressedColor = new Color(0.8f,0.8f,0.8f,1f); playCb.selectedColor = Color.white;
        playBtnGO.GetComponent<Button>().colors = playCb;

        var playLblRT = MakeTMP(playBtnRT, "Label", "\u25B6  PLAY ANIMATION", 15f, W100,
                                 TextAlignmentOptions.Center, true);
        Stretch(playLblRT);

        return displayRT;
    }

    // Called after Build() to wire PlayAnimation button — needs manager reference
    static Button _playAnimBtn;
    static void WirePlayAnimButton(Button btn, AvatarOutfitManager manager)
    {
        if (btn == null || manager == null) return;
        UnityEventTools.AddPersistentListener(btn.onClick, manager.PlayAnimation);
    }

    // ── Enter Stage button (bottom-centre of screen) ──────────────────────────
    static Button BuildEnterStageButton(RectTransform root)
    {
        var spr = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        Color STAGE_BG     = new Color(0.180f, 0.082f, 0.380f, 1.00f);
        Color STAGE_BORDER = new Color(0.420f, 0.220f, 0.760f, 1.00f);

        // Border
        var borderGO = new GameObject("EnterStageBtn_Border", typeof(RectTransform), typeof(Image));
        borderGO.transform.SetParent(root, false);
        var borderRT  = borderGO.GetComponent<RectTransform>();
        borderRT.anchorMin = borderRT.anchorMax = new Vector2(0.5f, 0f);
        borderRT.pivot     = new Vector2(0.5f, 0f);
        borderRT.sizeDelta = new Vector2(342f, 68f);
        borderRT.anchoredPosition = new Vector2(0f, 36f);
        var borderImg  = borderGO.GetComponent<Image>();
        borderImg.sprite = spr; borderImg.color = STAGE_BORDER; borderImg.type = Image.Type.Sliced;

        // Button face
        var btnGO = new GameObject("EnterStageBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        btnGO.transform.SetParent(borderRT, false);
        var btnRT  = btnGO.GetComponent<RectTransform>();
        btnRT.anchorMin = Vector2.zero; btnRT.anchorMax = Vector2.one;
        btnRT.offsetMin = new Vector2(2f, 2f); btnRT.offsetMax = new Vector2(-2f, -2f);
        var btnImg  = btnGO.GetComponent<Image>();
        btnImg.sprite = spr; btnImg.color = STAGE_BG; btnImg.type = Image.Type.Sliced;
        var btn = btnGO.GetComponent<Button>();
        btn.targetGraphic = btnImg;
        var bc = btn.colors;
        bc.highlightedColor = new Color(0.24f, 0.11f, 0.50f, 1f);
        bc.pressedColor     = new Color(0.14f, 0.06f, 0.30f, 1f);
        btn.colors = bc;

        // Label
        var lblRT = MakeTMP(btnRT, "Label", "\U0001F3A4  Enter Stage", 24f, W100,
                             TextAlignmentOptions.Center, true);
        Stretch(lblRT);

        return btn;
    }

    static RectTransform BuildOutfitPanel(RectTransform p)
    {
        var spr = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        // Border
        var borderGO = new GameObject("OutfitPanel_Border", typeof(RectTransform), typeof(Image));
        borderGO.transform.SetParent(p, false);
        var borderRT  = borderGO.GetComponent<RectTransform>();
        borderRT.anchorMin = new Vector2(0.67f, 0f);
        borderRT.anchorMax = new Vector2(1f, 1f);
        borderRT.offsetMin = new Vector2(0f, 30f);
        borderRT.offsetMax = new Vector2(-28f, -222f);
        var borderImg  = borderGO.GetComponent<Image>();
        borderImg.sprite = spr; borderImg.color = PANEL_BORDER; borderImg.type = Image.Type.Sliced;

        // Fill
        var panelGO = new GameObject("OutfitPanel", typeof(RectTransform), typeof(Image));
        panelGO.transform.SetParent(borderRT, false);
        var panelRT  = panelGO.GetComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero; panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = new Vector2(2f, 2f); panelRT.offsetMax = new Vector2(-2f, -2f);
        panelGO.GetComponent<Image>().color = PANEL_BG;

        // Panel title
        var panelTitleRT = MakeTMP(panelRT, "PanelTitle", "OUTFITS",
                                    18f, TITLE_YEL, TextAlignmentOptions.Center, true);
        panelTitleRT.anchorMin = new Vector2(0f, 1f);
        panelTitleRT.anchorMax = new Vector2(1f, 1f);
        panelTitleRT.pivot     = new Vector2(0.5f, 1f);
        panelTitleRT.sizeDelta = new Vector2(0f, 48f);
        panelTitleRT.anchoredPosition = new Vector2(0f, -8f);
        var panelTitleTMP = panelTitleRT.GetComponent<TextMeshProUGUI>();
        panelTitleTMP.characterSpacing = 4f;
        var bungee = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Bungee-Regular SDF.asset");
        if (bungee != null) panelTitleTMP.font = bungee;

        // Divider
        var divGO = new GameObject("Divider", typeof(RectTransform), typeof(Image));
        divGO.transform.SetParent(panelRT, false);
        var divRT  = divGO.GetComponent<RectTransform>();
        divRT.anchorMin = new Vector2(0.05f, 1f);
        divRT.anchorMax = new Vector2(0.95f, 1f);
        divRT.pivot     = new Vector2(0.5f, 1f);
        divRT.sizeDelta = new Vector2(0f, 2f);
        divRT.anchoredPosition = new Vector2(0f, -56f);
        divGO.GetComponent<Image>().color = new Color(0.38f, 0.18f, 0.72f, 0.55f);

        // ScrollRect
        var scrollGO = new GameObject("ScrollView",
                                       typeof(RectTransform), typeof(Image), typeof(ScrollRect));
        scrollGO.transform.SetParent(panelRT, false);
        var scrollRT  = scrollGO.GetComponent<RectTransform>();
        scrollRT.anchorMin = Vector2.zero; scrollRT.anchorMax = Vector2.one;
        scrollRT.offsetMin = new Vector2(0f, 0f); scrollRT.offsetMax = new Vector2(0f, -62f);
        scrollGO.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
        var scrollRect  = scrollGO.GetComponent<ScrollRect>();
        scrollRect.horizontal        = false;
        scrollRect.scrollSensitivity = 30f;
        scrollRect.movementType      = ScrollRect.MovementType.Elastic;

        // Viewport
        var vpGO = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        vpGO.transform.SetParent(scrollGO.transform, false);
        var vpRT  = vpGO.GetComponent<RectTransform>();
        Stretch(vpRT);
        vpGO.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.01f);
        vpGO.GetComponent<Mask>().showMaskGraphic = false;
        scrollRect.viewport = vpRT;

        // Content
        var contentGO = new GameObject("Content", typeof(RectTransform));
        contentGO.transform.SetParent(vpRT, false);
        var contentRT  = contentGO.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0f, 1f); contentRT.anchorMax = new Vector2(1f, 1f);
        contentRT.pivot     = new Vector2(0.5f, 1f);
        contentRT.sizeDelta = new Vector2(0f, 800f);
        contentRT.anchoredPosition = Vector2.zero;
        scrollRect.content = contentRT;

        return panelGO.GetComponent<RectTransform>();
    }

    // -- Helpers ---------------------------------------------------------------

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
                                   float size, Color color,
                                   TextAlignmentOptions align, bool bold)
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
}