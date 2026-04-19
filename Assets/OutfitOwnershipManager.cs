using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton — persists across scenes.
/// Tracks which outfit IDs the player owns using PlayerPrefs.
/// Outfit IDs matching isDefault=true or price=0 are always owned.
/// </summary>
public class OutfitOwnershipManager : MonoBehaviour
{
    public static OutfitOwnershipManager Instance { get; private set; }

    const string PREFS_KEY = "PopstarHub_OwnedOutfits";

    public event Action OnOwnershipChanged;

    readonly HashSet<int> _owned = new HashSet<int>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    /// <summary>Returns true if the outfit ID is owned.</summary>
    public bool IsOwned(int outfitID) => _owned.Contains(outfitID);

    /// <summary>
    /// Attempt to purchase an outfit using CoinManager.
    /// Returns true on success, false if insufficient funds or already owned.
    /// </summary>
    public bool TryPurchase(OutfitData outfit)
    {
        if (outfit == null) return false;
        if (IsOwned(outfit.outfitID)) return false;

        // Free outfits (default or price 0) unlock immediately
        if (outfit.isDefault || outfit.price <= 0)
        {
            Unlock(outfit.outfitID);
            return true;
        }

        if (CoinManager.Instance == null) return false;
        if (!CoinManager.Instance.TrySpend(outfit.price)) return false;

        Unlock(outfit.outfitID);
        return true;
    }

    /// <summary>Directly unlock an outfit ID without spending coins.</summary>
    public void Unlock(int outfitID)
    {
        _owned.Add(outfitID);
        Save();
        OnOwnershipChanged?.Invoke();
    }

    void Load()
    {
        _owned.Clear();
        string raw = PlayerPrefs.GetString(PREFS_KEY, "");
        if (string.IsNullOrEmpty(raw)) return;
        foreach (var part in raw.Split(','))
            if (int.TryParse(part.Trim(), out int id))
                _owned.Add(id);
    }

    void Save()
    {
        PlayerPrefs.SetString(PREFS_KEY, string.Join(",", _owned));
        PlayerPrefs.Save();
    }

    /// <summary>Wipe all owned outfits (testing).</summary>
    [ContextMenu("Reset Owned Outfits")]
    public void ResetOwnership()
    {
        _owned.Clear();
        Save();
        OnOwnershipChanged?.Invoke();
    }
}
