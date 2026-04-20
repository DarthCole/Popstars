using UnityEngine;

/// <summary>
/// Plays background music continuously across all scenes.
/// Attach to a GameObject in your very first loaded scene (MainMenuScene).
/// Assign a music AudioClip in the Inspector.
/// </summary>
public class BackgroundMusicManager : MonoBehaviour
{
    public static BackgroundMusicManager Instance { get; private set; }

    [Header("Music")]
    public AudioClip musicClip;

    [Range(0f, 1f)]
    public float volume = 0.4f;

    AudioSource _source;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _source = gameObject.AddComponent<AudioSource>();
        _source.clip        = musicClip;
        _source.loop        = true;
        _source.playOnAwake = false;
        _source.volume      = volume;

        if (musicClip != null)
            _source.Play();
    }

    /// Call this to stop the music (e.g. during karaoke playback)
    public void Stop()  => _source.Stop();

    /// Call this to resume after stopping
    public void Play()  { if (!_source.isPlaying) _source.Play(); }

    /// Adjust volume at runtime (0–1)
    public void SetVolume(float v) => _source.volume = Mathf.Clamp01(v);
}
