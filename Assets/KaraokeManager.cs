using UnityEngine;
using System;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(MicPitchDetector))]
[RequireComponent(typeof(LyricsSyncer))]
[RequireComponent(typeof(PitchScorer))]
public class KaraokeManager : MonoBehaviour
{
    public enum GameState { Idle, Playing, Paused, Complete }

    [Header("Song")]
    [SerializeField] public SongData songData;

    private AudioSource      _audio;
    private MicPitchDetector _mic;
    private LyricsSyncer     _syncer;
    private PitchScorer      _scorer;

    public GameState State      { get; private set; } = GameState.Idle;

    public float      CurrentTime  => _audio != null ? _audio.time : 0f;
    public float      Duration     => songData?.backingTrack != null ? songData.backingTrack.length : 0f;
    public float      Progress     => Duration > 0f ? Mathf.Clamp01(CurrentTime / Duration) : 0f;
    public float      Score        => _scorer != null ? _scorer.TotalScore : 0f;
    public int        Combo        => _scorer != null ? _scorer.ComboFrames : 0;
    public float      Multiplier   => _scorer != null ? _scorer.Multiplier : 1f;
    public float      MicPitch     => _mic    != null ? _mic.CurrentPitch : 0f;
    public bool       MicActive    => _mic    != null && _mic.IsVoiceDetected;
    public LyricLine? PrevLyric    => _syncer?.PrevLyric;
    public LyricLine? CurLyric     => _syncer?.CurrentLyric;
    public LyricLine? NextLyric    => _syncer?.NextLyric;
    public float      LineProgress => _syncer != null ? _syncer.LineProgress : 0f;

    public event Action<GameState> OnStateChanged;
    public event Action<float, int> OnSongComplete; // score, stars

    private void Awake()
    {
        _audio  = GetComponent<AudioSource>();
        _mic    = GetComponent<MicPitchDetector>();
        _syncer = GetComponent<LyricsSyncer>();
        _scorer = GetComponent<PitchScorer>();

        _scorer.OnSongComplete += (score, stars) =>
        {
            SetState(GameState.Complete);
            OnSongComplete?.Invoke(score, stars);
        };
    }

    public void StartSong(SongData data)
    {
        if (data == null || data.backingTrack == null)
        {
            Debug.LogError("[KaraokeManager] SongData or backingTrack is missing.");
            return;
        }

        songData        = data;
        _audio.clip     = data.backingTrack;
        _audio.volume   = 1f;

        _syncer.Initialize(data);
        _scorer.Initialize(_mic, _syncer);

        _audio.Play();
        BackgroundMusicManager.Instance?.Stop();
        SetState(GameState.Playing);
    }

    public void Pause()
    {
        if (State != GameState.Playing) return;
        _audio.Pause();
        SetState(GameState.Paused);
    }

    public void Resume()
    {
        if (State != GameState.Paused) return;
        _audio.UnPause();
        SetState(GameState.Playing);
    }

    public void Stop()
    {
        _audio.Stop();
        SetState(GameState.Idle);
    }

    public void Restart()
    {
        Stop();
        if (songData != null)
            StartSong(songData);
    }

    private void Update()
    {
        if (State != GameState.Playing) return;

        _syncer.UpdateTime(CurrentTime);

        if (!_audio.isPlaying && CurrentTime >= Duration - 0.1f)
            _scorer.CompleteSong();
    }

    private void SetState(GameState s)
    {
        if (State == s) return;
        State = s;
        OnStateChanged?.Invoke(s);
    }
}
