using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TriviaManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject playerSetupPanel;
    public GameObject questionCountPanel;
    public GameObject gamePanel;
    public GameObject answerRevealPanel;
    public GameObject resultsPanel;

    [Header("Player Setup")]
    public Transform playerListParent;
    public GameObject playerInputPrefab;
    public Button addPlayerButton;
    public Button startGameButton;

    [Header("Question Count")]
    public TMP_Text questionCountText;
    public Slider questionCountSlider;
    public Button beginButton;

    [Header("Game Screen")]
    public TMP_Text questionLabel;
    public TMP_Text questionNumberText;
    public Button revealAnswerButton;

    [Header("Answer Reveal")]
    public TMP_Text revealQuestionText;
    public TMP_Text correctAnswerText;
    public Transform playerCheckParent;
    public GameObject playerCheckPrefab;
    public Button nextQuestionButton;
    public TMP_Text scoreboardText;

    [Header("Results")]
    public Transform resultsListParent;
    public GameObject resultEntryPrefab;
    public TMP_Text winnerText;
    public Button playAgainButton;

    [Header("Audio")]
    public AudioClip drumrollClip;
    public AudioClip correctSFX;
    public AudioClip revealSFX;
    private AudioSource audioSource;

    private List<string> playerNames = new List<string>();
    private List<int> playerScores = new List<int>();
    private List<TriviaQuestion> allQuestions;
    private List<TriviaQuestion> gameQuestions;
    private int currentQuestionIndex = 0;
    private int totalQuestions = 10;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        allQuestions = TriviaQuestionBank.GetAllQuestions();
        ShowPanel(playerSetupPanel);

        questionCountSlider.minValue = 5;
        questionCountSlider.maxValue = 20;
        questionCountSlider.value = 10;
        questionCountSlider.wholeNumbers = true;
        questionCountSlider.onValueChanged.AddListener(OnSliderChanged);

        addPlayerButton.onClick.AddListener(AddPlayerInput);
        startGameButton.onClick.AddListener(GoToQuestionCount);
        beginButton.onClick.AddListener(StartGame);
        revealAnswerButton.onClick.AddListener(RevealAnswer);
        nextQuestionButton.onClick.AddListener(OnNextQuestion);
        playAgainButton.onClick.AddListener(PlayAgain);
    }

    private void ShowPanel(GameObject panel)
    {
        playerSetupPanel.SetActive(false);
        questionCountPanel.SetActive(false);
        gamePanel.SetActive(false);
        answerRevealPanel.SetActive(false);
        resultsPanel.SetActive(false);
        panel.SetActive(true);
    }

    public void AddPlayerInput()
    {
        Instantiate(playerInputPrefab, playerListParent);
    }

    private void OnSliderChanged(float value)
    {
        totalQuestions = Mathf.RoundToInt(value);
        questionCountText.text = totalQuestions.ToString();
    }

    public void GoToQuestionCount()
    {
        playerNames.Clear();
        playerScores.Clear();

        foreach (Transform child in playerListParent)
        {
            TMP_InputField input = child.GetComponentInChildren<TMP_InputField>();
            if (input != null && input.text.Trim() != "")
            {
                playerNames.Add(input.text.Trim());
                playerScores.Add(0);
            }
        }

        if (playerNames.Count < 2)
        {
            Debug.Log("Need at least 2 players");
            return;
        }

        questionCountText.text = totalQuestions.ToString();
        ShowPanel(questionCountPanel);
    }

    public void StartGame()
    {
        totalQuestions = Mathf.RoundToInt(questionCountSlider.value);
        gameQuestions = GetRandomQuestions(totalQuestions);
        currentQuestionIndex = 0;

        for (int i = 0; i < playerScores.Count; i++)
            playerScores[i] = 0;

        ShowQuestion();
    }

    private List<TriviaQuestion> GetRandomQuestions(int count)
    {
        List<TriviaQuestion> shuffled = new List<TriviaQuestion>(allQuestions);
        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var temp = shuffled[i];
            shuffled[i] = shuffled[j];
            shuffled[j] = temp;
        }
        return shuffled.GetRange(0, Mathf.Min(count, shuffled.Count));
    }

    private void ShowQuestion()
    {
        if (currentQuestionIndex >= gameQuestions.Count)
        {
            ShowResults();
            return;
        }

        TriviaQuestion q = gameQuestions[currentQuestionIndex];
        questionLabel.text = q.question;
        questionNumberText.text = "Question " + (currentQuestionIndex + 1) + " of " + gameQuestions.Count;
        ShowPanel(gamePanel);
    }

    public void RevealAnswer()
    {
        if (revealSFX != null)
            audioSource.PlayOneShot(revealSFX);

        TriviaQuestion q = gameQuestions[currentQuestionIndex];
        revealQuestionText.text = q.question;
        correctAnswerText.text = "Answer: " + q.answers[q.correctIndex];

        foreach (Transform child in playerCheckParent)
            Destroy(child.gameObject);

        for (int i = 0; i < playerNames.Count; i++)
        {
            GameObject entry = Instantiate(playerCheckPrefab, playerCheckParent);
            TMP_Text nameText = entry.GetComponentInChildren<TMP_Text>();
            Toggle toggle = entry.GetComponentInChildren<Toggle>();

            nameText.text = playerNames[i];
            int playerIndex = i;
            toggle.isOn = false;
            toggle.onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                {
                    playerScores[playerIndex]++;
                    if (correctSFX != null)
                        audioSource.PlayOneShot(correctSFX);
                }
                else
                    playerScores[playerIndex] = Mathf.Max(0, playerScores[playerIndex] - 1);

                UpdateScoreboard();
            });
        }

        UpdateScoreboard();
        ShowPanel(answerRevealPanel);
    }

    private void UpdateScoreboard()
    {
        string sb = "";
        for (int i = 0; i < playerNames.Count; i++)
        {
            sb += playerNames[i] + ": " + playerScores[i] + " pts";
            if (i < playerNames.Count - 1) sb += "  |  ";
        }
        if (scoreboardText != null)
            scoreboardText.text = sb;
    }

    public void OnNextQuestion()
    {
        currentQuestionIndex++;
        ShowQuestion();
    }

    private void ShowResults()
    {
        if (drumrollClip != null)
            audioSource.PlayOneShot(drumrollClip);

        foreach (Transform child in resultsListParent)
            Destroy(child.gameObject);

        int highestScore = 0;
        string winner = "";

        for (int i = 0; i < playerNames.Count; i++)
        {
            if (playerScores[i] > highestScore)
            {
                highestScore = playerScores[i];
                winner = playerNames[i];
            }
        }

        List<int> sortedIndices = new List<int>();
        for (int i = 0; i < playerNames.Count; i++)
            sortedIndices.Add(i);
        sortedIndices.Sort((a, b) => playerScores[b].CompareTo(playerScores[a]));

        foreach (int i in sortedIndices)
        {
            GameObject entry = Instantiate(resultEntryPrefab, resultsListParent);
            TMP_Text text = entry.GetComponentInChildren<TMP_Text>();
            text.text = playerNames[i] + " - " + playerScores[i] + " pts";
        }

        winnerText.text = winner + " wins!";
        ShowPanel(resultsPanel);
    }

    public void PlayAgain()
    {
        foreach (Transform child in playerListParent)
            Destroy(child.gameObject);

        ShowPanel(playerSetupPanel);
    }
}