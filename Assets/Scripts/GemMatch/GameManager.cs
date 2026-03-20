// File: Assets/Scripts/GemMatch/GameManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameTimer gameTimer;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private GemInputHandler inputHandler;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI starCoinsEarnedText;

    private int lastEarnedCoins;

    private void Start()
    {
        gameOverPanel.SetActive(false);
        gameTimer.OnTimeUp += HandleGameOver;
    }

    private void HandleGameOver()
    {
        inputHandler.SetInteractable(false);

        int finalScore = scoreManager.GetScore();
        lastEarnedCoins = CalculateStarCoins(finalScore);

        finalScoreText.text = $"Final Score: {finalScore}";
        starCoinsEarnedText.text = $"StarCoins Earned: {lastEarnedCoins}";

        gameOverPanel.SetActive(true);

        Debug.Log($"Game Over! Score: {finalScore} | StarCoins: {lastEarnedCoins}");
    }

    private int CalculateStarCoins(int score)
    {
        if (score >= 2000) return 50;
        if (score >= 1000) return 30;
        if (score >= 500) return 15;
        return 5;
    }

    // Called by Play Again button
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Called by Back to Hub button
    public void GoToHub()
    {
        Debug.Log("Returning to Hub with " + lastEarnedCoins + " StarCoins");
        // When your team builds the hub, change this to load the hub scene
        // For now it reloads the current scene as a placeholder
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Called by Store button
    public void GoToStore()
    {
        Debug.Log("Opening Store with " + lastEarnedCoins + " StarCoins");
        // When your team builds the store, change this to load the store scene
        // For now it reloads the current scene as a placeholder
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        gameTimer.OnTimeUp -= HandleGameOver;
    }
}