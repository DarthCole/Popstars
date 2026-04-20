using UnityEngine;
using System.Runtime.InteropServices;

public class MicPitchDetector : MonoBehaviour
{
    private const int   SAMPLE_RATE          = 44100;
    private const int   BUFFER_SIZE          = 4096;
    private const int   MIN_PITCH            = 80;
    private const int   MAX_PITCH            = 1200;
    private const float CONFIDENCE_THRESHOLD = 0.45f;
    private const float NOISE_GATE           = 0.006f;
    private const float SMOOTHING            = 0.5f;

    private AudioClip _micClip;
    private float[]   _buffer = new float[BUFFER_SIZE];
    private float     _smoothedPitch;

    public float CurrentPitch   { get; private set; }
    public float CurrentVolume  { get; private set; }
    public bool  IsVoiceDetected => CurrentVolume >= NOISE_GATE;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] static extern void  MicBridge_Start();
    [DllImport("__Internal")] static extern float MicBridge_GetPitch();
    [DllImport("__Internal")] static extern float MicBridge_GetVolume();
    [DllImport("__Internal")] static extern int   MicBridge_IsReady();
    [DllImport("__Internal")] static extern void  MicBridge_Stop();
#endif

    private void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        MicBridge_Start();
        Debug.Log("[MicPitchDetector] WebGL mic bridge started.");
#else
        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("[MicPitchDetector] No microphone found.");
            return;
        }
        _micClip = Microphone.Start(null, true, 10, SAMPLE_RATE);
        Debug.Log($"[MicPitchDetector] Started: {Microphone.devices[0]}");
#endif
    }

    private void Update()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (MicBridge_IsReady() == 0) return;
        CurrentVolume = MicBridge_GetVolume();
        CurrentPitch  = MicBridge_GetPitch();
#else
        if (_micClip == null) return;

        int pos = Microphone.GetPosition(null) - BUFFER_SIZE;
        if (pos < 0) return;

        _micClip.GetData(_buffer, pos);
        CurrentVolume = GetRMS(_buffer);

        if (CurrentVolume < NOISE_GATE)
        {
            CurrentPitch   = 0f;
            _smoothedPitch = 0f;
            return;
        }

        float raw = AutoCorrelate(_buffer);
        if (raw < 1f)
        {
            CurrentPitch = 0f;
            return;
        }

        _smoothedPitch = _smoothedPitch * SMOOTHING + raw * (1f - SMOOTHING);
        CurrentPitch   = _smoothedPitch;
#endif
    }

    private float AutoCorrelate(float[] buf)
    {
        int n = buf.Length;

        float mean = 0f;
        for (int i = 0; i < n; i++) mean += buf[i];
        mean /= n;

        float variance = 0f;
        for (int i = 0; i < n; i++) variance += (buf[i] - mean) * (buf[i] - mean);
        variance /= n;

        if (variance < 0.00001f) return 0f;

        int minLag     = SAMPLE_RATE / MAX_PITCH;
        int maxLag     = Mathf.Min(SAMPLE_RATE / MIN_PITCH, n / 2);
        float bestCorr = -1f;
        int   bestLag  = 0;

        for (int lag = minLag; lag <= maxLag; lag++)
        {
            float corr  = 0f;
            int   count = n - lag;
            for (int i = 0; i < count; i++)
                corr += (buf[i] - mean) * (buf[i + lag] - mean);

            corr = corr / count / variance;

            if (corr > bestCorr)
            {
                bestCorr = corr;
                bestLag  = lag;
            }
        }

        if (bestCorr < CONFIDENCE_THRESHOLD || bestLag == 0) return 0f;
        return (float)SAMPLE_RATE / bestLag;
    }

    private float GetRMS(float[] buf)
    {
        float sum = 0f;
        for (int i = 0; i < buf.Length; i++) sum += buf[i] * buf[i];
        return Mathf.Sqrt(sum / buf.Length);
    }

    private void OnDestroy()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        MicBridge_Stop();
#else
        if (Microphone.IsRecording(null)) Microphone.End(null);
#endif
    }
}
