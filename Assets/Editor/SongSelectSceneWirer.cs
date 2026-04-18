using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Wires the Song Select panel in one click.
/// Run AFTER:  PopstarHub ▶ Build Song Select UI
///             Karaoke    ▶ Wire Scene               (so KaraokeManager/KaraokeUI already exist)
///
/// Menu: Karaoke ▶ Wire Song Select
/// </summary>
public class SongSelectSceneWirer : EditorWindow
{
    [MenuItem("Karaoke/Wire Song Select")]
    static void Wire()
    {
        // ── 0. Find required scene objects ───────────────────────────────────
        var selectPanel = GameObject.Find("SongSelectPanel");
        if (selectPanel == null)
        {
            Debug.LogError("[SongSelectSceneWirer] 'SongSelectPanel' not found. " +
                           "Run  PopstarHub ▶ Build Song Select UI  first.");
            return;
        }

        var karaokePanel = GameObject.Find("KaraokePanel");
        if (karaokePanel == null)
        {
            Debug.LogError("[SongSelectSceneWirer] 'KaraokePanel' not found. " +
                           "Run  Karaoke ▶ Build Full Karaoke UI  first.");
            return;
        }

        var mgrGO = GameObject.Find("KaraokeManager");
        if (mgrGO == null)
        {
            Debug.LogError("[SongSelectSceneWirer] 'KaraokeManager' not found. " +
                           "Run  Karaoke ▶ Wire Scene  first.");
            return;
        }

        var manager   = mgrGO.GetComponent<KaraokeManager>();
        var karaokeUI = mgrGO.GetComponent<KaraokeUI>();

        if (manager == null || karaokeUI == null)
        {
            Debug.LogError("[SongSelectSceneWirer] KaraokeManager or KaraokeUI component missing " +
                           "on the KaraokeManager GameObject. Run  Karaoke ▶ Wire Scene  again.");
            return;
        }

        // ── 1. Add / find SongSelectUI on the panel ───────────────────────────
        var ui = selectPanel.GetComponent<SongSelectUI>();
        if (ui == null) ui = selectPanel.AddComponent<SongSelectUI>();

        var so = new SerializedObject(ui);

        // ── 2. Scene object references ────────────────────────────────────────
        so.FindProperty("manager").objectReferenceValue    = manager;
        so.FindProperty("karaokeUI").objectReferenceValue  = karaokeUI;
        so.FindProperty("karaokePanel").objectReferenceValue = karaokePanel;

        // Back button
        var backBtnT = selectPanel.transform.Find("BackButton");
        if (backBtnT != null)
            so.FindProperty("backButton").objectReferenceValue =
                backBtnT.GetComponent<Button>();
        else
            Debug.LogWarning("[SongSelectSceneWirer] BackButton not found under SongSelectPanel.");

        // ── 3. Song cards ─────────────────────────────────────────────────────
        var gridT = selectPanel.transform.Find("SongGrid");
        if (gridT != null)
        {
            var cardsProp = so.FindProperty("songCards");
            int count     = gridT.childCount;
            cardsProp.arraySize = count;
            for (int i = 0; i < count; i++)
                cardsProp.GetArrayElementAtIndex(i).objectReferenceValue =
                    gridT.GetChild(i).gameObject;
        }
        else
        {
            Debug.LogWarning("[SongSelectSceneWirer] SongGrid not found under SongSelectPanel.");
        }

        // ── 4. SongData assets (find all in project, sort alphabetically) ─────
        var guids = AssetDatabase.FindAssets("t:SongData");
        if (guids.Length == 0)
        {
            Debug.LogWarning("[SongSelectSceneWirer] No SongData assets found. " +
                             "Run  Karaoke ▶ Import All Songs from JSON  first.");
        }
        else
        {
            var songsProp = so.FindProperty("songs");
            songsProp.arraySize = guids.Length;

            // Sort by asset name for a consistent order
            System.Array.Sort(guids, (a, b) =>
                string.Compare(AssetDatabase.GUIDToAssetPath(a),
                               AssetDatabase.GUIDToAssetPath(b),
                               System.StringComparison.OrdinalIgnoreCase));

            for (int i = 0; i < guids.Length; i++)
            {
                string path  = AssetDatabase.GUIDToAssetPath(guids[i]);
                var    asset = AssetDatabase.LoadAssetAtPath<SongData>(path);
                songsProp.GetArrayElementAtIndex(i).objectReferenceValue = asset;
                Debug.Log($"[SongSelectSceneWirer] Slot {i}: {asset?.songName}");
            }
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(selectPanel);

        Debug.Log("[SongSelectSceneWirer] Done. " +
                  "Assign AudioClips to each SongData asset's 'Backing Track' field, then Press Play.");
    }
}
