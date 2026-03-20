// File: Assets/Scripts/GemMatch/GameTimer.cs
using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    [SerializeField] private float gameDuration = 60f;
    [SerializeField] private TextMeshProUGUI timerText;

    private float timeRemaining;
    private bool isRunning;

    // Event that other scripts can listen to when time runs out
    public System.Action OnTimeUp;

    private void Start()
    {
        timeRemaining = gameDuration;
        isRunning = true;
        UpdateTimerDisplay();
    }

    private void Update()
    {
        if (!isRunning) return;

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

    private void UpdateTimerDisplay()
    {
        if (timerText == null) return;

        int seconds = Mathf.CeilToInt(timeRemaining);
        timerText.text = $"Time: {seconds}s";

        // Turn red in the last 10 seconds
        if (timeRemaining <= 10f)
            timerText.color = Color.red;
    }
}