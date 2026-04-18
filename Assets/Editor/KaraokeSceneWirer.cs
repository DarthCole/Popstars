using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Wires the full karaoke scene in one click.
///
/// Prerequisites:
///   1. A Canvas exists in the scene.
///   2. "KaraokePanel" has already been built (Karaoke ▶ Build Full Karaoke UI).
///
/// What this does:
///   • Creates a "KaraokeManager" GameObject and adds:
///       AudioSource, MicPitchDetector, LyricsSyncer, PitchScorer, KaraokeUI
///   • Adds KaraokeResultsPanel component to the ResultsPanel UI object.
///   • Automatically fills every [SerializeField] in KaraokeUI and KaraokeResultsPanel
///     by matching GameObject names from the built hierarchy.
///
/// Menu: Karaoke ▶ Wire Scene
/// </summary>
public class KaraokeSceneWirer : EditorWindow
{
    [MenuItem("Karaoke/Wire Scene")]
    static void Wire()
    {
        // ── 0. Sanity checks ─────────────────────────────────────────────────
        var canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[KaraokeSceneWirer] No Canvas found. Add a Canvas to the scene first.");
            return;
        }

        var panelGO = GameObject.Find("KaraokePanel");
        if (panelGO == null)
        {
            Debug.LogError("[KaraokeSceneWirer] 'KaraokePanel' not found. " +
                           "Run  Karaoke ▶ Build Full Karaoke UI  first.");
            return;
        }

        // ── 1. Create / find KaraokeManager host  ───────────────────────────
        var mgrGO = GameObject.Find("KaraokeManager");
        if (mgrGO == null)
        {
            mgrGO = new GameObject("KaraokeManager");
            Debug.Log("[KaraokeSceneWirer] Created KaraokeManager GameObject.");
        }

        // Required components on the manager host
        EnsureComponent<AudioSource>(mgrGO);
        EnsureComponent<MicPitchDetector>(mgrGO);
        EnsureComponent<LyricsSyncer>(mgrGO);
        EnsureComponent<PitchScorer>(mgrGO);
        var mgr = EnsureComponent<KaraokeManager>(mgrGO);
        var ui  = EnsureComponent<KaraokeUI>(mgrGO);

        // ── 2. ResultsPanel component ────────────────────────────────────────
        var resultsGO   = panelGO.transform.Find("ResultsPanel")?.gameObject;
        KaraokeResultsPanel resultsComp = null;

        if (resultsGO != null)
        {
            resultsComp = EnsureComponent<KaraokeResultsPanel>(resultsGO);
            WireResultsPanel(resultsComp, resultsGO);
        }
        else
        {
            Debug.LogWarning("[KaraokeSceneWirer] ResultsPanel not found under KaraokePanel.");
        }

        // ── 3. Wire KaraokeUI ─────────────────────────────────────────────────
        WireUI(ui, mgr, panelGO, resultsComp);

        // ── 4. Mark dirty ─────────────────────────────────────────────────────
        EditorUtility.SetDirty(mgrGO);
        if (resultsGO != null) EditorUtility.SetDirty(resultsGO);

        Debug.Log("[KaraokeSceneWirer] Done — scene fully wired. Assign a SongData asset " +
                  "to KaraokeManager.songData in the Inspector, then press Play.");
    }

    // =========================================================================
    //  KaraokeUI wiring
    // =========================================================================

    static void WireUI(KaraokeUI ui, KaraokeManager mgr,
                       GameObject panel, KaraokeResultsPanel results)
    {
        var so = new SerializedObject(ui);

        // Manager reference
        so.FindProperty("manager").objectReferenceValue = mgr;

        // Panel references for back button navigation
        var selectPanel = GameObject.Find("SongSelectPanel");
        if (selectPanel != null)
            so.FindProperty("songSelectPanel").objectReferenceValue = selectPanel;
        so.FindProperty("karaokePanel").objectReferenceValue = panel.gameObject;

        // ── Top bar ───────────────────────────────────────────────────────────
        SetTMP  (so, "songTitleText", panel, "TopBar/SongInfo/SongTitle");
        SetTMP  (so, "artistText",    panel, "TopBar/SongInfo/ArtistName");
        SetTMP  (so, "scoreText",     panel, "TopBar/ScorePill/ScoreText");
        SetComp <Button>(so, "backButton", panel, "TopBar/BackButton");

        // ── Mic indicator ─────────────────────────────────────────────────────
        SetRT   (so, "micRing",      panel, "MicIndicator/MicRing");
        SetImg  (so, "micRingImage", panel, "MicIndicator/MicRing");
        SetImg  (so, "micDot",       panel, "MicIndicator/MicDot");
        SetImg  (so, "avatarImage",  panel, "MicIndicator/AvatarImage");

        // ── Lyrics ────────────────────────────────────────────────────────────
        SetTMP  (so, "prevLyricText",    panel, "LyricsArea/PrevLyric");
        SetTMP  (so, "currentLyricText", panel, "LyricsArea/CurrentLyric");
        SetTMP  (so, "nextLyricText",    panel, "LyricsArea/NextLyric");

        // ── Pitch bar ─────────────────────────────────────────────────────────
        SetImg  (so, "pitchBarFill",  panel, "PitchSection/PitchBarBG/PitchBarFill");
        SetTMP  (so, "pitchNoteLabel", panel, "PitchSection/NoteLabel");

        // ── Progress bar ──────────────────────────────────────────────────────
        SetImg  (so, "progressBarFill",  panel, "ProgressBar/ProgressTrack/ProgressFill");
        SetTMP  (so, "currentTimeLabel", panel, "ProgressBar/CurrentTimeLabel");
        SetTMP  (so, "totalTimeLabel",   panel, "ProgressBar/TotalTimeLabel");

        // ── Waveform bars (array) ─────────────────────────────────────────────
        var waveParent = panel.transform.Find("Waveform");
        if (waveParent != null)
        {
            var barsProp = so.FindProperty("waveformBars");
            int count    = waveParent.childCount;
            barsProp.arraySize = count;
            for (int i = 0; i < count; i++)
                barsProp.GetArrayElementAtIndex(i).objectReferenceValue =
                    waveParent.GetChild(i).GetComponent<RectTransform>();
        }
        else
        {
            Debug.LogWarning("[KaraokeSceneWirer] Waveform container not found.");
        }

        // ── Control buttons (direct children of KaraokePanel) ─────────────────
        SetComp<Button>(so, "restartButton", panel, "RestartButton");
        SetComp<Button>(so, "recordButton",  panel, "RecordButton");
        SetComp<Button>(so, "skipButton",    panel, "SkipButton");

        // recordButtonIcon — the Image on RecordButton (used for visual state swap)
        SetImg(so, "recordButtonIcon", panel, "RecordButton");

        // ── Results panel reference ───────────────────────────────────────────
        if (results != null)
            so.FindProperty("resultsPanel").objectReferenceValue = results;

        so.ApplyModifiedProperties();
    }

    // =========================================================================
    //  KaraokeResultsPanel wiring
    // =========================================================================

    static void WireResultsPanel(KaraokeResultsPanel comp, GameObject go)
    {
        var so = new SerializedObject(comp);

        // Text fields
        SetTMPLocal(so, "finalScoreText", go, "FinalScoreText");
        SetTMPLocal(so, "coinsText",      go, "CoinsText");

        // Stars array (Image[] starImages)
        var starsRow = go.transform.Find("StarsRow");
        if (starsRow != null)
        {
            var starsProp = so.FindProperty("starImages");
            int count     = starsRow.childCount;
            starsProp.arraySize = count;
            for (int i = 0; i < count; i++)
                starsProp.GetArrayElementAtIndex(i).objectReferenceValue =
                    starsRow.GetChild(i).GetComponent<Image>();
        }
        else
        {
            Debug.LogWarning("[KaraokeSceneWirer] StarsRow not found under ResultsPanel.");
        }

        // Buttons
        SetCompLocal<Button>(so, "playAgainButton", go, "PlayAgainButton");
        SetCompLocal<Button>(so, "backToHubButton", go, "BackToHubButton");

        // CanvasGroup is on the ResultsPanel GO itself
        so.FindProperty("canvasGroup").objectReferenceValue = go.GetComponent<CanvasGroup>();

        so.ApplyModifiedProperties();
    }

    // =========================================================================
    //  Helpers
    // =========================================================================

    static T EnsureComponent<T>(GameObject go) where T : Component
    {
        var c = go.GetComponent<T>();
        return c != null ? c : go.AddComponent<T>();
    }

    /// Find a child of 'panel' at 'path' and assign its TextMeshProUGUI.
    static void SetTMP(SerializedObject so, string prop, GameObject panel, string path)
    {
        var t = panel.transform.Find(path);
        if (t == null) { Debug.LogWarning($"[KaraokeSceneWirer] Not found: {path}"); return; }
        so.FindProperty(prop).objectReferenceValue = t.GetComponent<TextMeshProUGUI>();
    }

    /// Find a child of 'panel' at 'path' and assign its Image.
    static void SetImg(SerializedObject so, string prop, GameObject panel, string path)
    {
        var t = panel.transform.Find(path);
        if (t == null) { Debug.LogWarning($"[KaraokeSceneWirer] Not found: {path}"); return; }
        so.FindProperty(prop).objectReferenceValue = t.GetComponent<Image>();
    }

    /// Find a child of 'panel' at 'path' and assign its RectTransform.
    static void SetRT(SerializedObject so, string prop, GameObject panel, string path)
    {
        var t = panel.transform.Find(path);
        if (t == null) { Debug.LogWarning($"[KaraokeSceneWirer] Not found: {path}"); return; }
        so.FindProperty(prop).objectReferenceValue = t.GetComponent<RectTransform>();
    }

    /// Find a child of 'panel' at 'path' and assign the given component type.
    static void SetComp<T>(SerializedObject so, string prop, GameObject panel,
                            string path) where T : Component
    {
        var t = panel.transform.Find(path);
        if (t == null) { Debug.LogWarning($"[KaraokeSceneWirer] Not found: {path}"); return; }
        so.FindProperty(prop).objectReferenceValue = t.GetComponent<T>();
    }

    /// Variant of SetTMP that searches direct children of 'go' by name (not deep path).
    static void SetTMPLocal(SerializedObject so, string prop, GameObject go, string childName)
    {
        var t = go.transform.Find(childName);
        if (t == null) { Debug.LogWarning($"[KaraokeSceneWirer] Local not found: {childName}"); return; }
        so.FindProperty(prop).objectReferenceValue = t.GetComponent<TextMeshProUGUI>();
    }

    /// Variant of SetComp that searches direct children of 'go' by name (not deep path).
    static void SetCompLocal<T>(SerializedObject so, string prop, GameObject go,
                                 string childName) where T : Component
    {
        var t = go.transform.Find(childName);
        if (t == null) { Debug.LogWarning($"[KaraokeSceneWirer] Local not found: {childName}"); return; }
        so.FindProperty(prop).objectReferenceValue = t.GetComponent<T>();
    }
}
