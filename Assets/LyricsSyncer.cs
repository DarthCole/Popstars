using UnityEngine;
using System;

public class LyricsSyncer : MonoBehaviour
{
    public event Action<LyricLine> OnLyricChanged;

    private SongData _song;
    private int      _currentIndex = -1;

    public LyricLine? PrevLyric    => (_currentIndex > 0 && _song?.lines != null)
                                        ? (LyricLine?)_song.lines[_currentIndex - 1]
                                        : null;
    public LyricLine? CurrentLyric { get; private set; }
    public LyricLine? NextLyric    { get; private set; }
    public int        CurrentIndex => _currentIndex;

    /// <summary>0–1 progress through the current lyric line, used for word highlighting.</summary>
    public float LineProgress { get; private set; }

    public void Initialize(SongData song)
    {
        _song         = song;
        _currentIndex = -1;
        CurrentLyric  = null;
        NextLyric     = (song?.lines?.Length > 0) ? (LyricLine?)song.lines[0] : null;
        LineProgress  = 0f;
    }

    public void UpdateTime(float time)
    {
        if (_song == null || _song.lines == null) return;

        var lines    = _song.lines;
        int newIndex = -1;

        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].timestamp <= time) newIndex = i;
            else break;
        }

        if (newIndex != _currentIndex)
        {
            _currentIndex = newIndex;
            CurrentLyric  = newIndex >= 0 ? (LyricLine?)lines[newIndex] : null;
            NextLyric     = (newIndex + 1 < lines.Length) ? (LyricLine?)lines[newIndex + 1] : null;

            if (CurrentLyric.HasValue)
                OnLyricChanged?.Invoke(CurrentLyric.Value);
        }

        if (CurrentLyric.HasValue && _currentIndex >= 0)
        {
            float start    = CurrentLyric.Value.timestamp;
            float end      = (_currentIndex + 1 < lines.Length)
                             ? lines[_currentIndex + 1].timestamp
                             : start + 5f;
            float duration = Mathf.Max(0.01f, end - start);
            LineProgress   = Mathf.Clamp01((time - start) / duration);
        }
        else
        {
            LineProgress = 0f;
        }
    }
}
