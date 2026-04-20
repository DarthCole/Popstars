using UnityEngine;

[System.Serializable]
public struct LyricLine
{
    public float timestamp;
    public string text;
    public float targetPitch;
}

[CreateAssetMenu(menuName = "Karaoke/Song Data", fileName = "NewSong")]
public class SongData : ScriptableObject
{
    [Header("Song Info")]
    public string songName;
    public string artist;
    public int BPM;

    [Header("Audio")]
    public AudioClip backingTrack;

    [Header("Art")]
    public Sprite coverArt;

    [Header("Shop")]
    public int  songID;
    public int  price;
    public bool isDefault;

    [Header("Lyrics")]
    public LyricLine[] lines;
}
