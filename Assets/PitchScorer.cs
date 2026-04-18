using UnityEngine;
using System;

public class PitchScorer : MonoBehaviour
{
    private const float PERFECT_HZ    = 15f;
    private const float GOOD_HZ       = 35f;
    private const float ACCEPTABLE_HZ = 60f;

    private const float PERFECT_PTS    =  10f;
    private const float GOOD_PTS       =   6f;
    private const float ACCEPTABLE_PTS =   2f;
    private const float OFFKEY_PTS     =  -4f;

    private const int COMBO_TIER2 = 6;
    private const int COMBO_TIER3 = 16;

    public event Action<float, int> OnSongComplete; // totalScore, stars

    private MicPitchDetector _mic;
    private LyricsSyncer     _syncer;

    private float   _linePoints;
    private int     _lineFrames;
    private int     _voiceFrames;
    private float[] _lineScores  = new float[500];
    private int     _linesScored = 0;

    public float TotalScore  { get; private set; }
    public int   ComboFrames { get; private set; }
    public float Multiplier  => ComboFrames >= COMBO_TIER3 ? 2f
                              : ComboFrames >= COMBO_TIER2 ? 1.5f : 1f;

    public void Initialize(MicPitchDetector mic, LyricsSyncer syncer)
    {
        _mic    = mic;
        _syncer = syncer;
        ResetAll();
        _syncer.OnLyricChanged += _ => FinalizeCurrentLine();
    }

    private void Update()
    {
        if (_mic == null || _syncer == null) return;
        if (!_syncer.CurrentLyric.HasValue) return;

        float targetHz = _syncer.CurrentLyric.Value.targetPitch;
        float micHz    = _mic.CurrentPitch;

        _lineFrames++;

        if (micHz < 1f) return; // silence — no points, no penalty

        _voiceFrames++;
        float dev = Mathf.Abs(micHz - targetHz);
        float pts;

        if      (dev <= PERFECT_HZ)    { pts = PERFECT_PTS;    ComboFrames++; }
        else if (dev <= GOOD_HZ)       { pts = GOOD_PTS;       ComboFrames++; }
        else if (dev <= ACCEPTABLE_HZ) { pts = ACCEPTABLE_PTS; ComboFrames++; }
        else                           { pts = OFFKEY_PTS;     ComboFrames = 0; }

        _linePoints = Mathf.Max(0f, _linePoints + pts * Multiplier);
    }

    private void FinalizeCurrentLine()
    {
        if (_lineFrames == 0) return;

        float consistency = (float)_voiceFrames / _lineFrames;
        float lineScore   = Mathf.Clamp(_linePoints * consistency, 0f, 100f);

        if (_linesScored < _lineScores.Length)
            _lineScores[_linesScored] = lineScore;
        _linesScored++;

        float sum = 0f;
        int   cnt = Mathf.Min(_linesScored, _lineScores.Length);
        for (int i = 0; i < cnt; i++) sum += _lineScores[i];
        TotalScore = sum / cnt;

        ResetLine();
    }

    public void CompleteSong()
    {
        FinalizeCurrentLine();
        int stars = TotalScore >= 90f ? 3 : TotalScore >= 70f ? 2 : TotalScore >= 50f ? 1 : 0;
        OnSongComplete?.Invoke(TotalScore, stars);
    }

    public static int StarCoins(int stars) => stars switch { 3 => 500, 2 => 300, 1 => 100, _ => 0 };

    private void ResetLine()
    {
        _linePoints  = 0f;
        _lineFrames  = 0;
        _voiceFrames = 0;
        ComboFrames  = 0;
    }

    private void ResetAll()
    {
        TotalScore   = 0f;
        _linesScored = 0;
        ResetLine();
    }
}
