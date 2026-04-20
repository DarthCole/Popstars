using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton — persists across scenes.
/// Tracks which songs the player owns (default or purchased).
/// Mirrors the pattern used by StageOwnershipManager.
///
/// Setup: Add this component to the same persistent GameObject as StageOwnershipManager.
/// Assign all SongData assets to the 'allSongs' array in the Inspector.
/// </summary>
public class SongOwnershipManager : MonoBehaviour
{
    public static SongOwnershipManager Instance { get; private set; }

    const string OWNED_KEY = "PopstarHub_OwnedSongs";

    [Tooltip("Drag every SongData asset here.")]
    public SongData[] allSongs;

    public event Action OnOwnershipChanged;

    readonly HashSet<int> _owned = new HashSet<int>();

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    // ── Queries ───────────────────────────────────────────────────────────────

    /// Returns true if the player can play this song (free/default, or purchased).
    public bool IsOwned(int songID)
    {
        if (allSongs != null)
            foreach (var s in allSongs)
                if (s != null && s.songID == songID && (s.isDefault || s.price <= 0))
                    return true;
        return _owned.Contains(songID);
    }

    // ── Actions ───────────────────────────────────────────────────────────────

    /// Try to purchase a song with the player's StarCoins.
    public bool TryPurchase(SongData song)
    {
        if (song == null) return false;
        if (IsOwned(song.songID)) return false;

        if (song.isDefault || song.price <= 0)
        {
            Unlock(song.songID);
            return true;
        }

        if (CoinManager.Instance == null) return false;
        if (!CoinManager.Instance.TrySpend(song.price)) return false;

        Unlock(song.songID);
        return true;
    }

    /// Directly unlock a song without spending coins (e.g. for testing).
    public void Unlock(int songID)
    {
        _owned.Add(songID);
        Save();
        OnOwnershipChanged?.Invoke();
    }

    // ── Persistence ───────────────────────────────────────────────────────────

    void Save()
    {
        PlayerPrefs.SetString(OWNED_KEY, string.Join(",", _owned));
        PlayerPrefs.Save();
    }

    void Load()
    {
        _owned.Clear();
        string raw = PlayerPrefs.GetString(OWNED_KEY, "");
        if (string.IsNullOrEmpty(raw)) return;
        foreach (var part in raw.Split(','))
            if (int.TryParse(part, out int id))
                _owned.Add(id);
    }
}
