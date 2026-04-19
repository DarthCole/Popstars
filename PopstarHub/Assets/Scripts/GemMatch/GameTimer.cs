// File: Assets/Scripts/GemMatch/GameTimer.cs
using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    [SerializeField] private float gameDuration = 60f;
    [SerializeField] private TextMeshProUGUI timerText;

    private float timeRemaining;
    private bool isRunning;
    private bool isEnabled = true;

    // Event that other scripts can listen to when time runs out
    public System.Action OnTimeUp;

    private void Awake()
    {
        EnsureTimerText();
    }

    private void Start()
    {
        // Don't auto-start; wait for GameManager to configure and call StartTimer()
    }

    /// <summary>
    /// Configure and start the timer for a time-limited level.
    /// </summary>
    public void ConfigureAndStart(float duration)
    {
        gameDuration = duration;
        timeRemaining = gameDuration;
        isEnabled = true;
        isRunning = true;
        if (timerText != null)
            timerText.gameObject.SetActive(true);
        UpdateTimerDisplay();
    }

    /// <summary>
    /// Disable the timer for move-limited levels.
    /// </summary>
    public void SetDisabled()
    {
        isEnabled = false;
        isRunning = false;
        if (timerText != null)
            timerText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Stop the timer (e.g. when level ends).
    /// </summary>
    public void StopTimer()
    {
        isRunning = false;
    }

    private void Update()
    {
        if (!isRunning || !isEnabled) return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            isRunning = false;
            UpdateTimerDisplay();
            OnTimeUp?.Invoke();
            return;
        }

        UpdateTimerDisplay();
    }

    public bool IsRunning()
    {
        return isRunning;
    }

    public float GetTimeRemaining()
    {
        return timeRemaining;
    }

    private void UpdateTimerDisplay()
    {
        if (timerText == null) return;

        int seconds = Mathf.CeilToInt(timeRemaining);
        timerText.text = $"Time: {seconds}s";

        // Turn red in the last 10 seconds
        if (timeRemaining <= 10f)
            timerText.color = Color.red;
        else
            timerText.color = Color.white;
    }

    private void EnsureTimerText()
    {
        Canvas canvas = UIHierarchyBuilder.GetOrCreateCanvas("UICanvas");
        timerText = UIHierarchyBuilder.GetOrCreateTMPText(
            canvas.transform,
            "TimerText",
            "Time: --",
            new Vector2(0f, 460f),
            new Vector2(340f, 80f),
            42,
            TMPro.TextAlignmentOptions.Center,
            Color.white);

        // Start hidden; will be shown by ConfigureAndStart()
        timerText.gameObject.SetActive(false);
    }
}