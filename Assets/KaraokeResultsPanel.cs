using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class KaraokeResultsPanel : MonoBehaviour
{
    [Header("Display")]
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private Image[]         starImages;       // array of 3
    [SerializeField] private Color           starActiveColor   = new Color(1f, 0.85f, 0.1f);
    [SerializeField] private Color           starInactiveColor = new Color(0.3f, 0.3f, 0.3f, 0.4f);

    [Header("Buttons")]
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button backToHubButton;

    [Header("Panel")]
    [SerializeField] private CanvasGroup canvasGroup;

    private int _finalScore;
    private int _stars;
    private int _coins;

    private void Start()
    {
        playAgainButton.onClick.AddListener(OnPlayAgain);
        backToHubButton.onClick.AddListener(OnBackToHub);

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    public void Show(float score, int stars, int coins)
    {
        _finalScore = Mathf.RoundToInt(score);
        _stars      = Mathf.Clamp(stars, 0, 3);
        _coins      = coins;

        gameObject.SetActive(true);
        StartCoroutine(AnimateIn());
    }

    private IEnumerator AnimateIn()
    {
        // Fade panel in
        float elapsed = 0f;
        while (elapsed < 0.4f)
        {
            elapsed += Time.deltaTime;
            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Clamp01(elapsed / 0.4f);
            yield return null;
        }

        yield return StartCoroutine(AnimateScore());
        yield return StartCoroutine(AnimateStars());
        yield return StartCoroutine(AnimateCoins());
    }

    private IEnumerator AnimateScore()
    {
        float elapsed  = 0f;
        float duration = 0.8f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            int displayed = Mathf.RoundToInt(Mathf.Lerp(0, _finalScore, elapsed / duration));
            finalScoreText.text = displayed.ToString();
            yield return null;
        }
        finalScoreText.text = _finalScore.ToString();
    }

    private IEnumerator AnimateStars()
    {
        yield return new WaitForSeconds(0.2f);

        // Dim all stars first
        foreach (var img in starImages)
            img.color = starInactiveColor;

        // Pop in earned stars one by one
        for (int i = 0; i < _stars; i++)
        {
            yield return StartCoroutine(PopStar(starImages[i]));
            yield return new WaitForSeconds(0.15f);
        }
    }

    private IEnumerator PopStar(Image star)
    {
        star.color = starActiveColor;
        float elapsed  = 0f;
        float duration = 0.25f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t     = elapsed / duration;
            float scale = Mathf.Sin(t * Mathf.PI) * 0.4f + 1f; // 1.0 → 1.4 → 1.0
            star.transform.localScale = Vector3.one * scale;
            yield return null;
        }

        star.transform.localScale = Vector3.one;
    }

    private IEnumerator AnimateCoins()
    {
        yield return new WaitForSeconds(0.3f);

        float elapsed  = 0f;
        float duration = 0.6f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            int displayed = Mathf.RoundToInt(Mathf.Lerp(0, _coins, elapsed / duration));
            coinsText.text = $"+{displayed} StarCoins";
            yield return null;
        }
        coinsText.text = $"+{_coins} StarCoins";
    }

    private void OnPlayAgain()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    private void OnBackToHub()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Hub");
    }
}
