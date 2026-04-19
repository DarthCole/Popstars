using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Runtime controller for the Song Select screen.
/// Attach to the SongSelectPanel GameObject.
/// All references are wired automatically by Karaoke ▶ Wire Song Select.
/// </summary>
public class SongSelectUI : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private KaraokeManager manager;
    [SerializeField] private KaraokeUI      karaokeUI;
    [SerializeField] private GameObject     karaokePanel;

    [Header("Songs (order matches card order)")]
    [SerializeField] private SongData[] songs;

    [Header("Card Root GameObjects (SongCard_0 … SongCard_5)")]
    [SerializeField] private GameObject[] songCards;

    [Header("Navigation")]
    [SerializeField] private Button backButton;

    private void Start()
    {
        // Hide karaoke gameplay panel until a song is chosen
        if (karaokePanel != null)
            karaokePanel.SetActive(false);

        // Populate each card with real song data
        for (int i = 0; i < songCards.Length; i++)
        {
            if (songCards[i] == null) continue;

            if (i >= songs.Length || songs[i] == null)
            {
                songCards[i].SetActive(false);
                continue;
            }

            var song     = songs[i];
            var cardRoot = songCards[i];

            // Find the inner panel (CardInner) then look for labels inside it
            var inner      = cardRoot.transform.Find("CardInner");
            var searchRoot = inner != null ? inner : cardRoot.transform;

            var nameLabel   = searchRoot.Find("SongName")?.GetComponent<TextMeshProUGUI>();
            var artistLabel = searchRoot.Find("Artist")?.GetComponent<TextMeshProUGUI>();
            var audioStatus = searchRoot.Find("AudioStatus")?.GetComponent<TextMeshProUGUI>();
            var coverImg    = searchRoot.Find("CoverArt")?.GetComponent<Image>();

            if (nameLabel   != null) nameLabel.text   = song.songName.ToUpper();
            if (artistLabel != null) artistLabel.text = song.artist;

            if (coverImg != null)
            {
                if (song.coverArt != null)
                {
                    coverImg.sprite = song.coverArt;
                    coverImg.color  = Color.white;
                    coverImg.preserveAspect = true;
                }
            }

            // Show a warning badge if no backing track is linked yet
            if (audioStatus != null)
                audioStatus.text = song.backingTrack != null ? "" : "NO AUDIO";

            // Dim the card if no audio is assigned (still clickable — will log an error)
            var cardImg = cardRoot.GetComponent<Image>();
            if (cardImg != null && song.backingTrack == null)
                cardImg.color = new Color(cardImg.color.r, cardImg.color.g,
                                          cardImg.color.b, cardImg.color.a * 0.5f);

            // Wire click — capture index to avoid closure issue
            int idx = i;
            var btn = cardRoot.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(() => SelectSong(songs[idx]));
        }

        if (backButton != null)
            backButton.onClick.AddListener(OnBack);
    }

    private void SelectSong(SongData song)
    {
        if (song == null || song.backingTrack == null)
        {
            Debug.LogWarning($"[SongSelectUI] '{song?.songName}' has no AudioClip assigned to Backing Track.");
            return;
        }

        gameObject.SetActive(false);

        if (karaokePanel != null)
            karaokePanel.SetActive(true);

        karaokeUI.LoadSong(song);
    }

    private void OnBack()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenuScene");
    }
}
