// File: Assets/Scripts/GemMatch/MoveCounter.cs
using UnityEngine;
using TMPro;

/// <summary>
/// Tracks remaining moves for move-limited levels.
/// Inactive (hidden) for time-limited levels.
/// </summary>
public class MoveCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI movesText;

    private int movesRemaining;
    private bool isActive;

    /// <summary>
    /// Fired when the player runs out of moves.
    /// </summary>
    public System.Action OnMovesExhausted;

    private void Awake()
    {
        EnsureMovesText();
    }

    /// <summary>
    /// Call at the start of a move-limited level to initialize.
    /// </summary>
    public void Initialize(int totalMoves)
    {
        movesRemaining = totalMoves;
        isActive = true;
        UpdateDisplay();

        if (movesText != null)
            movesText.gameObject.SetActive(true);
    }

    /// <summary>
    /// Call to disable the move counter (for time-limited levels).
    /// </summary>
    public void SetInactive()
    {
        isActive = false;
        if (movesText != null)
            movesText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Consumes one move after a successful swap. Returns true if moves remain.
    /// </summary>
    public bool UseMove()
    {
        if (!isActive) return true;

        movesRemaining--;
        if (movesRemaining < 0) movesRemaining = 0;
        UpdateDisplay();

        if (movesRemaining <= 0)
        {
            OnMovesExhausted?.Invoke();
            return false;
        }

        return true;
    }

    public int GetMovesRemaining()
    {
        return movesRemaining;
    }

    public bool IsActive()
    {
        return isActive;
    }

    private void UpdateDisplay()
    {
        if (movesText == null) return;

        movesText.text = $"Moves: {movesRemaining}";

        // Turn red when low on moves
        if (movesRemaining <= 3)
            movesText.color = new Color(1f, 0.3f, 0.3f, 1f);
        else
            movesText.color = Color.white;
    }

    private void EnsureMovesText()
    {
        Canvas canvas = UIHierarchyBuilder.GetOrCreateCanvas("UICanvas");
        movesText = UIHierarchyBuilder.GetOrCreateTMPText(
            canvas.transform,
            "MovesText",
            "Moves: --",
            new UnityEngine.Vector2(0f, 460f),
            new UnityEngine.Vector2(340f, 80f),
            42,
            TMPro.TextAlignmentOptions.Center,
            Color.white);

        // Start hidden; will be activated by Initialize()
        movesText.gameObject.SetActive(false);
    }
}
