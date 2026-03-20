// File: Assets/Scripts/GemMatch/SoundManager.cs
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private AudioSource audioSource;

    // Generated sound clips
    private AudioClip swapClip;
    private AudioClip matchClip;
    private AudioClip comboClip;
    private AudioClip gameOverClip;

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        GenerateSounds();
    }

    public void PlaySwap()
    {
        audioSource.PlayOneShot(swapClip, 0.5f);
    }

    public void PlayMatch()
    {
        audioSource.PlayOneShot(matchClip, 0.7f);
    }

    public void PlayCombo()
    {
        audioSource.PlayOneShot(comboClip, 0.8f);
    }

    public void PlayGameOver()
    {
        audioSource.PlayOneShot(gameOverClip, 1f);
    }

    private void GenerateSounds()
    {
        // All sounds are generated with code — no audio files needed
        swapClip = CreateTone(440f, 0.08f, ToneType.Sine);
        matchClip = CreateChime(new float[] { 523f, 659f, 784f }, 0.15f);
        comboClip = CreateChime(new float[] { 523f, 659f, 784f, 1047f }, 0.12f);
        gameOverClip = CreateChime(new float[] { 784f, 659f, 523f, 392f }, 0.25f);
    }

    private enum ToneType { Sine, Square }

    private AudioClip CreateTone(float frequency, float duration, ToneType type)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = 1f - ((float)i / sampleCount); // Fade out

            if (type == ToneType.Sine)
                samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope;
            else
                samples[i] = (Mathf.Sin(2f * Mathf.PI * frequency * t) > 0 ? 1f : -1f) * envelope * 0.3f;
        }

        AudioClip clip = AudioClip.Create("tone", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    // Plays multiple notes in sequence for a chime effect
    private AudioClip CreateChime(float[] frequencies, float noteDuration)
    {
        int sampleRate = 44100;
        int samplesPerNote = Mathf.CeilToInt(sampleRate * noteDuration);
        int totalSamples = samplesPerNote * frequencies.Length;
        float[] samples = new float[totalSamples];

        for (int n = 0; n < frequencies.Length; n++)
        {
            for (int i = 0; i < samplesPerNote; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = 1f - ((float)i / samplesPerNote);
                int index = n * samplesPerNote + i;
                samples[index] = Mathf.Sin(2f * Mathf.PI * frequencies[n] * t) * envelope * 0.5f;
            }
        }

        AudioClip clip = AudioClip.Create("chime", totalSamples, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}