using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <summary>
/// Trivia game runtime logic.
/// Canvas is built by the editor script: PopstarHub > Build Trivia UI
/// This script finds the pre-built elements by name and wires all game logic.
/// No building or destroying happens at runtime.
/// </summary>
public class TriviaGameUI : MonoBehaviour
{
    // ── Palette (used for dynamically created elements at runtime) ────────────
    static readonly Color CARD_COL   = new Color(0.085f, 0.040f, 0.160f, 0.90f);
    static readonly Color YELLOW     = new Color(1f,    0.87f, 0.12f, 1f);
    static readonly Color DARK_TEXT  = new Color(0.14f, 0.05f, 0.28f, 1f);
    static readonly Color FADED      = new Color(1f, 1f, 1f, 0.55f);
    static readonly Color WHITE      = Color.white;
    static readonly Color GREEN_BG   = new Color(0.10f, 0.70f, 0.30f, 0.90f);
    static readonly Color RED_BG     = new Color(0.80f, 0.10f, 0.10f, 0.90f);
    static readonly Color ANS_NORMAL = new Color(0.16f, 0.07f, 0.32f, 0.95f);
    static readonly Color INPUT_COL  = new Color(1f, 1f, 1f, 0.10f);
    static readonly Color PLAYER_BTN = new Color(0.22f, 0.10f, 0.44f, 0.95f);

    [Header("Optional: drag a TMP Font Asset here")]
    public TMP_FontAsset font;

    // ── Sprites (generated at runtime for dynamic elements) ───────────────────
    Sprite sRound, sMed, sSmall, sCircle;

    // ── Audio ─────────────────────────────────────────────────────────────────
    AudioSource audio;
    AudioClip   sfxCorrect, sfxWrong, sfxVictory, sfxDrumroll;

    // ── Panels ────────────────────────────────────────────────────────────────
    [Header("Panels")]
    [SerializeField] GameObject panelMode;
    [SerializeField] GameObject panelPartySetup;
    [SerializeField] GameObject panelPartyCount;
    [SerializeField] GameObject panelPartyGame;
    [SerializeField] GameObject panelPartyReveal;
    [SerializeField] GameObject panelSoloGame;
    [SerializeField] GameObject panelResults;

    // ── Party refs ────────────────────────────────────────────────────────────
    [Header("Party Setup")]
    [SerializeField] Transform  nameListRT;

    [Header("Party Count")]
    [SerializeField] Slider     countSlider;
    [SerializeField] TMP_Text   countLabel;

    [Header("Party Game")]
    [SerializeField] TMP_Text   partyQNumTxt;
    [SerializeField] TMP_Text   partyQuestionTxt;

    [Header("Party Reveal")]
    [SerializeField] TMP_Text   revealQTxt;
    [SerializeField] TMP_Text   revealAnsTxt;
    [SerializeField] Transform  checkListRT;
    [SerializeField] TMP_Text   boardTxt;

    // ── Solo refs ─────────────────────────────────────────────────────────────
    [Header("Solo Game")]
    [SerializeField] TMP_Text     soloQNumTxt;
    [SerializeField] TMP_Text     soloStreakTxt;
    [SerializeField] TMP_Text     soloQTxt;
    [SerializeField] GameObject[] answerBtns = new GameObject[4];
    TMP_Text[]   answerTxts = new TMP_Text[4];
    Image[]      answerImgs = new Image[4];

    // ── Results refs ──────────────────────────────────────────────────────────
    [Header("Results")]
    [SerializeField] GameObject resultsListGO;
    [SerializeField] TMP_Text   resultsTitleTxt;
    [SerializeField] TMP_Text   resultsSubTxt;
    [SerializeField] TMP_Text   resultsCoinsTxt;

    // ── Game state ────────────────────────────────────────────────────────────
    const int SOLO_Q_COUNT = 15;
    const int COINS_PER_Q  = 10;

    List<string>         playerNames  = new List<string>();
    List<int>            playerScores = new List<int>();
    List<TMP_InputField> nameInputs   = new List<TMP_InputField>();

    int  partyQCount    = 10;
    int  soloCorrect;
    int  soloCoinsEarned;
    bool soloAnswered;
    bool isPartyMode;
    int  _soloCorrectIdx;

    List<TriviaQuestion> allQ;
    List<TriviaQuestion> gameQ;
    int                  currentQ;

    // ════════════════════════════════════════════════════════════════════════
    void Start()
    {
        audio       = gameObject.AddComponent<AudioSource>();
        sfxCorrect  = MakeCorrectSound();
        sfxWrong    = MakeWrongSound();
        sfxVictory  = MakeVictorySound();
        sfxDrumroll = MakeDrumroll();
        allQ        = TriviaQuestionBank.GetAllQuestions();
        BuildSprites();

        // ── Validate pre-wired refs (set by TriviaUIBuilder) ───────────────
        if (panelMode == null)
        {
            Debug.LogError("[TriviaGameUI] Panels not wired. Run PopstarHub > Build Trivia UI first.");
            return;
        }

        // ── Derive answer text/image refs from pre-wired answer buttons ───────
        for (int i = 0; i < 4; i++)
        {
            if (answerBtns[i] == null) continue;
            answerTxts[i] = answerBtns[i].transform.Find("AnsLbl")?.GetComponent<TMP_Text>();
            answerImgs[i] = answerBtns[i].GetComponent<Image>();
        }

        // ── Wire answer buttons (closure indices require runtime binding) ─────
        for (int i = 0; i < 4; i++)
        { int idx = i; answerBtns[i]?.GetComponent<Button>().onClick.AddListener(() => OnSoloAnswer(idx)); }

        // ── Wire slider (UnityAction<float> — kept at runtime) ────────────────
        if (countSlider != null)
            countSlider.onValueChanged.AddListener(v => {
                partyQCount = Mathf.RoundToInt(v);
                if (countLabel != null) countLabel.text = partyQCount.ToString();
            });

        // ── Clear editor placeholder rows ─────────────────────────────────────
        if (nameListRT  != null) foreach (Transform child in nameListRT)  Destroy(child.gameObject);
        if (checkListRT != null) foreach (Transform child in checkListRT) Destroy(child.gameObject);

        Show(panelMode);
    }

    // ── Public button targets ─────────────────────────────────────────────────
    // Navigation buttons are wired persistently by TriviaUIBuilder via
    // UnityEventTools.AddPersistentListener — these must be public.
    public void OnPartyPlayPressed()    { isPartyMode = true; ResetParty(); Show(panelPartySetup); }
    public void OnSoloPlayPressed()     { isPartyMode = false; StartSolo(); }
    public void BackToHub()             { SceneManager.LoadScene("MainMenuScene"); }
    public void OnAddPlayerPressed()    { AddNameInput(); }
    public void OnPartyContinue()       { PartySetupDone(); }
    public void ShowModePanel()         { Show(panelMode); }
    public void ShowPartySetupPanel()   { Show(panelPartySetup); }
    public void OnLetsGoPressed()       { StartParty(); }
    public void OnRevealPressed()       { RevealParty(); }
    public void OnNextQPressed()        { NextPartyQ(); }
    public void OnPlayAgainPressed()    { PlayAgain(); }

    // ════════════════════════════════════════════════════════════
    // SPRITES — needed for player buttons, result rows etc.
    // ════════════════════════════════════════════════════════════
    void BuildSprites()
    {
        sRound  = MakeRounded(256, 256, 50);
        sMed    = MakeRounded(128, 128, 30);
        sSmall  = MakeRounded(64,  64,  20);
        sCircle = MakeRounded(64,  64,  32);
    }

    Sprite MakeRounded(int w, int h, int r)
    {
        var t  = new Texture2D(w, h);
        t.filterMode = FilterMode.Bilinear;
        var px = new Color[w * h];
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            float dx = 0, dy = 0;
            if (x < r)          dx = r - x;
            else if (x > w-r-1) dx = x - (w-r-1);
            if (y < r)          dy = r - y;
            else if (y > h-r-1) dy = y - (h-r-1);
            float a = 1f - Mathf.Clamp01((Mathf.Sqrt(dx*dx+dy*dy) - r + 1.5f) / 1.5f);
            px[y*w+x] = new Color(1,1,1,a);
        }
        t.SetPixels(px); t.Apply();
        var b = new Vector4(r+4, r+4, r+4, r+4);
        return Sprite.Create(t, new Rect(0,0,w,h), new Vector2(.5f,.5f), 100, 0,
                             SpriteMeshType.FullRect, b);
    }

    // ════════════════════════════════════════════════════════════
    // PARTY LOGIC
    // ════════════════════════════════════════════════════════════
    void ResetParty()
    {
        foreach (Transform child in nameListRT) Destroy(child.gameObject);
        nameInputs.Clear();
        AddNameInput(); AddNameInput();
    }

    void AddNameInput()
    {
        int n = nameInputs.Count + 1;
        var row = MakeBox("Row"+n, nameListRT, new Vector2(640, 66), Vector2.zero, INPUT_COL, sMed);
        var border = MakeImg("Border", row.transform, new Color(1,1,1,0.28f), sMed, stretch:true);
        border.raycastTarget = false;

        var field = row.AddComponent<TMP_InputField>();
        var area  = new GameObject("Area"); area.transform.SetParent(row.transform, false);
        var aRT   = area.AddComponent<RectTransform>();
        aRT.anchorMin = Vector2.zero; aRT.anchorMax = Vector2.one;
        aRT.offsetMin = new Vector2(20, 4); aRT.offsetMax = new Vector2(-20, -4);
        area.AddComponent<RectMask2D>();

        var ph = ChildTMP(area.transform, "Player "+n+"...", 22, new Color(1,1,1,0.35f));
        var tx = ChildTMP(area.transform, "", 22, WHITE);
        field.textViewport = aRT; field.textComponent = tx; field.placeholder = ph;
        if (font) { tx.font = font; ph.font = font; }
        nameInputs.Add(field);
    }

    void PartySetupDone()
    {
        playerNames.Clear(); playerScores.Clear();
        foreach (var f in nameInputs)
            if (f && f.text.Trim() != "") { playerNames.Add(f.text.Trim()); playerScores.Add(0); }
        if (playerNames.Count < 2) { Debug.Log("[Trivia] Need at least 2 players."); return; }
        countLabel.text = partyQCount.ToString();
        Show(panelPartyCount);
    }

    void StartParty()
    {
        partyQCount = Mathf.RoundToInt(countSlider.value);
        gameQ = Shuffled(allQ).GetRange(0, Mathf.Min(partyQCount, allQ.Count));
        currentQ = 0;
        for (int i = 0; i < playerScores.Count; i++) playerScores[i] = 0;
        ShowPartyQ();
    }

    void ShowPartyQ()
    {
        if (currentQ >= gameQ.Count) { ShowPartyResults(); return; }
        var q = gameQ[currentQ];
        partyQuestionTxt.text = q.question;
        partyQNumTxt.text = $"Question {currentQ+1} of {gameQ.Count}";
        Show(panelPartyGame);
    }

    void RevealParty()
    {
        var q = gameQ[currentQ];
        revealQTxt.text   = q.question;
        revealAnsTxt.text = "✅  " + q.answers[q.correctIndex];

        foreach (Transform c in checkListRT) Destroy(c.gameObject);

        for (int i = 0; i < playerNames.Count; i++)
        {
            int pi = i;
            var row = MakeBox("P"+i, checkListRT, new Vector2(360, 70), Vector2.zero, PLAYER_BTN, sMed);
            var lbl = ChildTMP(row.transform,
                               playerNames[i] + "  —  " + playerScores[i] + " pts",
                               24, WHITE, FontStyles.Normal);
            lbl.alignment = TextAlignmentOptions.Center;
            var lrt = lbl.GetComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
            lrt.offsetMin = new Vector2(12,0); lrt.offsetMax = new Vector2(-12,0);

            var btn = row.AddComponent<Button>();
            btn.targetGraphic = row.GetComponent<Image>();
            var bc = btn.colors;
            bc.highlightedColor = new Color(0.3f, 0.8f, 0.4f, 0.95f);
            bc.pressedColor     = new Color(0.15f, 0.70f, 0.3f, 1f);
            btn.colors = bc;
            btn.onClick.AddListener(() => {
                playerScores[pi]++;
                audio.PlayOneShot(sfxCorrect);
                lbl.text = playerNames[pi] + "  —  " + playerScores[pi] + " pts ✅";
                RefreshBoard();
            });
        }
        RefreshBoard();
        Show(panelPartyReveal);
    }

    void RefreshBoard()
    {
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < playerNames.Count; i++)
        {
            sb.Append(playerNames[i]).Append(": ").Append(playerScores[i]).Append(" pts");
            if (i < playerNames.Count-1) sb.Append("   |   ");
        }
        boardTxt.text = sb.ToString();
    }

    void NextPartyQ() { currentQ++; ShowPartyQ(); }

    void ShowPartyResults()
    {
        foreach (Transform c in resultsListGO.transform) Destroy(c.gameObject);

        int best = 0; string winner = "";
        for (int i = 0; i < playerNames.Count; i++)
            if (playerScores[i] > best) { best = playerScores[i]; winner = playerNames[i]; }

        var sorted = new List<int>();
        for (int i = 0; i < playerNames.Count; i++) sorted.Add(i);
        sorted.Sort((a,b) => playerScores[b].CompareTo(playerScores[a]));

        resultsTitleTxt.text = "🏆 Game Over!";
        resultsSubTxt.text   = winner + " wins with " + best + " points!";
        resultsCoinsTxt.text = "";

        int rank = 1;
        foreach (int i in sorted)
        {
            bool top = rank == 1;
            var row = MakeBox("R"+rank, resultsListGO.transform, new Vector2(700, 66), Vector2.zero,
                              top ? new Color(1f,.87f,.12f,.30f) : CARD_COL, sMed);
            var t = ChildTMP(row.transform,
                             $"#{rank}   {playerNames[i]}   —   {playerScores[i]} pts",
                             top ? 28 : 22, WHITE, top ? FontStyles.Bold : FontStyles.Normal);
            t.alignment = TextAlignmentOptions.Center;
            var tRT = t.GetComponent<RectTransform>();
            tRT.anchorMin = Vector2.zero; tRT.anchorMax = Vector2.one;
            tRT.offsetMin = new Vector2(16,0); tRT.offsetMax = new Vector2(-16,0);
            rank++;
        }
        StartCoroutine(PlayResultsSFX());
        Show(panelResults);
    }

    // ════════════════════════════════════════════════════════════
    // SOLO LOGIC
    // ════════════════════════════════════════════════════════════
    void StartSolo()
    {
        gameQ           = Shuffled(allQ).GetRange(0, Mathf.Min(SOLO_Q_COUNT, allQ.Count));
        currentQ        = 0;
        soloCorrect     = 0;
        soloCoinsEarned = 0;
        ShowSoloQ();
    }

    void ShowSoloQ()
    {
        if (currentQ >= gameQ.Count) { ShowSoloResults(); return; }
        soloAnswered = false;
        var q = gameQ[currentQ];

        soloQNumTxt.text  = $"Question {currentQ+1} of {gameQ.Count}";
        soloStreakTxt.text = $"⭐ {soloCoinsEarned} coins";
        soloQTxt.text     = q.question;

        string correct = q.answers[q.correctIndex];
        var choices = new List<string> { correct };
        if (q.wrongAnswers != null)
            foreach (var w in q.wrongAnswers) choices.Add(w);
        while (choices.Count < 4) choices.Add("?");

        for (int i = choices.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var tmp = choices[i]; choices[i] = choices[j]; choices[j] = tmp;
        }
        _soloCorrectIdx = choices.IndexOf(correct);

        for (int i = 0; i < 4; i++)
        {
            answerTxts[i].text = choices[i];
            answerImgs[i].color = ANS_NORMAL;
            answerBtns[i].GetComponent<Button>().interactable = true;
        }
        Show(panelSoloGame);
    }

    void OnSoloAnswer(int chosen)
    {
        if (soloAnswered) return;
        soloAnswered = true;
        for (int i = 0; i < 4; i++)
            answerBtns[i].GetComponent<Button>().interactable = false;

        if (chosen == _soloCorrectIdx)
        {
            answerImgs[chosen].color = GREEN_BG;
            soloCorrect++;
            soloCoinsEarned += COINS_PER_Q;
            audio.PlayOneShot(sfxCorrect);
        }
        else
        {
            answerImgs[chosen].color = RED_BG;
            answerImgs[_soloCorrectIdx].color = GREEN_BG;
            audio.PlayOneShot(sfxWrong);
        }
        StartCoroutine(AdvanceSoloAfterDelay(2f));
    }

    IEnumerator AdvanceSoloAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        currentQ++;
        ShowSoloQ();
    }

    void ShowSoloResults()
    {
        if (CoinManager.Instance != null)
            CoinManager.Instance.AddCoins(soloCoinsEarned);

        foreach (Transform c in resultsListGO.transform) Destroy(c.gameObject);

        resultsTitleTxt.text = "🎯 Solo Results!";
        resultsSubTxt.text   = $"{soloCorrect} / {SOLO_Q_COUNT} correct!";
        resultsCoinsTxt.text = $"⭐ +{soloCoinsEarned} StarCoins earned!";

        var bar = MakeBox("Bar", resultsListGO.transform, new Vector2(700, 80), Vector2.zero, CARD_COL, sMed);
        var bt  = ChildTMP(bar.transform,
                           $"Correct: {soloCorrect}   |   Wrong: {SOLO_Q_COUNT - soloCorrect}",
                           26, WHITE, FontStyles.Normal);
        bt.alignment = TextAlignmentOptions.Center;
        var bRT = bt.GetComponent<RectTransform>();
        bRT.anchorMin = Vector2.zero; bRT.anchorMax = Vector2.one;
        bRT.offsetMin = new Vector2(16,0); bRT.offsetMax = new Vector2(-16,0);

        string grade =
            soloCorrect == SOLO_Q_COUNT        ? "Perfect! 🌟" :
            soloCorrect >= SOLO_Q_COUNT * 0.8f ? "Excellent! 🔥" :
            soloCorrect >= SOLO_Q_COUNT * 0.6f ? "Good job! 👏" :
            soloCorrect >= SOLO_Q_COUNT * 0.4f ? "Keep practising 📖" :
                                                 "Better luck next time 💪";

        var gbar = MakeBox("Grade", resultsListGO.transform, new Vector2(700, 70), Vector2.zero,
                           new Color(1f,.87f,.12f,.22f), sMed);
        var gt = ChildTMP(gbar.transform, grade, 28, YELLOW, FontStyles.Bold);
        gt.alignment = TextAlignmentOptions.Center;
        var gRT = gt.GetComponent<RectTransform>();
        gRT.anchorMin = Vector2.zero; gRT.anchorMax = Vector2.one;
        gRT.offsetMin = new Vector2(16,0); gRT.offsetMax = new Vector2(-16,0);

        StartCoroutine(PlayResultsSFX());
        Show(panelResults);
    }

    // ════════════════════════════════════════════════════════════
    // SHARED
    // ════════════════════════════════════════════════════════════
    void PlayAgain()
    {
        if (isPartyMode) { ResetParty(); Show(panelPartySetup); }
        else             { StartSolo(); }
    }

    void Show(GameObject target)
    {
        foreach (var p in new[]{ panelMode, panelPartySetup, panelPartyCount,
                                  panelPartyGame, panelPartyReveal, panelSoloGame, panelResults })
            if (p) p.SetActive(false);
        target.SetActive(true);
    }

    IEnumerator PlayResultsSFX()
    {
        audio.PlayOneShot(sfxDrumroll);
        yield return new WaitForSeconds(sfxDrumroll.length);
        audio.PlayOneShot(sfxVictory);
    }

    List<TriviaQuestion> Shuffled(List<TriviaQuestion> src)
    {
        var s = new List<TriviaQuestion>(src);
        for (int i = s.Count-1; i > 0; i--)
        {
            int j = Random.Range(0, i+1);
            var tmp = s[i]; s[i] = s[j]; s[j] = tmp;
        }
        return s;
    }

    // ════════════════════════════════════════════════════════════
    // UI HELPERS (used only for dynamic runtime elements)
    // ════════════════════════════════════════════════════════════
    Image MakeImg(string name, Transform parent, Color color, Sprite sprite = null, bool stretch = false)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.color = color;
        if (sprite != null) { img.sprite = sprite; img.type = Image.Type.Sliced; }
        if (stretch)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }
        return img;
    }

    GameObject MakeBox(string name, Transform parent, Vector2 size, Vector2 pos,
                       Color color, Sprite sprite)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = size; rt.anchoredPosition = pos;
        var img = go.GetComponent<Image>();
        img.color = color; img.sprite = sprite; img.type = Image.Type.Sliced;
        return go;
    }

    TMP_Text ChildTMP(Transform parent, string text, int size, Color color,
                      FontStyles style = FontStyles.Normal)
    {
        var go = new GameObject("T", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<TextMeshProUGUI>();
        t.text = text; t.fontSize = size; t.color = color; t.fontStyle = style;
        t.alignment = TextAlignmentOptions.Center; t.enableWordWrapping = false;
        if (font) t.font = font;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return t;
    }

    // ════════════════════════════════════════════════════════════
    // AUDIO GENERATORS
    // ════════════════════════════════════════════════════════════
    AudioClip MakeCorrectSound()
    {
        int rate = 44100; int len = (int)(rate * 0.55f);
        float[] d = new float[len];
        float[] notes = { 523.25f, 659.25f, 783.99f, 1046.50f };
        int n = len / notes.Length;
        for (int ni = 0; ni < notes.Length; ni++)
        {
            float f = notes[ni]; int s = ni*n; int e = Mathf.Min(s+n,len);
            for (int i=s;i<e;i++){ float t2=(float)(i-s)/n; float env=Mathf.Sin(t2*Mathf.PI);
                d[i]+=0.40f*env*Mathf.Sin(2f*Mathf.PI*f*i/rate)+0.12f*env*Mathf.Sin(4f*Mathf.PI*f*i/rate); }
        }
        var c = AudioClip.Create("OK", len, 1, rate, false); c.SetData(d,0); return c;
    }

    AudioClip MakeWrongSound()
    {
        int rate = 44100; int len = (int)(rate * 0.45f);
        float[] d = new float[len];
        float[] notes = { 311.13f, 233.08f };
        int n = len / notes.Length;
        for (int ni = 0; ni < notes.Length; ni++)
        {
            float f = notes[ni]; int s = ni*n; int e = Mathf.Min(s+n,len);
            for (int i=s;i<e;i++){ float t2=(float)(i-s)/n; float env=Mathf.Sin(t2*Mathf.PI);
                d[i]+=0.45f*env*Mathf.Sin(2f*Mathf.PI*f*i/rate); }
        }
        var c = AudioClip.Create("Wrong", len, 1, rate, false); c.SetData(d,0); return c;
    }

    AudioClip MakeDrumroll()
    {
        int rate = 44100; float dur = 2.5f; int len = (int)(rate*dur);
        float[] d = new float[len]; float beat = 0.18f; float t2 = 0f;
        while (t2 < dur)
        {
            float prog = t2/dur; int hs = Mathf.Min((int)(t2*rate), len-1);
            int hlen = (int)(rate*0.022f); float vol = 0.25f + 0.55f*prog;
            for (int i=0;i<hlen&&hs+i<len;i++)
            { float env=Mathf.Exp(-i/(hlen*0.4f)); d[hs+i]+=vol*env*(Random.value*2f-1f); }
            beat = Mathf.Lerp(0.18f, 0.036f, prog); t2 += beat;
        }
        var c = AudioClip.Create("DR", len, 1, rate, false); c.SetData(d,0); return c;
    }

    AudioClip MakeVictorySound()
    {
        int rate = 44100; float dur = 2f; int len = (int)(rate*dur);
        float[] d = new float[len];
        (float f, float s, float l)[] n = {
            (392f,.00f,.10f),(523f,.10f,.10f),(659f,.20f,.10f),(784f,.30f,.28f),
            (659f,.58f,.10f),(784f,.68f,.10f),(1047f,.78f,.55f)
        };
        foreach (var (f,st,le) in n)
        {
            int s=(int)(st*rate); int e=Mathf.Min(s+(int)(le*rate),len);
            for (int i=s;i<e;i++){ float t2=(float)(i-s)/(e-s); float env=t2<.1f?t2/.1f:t2>.7f?(1f-t2)/.3f:1f;
                d[i]+=0.32f*env*Mathf.Sin(2f*Mathf.PI*f*i/rate)+0.08f*env*Mathf.Sin(4f*Mathf.PI*f*i/rate); }
        }
        float mx=0; foreach(var v in d) if(Mathf.Abs(v)>mx) mx=Mathf.Abs(v);
        if(mx>0.01f) for(int i=0;i<d.Length;i++) d[i]/=mx*1.1f;
        var c = AudioClip.Create("Win", len, 1, rate, false); c.SetData(d,0); return c;
    }
}
