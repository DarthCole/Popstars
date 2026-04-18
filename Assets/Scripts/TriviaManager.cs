using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TriviaManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject startPanel;
    public GameObject quizPanel;
    public GameObject resultPanel;

    [Header("Quiz UI")]
    public TMP_Text questionText;
    public TMP_Text[] answerTexts;
    public UnityEngine.UI.Button[] answerButtons;
    public TMP_Text scoreText;
    public TMP_Text streakText;
    public TMP_Text progressText;
    public TMP_Text feedbackText;
    public UnityEngine.UI.Image timerFill;

    [Header("Result UI")]
    public TMP_Text finalScoreText;
    public TMP_Text finalRankText;

    [Header("Settings")]
    public float timePerQuestion = 15f;
    public int questionsPerRound = 10;

    private List<TriviaQuestion> questions = new List<TriviaQuestion>();
    private TriviaQuestion currentQuestion;
    private int currentQuestionIndex = 0;
    private int score = 0;
    private int streak = 0;
    private float timer;
    private bool answered = false;

    void Start()
    {
        LoadQuestions();
        ShuffleQuestions();
        SetupButtonListeners();

        startPanel.SetActive(true);
        quizPanel.SetActive(false);
        resultPanel.SetActive(false);
    }

    void Update()
    {
        if (!quizPanel.activeSelf || answered) return;

        timer -= Time.deltaTime;
        timerFill.fillAmount = timer / timePerQuestion;

        if (timer <= 0f)
        {
            StartCoroutine(HandleTimeout());
        }
    }

    public void StartGame()
    {
        score = 0;
        streak = 0;
        currentQuestionIndex = 0;

        ShuffleQuestions();

        startPanel.SetActive(false);
        resultPanel.SetActive(false);
        quizPanel.SetActive(true);

        ShowNextQuestion();
    }

    void LoadQuestions()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("popstar_trivia_questions");
        string wrappedJson = "{ \"questions\": " + jsonFile.text + "}";
        TriviaQuestionList loaded = JsonUtility.FromJson<TriviaQuestionList>(wrappedJson);
        questions = loaded.questions;
    }

    void ShuffleQuestions()
    {
        for (int i = 0; i < questions.Count; i++)
        {
            TriviaQuestion temp = questions[i];
            int randomIndex = UnityEngine.Random.Range(i, questions.Count);
            questions[i] = questions[randomIndex];
            questions[randomIndex] = temp;
        }
    }

    void SetupButtonListeners()
    {
        for (int i = 0; i < answerButtons.Length; i++)
        {
            int index = i;
            answerButtons[i].onClick.AddListener(() => SelectAnswer(index));
        }
    }

    void ShowNextQuestion()
    {
        if (currentQuestionIndex >= questionsPerRound || currentQuestionIndex >= questions.Count)
        {
            EndQuiz();
            return;
        }

        answered = false;
        feedbackText.text = "";
        timer = timePerQuestion;
        timerFill.fillAmount = 1f;

        currentQuestion = questions[currentQuestionIndex];
        questionText.text = currentQuestion.question;
        progressText.text = "Question " + (currentQuestionIndex + 1) + " / " + questionsPerRound;

        for (int i = 0; i < answerTexts.Length; i++)
        {
            answerTexts[i].text = currentQuestion.options[i];
            answerButtons[i].interactable = true;
        }

        UpdateScoreUI();
    }

    void SelectAnswer(int selectedIndex)
    {
        if (answered) return;
        answered = true;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerButtons[i].interactable = false;
        }

        if (selectedIndex == currentQuestion.correctIndex)
        {
            score += 100;
            streak++;

            if (streak >= 3)
            {
                score += 50;
            }

            feedbackText.text = "The crowd loves that!\n" + currentQuestion.explanation;
        }
        else
        {
            streak = 0;
            feedbackText.text = "That note slipped!\n" + currentQuestion.explanation;
        }

        UpdateScoreUI();
        StartCoroutine(NextQuestionDelay());
    }

    IEnumerator HandleTimeout()
    {
        answered = true;
        streak = 0;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerButtons[i].interactable = false;
        }

        feedbackText.text = "Time's up!\n" + currentQuestion.explanation;
        UpdateScoreUI();

        yield return new WaitForSeconds(2f);

        currentQuestionIndex++;
        ShowNextQuestion();
    }

    IEnumerator NextQuestionDelay()
    {
        yield return new WaitForSeconds(2f);
        currentQuestionIndex++;
        ShowNextQuestion();
    }

    void UpdateScoreUI()
    {
        scoreText.text = "Score: " + score;
        streakText.text = "Streak: " + streak;
    }

    void EndQuiz()
    {
        quizPanel.SetActive(false);
        resultPanel.SetActive(true);

        finalScoreText.text = "Final Score: " + score;
        finalRankText.text = "Rank: " + GetRank();
    }

    string GetRank()
    {
        if (score >= 1200) return "Legend";
        if (score >= 900) return "Headliner";
        if (score >= 600) return "Rising Star";
        return "Backup Singer";
    }
}