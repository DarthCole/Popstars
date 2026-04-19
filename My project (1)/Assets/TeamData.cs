// Holds the selected team between scenes
public static class TeamData
{
    public static ArtistData[] SelectedTeam { get; set; } = new ArtistData[3];
    public static int ActiveArtistIndex { get; set; } = 0;

    public static ArtistData ActiveArtist =>
        SelectedTeam != null && SelectedTeam.Length > ActiveArtistIndex
            ? SelectedTeam[ActiveArtistIndex]
            : null;
}