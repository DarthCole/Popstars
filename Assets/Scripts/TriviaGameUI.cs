using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class TriviaGameUI : MonoBehaviour
{
    // ── colours ──────────────────────────────────────────────
    private Color bgPurple    = new Color(0.42f, 0.10f, 0.82f, 1f);
    private Color cardPurple  = new Color(0.55f, 0.25f, 0.90f, 0.55f);
    private Color inputBorder = new Color(1f,    1f,    1f,    0.35f);
    private Color yellowBtn   = new Color(1f,    0.87f, 0.12f, 1f);
    private Color darkText    = new Color(0.14f, 0.05f, 0.28f, 1f);
    private Color whiteCard   = new Color(1f,    1f,    1f,    0.97f);
    private Color fadedWhite  = new Color(1f,    1f,    1f,    0.60f);
    private Color greenCircle = new Color(0.52f, 0.82f, 0.24f, 1f);
    private Color greenCheck  = new Color(0.18f, 0.85f, 0.38f, 1f);
    private Color pillDark    = new Color(0.22f, 0.10f, 0.40f, 0.95f);

    [Header("Font — drag Nunito-Bold SDF here")]
    public TMP_FontAsset nunitoBold;

    // ── generated audio clips ─────────────────────────────────
    private AudioClip correctSFX;
    private AudioClip drumrollSFX;
    private AudioClip victorySFX;
    private AudioSource audioSource;

    // ── sprites ───────────────────────────────────────────────
    private Sprite roundLg, roundMd, roundSm, circleSprite;

    // ── panels ────────────────────────────────────────────────
    private GameObject setupPanel, countPanel, gamePanel, revealPanel, resultsPanel;

    // ── setup ─────────────────────────────────────────────────
    private Transform playerListParent;
    private List<TMP_InputField> playerInputs = new List<TMP_InputField>();

    // ── count ─────────────────────────────────────────────────
    private Slider   questionSlider;
    private TMP_Text sliderLabel;
    private int      totalQuestions = 10;

    // ── game ──────────────────────────────────────────────────
    private TMP_Text questionTxt, questionNumTxt;

    // ── reveal ────────────────────────────────────────────────
    private TMP_Text  revealQTxt, answerTxt, scoreboardTxt;
    private Transform checkListParent;

    // ── results ───────────────────────────────────────────────
    private Transform resultsListParent;
    private TMP_Text  winnerTxt;

    // ── state ─────────────────────────────────────────────────
    private List<string>         playerNames  = new List<string>();
    private List<int>            playerScores = new List<int>();
    private List<TriviaQuestion> allQ, gameQ;
    private int currentQ = 0;

    private Canvas canvas;

    // ═══════════════════════════════════════════════════════════
    void Start()
    {
        audioSource  = gameObject.AddComponent<AudioSource>();
        correctSFX   = GenerateCorrectSound();
        drumrollSFX  = GenerateDrumroll();
        victorySFX   = GenerateVictory();

        allQ = TriviaQuestionBank.GetAllQuestions();
        BuildSprites();
        BuildCanvas();
        BuildBackground();
        BuildSetupPanel();
        BuildCountPanel();
        BuildGamePanel();
        BuildRevealPanel();
        BuildResultsPanel();
        Show(setupPanel);
    }

    // ═══════════════════════════════════════════════════════════
    // SOUND GENERATORS
    // ═══════════════════════════════════════════════════════════

    // Happy rising chime — plays when a correct player is ticked
    private AudioClip GenerateCorrectSound()
    {
        int sampleRate = 44100;
        float duration = 0.6f;
        int samples    = Mathf.RoundToInt(sampleRate * duration);
        float[] data   = new float[samples];

        float[] notes = { 523.25f, 659.25f, 783.99f, 1046.50f }; // C5 E5 G5 C6
        int noteSamples = samples / notes.Length;

        for (int n = 0; n < notes.Length; n++)
        {
            float freq = notes[n];
            int start  = n * noteSamples;
            int end    = Mathf.Min(start + noteSamples, samples);
            for (int i = start; i < end; i++)
            {
                float t       = (float)(i - start) / noteSamples;
                float env     = Mathf.Sin(t * Mathf.PI);          // bell envelope
                data[i]      += 0.4f * env * Mathf.Sin(2f * Mathf.PI * freq * i / sampleRate);
                data[i]      += 0.15f * env * Mathf.Sin(4f * Mathf.PI * freq * i / sampleRate);
            }
        }

        var clip = AudioClip.Create("Correct", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    // Snare drum roll that builds — plays before winner is shown
    private AudioClip GenerateDrumroll()
    {
        int   sampleRate = 44100;
        float duration   = 3.0f;
        int   samples    = Mathf.RoundToInt(sampleRate * duration);
        float[] data     = new float[samples];

        // start slow (every 0.2s), accelerate to every 0.04s
        float beatInterval = 0.2f;
        float time         = 0f;

        while (time < duration)
        {
            float progress = time / duration;
            // snare hit: white noise burst with fast decay
            int hitStart = Mathf.Min((int)(time * sampleRate), samples - 1);
            float hitDuration = 0.025f;
            int hitSamples    = Mathf.RoundToInt(sampleRate * hitDuration);
            float vol = 0.3f + 0.5f * progress;      // gets louder

            for (int i = 0; i < hitSamples && hitStart + i < samples; i++)
            {
                float env  = Mathf.Exp(-i / (hitSamples * 0.4f));
                data[hitStart + i] += vol * env * (Random.value * 2f - 1f);
            }

            beatInterval = Mathf.Lerp(0.2f, 0.04f, progress);
            time        += beatInterval;
        }

        var clip = AudioClip.Create("Drumroll", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    // Victory fanfare — ascending triumphant notes
    private AudioClip GenerateVictory()
    {
        int   sampleRate = 44100;
        float duration   = 2.5f;
        int   samples    = Mathf.RoundToInt(sampleRate * duration);
        float[] data     = new float[samples];

        // fanfare melody: short punchy notes
        (float freq, float start, float len)[] notes =
        {
            (392.00f, 0.00f, 0.12f),   // G4
            (523.25f, 0.12f, 0.12f),   // C5
            (659.25f, 0.24f, 0.12f),   // E5
            (783.99f, 0.36f, 0.30f),   // G5  (held)
            (659.25f, 0.66f, 0.12f),   // E5
            (783.99f, 0.78f, 0.12f),   // G5
            (1046.5f, 0.90f, 0.60f),   // C6  (long held)
        };

        foreach (var (freq, startT, len) in notes)
        {
            int s = Mathf.RoundToInt(startT * sampleRate);
            int e = Mathf.Min(s + Mathf.RoundToInt(len * sampleRate), samples);
            int n = e - s;
            for (int i = 0; i < n; i++)
            {
                float t   = (float)i / n;
                float env = t < 0.1f ? t / 0.1f :
                            t > 0.7f ? (1f - t) / 0.3f : 1f;
                float sample = 0.35f * env * Mathf.Sin(2f * Mathf.PI * freq * (s + i) / sampleRate)
                             + 0.10f * env * Mathf.Sin(4f * Mathf.PI * freq * (s + i) / sampleRate)
                             + 0.05f * env * Mathf.Sin(6f * Mathf.PI * freq * (s + i) / sampleRate);
                data[s + i] += sample;
            }
        }

        // Normalize
        float max = 0f;
        foreach (var s in data) if (Mathf.Abs(s) > max) max = Mathf.Abs(s);
        if (max > 0.01f) for (int i = 0; i < data.Length; i++) data[i] /= max * 1.1f;

        var clip = AudioClip.Create("Victory", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    // ═══════════════════════════════════════════════════════════
    // SPRITE FACTORY
    // ═══════════════════════════════════════════════════════════
    private void BuildSprites()
    {
        roundLg      = MakeRoundedSprite(256, 256, 60);
        roundMd      = MakeRoundedSprite(128, 128, 36);
        roundSm      = MakeRoundedSprite(64,  64,  22);
        circleSprite = MakeRoundedSprite(64,  64,  32);
    }

    private Sprite MakeRoundedSprite(int w, int h, int r)
    {
        Texture2D t = new Texture2D(w, h);
        t.filterMode = FilterMode.Bilinear;
        Color[] px = new Color[w * h];
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            float dx = 0, dy = 0;
            if (x < r)          dx = r - x;
            else if (x > w-r-1) dx = x - (w-r-1);
            if (y < r)          dy = r - y;
            else if (y > h-r-1) dy = y - (h-r-1);
            float a = 1f - Mathf.Clamp01((Mathf.Sqrt(dx*dx+dy*dy)-r+1.5f)/1.5f);
            px[y*w+x] = new Color(1,1,1,a);
        }
        t.SetPixels(px); t.Apply();
        Vector4 b = new Vector4(r+4, r+4, r+4, r+4);
        return Sprite.Create(t, new Rect(0,0,w,h), new Vector2(.5f,.5f), 100, 0,
                             SpriteMeshType.FullRect, b);
    }

    // ═══════════════════════════════════════════════════════════
    // CANVAS
    // ═══════════════════════════════════════════════════════════
    private void BuildCanvas()
    {
        var go = new GameObject("TriviaCanvas");
        canvas = go.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var cs = go.AddComponent<CanvasScaler>();
        cs.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        cs.matchWidthOrHeight  = 0.5f;
        go.AddComponent<GraphicRaycaster>();
        if (!FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>())
        {
            var es = new GameObject("ES");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }
    }

    // ═══════════════════════════════════════════════════════════
    // BACKGROUND
    // ═══════════════════════════════════════════════════════════
    private void BuildBackground()
    {
        var bg = Img("BG", canvas.transform, Vector2.zero, Vector2.zero, bgPurple, null, true);
        Blob(bg.transform, new Vector2(-550,  280), 480, new Color(0.7f,0.2f,1f,0.09f));
        Blob(bg.transform, new Vector2( 580, -220), 540, new Color(0.3f,0.1f,1f,0.07f));
        Blob(bg.transform, new Vector2( 300,  380), 300, new Color(1f,0.5f,0.8f,0.06f));
        for (int i=0;i<18;i++)
            StarDot(bg.transform,
                new Vector2(Random.Range(-900f,900f), Random.Range(-490f,490f)),
                Random.Range(3f,9f), Random.Range(0.15f,0.45f));
    }

    private void Blob(Transform p, Vector2 pos, float s, Color c)
    { var g=Img("Blob",p,new Vector2(s,s),pos,c,circleSprite); g.GetComponent<Image>().raycastTarget=false; }

    private void StarDot(Transform p, Vector2 pos, float s, float a)
    { var g=Img("Star",p,new Vector2(s,s),pos,new Color(1,1,1,a),circleSprite); g.GetComponent<Image>().raycastTarget=false; }

    // ═══════════════════════════════════════════════════════════
    // PANEL 1 — PLAYER SETUP
    // ═══════════════════════════════════════════════════════════
    private void BuildSetupPanel()
    {
        setupPanel = Panel("SetupPanel");
        Lbl(setupPanel,"Players",70,Color.white,new Vector2(0,390),FontStyles.Bold);
        Lbl(setupPanel,"Enter names below. Play one-on-one or\nteam vs. team. Highest score wins!",
            22,fadedWhite,new Vector2(0,310),FontStyles.Normal);

        var list = new GameObject("PlayerList"); list.transform.SetParent(setupPanel.transform,false);
        var lRT  = list.AddComponent<RectTransform>();
        lRT.sizeDelta=new Vector2(620,300); lRT.anchoredPosition=new Vector2(0,80);
        var vlg  = list.AddComponent<VerticalLayoutGroup>();
        vlg.spacing=16; vlg.childAlignment=TextAnchor.UpperCenter;
        vlg.childControlWidth=true; vlg.childControlHeight=false;
        vlg.childForceExpandWidth=true; vlg.childForceExpandHeight=false;
        playerListParent=list.transform;

        AddInput(); AddInput();

        Btn(setupPanel.transform,"Add Player",new Color(1,1,1,0.18f),Color.white,
            new Vector2(0,-80),new Vector2(620,62),()=>AddInput());
        Btn(setupPanel.transform,"Continue",yellowBtn,darkText,
            new Vector2(0,-370),new Vector2(620,70),()=>GoToCount());
    }

    private void AddInput()
    {
        int n = playerInputs.Count+1;
        var obj    = Img("Input"+n,playerListParent,new Vector2(620,66),
                         Vector2.zero,new Color(1,1,1,0.10f),roundLg);
        var border = Img("Border",obj.transform,Vector2.zero,Vector2.zero,
                         inputBorder,roundLg,true);
        border.GetComponent<Image>().raycastTarget=false;

        var field = obj.AddComponent<TMP_InputField>();
        var area  = new GameObject("Area"); area.transform.SetParent(obj.transform,false);
        var aRT   = area.AddComponent<RectTransform>();
        aRT.anchorMin=Vector2.zero; aRT.anchorMax=Vector2.one;
        aRT.offsetMin=new Vector2(22,5); aRT.offsetMax=new Vector2(-22,-5);
        area.AddComponent<RectMask2D>();

        var ph = TMPChild(area.transform,"Player or team name #"+n,22,new Color(1,1,1,.35f));
        var tx = TMPChild(area.transform,"",22,Color.white);
        field.textViewport=aRT; field.textComponent=tx;
        field.placeholder=ph;  field.pointSize=22;
        if (nunitoBold){ tx.font=nunitoBold; ph.font=nunitoBold; }
        playerInputs.Add(field);
    }

    // ═══════════════════════════════════════════════════════════
    // PANEL 2 — QUESTION COUNT
    // ═══════════════════════════════════════════════════════════
    private void BuildCountPanel()
    {
        countPanel = Panel("CountPanel");
        Lbl(countPanel,"How many\nquestions?",66,Color.white,new Vector2(0,360),FontStyles.Bold);
        Lbl(countPanel,"Set the stage — choose the amount of\nquestions before the game ends.",
            22,fadedWhite,new Vector2(0,255),FontStyles.Normal);
        Lbl(countPanel,"Number of questions",19,fadedWhite,new Vector2(0,105),FontStyles.Normal);

        var pill   = Img("Pill",countPanel.transform,new Vector2(620,82),
                         new Vector2(0,38),whiteCard,roundLg);
        var knob   = Img("Knob",pill.transform,new Vector2(62,62),
                         new Vector2(-220,0),greenCircle,circleSprite);
        sliderLabel = TMPInside(knob,"10",26,darkText,FontStyles.Bold);

        var slObj = new GameObject("Slider"); slObj.transform.SetParent(pill.transform,false);
        var slRT  = slObj.AddComponent<RectTransform>();
        slRT.anchorMin=Vector2.zero; slRT.anchorMax=Vector2.one;
        slRT.offsetMin=new Vector2(30,10); slRT.offsetMax=new Vector2(-30,-10);

        var fa   = new GameObject("FA"); fa.transform.SetParent(slObj.transform,false);
        var faRT = fa.AddComponent<RectTransform>();
        faRT.anchorMin=new Vector2(0,.35f); faRT.anchorMax=new Vector2(1,.65f);
        faRT.offsetMin=faRT.offsetMax=Vector2.zero;
        var fill = new GameObject("Fill"); fill.transform.SetParent(fa.transform,false);
        fill.AddComponent<Image>().color=new Color(0,0,0,0);
        var fRT  = fill.GetComponent<RectTransform>();
        fRT.anchorMin=Vector2.zero; fRT.anchorMax=Vector2.one;
        fRT.offsetMin=fRT.offsetMax=Vector2.zero;

        var ha   = new GameObject("HA"); ha.transform.SetParent(slObj.transform,false);
        var haRT = ha.AddComponent<RectTransform>();
        haRT.anchorMin=Vector2.zero; haRT.anchorMax=Vector2.one;
        haRT.offsetMin=new Vector2(10,0); haRT.offsetMax=new Vector2(-10,0);
        var h    = new GameObject("Handle"); h.transform.SetParent(ha.transform,false);
        h.AddComponent<Image>().color=new Color(0,0,0,0);
        var hRT  = h.GetComponent<RectTransform>(); hRT.sizeDelta=new Vector2(62,62);

        questionSlider = slObj.AddComponent<Slider>();
        questionSlider.fillRect=fRT; questionSlider.handleRect=hRT;
        questionSlider.minValue=5; questionSlider.maxValue=20;
        questionSlider.value=10;   questionSlider.wholeNumbers=true;
        questionSlider.direction=Slider.Direction.LeftToRight;

        var knobRT = knob.GetComponent<RectTransform>();
        questionSlider.onValueChanged.AddListener(v=>{
            totalQuestions=Mathf.RoundToInt(v);
            sliderLabel.text=totalQuestions.ToString();
            float t=(v-5f)/15f;
            float half=620f/2f-40f;
            knobRT.anchoredPosition=new Vector2(Mathf.Lerp(-half,half,t),0);
        });

        Btn(countPanel.transform,"Start Game!",yellowBtn,darkText,
            new Vector2(0,-360),new Vector2(620,70),()=>StartGame());
    }

    // ═══════════════════════════════════════════════════════════
    // PANEL 3 — GAME
    // ═══════════════════════════════════════════════════════════
    private void BuildGamePanel()
    {
        gamePanel = Panel("GamePanel");
        Lbl(gamePanel,"Read this question aloud:",21,fadedWhite,new Vector2(0,430),FontStyles.Normal);

        var card = Img("Card",gamePanel.transform,new Vector2(820,360),
                       new Vector2(0,110),whiteCard,roundLg);
        var pill = Img("Pill",card.transform,new Vector2(160,38),
                       new Vector2(0,152),pillDark,roundSm);
        TMPInside(pill,"♫  Music",16,Color.white,FontStyles.Bold);

        questionTxt = TMPInside(card,"Question",38,darkText,FontStyles.Bold);
        questionTxt.enableWordWrapping=true;
        var qRT = questionTxt.GetComponent<RectTransform>();
        qRT.anchorMin=new Vector2(.05f,.08f); qRT.anchorMax=new Vector2(.95f,.78f);
        qRT.offsetMin=qRT.offsetMax=Vector2.zero;

        questionNumTxt = Lbl(gamePanel,"Question 1 of 10",17,fadedWhite,
                             new Vector2(0,-85),FontStyles.Normal).GetComponent<TMP_Text>();

        Btn(gamePanel.transform,"Reveal Answer",yellowBtn,darkText,
            new Vector2(0,-350),new Vector2(480,68),()=>RevealAnswer());
    }

    // ═══════════════════════════════════════════════════════════
    // PANEL 4 — ANSWER REVEAL
    // ═══════════════════════════════════════════════════════════
    private void BuildRevealPanel()
    {
        revealPanel = Panel("RevealPanel");

        revealQTxt = Lbl(revealPanel,"Q",22,fadedWhite,
                         new Vector2(0,440),FontStyles.Normal).GetComponent<TMP_Text>();
        revealQTxt.GetComponent<RectTransform>().sizeDelta=new Vector2(860,44);

        answerTxt = Lbl(revealPanel,"Answer:",40,yellowBtn,
                        new Vector2(0,374),FontStyles.Bold).GetComponent<TMP_Text>();
        answerTxt.GetComponent<RectTransform>().sizeDelta=new Vector2(860,52);

        Lbl(revealPanel,"Who got the answer right?",19,fadedWhite,
            new Vector2(0,308),FontStyles.Normal);

        var list = new GameObject("CheckList"); list.transform.SetParent(revealPanel.transform,false);
        var lRT  = list.AddComponent<RectTransform>();
        lRT.sizeDelta=new Vector2(620,280); lRT.anchoredPosition=new Vector2(0,90);
        var vlg  = list.AddComponent<VerticalLayoutGroup>();
        vlg.spacing=10; vlg.childAlignment=TextAnchor.UpperCenter;
        vlg.childControlWidth=true; vlg.childControlHeight=false;
        vlg.childForceExpandWidth=true; vlg.childForceExpandHeight=false;
        checkListParent=list.transform;

        scoreboardTxt = Lbl(revealPanel,"",16,fadedWhite,
                            new Vector2(0,-108),FontStyles.Normal).GetComponent<TMP_Text>();
        scoreboardTxt.GetComponent<RectTransform>().sizeDelta=new Vector2(920,30);

        Btn(revealPanel.transform,"Next Question",yellowBtn,darkText,
            new Vector2(0,-350),new Vector2(480,68),()=>NextQ());
    }

    // ═══════════════════════════════════════════════════════════
    // PANEL 5 — RESULTS
    // ═══════════════════════════════════════════════════════════
    private void BuildResultsPanel()
    {
        resultsPanel = Panel("ResultsPanel");
        Lbl(resultsPanel,"WINNER",19,fadedWhite,new Vector2(0,445),FontStyles.Normal);
        winnerTxt = Lbl(resultsPanel,"?",60,yellowBtn,
                        new Vector2(0,375),FontStyles.Bold).GetComponent<TMP_Text>();
        Lbl(resultsPanel,"Final Scores",21,fadedWhite,new Vector2(0,305),FontStyles.Normal);

        var list = new GameObject("Results"); list.transform.SetParent(resultsPanel.transform,false);
        var lRT  = list.AddComponent<RectTransform>();
        lRT.sizeDelta=new Vector2(620,360); lRT.anchoredPosition=new Vector2(0,40);
        var vlg  = list.AddComponent<VerticalLayoutGroup>();
        vlg.spacing=10; vlg.childAlignment=TextAnchor.UpperCenter;
        vlg.childControlWidth=true; vlg.childControlHeight=false;
        vlg.childForceExpandWidth=true; vlg.childForceExpandHeight=false;
        resultsListParent=list.transform;

        Btn(resultsPanel.transform,"Play Again",yellowBtn,darkText,
            new Vector2(0,-370),new Vector2(480,68),()=>PlayAgain());
    }

    // ═══════════════════════════════════════════════════════════
    // GAME LOGIC
    // ═══════════════════════════════════════════════════════════
    private void GoToCount()
    {
        playerNames.Clear(); playerScores.Clear();
        foreach (var f in playerInputs)
            if (f && f.text.Trim()!="") { playerNames.Add(f.text.Trim()); playerScores.Add(0); }
        if (playerNames.Count<2){ Debug.Log("Need 2+ players"); return; }
        Show(countPanel);
    }

    private void StartGame()
    {
        totalQuestions=Mathf.RoundToInt(questionSlider.value);
        gameQ=Shuffle(allQ).GetRange(0,Mathf.Min(totalQuestions,allQ.Count));
        currentQ=0;
        for (int i=0;i<playerScores.Count;i++) playerScores[i]=0;
        ShowQ();
    }

    private void ShowQ()
    {
        if (currentQ>=gameQ.Count){ ShowResults(); return; }
        questionTxt.text    = gameQ[currentQ].question;
        questionNumTxt.text = $"Question {currentQ+1} of {gameQ.Count}";
        Show(gamePanel);
    }

    private void RevealAnswer()
    {
        var q = gameQ[currentQ];
        revealQTxt.text = q.question;
        answerTxt.text  = "Answer: "+q.answers[q.correctIndex];
        foreach (Transform c in checkListParent) Destroy(c.gameObject);

        for (int i=0;i<playerNames.Count;i++)
        {
            int pi=i;
            var row = Img("Row"+i,checkListParent,new Vector2(620,64),
                          Vector2.zero,cardPurple,roundMd);
            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing=15; hlg.padding=new RectOffset(24,24,10,10);
            hlg.childAlignment=TextAnchor.MiddleLeft;
            hlg.childControlWidth=false; hlg.childForceExpandWidth=false;

            var nGO  = new GameObject("N"); nGO.transform.SetParent(row.transform,false);
            var nTxt = nGO.AddComponent<TextMeshProUGUI>();
            nTxt.text=playerNames[i]; nTxt.fontSize=23;
            nTxt.fontStyle=FontStyles.Bold; nTxt.color=Color.white;
            if (nunitoBold) nTxt.font=nunitoBold;
            nGO.GetComponent<RectTransform>().sizeDelta=new Vector2(440,44);

            var tGO  = new GameObject("Tog"); tGO.transform.SetParent(row.transform,false);
            tGO.AddComponent<RectTransform>().sizeDelta=new Vector2(44,44);
            var bgImg = Img("BG",tGO.transform,new Vector2(44,44),Vector2.zero,
                            new Color(1,1,1,.22f),circleSprite).GetComponent<Image>();
            var ckImg = Img("Ck",tGO.transform,new Vector2(32,32),Vector2.zero,
                            greenCheck,circleSprite).GetComponent<Image>();
            var ckRT  = ckImg.GetComponent<RectTransform>();
            ckRT.anchorMin=new Vector2(.14f,.14f); ckRT.anchorMax=new Vector2(.86f,.86f);
            ckRT.offsetMin=ckRT.offsetMax=Vector2.zero; ckRT.sizeDelta=Vector2.zero;

            var tog = tGO.AddComponent<Toggle>();
            tog.targetGraphic=bgImg; tog.graphic=ckImg; tog.isOn=false;
            tog.onValueChanged.AddListener(on=>{
                playerScores[pi]=Mathf.Max(0,playerScores[pi]+(on?1:-1));
                UpdateBoard();
                if (on) audioSource.PlayOneShot(correctSFX);   // ← correct chime
            });
        }
        UpdateBoard();
        Show(revealPanel);
    }

    private void UpdateBoard()
    {
        var sb=new System.Text.StringBuilder();
        for (int i=0;i<playerNames.Count;i++)
        {
            sb.Append(playerNames[i]).Append(": ").Append(playerScores[i]).Append(" pts");
            if (i<playerNames.Count-1) sb.Append("   |   ");
        }
        scoreboardTxt.text=sb.ToString();
    }

    private void NextQ(){ currentQ++; ShowQ(); }

    private void ShowResults()
    {
        foreach (Transform c in resultsListParent) Destroy(c.gameObject);

        int best=0; string winner="";
        for (int i=0;i<playerNames.Count;i++)
            if (playerScores[i]>best){ best=playerScores[i]; winner=playerNames[i]; }

        var idx=new List<int>(); for(int i=0;i<playerNames.Count;i++) idx.Add(i);
        idx.Sort((a,b)=>playerScores[b].CompareTo(playerScores[a]));

        int rank=1;
        foreach (int i in idx)
        {
            bool top=rank==1;
            var row=Img("R"+rank,resultsListParent,new Vector2(620,60),
                        Vector2.zero,top?new Color(1f,.87f,.12f,.35f):cardPurple,roundMd);
            TMPInside(row,$"#{rank}   {playerNames[i]}   —   {playerScores[i]} pts",
                      top?28:22,Color.white,top?FontStyles.Bold:FontStyles.Normal);
            rank++;
        }
        winnerTxt.text=winner+" wins!";

        StartCoroutine(DrumrollThenVictory());  // ← drumroll + fanfare
        Show(resultsPanel);
    }

    private IEnumerator DrumrollThenVictory()
    {
        audioSource.PlayOneShot(drumrollSFX);
        yield return new WaitForSeconds(drumrollSFX.length);
        audioSource.PlayOneShot(victorySFX);
    }

    private void PlayAgain()
    {
        playerInputs.Clear();
        foreach (Transform c in playerListParent) Destroy(c.gameObject);
        AddInput(); AddInput();
        Show(setupPanel);
    }

    // ═══════════════════════════════════════════════════════════
    // UI HELPERS
    // ═══════════════════════════════════════════════════════════
    private void Show(GameObject p)
    {
        foreach (var go in new[]{setupPanel,countPanel,gamePanel,revealPanel,resultsPanel})
            if (go) go.SetActive(false);
        p.SetActive(true);
    }

    private GameObject Panel(string name)
    {
        var go=new GameObject(name); go.transform.SetParent(canvas.transform,false);
        var rt=go.AddComponent<RectTransform>();
        rt.anchorMin=Vector2.zero; rt.anchorMax=Vector2.one;
        rt.offsetMin=rt.offsetMax=Vector2.zero;
        return go;
    }

    private GameObject Img(string name,Transform parent,Vector2 size,Vector2 pos,
                           Color color,Sprite sprite,bool stretch=false)
    {
        var go=new GameObject(name); go.transform.SetParent(parent,false);
        var rt=go.AddComponent<RectTransform>();
        if (stretch){ rt.anchorMin=Vector2.zero; rt.anchorMax=Vector2.one;
                      rt.offsetMin=rt.offsetMax=Vector2.zero; }
        else{ rt.sizeDelta=size; rt.anchoredPosition=pos; }
        var img=go.AddComponent<Image>(); img.color=color;
        if (sprite){ img.sprite=sprite; img.type=Image.Type.Sliced; img.pixelsPerUnitMultiplier=1; }
        return go;
    }

    private GameObject Lbl(GameObject parent,string text,int size,Color color,
                           Vector2 pos,FontStyles style)
        =>Lbl(parent.transform,text,size,color,pos,style);

    private GameObject Lbl(Transform parent,string text,int size,Color color,
                           Vector2 pos,FontStyles style)
    {
        var go=new GameObject("L"); go.transform.SetParent(parent,false);
        var t=go.AddComponent<TextMeshProUGUI>();
        t.text=text; t.fontSize=size; t.color=color; t.fontStyle=style;
        t.alignment=TextAlignmentOptions.Center; t.enableWordWrapping=true;
        t.raycastTarget=false;
        if (nunitoBold) t.font=nunitoBold;
        var rt=go.GetComponent<RectTransform>();
        rt.sizeDelta=new Vector2(920,size*2+30); rt.anchoredPosition=pos;
        return go;
    }

    private TMP_Text TMPInside(GameObject parent,string text,int size,
                               Color color,FontStyles style)
    {
        var go=new GameObject("T"); go.transform.SetParent(parent.transform,false);
        var t=go.AddComponent<TextMeshProUGUI>();
        t.text=text; t.fontSize=size; t.color=color; t.fontStyle=style;
        t.alignment=TextAlignmentOptions.Center; t.enableWordWrapping=true;
        t.raycastTarget=false;
        if (nunitoBold) t.font=nunitoBold;
        var rt=go.GetComponent<RectTransform>();
        rt.anchorMin=Vector2.zero; rt.anchorMax=Vector2.one;
        rt.offsetMin=rt.offsetMax=Vector2.zero;
        return t;
    }

    private TMP_Text TMPChild(Transform parent,string text,int size,Color color)
    {
        var go=new GameObject("T"); go.transform.SetParent(parent,false);
        var t=go.AddComponent<TextMeshProUGUI>();
        t.text=text; t.fontSize=size; t.color=color;
        t.alignment=TextAlignmentOptions.MidlineLeft;
        if (nunitoBold) t.font=nunitoBold;
        var rt=go.GetComponent<RectTransform>();
        rt.anchorMin=Vector2.zero; rt.anchorMax=Vector2.one;
        rt.offsetMin=rt.offsetMax=Vector2.zero;
        return t;
    }

    private void Btn(Transform parent,string label,Color bg,Color txtCol,
                     Vector2 pos,Vector2 size,UnityEngine.Events.UnityAction action)
    {
        var go=Img("Btn_"+label,parent,size,pos,bg,roundLg);
        var btn=go.AddComponent<Button>();
        var cb=btn.colors;
        cb.highlightedColor=new Color(bg.r*.9f,bg.g*.9f,bg.b*.9f,1f);
        cb.pressedColor=new Color(bg.r*.75f,bg.g*.75f,bg.b*.75f,1f);
        btn.colors=cb; btn.onClick.AddListener(action);
        TMPInside(go,label,26,txtCol,FontStyles.Bold);
    }

    private List<TriviaQuestion> Shuffle(List<TriviaQuestion> src)
    {
        var s=new List<TriviaQuestion>(src);
        for(int i=s.Count-1;i>0;i--){int j=Random.Range(0,i+1);var t=s[i];s[i]=s[j];s[j]=t;}
        return s;
    }
}