// File: Assets/Scripts/GemMatch/ObjectiveTracker.cs
using UnityEngine;
using TMPro;

/// <summary>
/// Displays objective progress during gameplay and fires events on completion.
/// </summary>
public class ObjectiveTracker : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI objectiveText;

    private LevelObjective currentObjective;
    private bool objectiveComplete;

    /// <summary>
    /// Fired when the objective is completed.
    /// </summary>
    public System.Action OnObjectiveComplete;

    private void Awake()
    {
        EnsureUI();
    }

    /// <summary>
    /// Initialize with a level's objective. Pass null for pure-score levels.
    /// </summary>
    public void Initialize(LevelObjective objective)
    {
        if (objective == null || objective.objectiveType == ObjectiveType.Score)
        {
            currentObjective = null;
            objectiveComplete = true; // Score-only levels don't need objective tracking
            SetVisible(false);
            return;
        }

        currentObjective = objective.Clone();
        objectiveComplete = false;
        SetVisible(true);
        UpdateDisplay();
    }

    /// <summary>
    /// Record a gem being cleared.
    /// </summary>
    public void OnGemCleared(Gem.GemType gemType)
    {
        if (currentObjective == null || objectiveComplete) return;

        if (currentObjective.objectiveType == ObjectiveType.ClearColor &&
            gemType == currentObjective.targetGemType)
        {
            currentObjective.RecordProgress(1);
            CheckCompletion();
        }

        UpdateDisplay();
    }

    /// <summary>
    /// Record an ice block being broken.
    /// </summary>
    public void OnIceBroken()
    {
        if (currentObjective == null || objectiveComplete) return;

        if (currentObjective.objectiveType == ObjectiveType.BreakIce)
        {
            currentObjective.RecordProgress(1);
            CheckCompletion();
        }

        UpdateDisplay();
    }

    /// <summary>
    /// Record a stone block being destroyed.
    /// </summary>
    public void OnStoneDestroyed()
    {
        if (currentObjective == null || objectiveComplete) return;

        if (currentObjective.objectiveType == ObjectiveType.ClearStone)
        {
            currentObjective.RecordProgress(1);
            CheckCompletion();
        }

        UpdateDisplay();
    }

    /// <summary>
    /// Record a star gem being collected (reached bottom row).
    /// </summary>
    public void OnStarCollected()
    {
        if (currentObjective == null || objectiveComplete) return;

        if (currentObjective.objectiveType == ObjectiveType.CollectStars)
        {
            currentObjective.RecordProgress(1);
            CheckCompletion();
        }

        UpdateDisplay();
    }

    /// <summary>
    /// Returns true if the objective is satisfied (or if there is no special objective).
    /// </summary>
    public bool IsObjectiveMet()
    {
        return objectiveComplete;
    }

    public void SetVisible(bool visible)
    {
        if (objectiveText != null)
            objectiveText.gameObject.SetActive(visible);
    }

    private void CheckCompletion()
    {
        if (currentObjective != null && currentObjective.IsComplete() && !objectiveComplete)
        {
            objectiveComplete = true;
            Debug.Log($"Objective completed: {currentObjective.GetBriefDescription()}");
            OnObjectiveComplete?.Invoke();
        }
    }

    private void UpdateDisplay()
    {
        if (objectiveText == null || currentObjective == null) return;

        objectiveText.text = currentObjective.GetDisplayText();

        if (objectiveComplete)
            objectiveText.color = new Color(0.3f, 1f, 0.5f, 1f); // green
        else
            objectiveText.color = new Color(1f, 0.85f, 0.5f, 1f); // warm yellow
    }

    private void EnsureUI()
    {
        Canvas canvas = UIHierarchyBuilder.GetOrCreateCanvas("UICanvas");
        objectiveText = UIHierarchyBuilder.GetOrCreateTMPText(
            canvas.transform,
            "ObjectiveTrackerText",
            "",
            new Vector2(700f, 460f),
            new Vector2(380f, 80f),
            32,
            TMPro.TextAlignmentOptions.Right,
            new Color(1f, 0.85f, 0.5f, 1f));
        objectiveText.gameObject.SetActive(false);
    }
}
