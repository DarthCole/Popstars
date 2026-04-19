using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KaraokeUIBuilder : EditorWindow
{
    static readonly Color BG        = new Color(0.039f, 0.027f, 0.094f, 1f);
    static readonly Color PURPLE    = new Color(0.659f, 0.333f, 0.969f, 1f);
    static readonly Color PURP_DIM  = new Color(0.659f, 0.333f, 0.969f, 0.18f);
    static readonly Color PURP_RING = new Color(0.200f, 0.075f, 0.380f, 0.90f);
    static readonly Color PILL_BG   = new Color(0.278f, 0.110f, 0.471f, 0.90f);
    static readonly Color BAR_BG    = new Color(0.094f, 0.047f, 0.196f, 1f);
    static readonly Color BTN       = new Color(0.153f, 0.067f, 0.294f, 1f);
    static readonly Color BTN_BIG   = new Color(0.106f, 0.039f, 0.216f, 1f);
    static readonly Color OVERLAY   = new Color(0.016f, 0.008f, 0.055f, 0.94f);
    static readonly Color W100      = Color.white;
    static readonly Color W55       = new Color(1, 1, 1, 0.55f);
    static readonly Color W35       = new Color(1, 1, 1, 0.35f);
    static readonly Color GOLD      = new Color(1.00f, 0.843f, 0.098f, 1.00f);

    [MenuItem("Karaoke/Build Full Karaoke UI")]
    static void Build()
    {
        var canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null) { Debug.LogError("[Builder] No Canvas found."); return; }

        var scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;

        var old = GameObject.Find("KaraokePanel");
        if (old != null) Object.DestroyImmediate(old);

        var root  = canvas.GetComponent<RectTransform>();
        var panel = E(root, "KaraokePanel"); Stretch(panel);

        BuildBackground(panel);
        BuildTopBar(panel);
        BuildMicIndicator(panel);
        BuildLyrics(panel);
        BuildPitchBar(panel);
        BuildProgressBar(panel);
        BuildWaveform(panel);
        BuildControls(panel);
        BuildResultsPanel(panel);

        EditorUtility.SetDirty(canvas.gameObject);
        Debug.Log("[KaraokeUIBuilder] Full UI built.");
    }

    static void BuildBackground(RectTransform p)
    {
        var bg = Img(p, "Background", BG); Stretch(bg);

        string dir = "Assets/Textures", path = dir + "/KaraokeGlow.png";
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        if (!File.Exists(path))
        {
            const int S = 256;
            var tex = new Texture2D(S, S, TextureFormat.RGBA32, false);
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++) {
                    float dx = (x-S*.5f)/(S*.5f), dy = (y-S*.5f)/(S*.5f);
                    float a  = Mathf.Clamp01(1f-Mathf.Sqrt(dx*dx+dy*dy))*.5f;
                    tex.SetPixel(x,y,new Color(.26f,.08f,.55f,a));
                }
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }
        var go = new GameObject("CenterGlow", typeof(RectTransform), typeof(RawImage));
        go.transform.SetParent(p, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(.5f,.5f);
        rt.pivot = new Vector2(.5f,.5f);
        rt.sizeDelta = new Vector2(1300,1300);
        rt.anchoredPosition = Vector2.zero;
        var ri = go.GetComponent<RawImage>();
        ri.texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    static void BuildTopBar(RectTransform p)
    {
        var bar = E(p, "TopBar");
        bar.anchorMin = new Vector2(0,1); bar.anchorMax = new Vector2(1,1);
        bar.pivot = new Vector2(.5f,1);
        bar.offsetMin = new Vector2(0,-90); bar.offsetMax = Vector2.zero;

        var bGO = new GameObject("BackButton", typeof(RectTransform), typeof(Image), typeof(Button));
        bGO.transform.SetParent(bar, false);
        var bRT = bGO.GetComponent<RectTransform>();
        bRT.anchorMin = bRT.anchorMax = new Vector2(0,.5f); bRT.pivot = new Vector2(0,.5f);
        bRT.sizeDelta = new Vector2(65,65); bRT.anchoredPosition = new Vector2(30,0);
        bGO.GetComponent<Image>().color = new Color(0,0,0,0);
        var arr = Img(bRT,"ArrowIcon",W100); Stretch(arr);

        var info = E(bar,"SongInfo");
        info.anchorMin = new Vector2(.25f,0); info.anchorMax = new Vector2(.75f,1);
        info.offsetMin = info.offsetMax = Vector2.zero;

        var title = Tmp(info,"SongTitle","YELLOW",30,W100,TextAlignmentOptions.Center,true);
        title.anchorMin = new Vector2(0,.52f); title.anchorMax = new Vector2(1,1);
        title.offsetMin = title.offsetMax = Vector2.zero;

        var art = Tmp(info,"ArtistName","COLDPLAY",18,W55,TextAlignmentOptions.Center,false);
        art.anchorMin = new Vector2(0,0); art.anchorMax = new Vector2(1,.52f);
        art.offsetMin = art.offsetMax = Vector2.zero;

        var pill = Img(bar,"ScorePill",PILL_BG);
        pill.anchorMin = pill.anchorMax = new Vector2(1,.5f); pill.pivot = new Vector2(1,.5f);
        pill.sizeDelta = new Vector2(130,50); pill.anchoredPosition = new Vector2(-25,0);
        var sc = Tmp(pill,"ScoreText","847",30,W100,TextAlignmentOptions.Center,true); Stretch(sc);
    }

    static void BuildMicIndicator(RectTransform p)
    {
        var mic = E(p,"MicIndicator");
        mic.anchorMin = mic.anchorMax = new Vector2(.5f,.5f);
        mic.pivot = new Vector2(.5f,.5f);
        mic.sizeDelta = new Vector2(165,165);
        mic.anchoredPosition = new Vector2(0,255);

        var ring = Img(mic,"MicRing",PURP_RING); Stretch(ring);
        var av   = Img(mic,"AvatarImage",new Color(.12f,.05f,.27f,1f));
        av.anchorMin = new Vector2(.1f,.1f); av.anchorMax = new Vector2(.9f,.9f);
        av.offsetMin = av.offsetMax = Vector2.zero;
        var dot  = Img(mic,"MicDot",PURPLE);
        dot.anchorMin = dot.anchorMax = new Vector2(.78f,.08f);
        dot.pivot = new Vector2(.5f,.5f); dot.sizeDelta = new Vector2(28,28);
        dot.anchoredPosition = Vector2.zero;
    }

    static void BuildLyrics(RectTransform p)
    {
        var area = E(p,"LyricsArea");
        area.anchorMin = area.anchorMax = new Vector2(.5f,.5f);
        area.pivot = new Vector2(.5f,.5f);
        area.sizeDelta = new Vector2(900,215);
        area.anchoredPosition = new Vector2(0,75);

        var prev = Tmp(area,"PrevLyric","Look at the stars",24,W35,TextAlignmentOptions.Center,false);
        prev.anchorMin = new Vector2(0,.68f); prev.anchorMax = new Vector2(1,1);
        prev.offsetMin = prev.offsetMax = Vector2.zero;

        var cur = Tmp(area,"CurrentLyric","Look how they <color=#A855F7><u>shine</u></color> for you",
                      42,W100,TextAlignmentOptions.Center,true);
        cur.anchorMin = new Vector2(0,.29f); cur.anchorMax = new Vector2(1,.72f);
        cur.offsetMin = cur.offsetMax = Vector2.zero;
        cur.GetComponent<TextMeshProUGUI>().enableWordWrapping = true;

        var nxt = Tmp(area,"NextLyric","And everything you do",24,W55,TextAlignmentOptions.Center,false);
        nxt.anchorMin = new Vector2(0,0); nxt.anchorMax = new Vector2(1,.34f);
        nxt.offsetMin = nxt.offsetMax = Vector2.zero;
    }

    static void BuildPitchBar(RectTransform p)
    {
        var sec = E(p,"PitchSection");
        sec.anchorMin = sec.anchorMax = new Vector2(.5f,.5f);
        sec.pivot = new Vector2(.5f,.5f);
        sec.sizeDelta = new Vector2(900,68);
        sec.anchoredPosition = new Vector2(0,-65);

        var lblL = Tmp(sec,"PitchLabelLeft","YOUR PITCH",15,W35,TextAlignmentOptions.Left,false);
        lblL.anchorMin = new Vector2(0,.54f); lblL.anchorMax = new Vector2(.5f,1);
        lblL.offsetMin = lblL.offsetMax = Vector2.zero;

        var lblR = Tmp(sec,"NoteLabel","A4  \u2014  440 HZ",15,W35,TextAlignmentOptions.Right,false);
        lblR.anchorMin = new Vector2(.5f,.54f); lblR.anchorMax = new Vector2(1,1);
        lblR.offsetMin = lblR.offsetMax = Vector2.zero;

        var track = Img(sec,"PitchBarBG",BAR_BG);
        track.anchorMin = new Vector2(0,0); track.anchorMax = new Vector2(1,.50f);
        track.offsetMin = track.offsetMax = Vector2.zero;

        var zone = Img(track,"TargetZone",PURP_DIM); Stretch(zone);

        var fill = Img(track,"PitchBarFill",PURPLE); Stretch(fill);
        var fi = fill.GetComponent<Image>();
        fi.type = Image.Type.Filled; fi.fillMethod = Image.FillMethod.Horizontal; fi.fillAmount = .57f;
    }

    static void BuildProgressBar(RectTransform p)
    {
        var sec = E(p,"ProgressBar");
        sec.anchorMin = sec.anchorMax = new Vector2(.5f,.5f);
        sec.pivot = new Vector2(.5f,.5f);
        sec.sizeDelta = new Vector2(900,50);
        sec.anchoredPosition = new Vector2(0,-150);

        var track = Img(sec,"ProgressTrack",BAR_BG);
        track.anchorMin = new Vector2(0,.35f); track.anchorMax = new Vector2(1,.78f);
        track.offsetMin = track.offsetMax = Vector2.zero;

        var fill = Img(track,"ProgressFill",PURPLE); Stretch(fill);
        var fi = fill.GetComponent<Image>();
        fi.type = Image.Type.Filled; fi.fillMethod = Image.FillMethod.Horizontal; fi.fillAmount = .36f;

        var cur = Tmp(sec,"CurrentTimeLabel","1:12",17,W55,TextAlignmentOptions.Left,false);
        cur.anchorMin = new Vector2(0,0); cur.anchorMax = new Vector2(.12f,.38f);
        cur.offsetMin = cur.offsetMax = Vector2.zero;

        var tot = Tmp(sec,"TotalTimeLabel","3:10",17,W55,TextAlignmentOptions.Right,false);
        tot.anchorMin = new Vector2(.88f,0); tot.anchorMax = new Vector2(1,.38f);
        tot.offsetMin = tot.offsetMax = Vector2.zero;
    }

    static void BuildWaveform(RectTransform p)
    {
        var w = E(p,"Waveform");
        w.anchorMin = w.anchorMax = new Vector2(.5f,.5f);
        w.pivot = new Vector2(.5f,.5f);
        w.sizeDelta = new Vector2(120,32);
        w.anchoredPosition = new Vector2(0,-230);

        float[] hs = {8,14,21,28,21,14,8};
        for (int i = 0; i < 7; i++) {
            var bar = Img(w,$"WaveBar{i}",new Color(.659f,.333f,.969f,.52f));
            bar.anchorMin = bar.anchorMax = new Vector2(.5f,.5f);
            bar.pivot = new Vector2(.5f,.5f);
            bar.sizeDelta = new Vector2(7,hs[i]);
            bar.anchoredPosition = new Vector2(-42f+i*14f,0);
        }
    }

    static void BuildControls(RectTransform p)
    {
        // Restart  — ?
        MkRoundBtn(p,"RestartButton","\u21ba",32,BTN,new Vector2(.5f,.5f),new Vector2(-115,-320),new Vector2(72,72));
        // Record  — ? (filled circle icon; swap for mic sprite in Inspector)
        MkRoundBtn(p,"RecordButton","\u25CF",38,BTN_BIG,new Vector2(.5f,.5f),new Vector2(0,-320),new Vector2(92,92));
        // Skip  — ?
        MkRoundBtn(p,"SkipButton","\u23ED",28,BTN,new Vector2(.5f,.5f),new Vector2(115,-320),new Vector2(72,72));
        // Down arrow  — ?
        MkRoundBtn(p,"DownloadButton","\u2193",26,BTN,new Vector2(.5f,0),new Vector2(0,22),new Vector2(58,58));
    }

    static void BuildResultsPanel(RectTransform p)
    {
        var r = E(p,"ResultsPanel"); Stretch(r);
        r.gameObject.AddComponent<CanvasGroup>();
        var ov = Img(r,"ResultsOverlay",OVERLAY); Stretch(ov);

        var sc = Tmp(r,"FinalScoreText","0",90,W100,TextAlignmentOptions.Center,true);
        sc.anchorMin = sc.anchorMax = new Vector2(.5f,.5f); sc.pivot = new Vector2(.5f,.5f);
        sc.sizeDelta = new Vector2(500,130); sc.anchoredPosition = new Vector2(0,130);

        var sr = E(r,"StarsRow");
        sr.anchorMin = sr.anchorMax = new Vector2(.5f,.5f); sr.pivot = new Vector2(.5f,.5f);
        sr.sizeDelta = new Vector2(290,85); sr.anchoredPosition = new Vector2(0,10);
        var hg = sr.gameObject.AddComponent<HorizontalLayoutGroup>();
        hg.spacing = 18; hg.childAlignment = TextAnchor.MiddleCenter;
        hg.childForceExpandWidth = hg.childForceExpandHeight = false;

        for (int i = 0; i < 3; i++) {
            var s = new GameObject($"Star{i+1}",typeof(RectTransform),typeof(Image));
            s.transform.SetParent(sr,false);
            s.GetComponent<RectTransform>().sizeDelta = new Vector2(80,80);
            s.GetComponent<Image>().color = new Color(.24f,.24f,.24f,.4f);
        }

        var co = Tmp(r,"CoinsText","+0 StarCoins",30,GOLD,TextAlignmentOptions.Center,false);
        co.anchorMin = co.anchorMax = new Vector2(.5f,.5f); co.pivot = new Vector2(.5f,.5f);
        co.sizeDelta = new Vector2(500,55); co.anchoredPosition = new Vector2(0,-80);

        MkBtn(r,"PlayAgainButton","Play Again",22,PILL_BG,new Vector2(.5f,.5f),new Vector2(-120,-175),new Vector2(210,62));
        MkBtn(r,"BackToHubButton","Back to Hub",22,BTN,new Vector2(.5f,.5f),new Vector2(120,-175),new Vector2(210,62));

        r.gameObject.SetActive(false);
    }

    // -- Helpers ---------------------------------------------------------------

    static RectTransform E(RectTransform parent, string name) {
        var go = new GameObject(name,typeof(RectTransform));
        go.transform.SetParent(parent,false);
        return go.GetComponent<RectTransform>();
    }

    static RectTransform Img(RectTransform parent, string name, Color color) {
        var go = new GameObject(name,typeof(RectTransform),typeof(Image));
        go.transform.SetParent(parent,false);
        go.GetComponent<Image>().color = color;
        return go.GetComponent<RectTransform>();
    }

    static RectTransform Tmp(RectTransform parent, string name, string text,
                              float size, Color color, TextAlignmentOptions align, bool bold) {
        var go = new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI));
        go.transform.SetParent(parent,false);
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = size; tmp.color = color;
        tmp.alignment = align; tmp.fontStyle = bold?FontStyles.Bold:FontStyles.Normal;
        tmp.enableWordWrapping = false; tmp.richText = true;
        tmp.overflowMode = TextOverflowModes.Overflow;
        return go.GetComponent<RectTransform>();
    }

    static void Stretch(RectTransform rt) {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    // Flat rectangle button — used for results panel text buttons
    static void MkBtn(RectTransform parent, string name, string label,
                      float fontSize, Color bg, Vector2 anchor, Vector2 pos, Vector2 size) {
        var go = new GameObject(name,typeof(RectTransform),typeof(Image),typeof(Button));
        go.transform.SetParent(parent,false);
        go.GetComponent<Image>().color = bg;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor; rt.pivot = new Vector2(.5f,.5f);
        rt.sizeDelta = size; rt.anchoredPosition = pos;
        var lbl = Tmp(rt,"Label",label,fontSize,Color.white,TextAlignmentOptions.Center,false);
        Stretch(lbl);
    }

    // Circular button with white outline ring + centred icon label
    static void MkRoundBtn(RectTransform parent, string name, string icon,
                           float fontSize, Color bg, Vector2 anchor, Vector2 pos, Vector2 size) {
        var knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

        // White outline ring (slightly larger)
        var ringGO = new GameObject(name+"_Ring", typeof(RectTransform), typeof(Image));
        ringGO.transform.SetParent(parent, false);
        var ringRT = ringGO.GetComponent<RectTransform>();
        ringRT.anchorMin = ringRT.anchorMax = anchor;
        ringRT.pivot     = new Vector2(.5f,.5f);
        ringRT.sizeDelta = size + new Vector2(6,6);
        ringRT.anchoredPosition = pos;
        var ringImg = ringGO.GetComponent<Image>();
        ringImg.sprite = knob;
        ringImg.color  = new Color(1,1,1,0.35f);  // subtle white glow
        ringImg.type   = Image.Type.Simple;

        // Filled circle button
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot     = new Vector2(.5f,.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
        var img = go.GetComponent<Image>();
        img.sprite = knob;
        img.color  = bg;
        img.type   = Image.Type.Simple;

        // Icon label
        var lbl = Tmp(rt,"Label",icon,fontSize,Color.white,TextAlignmentOptions.Center,false);
        Stretch(lbl);
    }
}
