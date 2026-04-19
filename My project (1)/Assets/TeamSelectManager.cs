using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class TeamSelectManager : MonoBehaviour
{
    [Header("References")]
    public Transform gridContent;
    public GameObject artistCardPrefab;
    public ArtistData[] allArtists;

    [Header("Selected Team Display")]
    public Image[] selectedPortraits;   // 3 slots
    public TMP_Text[] selectedNames;    // 3 slots
    public Button startBattleButton;

    // ── Runtime State ─────────────────────────
    private ArtistData[] _selectedTeam = new ArtistData[1];
    private int _selectedCount = 0;

    // Class colors
    private static readonly Color32 ColorRed = new Color32(220, 50, 50, 255);
    private static readonly Color32 ColorBlack = new Color32(40, 40, 40, 255);
    private static readonly Color32 ColorGreen = new Color32(50, 180, 80, 255);
    private static readonly Color32 ColorBlue = new Color32(50, 120, 220, 255);
    private static readonly Color32 ColorPurple = new Color32(140, 60, 200, 255);

    void Start()
    {
        if (startBattleButton != null)
            startBattleButton.interactable = false;

        PopulateGrid();
    }

    void PopulateGrid()
    {
        foreach (ArtistData artist in allArtists)
        {
            GameObject card = Instantiate(artistCardPrefab, gridContent);

            // Set name
            TMP_Text nameText = card.transform.Find("ArtistName").GetComponent<TMP_Text>();
            if (nameText != null) nameText.text = artist.artistName;

            // Set class name and color
            TMP_Text classText = card.transform.Find("ClassName").GetComponent<TMP_Text>();
            if (classText != null)
            {
                classText.text = GetClassName(artist.artistClass);
                classText.color = GetClassColor(artist.artistClass);
            }

            // Set portrait color as placeholder
            Image portrait = card.transform.Find("Portrait").GetComponent<Image>();
            if (portrait != null)
                portrait.color = GetClassColor(artist.artistClass);

            // Wire up click
            Button btn = card.GetComponent<Button>();
            if (btn == null) btn = card.AddComponent<Button>();
            ArtistData captured = artist;
            btn.onClick.AddListener(() => OnArtistSelected(captured, card));
        }
    }

    void OnArtistSelected(ArtistData artist, GameObject card)
    {
        if (_selectedCount >= 1) return;

        // Check not already selected
        foreach (var a in _selectedTeam)
            if (a == artist) return;

        _selectedTeam[_selectedCount] = artist;

        // Update slot UI
        if (selectedPortraits != null && _selectedCount < selectedPortraits.Length)
            selectedPortraits[_selectedCount].color = GetClassColor(artist.artistClass);

        if (selectedNames != null && _selectedCount < selectedNames.Length)
            selectedNames[_selectedCount].text = artist.artistName;

        // Highlight card
        Image bg = card.transform.Find("CardBackground").GetComponent<Image>();
        if (bg != null) bg.color = new Color32(255, 215, 0, 255); // Gold highlight

        _selectedCount++;

        if (_selectedCount == 1)
        {
            if (startBattleButton != null)
                startBattleButton.interactable = true;
        }
    }

    public void OnStartBattle()
    {
        // Save selected team to a static holder so CombatScene can read it
        TeamData.SelectedTeam = _selectedTeam;
        SceneManager.LoadScene("CombatScene");
    }

    Color32 GetClassColor(ArtistClass cls)
    {
        return cls switch
        {
            ArtistClass.Powerhouse => ColorRed,
            ArtistClass.Striker => ColorBlack,
            ArtistClass.Technician => ColorGreen,
            ArtistClass.Acrobat => ColorBlue,
            ArtistClass.Trickster => ColorPurple,
            _ => ColorBlack
        };
    }

    string GetClassName(ArtistClass cls)
    {
        return cls switch
        {
            ArtistClass.Powerhouse => "Vocalist",
            ArtistClass.Striker => "Heavyweight",
            ArtistClass.Technician => "Songwriter",
            ArtistClass.Acrobat => "Performer",
            ArtistClass.Trickster => "Trendsetter",
            _ => "Unknown"
        };
    }
}