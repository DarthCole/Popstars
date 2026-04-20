using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KaraokeUI : MonoBehaviour
{
    [Header("Manager")]
    [SerializeField] private KaraokeManager manager;
    [SerializeField] private GameObject     songSelectPanel;
    [SerializeField] private GameObject     karaokePanel;

    [Header("Top Bar")]
    [SerializeField] private TextMeshProUGUI songTitleText;
    [SerializeField] private TextMeshProUGUI artistText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Button          backButton;

    [Header("Mic Indicator")]
    [SerializeField] private RectTransform micRing;
    [SerializeField] private Image         micRingImage;
    [SerializeField] private Image         micDot;
    [SerializeField] private Image         avatarImage;

    [Header("Lyrics")]
    [SerializeField] private TextMeshProUGUI prevLyricText;
    [SerializeField] private TextMeshProUGUI currentLyricText;
    [SerializeField] private TextMeshProUGUI nextLyricText;

    [Header("Pitch Bar")]
    [SerializeField] private Image           pitchBarFill;
    [SerializeField] private TextMeshProUGUI pitchNoteLabel;

    [Header("Progress Bar")]
    [SerializeField] private Image           progressBarFill;
    [SerializeField] private TextMeshProUGUI currentTimeLabel;
    [SerializeField] private TextMeshProUGUI totalTimeLabel;

    [Header("Waveform")]
    [SerializeField] private RectTransform[] waveformBars;

    [Header("Controls")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button recordButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private Image  recordButtonIcon;

    [Header("Results Panel")]
    [SerializeField] private KaraokeResultsPanel resultsPanel;

    private static readonly Color PurpleActive   = new Color(0.66f, 0.33f, 1f,  1f);
    private static readonly Color PurpleDim      = new Color(0.66f, 0.33f, 1f,  0.3f);
    private static readonly Color PitchGreen     = new Color(0.2f,  0.9f,  0.4f, 1f);
    private static readonly Color PitchYellow    = new Color(1f,    0.85f, 0.2f, 1f);
    private static readonly Color PitchRed       = new Color(1f,    0.3f,  0.3f, 1f);

    private bool   _isRecording   = false;
    private string _lastNoteLabel = "— — —";

    private void Start()
    {
        backButton.onClick.AddListener(OnBack);
        restartButton.onClick.AddListener(() => manager.Restart());
        skipButton.onClick.AddListener(OnSkip);
        recordButton.onClick.AddListener(OnRecordToggle);

        manager.OnStateChanged  += HandleStateChanged;
        manager.OnSongComplete  += HandleSongComplete;

        // Force filled image type at runtime — survives builder rebuilds
        if (progressBarFill != null)
        {
            progressBarFill.type        = Image.Type.Filled;
            progressBarFill.fillMethod  = Image.FillMethod.Horizontal;
            progressBarFill.fillOrigin  = (int)Image.OriginHorizontal.Left;
            progressBarFill.fillAmount  = 0f;
        }
        if (pitchBarFill != null)
        {
            pitchBarFill.type       = Image.Type.Filled;
            pitchBarFill.fillMethod = Image.FillMethod.Horizontal;
            pitchBarFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            pitchBarFill.fillAmount = 0f;
        }

        // Find avatarImage at runtime if not wired via Inspector
        if (avatarImage == null)
        {
            var micIndicator = GameObject.Find("MicIndicator");
            if (micIndicator != null)
            {
                var av = micIndicator.transform.Find("AvatarImage");
                if (av != null) avatarImage = av.GetComponent<Image>();
            }
        }

        // Populate static info
        if (manager.songData != null)
        {
            songTitleText.text  = manager.songData.songName.ToUpper();
            artistText.text     = manager.songData.artist.ToUpper();
            totalTimeLabel.text = FormatTime(manager.Duration);
        }

        if (resultsPanel != null)
            resultsPanel.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (manager.State == KaraokeManager.GameState.Playing ||
            manager.State == KaraokeManager.GameState.Paused)
        {
            UpdateScore();
            UpdateLyrics();
            UpdatePitchBar();
            UpdateProgress();
            UpdateMicIndicator();
            UpdateWaveform();
        }
    }

    // ── Score ─────────────────────────────────────────────────────────────────

    private void UpdateScore()
    {
        scoreText.text = Mathf.RoundToInt(manager.Score).ToString();
    }

    // ── Lyrics ────────────────────────────────────────────────────────────────

    private void UpdateLyrics()
    {
        // Previous line
        if (manager.PrevLyric.HasValue)
        {
            prevLyricText.text  = manager.PrevLyric.Value.text;
            prevLyricText.alpha = 0.35f;
        }
        else
        {
            prevLyricText.text = "";
        }

        // Current line with highlighted word
        if (manager.CurLyric.HasValue)
        {
            currentLyricText.text = HighlightWord(manager.CurLyric.Value.text, manager.LineProgress);
        }
        else
        {
            currentLyricText.text = "";
        }

        // Next line
        if (manager.NextLyric.HasValue)
        {
            nextLyricText.text  = manager.NextLyric.Value.text;
            nextLyricText.alpha = 0.55f;
        }
        else
        {
            nextLyricText.text = "";
        }
    }

    private string HighlightWord(string text, float progress)
    {
        if (string.IsNullOrEmpty(text)) return text;

        string[] words   = text.Split(' ');
        int      wordIdx = Mathf.Clamp(Mathf.FloorToInt(progress * words.Length), 0, words.Length - 1);

        words[wordIdx] = $"<color=#A855F7>{words[wordIdx]}</color>";
        return string.Join(" ", words);
    }

    // ── Pitch Bar ─────────────────────────────────────────────────────────────

    private void UpdatePitchBar()
    {
        float micHz    = manager.MicPitch;
        float targetHz = manager.CurLyric.HasValue ? manager.CurLyric.Value.targetPitch : 0f;

        // Always show last known note — only update label when mic is active
        pitchNoteLabel.text = _lastNoteLabel;

        if (micHz < 1f)
        {
            pitchBarFill.fillAmount = Mathf.MoveTowards(pitchBarFill.fillAmount, 0f, Time.deltaTime * 2f);
            return;
        }

        // Update and hold the detected note name
        string noteName  = HzToNoteName(micHz);
        _lastNoteLabel   = $"{noteName}  —  {Mathf.RoundToInt(micHz)} HZ";
        pitchNoteLabel.text = _lastNoteLabel;

        if (targetHz < 1f)
        {
            pitchBarFill.fillAmount = 0.5f;
            pitchBarFill.color      = PurpleDim;
            return;
        }

        float deviation = Mathf.Abs(micHz - targetHz);
        pitchBarFill.fillAmount = 1f - Mathf.Clamp01(deviation / 100f);

        if      (deviation <= 15f) pitchBarFill.color = PitchGreen;
        else if (deviation <= 60f) pitchBarFill.color = PitchYellow;
        else                       pitchBarFill.color = PitchRed;
    }

    // ── Progress ──────────────────────────────────────────────────────────────

    private void UpdateProgress()
    {
        // Self-heal: find ProgressFill at runtime if reference was lost after a rebuild
        if (progressBarFill == null)
        {
            var t = GameObject.Find("ProgressFill");
            if (t != null)
            {
                progressBarFill            = t.GetComponent<Image>();
                progressBarFill.type       = Image.Type.Filled;
                progressBarFill.fillMethod = Image.FillMethod.Horizontal;
                progressBarFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            }
        }
        if (progressBarFill == null) return;

        progressBarFill.fillAmount = manager.Progress;
        if (currentTimeLabel != null)
            currentTimeLabel.text  = FormatTime(manager.CurrentTime);
    }

    // ── Mic Ring ──────────────────────────────────────────────────────────────

    private void UpdateMicIndicator()
    {
        if (micRingImage == null || micDot == null) return;

        if (!_isRecording)
        {
            micRingImage.color = PurpleDim;
            micDot.color       = PurpleDim;
            return;
        }

        float pulse          = (Mathf.Sin(Time.time * 4f) + 1f) * 0.5f;
        micRingImage.color   = Color.Lerp(PurpleDim, PurpleActive, pulse);
        if (micRing != null)
            micRing.localScale = Vector3.one * (1f + pulse * 0.06f);
        micDot.color         = manager.MicActive ? PurpleActive : PurpleDim;
    }

    // ── Waveform ──────────────────────────────────────────────────────────────

    private void UpdateWaveform()
    {
        if (waveformBars == null || waveformBars.Length == 0) return;

        float speed     = manager.MicActive ? 8f : 3f;
        float maxHeight = manager.MicActive ? 20f : 6f;

        for (int i = 0; i < waveformBars.Length; i++)
        {
            float phase  = Time.time * speed + i * 0.7f;
            float height = (Mathf.Sin(phase) + 1f) * 0.5f * maxHeight + 3f;
            var   size   = waveformBars[i].sizeDelta;
            waveformBars[i].sizeDelta = new Vector2(size.x, height);
        }
    }

    // ── Buttons ───────────────────────────────────────────────────────────────

    private void OnRecordToggle()
    {
        if (manager.State == KaraokeManager.GameState.Idle)
        {
            manager.StartSong(manager.songData);
            _isRecording = true;
        }
        else if (manager.State == KaraokeManager.GameState.Playing)
        {
            manager.Pause();
        }
        else if (manager.State == KaraokeManager.GameState.Paused)
        {
            manager.Resume();
        }
    }

    private void OnSkip()
    {
        if (manager.songData == null) return;
        float skipTo = manager.CurrentTime + 10f;
        if (skipTo < manager.Duration)
            manager.GetComponent<AudioSource>().time = skipTo;
    }

    private void OnBack()
    {
        manager.Stop();
        BackgroundMusicManager.Instance?.Play();
        if (songSelectPanel != null) songSelectPanel.SetActive(true);
        if (karaokePanel    != null) karaokePanel.SetActive(false);
    }

    // ── Event Handlers ────────────────────────────────────────────────────────

    private void HandleStateChanged(KaraokeManager.GameState state)
    {
        _isRecording = (state == KaraokeManager.GameState.Playing);

        if (state == KaraokeManager.GameState.Idle)
            _isRecording = false;
    }

    private void HandleSongComplete(float score, int stars)
    {
        if (resultsPanel != null)
        {
            resultsPanel.gameObject.SetActive(true);
            resultsPanel.Show(score, stars, PitchScorer.StarCoins(stars));
        }
    }

    // ── Public API for SongSelectUI ───────────────────────────────────────────

    /// <summary>
    /// Called by SongSelectUI when the player picks a song.
    /// Sets up the display and arms the song — the Record button starts playback.
    /// </summary>
    public void LoadSong(SongData song)
    {
        // Store on manager so OnRecordToggle can call StartSong()
        manager.songData = song;

        // Update top-bar text immediately
        if (songTitleText  != null) songTitleText.text  = song.songName.ToUpper();
        if (artistText     != null) artistText.text     = song.artist.ToUpper();
        if (totalTimeLabel != null && song.backingTrack != null)
            totalTimeLabel.text = FormatTime(song.backingTrack.length);

        // Reset score / progress display
        if (scoreText        != null) scoreText.text        = "0";
        if (currentTimeLabel != null) currentTimeLabel.text = "0:00";
        if (progressBarFill  != null) progressBarFill.fillAmount = 0f;
        if (pitchBarFill     != null) pitchBarFill.fillAmount    = 0f;

        // Clear lyrics
        if (prevLyricText    != null) prevLyricText.text    = "";
        if (currentLyricText != null) currentLyricText.text = "";
        if (nextLyricText    != null) nextLyricText.text    = "";

        _lastNoteLabel = "\u2014 \u2014 \u2014";
        if (pitchNoteLabel != null) pitchNoteLabel.text = _lastNoteLabel;

        // Cover art — find avatarImage at runtime if Inspector wiring was lost
        if (avatarImage == null)
        {
            var mic = GameObject.Find("MicIndicator");
            if (mic != null)
            {
                var t = mic.transform.Find("AvatarImage");
                if (t != null) avatarImage = t.GetComponent<Image>();
            }
        }
        if (avatarImage != null)
        {
            if (song.coverArt != null)
            {
                avatarImage.sprite         = song.coverArt;
                avatarImage.color          = Color.white;
                avatarImage.preserveAspect = true;
                avatarImage.type           = Image.Type.Simple;
            }
            else
            {
                avatarImage.sprite = null;
                avatarImage.color  = new Color(0.12f, 0.05f, 0.27f, 1f);
            }
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string FormatTime(float seconds)
    {
        int m = Mathf.FloorToInt(seconds) / 60;
        int s = Mathf.FloorToInt(seconds) % 60;
        return $"{m}:{s:D2}";
    }

    private static string HzToNoteName(float hz)
    {
        if (hz < 20f) return "—";
        float semitones = 12f * Mathf.Log(hz / 440f, 2f);
        int   rounded   = Mathf.RoundToInt(semitones);

        string[] notes     = { "C","C#","D","D#","E","F","F#","G","G#","A","A#","B" };
        int      noteIndex = ((rounded + 9) % 12 + 12) % 12;
        int      octave    = 4 + Mathf.FloorToInt((rounded + 9) / 12f);
        return $"{notes[noteIndex]}{octave}";
    }
}
