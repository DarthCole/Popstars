// File: Assets/Scripts/GemMatch/SoundManager.cs
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private AudioSource sfxSource;
    private AudioSource musicSource;

    // Generated sound clips
    private AudioClip swapClip;
    private AudioClip matchClip;
    private AudioClip comboClip;
    private AudioClip gameOverClip;
    private AudioClip iceBreakClip;
    private AudioClip chainBreakClip;
    private AudioClip shuffleClip;
    private AudioClip achievementClip;
    private AudioClip levelCompleteClip;

    private float musicVolume = 0.3f;

    private void Awake()
    {
        sfxSource = gameObject.AddComponent<AudioSource>();
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = musicVolume;
        GenerateSounds();
    }

    public void PlaySwap() => sfxSource.PlayOneShot(swapClip, 0.5f);
    public void PlayMatch() => sfxSource.PlayOneShot(matchClip, 0.7f);
    public void PlayCombo() => sfxSource.PlayOneShot(comboClip, 0.8f);
    public void PlayGameOver() => sfxSource.PlayOneShot(gameOverClip, 1f);
    public void PlayIceBreak() => sfxSource.PlayOneShot(iceBreakClip, 0.6f);
    public void PlayChainBreak() => sfxSource.PlayOneShot(chainBreakClip, 0.6f);
    public void PlayShuffle() => sfxSource.PlayOneShot(shuffleClip, 0.5f);
    public void PlayAchievement() => sfxSource.PlayOneShot(achievementClip, 0.8f);
    public void PlayLevelComplete() => sfxSource.PlayOneShot(levelCompleteClip, 0.9f);

    /// <summary>
    /// Play level-appropriate background music.
    /// </summary>
    public void PlayLevelMusic(int levelNumber)
    {
        AudioClip clip = GenerateLevelMusic(levelNumber);
        if (clip != null)
        {
            musicSource.clip = clip;
            musicSource.volume = musicVolume;
            musicSource.Play();
        }
    }

    /// <summary>
    /// Stop background music.
    /// </summary>
    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
    }

    private void GenerateSounds()
    {
        swapClip = CreateTone(440f, 0.08f, ToneType.Sine);
        matchClip = CreateChime(new float[] { 523f, 659f, 784f }, 0.15f);
        comboClip = CreateChime(new float[] { 523f, 659f, 784f, 1047f }, 0.12f);
        gameOverClip = CreateChime(new float[] { 784f, 659f, 523f, 392f }, 0.25f);
        iceBreakClip = CreateCrackle(0.15f);
        chainBreakClip = CreateChime(new float[] { 330f, 440f }, 0.1f);
        shuffleClip = CreateSweep(200f, 800f, 0.4f);
        achievementClip = CreateChime(new float[] { 523f, 659f, 784f, 1047f, 1319f }, 0.12f);
        levelCompleteClip = CreateChime(new float[] { 392f, 523f, 659f, 784f, 1047f }, 0.2f);
    }

    private enum ToneType { Sine, Square, Triangle }

    private AudioClip CreateTone(float frequency, float duration, ToneType type)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = 1f - ((float)i / sampleCount);

            switch (type)
            {
                case ToneType.Sine:
                    samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope;
                    break;
                case ToneType.Square:
                    samples[i] = (Mathf.Sin(2f * Mathf.PI * frequency * t) > 0 ? 1f : -1f) * envelope * 0.3f;
                    break;
                case ToneType.Triangle:
                    float phase = (t * frequency) % 1f;
                    samples[i] = (Mathf.Abs(phase * 4f - 2f) - 1f) * envelope * 0.5f;
                    break;
            }
        }

        AudioClip clip = AudioClip.Create("tone", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

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

    /// <summary>
    /// Crackling/breaking sound for ice.
    /// </summary>
    private AudioClip CreateCrackle(float duration)
    {
        int sampleRate = 44100;
        int count = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[count];

        for (int i = 0; i < count; i++)
        {
            float t = (float)i / count;
            float envelope = 1f - t;
            // Noise with high-pass character
            float noise = (Random.value * 2f - 1f) * envelope * 0.4f;
            float tone = Mathf.Sin(2f * Mathf.PI * 2000f * (float)i / sampleRate) * envelope * 0.2f;
            samples[i] = noise + tone;
        }

        AudioClip clip = AudioClip.Create("crackle", count, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    /// <summary>
    /// Sweep sound for shuffle.
    /// </summary>
    private AudioClip CreateSweep(float startFreq, float endFreq, float duration)
    {
        int sampleRate = 44100;
        int count = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[count];

        for (int i = 0; i < count; i++)
        {
            float t = (float)i / count;
            float freq = Mathf.Lerp(startFreq, endFreq, t);
            float envelope = Mathf.Sin(t * Mathf.PI); // bell curve
            samples[i] = Mathf.Sin(2f * Mathf.PI * freq * (float)i / sampleRate) * envelope * 0.3f;
        }

        AudioClip clip = AudioClip.Create("sweep", count, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    /// <summary>
    /// Generate a simple procedural background music loop for a given level tier.
    /// </summary>
    private AudioClip GenerateLevelMusic(int levelNumber)
    {
        int sampleRate = 44100;

        // Define scales and tempos for each tier
        float[] scale;
        float bpm;
        ToneType tone;

        if (levelNumber <= 3)
        {
            // Tier 1: C major pentatonic, chill
            scale = new float[] { 261.6f, 293.7f, 329.6f, 392.0f, 440.0f, 523.3f };
            bpm = 100f;
            tone = ToneType.Sine;
        }
        else if (levelNumber <= 6)
        {
            // Tier 2: A minor pentatonic, mid-energy
            scale = new float[] { 220.0f, 261.6f, 293.7f, 329.6f, 392.0f, 440.0f };
            bpm = 120f;
            tone = ToneType.Sine;
        }
        else if (levelNumber <= 9)
        {
            // Tier 3: E minor, building tension
            scale = new float[] { 164.8f, 196.0f, 220.0f, 246.9f, 293.7f, 329.6f, 392.0f };
            bpm = 140f;
            tone = ToneType.Triangle;
        }
        else
        {
            // Tier 4: D minor, intense
            scale = new float[] { 146.8f, 174.6f, 196.0f, 220.0f, 261.6f, 293.7f, 349.2f };
            bpm = 160f;
            tone = ToneType.Triangle;
        }

        float beatDuration = 60f / bpm;
        int beatsPerBar = 4;
        int bars = 4;
        int totalBeats = bars * beatsPerBar;
        float totalDuration = totalBeats * beatDuration;
        int totalSamples = Mathf.CeilToInt(sampleRate * totalDuration);
        float[] samples = new float[totalSamples];

        // Seed for reproducible melody per level
        Random.State savedState = Random.state;
        Random.InitState(levelNumber * 42);

        // Generate melody
        for (int beat = 0; beat < totalBeats; beat++)
        {
            float noteFreq = scale[Random.Range(0, scale.Length)];
            float noteDuration = beatDuration * 0.8f;
            int noteStart = Mathf.FloorToInt(beat * beatDuration * sampleRate);
            int noteLength = Mathf.CeilToInt(noteDuration * sampleRate);

            for (int i = 0; i < noteLength && (noteStart + i) < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = 1f - ((float)i / noteLength);
                envelope *= envelope; // soft decay
                float value;

                switch (tone)
                {
                    case ToneType.Triangle:
                        float phase = (t * noteFreq) % 1f;
                        value = (Mathf.Abs(phase * 4f - 2f) - 1f) * 0.15f;
                        break;
                    default:
                        value = Mathf.Sin(2f * Mathf.PI * noteFreq * t) * 0.15f;
                        break;
                }

                samples[noteStart + i] += value * envelope;
            }

            // Bass note (root of scale, one octave below, on beats 1 and 3)
            if (beat % 2 == 0)
            {
                float bassFreq = scale[0] * 0.5f;
                float bassDuration = beatDuration * 0.6f;
                int bassLength = Mathf.CeilToInt(bassDuration * sampleRate);

                for (int i = 0; i < bassLength && (noteStart + i) < totalSamples; i++)
                {
                    float t = (float)i / sampleRate;
                    float envelope = 1f - ((float)i / bassLength);
                    samples[noteStart + i] += Mathf.Sin(2f * Mathf.PI * bassFreq * t) * 0.1f * envelope;
                }
            }
        }

        Random.state = savedState;

        AudioClip clip = AudioClip.Create($"LevelMusic_{levelNumber}", totalSamples, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}