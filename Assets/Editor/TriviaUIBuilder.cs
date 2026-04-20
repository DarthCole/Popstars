using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// PopstarHub — Trivia screen builder.
/// Menu: PopstarHub ▶ Build Trivia UI
/// Destroys and rebuilds "TriviaCanvas" each run.
/// At runtime, TriviaGameUI.Start() detects this canvas and wires callbacks automatically.
/// Reference resolution: 1920 × 1080.
/// </summary>
public static class TriviaUIBuilder
{
    // ── Palette (matches TriviaGameUI) ───────────────────────────────────────
    static readonly Color BG_COL     = new Color(0.039f, 0.016f, 0.075f, 1f);
    static readonly Color CARD_COL   = new Color(0.085f, 0.040f, 0.160f, 0.90f);
    static readonly Color YELLOW     = new Color(1f,    0.87f, 0.12f, 1f);
    static readonly Color DARK_TEXT  = new Color(0.14f, 0.05f, 0.28f, 1f);
    static readonly Color FADED      = new Color(1f, 1f, 1f, 0.55f);
    static readonly Color WHITE      = Color.white;
    static readonly Color ANS_NORMAL = new Color(0.16f, 0.07f, 0.32f, 0.95f);
    static readonly Color ANS_BORDER = new Color(0.50f, 0.27f, 0.78f, 0.75f);
    static readonly Color INPUT_COL  = new Color(1f, 1f, 1f, 0.10f);

    static Sprite sRound;
    static TMP_FontAsset font;

    // ── Entry point ──────────────────────────────────────────────────────────
    [MenuItem("PopstarHub/Build Trivia UI")]
    static void Build()
    {
        sRound = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        font   = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Popstar SDF.asset");

        // Destroy any previously built canvas
        var old = GameObject.Find("TriviaCanvas");
        if (old != null) Object.DestroyImmediate(old);

        // Create new canvas
        var canvasGO = new GameObject("TriviaCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        var root = canvasGO.GetComponent<RectTransform>();

        BuildBackground(root);

        var mode    = BuildModePanel(root);
        var pSetup  = BuildPartySetupPanel(root);
        var pCount  = BuildPartyCountPanel(root);
        var pGame   = BuildPartyGamePanel(root);
        var pReveal = BuildPartyRevealPanel(root);
        var solo    = BuildSoloGamePanel(root);
        var results = BuildResultsPanel(root);

        // Show only ModePanel so you can see the landing screen in Scene view
        mode.SetActive(true);
        pSetup.SetActive(false);
        pCount.SetActive(false);
        pGame.SetActive(false);
        pReveal.SetActive(false);
        solo.SetActive(false);
        results.SetActive(false);

        // ── Attach and wire TriviaGameUI ────────────────────────────────────────
        WireTriviaGameUI(canvasGO);

        EditorUtility.SetDirty(canvasGO);
        Debug.Log("[TriviaUIBuilder] Done. Select panels in Hierarchy to tweak in Inspector.");
    }

    // ── Background ───────────────────────────────────────────────────────────
    static void BuildBackground(RectTransform root)
    {
        var bg = Img(root, "BG", BG_COL, false);
        Stretch(bg);

        // Soft colour blobs
        Blob(bg, new Vector2(-560,  290), 500, new Color(0.7f, 0.2f, 1f, 0.08f));
        Blob(bg, new Vector2( 580, -230), 560, new Color(0.3f, 0.1f, 1f, 0.06f));
        Blob(bg, new Vector2( 280,  380), 320, new Color(1f,   0.5f, 0.8f, 0.05f));

        // Star dots
        float[] sx = { -830f,-560f, 200f, 620f,-290f, 480f,-720f, 860f, 120f,-140f,
                        350f,-490f, 700f,-660f, 40f, 810f,-380f, 560f,-240f, 270f,-740f, 930f };
        float[] sy = {  460f, 360f, 490f, 480f, 420f,-450f, 160f, 380f,-360f, 310f,
                        250f,-280f, 290f,-400f,-480f,  90f,-300f,-130f, 130f,-420f, 230f,-200f };
        float[] ss = {    4f,   3f,   3f,   2f,   4f,   3f,   2f,   3f,   3f,   4f,
                          2f,   3f,   3f,   2f,   3f,   4f,   3f,   2f,   3f,   2f,   2f,   3f };

        var sf = E(bg, "StarField"); Stretch(sf);
        for (int i = 0; i < sx.Length; i++)
        {
            var dot = new GameObject("Star", typeof(RectTransform), typeof(Image));
            dot.transform.SetParent(sf, false);
            var rt = dot.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = Vector2.one * ss[i];
            rt.anchoredPosition = new Vector2(sx[i], sy[i]);
            dot.GetComponent<Image>().color = new Color(1, 1, 1, 0.22f);
        }
    }

    static void Blob(RectTransform parent, Vector2 pos, float size, Color color)
    {
        var go = new GameObject("Blob", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = Vector2.one * size;
        rt.anchoredPosition = pos;
        var img = go.GetComponent<Image>();
        img.color  = color;
        img.sprite = sRound;
        img.type   = Image.Type.Sliced;
        img.raycastTarget = false;
    }

    // ── PANEL 0 — Mode Select ────────────────────────────────────────────────
    static GameObject BuildModePanel(RectTransform root)
    {
        var p = Panel(root, "ModePanel");

        var title = Tmp(p, "Title", "Popstar <color=#EB6BA6>Trivia</color>", 78, WHITE, FontStyles.Bold);
        title.richText = true;
        title.enableWordWrapping = false;
        SizePos(title, new Vector2(900, 120), new Vector2(0, 340));

        var sub = Tmp(p, "Subtitle", "Choose your game mode", 26, FADED, FontStyles.Normal);
        SizePos(sub, new Vector2(700, 44), new Vector2(0, 255));

        // Party card
        var party = Box(p, "PartyCard", new Vector2(420, 420), new Vector2(-240, -20), CARD_COL);
        Tmp(party, "Emoji", "\U0001F389", 72, WHITE).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 100);
        Tmp(party, "CardTitle", "Party Mode", 36, WHITE, FontStyles.Bold).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 5);
        var pd = Tmp(party, "CardDesc", "Take turns & score points\nwith friends", 22, FADED);
        pd.enableWordWrapping = true;
        SizePos(pd, new Vector2(380, 70), new Vector2(0, -75));
        Btn(party, "Btn_PartyPlay", "Play", YELLOW, DARK_TEXT, new Vector2(0, -160), new Vector2(300, 64));

        // Solo card
        var solo = Box(p, "SoloCard", new Vector2(420, 420), new Vector2(240, -20), CARD_COL);
        Tmp(solo, "Emoji", "\U0001F3AF", 72, WHITE).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 100);
        Tmp(solo, "CardTitle", "Solo Mode", 36, WHITE, FontStyles.Bold).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 5);
        var sd = Tmp(solo, "CardDesc", "Answer 15 questions &\nearn \u2B50 StarCoins", 22, FADED);
        sd.enableWordWrapping = true;
        SizePos(sd, new Vector2(380, 70), new Vector2(0, -75));
        Btn(solo, "Btn_SoloPlay", "Play", YELLOW, DARK_TEXT, new Vector2(0, -160), new Vector2(300, 64));

        Btn(p, "Btn_BackToHub", "\u2190 Back to Hub", new Color(1,1,1,0.12f), WHITE, new Vector2(0,-460), new Vector2(340,56));
        return p.gameObject;
    }

    // ── PANEL 1 — Party Setup ────────────────────────────────────────────────
    static GameObject BuildPartySetupPanel(RectTransform root)
    {
        var p = Panel(root, "PartySetup");

        var t = Tmp(p, "Title", "Who's Playing?", 62, WHITE, FontStyles.Bold);
        SizePos(t, new Vector2(900, 90), new Vector2(0, 420));
        var s = Tmp(p, "Sub", "Enter player names below (min 2)", 24, FADED);
        SizePos(s, new Vector2(700, 40), new Vector2(0, 348));

        // Name list container (rows added at runtime by TriviaGameUI)
        var list = E(p, "NameList");
        SizePos(list, new Vector2(640, 320), new Vector2(0, 100));
        var vlg = list.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 14;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth  = true;  vlg.childForceExpandWidth  = true;
        vlg.childControlHeight = false; vlg.childForceExpandHeight = false;

        // Two placeholder name rows so layout is visible in editor
        NameRow(list, "Player 1...");
        NameRow(list, "Player 2...");

        Btn(p, "Btn_AddPlayer", "+ Add Player", new Color(1,1,1,0.16f), WHITE, new Vector2(0, -90),  new Vector2(640, 60));
        Btn(p, "Btn_Continue",  "Continue \u2192",  YELLOW,             DARK_TEXT, new Vector2(0,-370), new Vector2(640, 68));
        Btn(p, "Btn_BackMode",  "\u2190 Back",  new Color(1,1,1,0.10f), FADED,     new Vector2(0,-450), new Vector2(240, 52));
        return p.gameObject;
    }

    static void NameRow(RectTransform parent, string placeholder)
    {
        var row = Box(parent, "NameRow", new Vector2(640, 66), Vector2.zero, INPUT_COL);
        var t   = Tmp(row, "Placeholder", placeholder, 22, new Color(1,1,1,0.35f));
        t.enableWordWrapping = false;
        Stretch(t.GetComponent<RectTransform>(), new Vector2(20, 4), new Vector2(-20, -4));
    }

    // ── PANEL 2 — Party Count ────────────────────────────────────────────────
    static GameObject BuildPartyCountPanel(RectTransform root)
    {
        var p = Panel(root, "PartyCount");

        var t = Tmp(p, "Title", "How many\nquestions?", 62, WHITE, FontStyles.Bold);
        t.enableWordWrapping = true;
        SizePos(t, new Vector2(700, 160), new Vector2(0, 380));

        // Pill showing count
        var pill = Box(p, "Pill", new Vector2(640, 90), new Vector2(0, 220), new Color(1,1,1,0.95f));
        var countLbl = Tmp(pill, "CountLbl", "10", 40, DARK_TEXT, FontStyles.Bold);
        countLbl.alignment = TextAlignmentOptions.Center;
        var clRT = countLbl.GetComponent<RectTransform>();
        clRT.anchorMin = clRT.anchorMax = new Vector2(0.5f, 0.5f);
        clRT.sizeDelta = new Vector2(200, 70);
        clRT.anchoredPosition = Vector2.zero;

        // Slider
        var slGO = new GameObject("Slider");
        slGO.transform.SetParent(p, false);
        var slRT = slGO.AddComponent<RectTransform>();
        slRT.sizeDelta = new Vector2(560, 60);
        slRT.anchoredPosition = new Vector2(0, 120);

        var fa  = E(slGO.GetComponent<RectTransform>(), "FillArea");
        fa.anchorMin = new Vector2(0, .35f); fa.anchorMax = new Vector2(1, .65f);
        fa.offsetMin = fa.offsetMax = Vector2.zero;
        var fill = new GameObject("Fill"); fill.transform.SetParent(fa, false);
        fill.AddComponent<Image>().color = new Color(0,0,0,0);
        var fRT = fill.GetComponent<RectTransform>();
        fRT.anchorMin = Vector2.zero; fRT.anchorMax = Vector2.one;
        fRT.offsetMin = fRT.offsetMax = Vector2.zero;

        var ha = E(slRT, "HA");
        ha.anchorMin = Vector2.zero; ha.anchorMax = Vector2.one;
        ha.offsetMin = new Vector2(10,0); ha.offsetMax = new Vector2(-10,0);
        var h  = new GameObject("Handle"); h.transform.SetParent(ha, false);
        var hImg = h.AddComponent<Image>(); hImg.color = YELLOW; hImg.sprite = sRound; hImg.type = Image.Type.Sliced;
        var hRT = h.GetComponent<RectTransform>(); hRT.sizeDelta = new Vector2(62, 62);

        var slider = slGO.AddComponent<Slider>();
        slider.fillRect = fRT; slider.handleRect = hRT;
        slider.minValue = 5; slider.maxValue = 20; slider.value = 10;
        slider.wholeNumbers = true;

        var rangeTxt = Tmp(p, "Range", "5                                                    20", 16, FADED);
        SizePos(rangeTxt, new Vector2(560, 28), new Vector2(0, 62));

        Btn(p, "Btn_LetsGo",   "Let's Go! \U0001F389", YELLOW,                DARK_TEXT, new Vector2(0,-180), new Vector2(560,70));
        Btn(p, "Btn_BackSetup","\u2190 Back",           new Color(1,1,1,0.10f),FADED,    new Vector2(0,-260), new Vector2(240,52));
        return p.gameObject;
    }

    // ── PANEL 3 — Party Game ─────────────────────────────────────────────────
    static GameObject BuildPartyGamePanel(RectTransform root)
    {
        var p = Panel(root, "PartyGame");

        var qNum = Tmp(p, "QNumTxt", "Question 1 of 10", 20, FADED);
        SizePos(qNum, new Vector2(700, 36), new Vector2(0, 440));

        var card = Box(p, "QCard", new Vector2(900, 320), new Vector2(0, 200), CARD_COL);
        var qTxt = Tmp(card, "QuestionTxt", "Question text will appear here", 36, WHITE, FontStyles.Bold);
        qTxt.alignment = TextAlignmentOptions.Center;
        qTxt.enableWordWrapping = true;
        Stretch(qTxt.GetComponent<RectTransform>(), new Vector2(48, 20), new Vector2(-48, -20));

        Btn(p, "Btn_Reveal",  "Reveal Answer \u25B6", YELLOW,                DARK_TEXT, new Vector2(0,-220), new Vector2(480,68));
        Btn(p, "Btn_QuitGame","\u2190 Quit",           new Color(1,1,1,0.10f),FADED,    new Vector2(0,-450), new Vector2(240,52));
        return p.gameObject;
    }

    // ── PANEL 4 — Party Reveal ───────────────────────────────────────────────
    static GameObject BuildPartyRevealPanel(RectTransform root)
    {
        var p = Panel(root, "PartyReveal");

        var rq = Tmp(p, "RevealQTxt", "Question text", 20, FADED);
        rq.enableWordWrapping = true;
        SizePos(rq, new Vector2(920, 44), new Vector2(0, 458));

        var ra = Tmp(p, "RevealAnsTxt", "\u2705  Correct Answer", 36, YELLOW, FontStyles.Bold);
        SizePos(ra, new Vector2(920, 52), new Vector2(0, 400));

        var inst = Tmp(p, "Instruction", "Tap the player(s) who got it right \u2193", 22, FADED);
        SizePos(inst, new Vector2(700, 36), new Vector2(0, 342));

        // Player buttons container (populated at runtime)
        var btns = E(p, "PlayerBtns");
        SizePos(btns, new Vector2(760, 300), new Vector2(0, 110));
        var glg = btns.gameObject.AddComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(360, 70); glg.spacing = new Vector2(16, 14);
        glg.childAlignment = TextAnchor.UpperCenter;

        // Placeholder player button
        var pb = Box(btns, "PlayerBtn", new Vector2(360, 70), Vector2.zero, new Color(0.22f,0.10f,0.44f,0.95f));
        pb.gameObject.AddComponent<Button>().targetGraphic = pb.GetComponent<Image>();
        var pt = Tmp(pb, "PlayerName", "Player 1 — 0 pts", 24, WHITE);
        pt.alignment = TextAlignmentOptions.Center;
        Stretch(pt.GetComponent<RectTransform>(), new Vector2(12,0), new Vector2(-12,0));

        var board = Tmp(p, "BoardTxt", "Player 1: 0 pts   |   Player 2: 0 pts", 18, FADED);
        board.enableWordWrapping = false;
        SizePos(board, new Vector2(940, 32), new Vector2(0, -112));

        Btn(p, "Btn_NextQ",     "Next Question \u2192", YELLOW,                DARK_TEXT, new Vector2(0,-350), new Vector2(480,68));
        Btn(p, "Btn_QuitReveal","\u2190 Quit",           new Color(1,1,1,0.10f),FADED,    new Vector2(0,-450), new Vector2(240,52));
        return p.gameObject;
    }

    // ── PANEL 5 — Solo Game ──────────────────────────────────────────────────
    static GameObject BuildSoloGamePanel(RectTransform root)
    {
        var p = Panel(root, "SoloGame");

        var qNum = Tmp(p, "SoloQNumTxt", "Question 1 of 15", 20, FADED);
        SizePos(qNum, new Vector2(600, 36), new Vector2(-200, 470));

        var coins = Tmp(p, "SoloCoinsTxt", "\u2B50 0 coins", 22, YELLOW);
        SizePos(coins, new Vector2(280, 36), new Vector2(400, 470));

        var card = Box(p, "QCard", new Vector2(1000, 200), new Vector2(0, 290), CARD_COL);
        var qTxt = Tmp(card, "SoloQTxt", "Question text will appear here", 34, WHITE, FontStyles.Bold);
        qTxt.alignment = TextAlignmentOptions.Center;
        qTxt.enableWordWrapping = true;
        Stretch(qTxt.GetComponent<RectTransform>(), new Vector2(36, 12), new Vector2(-36, -12));

        // 2×2 answer grid
        var grid = E(p, "AnswerGrid");
        SizePos(grid, new Vector2(1040, 260), new Vector2(0, -120));
        var glg = grid.gameObject.AddComponent<GridLayoutGroup>();
        glg.cellSize        = new Vector2(500, 112);
        glg.spacing         = new Vector2(20, 20);
        glg.childAlignment  = TextAnchor.MiddleCenter;
        glg.constraint      = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = 2;

        string[] labels = { "Answer A", "Answer B", "Answer C", "Answer D" };
        for (int i = 0; i < 4; i++)
        {
            var ans = Box(grid, "Ans" + i, new Vector2(500, 112), Vector2.zero, ANS_NORMAL);
            var border = new GameObject("Border", typeof(RectTransform), typeof(Image));
            border.transform.SetParent(ans, false);
            var bImg = border.GetComponent<Image>();
            bImg.color = ANS_BORDER; bImg.sprite = sRound; bImg.type = Image.Type.Sliced;
            bImg.raycastTarget = false;
            Stretch(border.GetComponent<RectTransform>());

            var lbl = Tmp(ans, "AnsLbl", labels[i], 28, WHITE);
            lbl.alignment = TextAlignmentOptions.Center;
            lbl.enableWordWrapping = true;
            Stretch(lbl.GetComponent<RectTransform>(), new Vector2(18, 8), new Vector2(-18, -8));

            var btn = ans.gameObject.AddComponent<Button>();
            btn.targetGraphic = ans.GetComponent<Image>();
        }

        Btn(p, "Btn_QuitSolo", "\u2190 Quit", new Color(1,1,1,0.10f), FADED, new Vector2(0,-460), new Vector2(240,52));
        return p.gameObject;
    }

    // ── PANEL 6 — Results ────────────────────────────────────────────────────
    static GameObject BuildResultsPanel(RectTransform root)
    {
        var p = Panel(root, "Results");

        var title = Tmp(p, "ResultsTitle", "\U0001F3C6 Results!", 66, YELLOW, FontStyles.Bold);
        SizePos(title, new Vector2(700, 88), new Vector2(0, 440));

        var sub = Tmp(p, "ResultsSub", "Player wins!", 30, WHITE, FontStyles.Bold);
        SizePos(sub, new Vector2(900, 60), new Vector2(0, 370));

        var coins = Tmp(p, "ResultsCoins", "\u2B50 +0 StarCoins earned!", 28, YELLOW);
        SizePos(coins, new Vector2(700, 44), new Vector2(0, 300));

        // List for results rows (populated at runtime)
        var list = E(p, "ResultsList");
        SizePos(list, new Vector2(700, 380), new Vector2(0, 10));
        var vlg = list.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 12; vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth  = true;  vlg.childForceExpandWidth  = true;
        vlg.childControlHeight = false; vlg.childForceExpandHeight = false;

        Btn(p, "Btn_PlayAgain",  "Play Again",       YELLOW,                DARK_TEXT, new Vector2(0,-340), new Vector2(400,68));
        Btn(p, "Btn_BackToHub2", "\u2190 Back to Hub",new Color(1,1,1,0.12f),WHITE,    new Vector2(0,-420), new Vector2(400,56));
        return p.gameObject;
    }

    // ── TriviaGameUI wiring ──────────────────────────────────────────────────

    /// Finds or creates the TriviaManager host, adds TriviaGameUI, wires all
    /// SerializedObject properties, and wires every button onClick persistently
    /// via UnityEventTools (same pattern as ShopUIBuilder).
    static void WireTriviaGameUI(GameObject canvasGO)
    {
        // ── Host object ────────────────────────────────────────────────────────
        var host = GameObject.Find("TriviaManager");
        if (host == null) host = new GameObject("TriviaManager");

        var tgui = host.GetComponent<TriviaGameUI>();
        if (tgui == null) tgui = host.AddComponent<TriviaGameUI>();

        var root = canvasGO.transform;
        var so   = new SerializedObject(tgui);

        // ── Font ──────────────────────────────────────────────────────────────
        so.FindProperty("font").objectReferenceValue = font;

        // ── Panels ────────────────────────────────────────────────────────────
        so.FindProperty("panelMode")       .objectReferenceValue = root.Find("ModePanel")?.gameObject;
        so.FindProperty("panelPartySetup") .objectReferenceValue = root.Find("PartySetup")?.gameObject;
        so.FindProperty("panelPartyCount") .objectReferenceValue = root.Find("PartyCount")?.gameObject;
        so.FindProperty("panelPartyGame")  .objectReferenceValue = root.Find("PartyGame")?.gameObject;
        so.FindProperty("panelPartyReveal").objectReferenceValue = root.Find("PartyReveal")?.gameObject;
        so.FindProperty("panelSoloGame")   .objectReferenceValue = root.Find("SoloGame")?.gameObject;
        so.FindProperty("panelResults")    .objectReferenceValue = root.Find("Results")?.gameObject;

        // ── Party Setup ───────────────────────────────────────────────────────
        so.FindProperty("nameListRT").objectReferenceValue =
            root.Find("PartySetup/NameList");

        // ── Party Count ───────────────────────────────────────────────────────
        so.FindProperty("countSlider").objectReferenceValue =
            root.Find("PartyCount/Slider")?.GetComponent<Slider>();
        so.FindProperty("countLabel").objectReferenceValue =
            root.Find("PartyCount/Pill/CountLbl")?.GetComponent<TMP_Text>();

        // ── Party Game ────────────────────────────────────────────────────────
        so.FindProperty("partyQNumTxt").objectReferenceValue =
            root.Find("PartyGame/QNumTxt")?.GetComponent<TMP_Text>();
        so.FindProperty("partyQuestionTxt").objectReferenceValue =
            root.Find("PartyGame/QCard/QuestionTxt")?.GetComponent<TMP_Text>();

        // ── Party Reveal ──────────────────────────────────────────────────────
        so.FindProperty("revealQTxt").objectReferenceValue =
            root.Find("PartyReveal/RevealQTxt")?.GetComponent<TMP_Text>();
        so.FindProperty("revealAnsTxt").objectReferenceValue =
            root.Find("PartyReveal/RevealAnsTxt")?.GetComponent<TMP_Text>();
        so.FindProperty("checkListRT").objectReferenceValue =
            root.Find("PartyReveal/PlayerBtns");
        so.FindProperty("boardTxt").objectReferenceValue =
            root.Find("PartyReveal/BoardTxt")?.GetComponent<TMP_Text>();

        // ── Solo Game ─────────────────────────────────────────────────────────
        so.FindProperty("soloQNumTxt").objectReferenceValue =
            root.Find("SoloGame/SoloQNumTxt")?.GetComponent<TMP_Text>();
        so.FindProperty("soloStreakTxt").objectReferenceValue =
            root.Find("SoloGame/SoloCoinsTxt")?.GetComponent<TMP_Text>();
        so.FindProperty("soloQTxt").objectReferenceValue =
            root.Find("SoloGame/QCard/SoloQTxt")?.GetComponent<TMP_Text>();

        var ansArr = so.FindProperty("answerBtns");
        ansArr.arraySize = 4;
        for (int i = 0; i < 4; i++)
            ansArr.GetArrayElementAtIndex(i).objectReferenceValue =
                root.Find("SoloGame/AnswerGrid/Ans" + i)?.gameObject;

        // ── Results ───────────────────────────────────────────────────────────
        so.FindProperty("resultsListGO").objectReferenceValue =
            root.Find("Results/ResultsList")?.gameObject;
        so.FindProperty("resultsTitleTxt").objectReferenceValue =
            root.Find("Results/ResultsTitle")?.GetComponent<TMP_Text>();
        so.FindProperty("resultsSubTxt").objectReferenceValue =
            root.Find("Results/ResultsSub")?.GetComponent<TMP_Text>();
        so.FindProperty("resultsCoinsTxt").objectReferenceValue =
            root.Find("Results/ResultsCoins")?.GetComponent<TMP_Text>();

        so.ApplyModifiedProperties();

        // ── Wire buttons persistently via UnityEventTools ────────────────────
        // Mode Panel
        WireBtn(root, "ModePanel/PartyCard/Btn_PartyPlay", tgui.OnPartyPlayPressed);
        WireBtn(root, "ModePanel/SoloCard/Btn_SoloPlay",   tgui.OnSoloPlayPressed);
        WireBtn(root, "ModePanel/Btn_BackToHub",            tgui.BackToHub);

        // Party Setup
        WireBtn(root, "PartySetup/Btn_AddPlayer", tgui.OnAddPlayerPressed);
        WireBtn(root, "PartySetup/Btn_Continue",  tgui.OnPartyContinue);
        WireBtn(root, "PartySetup/Btn_BackMode",  tgui.ShowModePanel);

        // Party Count
        WireBtn(root, "PartyCount/Btn_LetsGo",    tgui.OnLetsGoPressed);
        WireBtn(root, "PartyCount/Btn_BackSetup", tgui.ShowPartySetupPanel);

        // Party Game
        WireBtn(root, "PartyGame/Btn_Reveal",   tgui.OnRevealPressed);
        WireBtn(root, "PartyGame/Btn_QuitGame", tgui.ShowModePanel);

        // Party Reveal
        WireBtn(root, "PartyReveal/Btn_NextQ",      tgui.OnNextQPressed);
        WireBtn(root, "PartyReveal/Btn_QuitReveal", tgui.ShowModePanel);

        // Solo Game
        WireBtn(root, "SoloGame/Btn_QuitSolo", tgui.ShowModePanel);

        // Results
        WireBtn(root, "Results/Btn_PlayAgain",   tgui.OnPlayAgainPressed);
        WireBtn(root, "Results/Btn_BackToHub2",  tgui.BackToHub);

        EditorUtility.SetDirty(host);
        Debug.Log("[TriviaUIBuilder] TriviaGameUI wired on " + host.name +
                  ". All panel refs and buttons persistently bound.");
    }

    /// Finds a Button at the given path under root and adds a persistent listener.
    static void WireBtn(Transform root, string path,
                        UnityEngine.Events.UnityAction action)
    {
        var t = root.Find(path);
        if (t == null) { Debug.LogWarning("[TriviaUIBuilder] Path not found: " + path); return; }
        var btn = t.GetComponent<Button>();
        if (btn == null) { Debug.LogWarning("[TriviaUIBuilder] No Button on: " + path); return; }
        // Clear any leftover listeners from a previous build run
        UnityEventTools.RemovePersistentListener(btn.onClick, action);
        UnityEventTools.AddPersistentListener(btn.onClick, action);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    /// Full-stretch child panel
    static RectTransform Panel(RectTransform parent, string name)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return rt;
    }

    /// Empty container
    static RectTransform E(RectTransform parent, string name)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false);
        return go.AddComponent<RectTransform>();
    }

    /// Sized/positioned box with sliced sRound sprite
    static RectTransform Box(RectTransform parent, string name, Vector2 size, Vector2 pos, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = size; rt.anchoredPosition = pos;
        var img = go.GetComponent<Image>();
        img.color = color; img.sprite = sRound; img.type = Image.Type.Sliced;
        return rt;
    }

    /// TMP label
    static TextMeshProUGUI Tmp(RectTransform parent, string name, string text,
                                float size, Color color, FontStyles style = FontStyles.Normal)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<TextMeshProUGUI>();
        t.text = text; t.fontSize = size; t.color = color; t.fontStyle = style;
        t.alignment = TextAlignmentOptions.Center;
        t.enableWordWrapping = false;
        t.raycastTarget = false;
        if (font != null) t.font = font;
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(960, size * 2 + 20);
        return t;
    }

    /// Visual-only button (callbacks wired at runtime by TriviaGameUI)
    static void Btn(RectTransform parent, string name, string label,
                    Color bg, Color textCol, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = size; rt.anchoredPosition = pos;
        var img = go.GetComponent<Image>();
        img.color = bg; img.sprite = sRound; img.type = Image.Type.Sliced;
        var btn = go.GetComponent<Button>();
        var bc = btn.colors;
        bc.highlightedColor = new Color(bg.r * .88f, bg.g * .88f, bg.b * .88f, 1f);
        bc.pressedColor     = new Color(bg.r * .70f, bg.g * .70f, bg.b * .70f, 1f);
        btn.colors = bc; btn.targetGraphic = img;

        var lbl = new GameObject("Lbl", typeof(RectTransform), typeof(TextMeshProUGUI));
        lbl.transform.SetParent(go.transform, false);
        var lt = lbl.GetComponent<TextMeshProUGUI>();
        lt.text = label; lt.fontSize = 26; lt.color = textCol;
        lt.fontStyle = FontStyles.Bold;
        lt.alignment = TextAlignmentOptions.Center;
        lt.raycastTarget = false;
        if (font != null) lt.font = font;
        var lrt = lbl.GetComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
        lrt.offsetMin = lrt.offsetMax = Vector2.zero;
    }

    /// Image (background fills etc.)
    static RectTransform Img(RectTransform parent, string name, Color color, bool raycast = true)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.color = color; img.raycastTarget = raycast;
        return go.GetComponent<RectTransform>();
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static void Stretch(RectTransform rt, Vector2 offsetMin, Vector2 offsetMax)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = offsetMin; rt.offsetMax = offsetMax;
    }

    static void SizePos(TextMeshProUGUI t, Vector2 size, Vector2 pos)
    {
        var rt = t.GetComponent<RectTransform>();
        rt.sizeDelta = size; rt.anchoredPosition = pos;
    }

    static void SizePos(RectTransform rt, Vector2 size, Vector2 pos)
    {
        rt.sizeDelta = size; rt.anchoredPosition = pos;
    }
}
