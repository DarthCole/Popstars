using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class SongDataImporter : EditorWindow
{
    [MenuItem("Karaoke/Import All Songs from JSON")]
    public static void ImportAll()
    {
        string jsonFolder = Path.Combine(Application.dataPath, "SongData");

        if (!Directory.Exists(jsonFolder))
        {
            Debug.LogError($"[SongDataImporter] Folder not found: {jsonFolder}\nRun Tools/convert_song_data.py first.");
            return;
        }

        string[] jsonFiles = Directory.GetFiles(jsonFolder, "*.json");

        if (jsonFiles.Length == 0)
        {
            Debug.LogWarning("[SongDataImporter] No JSON files found in Assets/SongData/");
            return;
        }

        int imported = 0;

        foreach (string jsonFile in jsonFiles)
        {
            string raw  = File.ReadAllText(jsonFile);
            SongJson src = JsonUtility.FromJson<SongJson>(raw);

            if (src == null || src.lines == null)
            {
                Debug.LogWarning($"[SongDataImporter] Could not parse: {jsonFile}");
                continue;
            }

            string assetName = Path.GetFileNameWithoutExtension(jsonFile);
            string assetPath = $"Assets/SongData/{assetName}.asset";

            SongData asset = AssetDatabase.LoadAssetAtPath<SongData>(assetPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<SongData>();
                AssetDatabase.CreateAsset(asset, assetPath);
            }

            asset.songName    = src.songName;
            asset.artist      = src.artist;
            asset.lines       = new LyricLine[src.lines.Count];

            for (int i = 0; i < src.lines.Count; i++)
            {
                asset.lines[i] = new LyricLine
                {
                    timestamp   = src.lines[i].timestamp,
                    text        = src.lines[i].text,
                    targetPitch = src.lines[i].targetPitch,
                };
            }

            EditorUtility.SetDirty(asset);
            imported++;
            Debug.Log($"[SongDataImporter] {src.songName} — {src.lines.Count} lines");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[SongDataImporter] Done. {imported} song(s) imported.");
    }

    // ── JSON shapes (mirrors the Python output) ───────────────────────────────

    [System.Serializable]
    private class SongJson
    {
        public string          songName;
        public string          artist;
        public List<LineJson>  lines;
    }

    [System.Serializable]
    private class LineJson
    {
        public float  timestamp;
        public string text;
        public float  targetPitch;
    }
}
