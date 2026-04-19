using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ResultManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text resultText;
    public TMP_Text subtitleText;
    public Image background;
    public Button playAgainButton;
    public Button mainMenuButton;

    // Colors
    private static readonly Color32 VictoryColor = new Color32(255, 215, 0, 255); // Gold
    private static readonly Color32 DefeatColor = new Color32(220, 50, 50, 255); // Red
    private static readonly Color32 WinBG = new Color32(10, 40, 10, 255); // Dark green
    private static readonly Color32 LoseBG = new Color32(40, 10, 10, 255); // Dark red

    void Start()
    {
        bool playerWon = ResultData.PlayerWon;

        // Set result text
        if (resultText != null)
        {
            resultText.text = playerWon ? "VICTORY!" : "DEFEAT!";
            resultText.color = playerWon ? VictoryColor : DefeatColor;
        }

        // Set subtitle
        if (subtitleText != null)
        {
            subtitleText.text = playerWon
                ? "You crushed the competition!"
                : "Better luck next time...";
        }

        // Set background color
        if (background != null)
            background.color = playerWon ? WinBG : LoseBG;

        // Wire buttons
        if (playAgainButton != null)
            playAgainButton.onClick.AddListener(OnPlayAgain);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenu);
    }

    void OnPlayAgain()
    {
        SceneManager.LoadScene("TeamSelect");
    }

    void OnMainMenu()
    {
        SceneManager.LoadScene("TeamSelect");
    }
}