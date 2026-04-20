using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton — persists across scenes.
/// Tracks which stage IDs the player owns and which one is currently equipped.
///
/// Setup: Add this component to a persistent GameObject in your first loaded scene.
/// Assign all StageData assets to the 'allStages' array in the Inspector.
/// </summary>
public class StageOwnershipManager : MonoBehaviour
{
    public static StageOwnershipManager Instance { get; private set; }

    const string OWNED_KEY    = "PopstarHub_OwnedStages";
    const string EQUIPPED_KEY = "PopstarHub_EquippedStage";

    [Tooltip("Drag every StageData asset here so the manager can find them at runtime.")]
    public StageData[] allStages;

    public event Action OnOwnershipChanged;
    public event Action OnEquipChanged;

    readonly HashSet<int> _owned = new HashSet<int>();
    int _equippedID = -1;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    // ── Queries ───────────────────────────────────────────────────────────────

    /// Returns true if the player owns this stage (default or purchased).
    public bool IsOwned(int stageID)
    {
        if (allStages != null)
            foreach (var s in allStages)
                if (s != null && s.stageID == stageID && (s.isDefault || s.price <= 0))
                    return true;
        return _owned.Contains(stageID);
    }

    /// Returns true if this stage is currently equipped.
    public bool IsEquipped(int stageID) => _equippedID == stageID;

    /// Returns the currently equipped StageData, or the first default stage as fallback.
    public StageData GetEquippedStage()
    {
        if (allStages == null || allStages.Length == 0) return null;
        foreach (var s in allStages)
            if (s != null && s.stageID == _equippedID) return s;
        // Fallback: default stage
        foreach (var s in allStages)
            if (s != null && s.isDefault) return s;
        return allStages[0];
    }

    // ── Actions ───────────────────────────────────────────────────────────────

    /// Try to purchase a stage with the player's StarCoins.
    /// Returns true on success. Automatically equips on first purchase.
    public bool TryPurchase(StageData stage)
    {
        if (stage == null) return false;
        if (IsOwned(stage.stageID)) return false;

        if (stage.isDefault || stage.price <= 0)
        {
            Unlock(stage.stageID);
            return true;
        }

        if (CoinManager.Instance == null) return false;
        if (!CoinManager.Instance.TrySpend(stage.price)) return false;

        Unlock(stage.stageID);
        return true;
    }

    /// Directly unlock a stage without spending coins.
    public void Unlock(int stageID)
    {
        _owned.Add(stageID);
        Save();
        OnOwnershipChanged?.Invoke();
    }

    /// Set the active stage for the next Stage Scene visit.
    public void EquipStage(int stageID)
    {
        if (!IsOwned(stageID)) return;
        _equippedID = stageID;
        PlayerPrefs.SetInt(EQUIPPED_KEY, stageID);
        PlayerPrefs.Save();
        OnEquipChanged?.Invoke();
    }

    // ── Persistence ───────────────────────────────────────────────────────────
    void Load()
    {
        _owned.Clear();
        string raw = PlayerPrefs.GetString(OWNED_KEY, "");
        if (!string.IsNullOrEmpty(raw))
            foreach (var part in raw.Split(','))
                if (int.TryParse(part.Trim(), out int id))
                    _owned.Add(id);

        // Equip the saved stage, defaulting to the first isDefault stage
        int defaultID = -1;
        if (allStages != null)
            foreach (var s in allStages)
                if (s != null && s.isDefault) { defaultID = s.stageID; break; }

        _equippedID = PlayerPrefs.GetInt(EQUIPPED_KEY, defaultID);
    }

    void Save()
    {
        PlayerPrefs.SetString(OWNED_KEY, string.Join(",", _owned));
        PlayerPrefs.Save();
    }

    [ContextMenu("Reset Owned Stages")]
    public void ResetOwnership()
    {
        _owned.Clear();
        PlayerPrefs.DeleteKey(OWNED_KEY);
        PlayerPrefs.DeleteKey(EQUIPPED_KEY);
        Load();
        OnOwnershipChanged?.Invoke();
        OnEquipChanged?.Invoke();
    }
}
