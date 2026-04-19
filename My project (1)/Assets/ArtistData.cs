using UnityEngine;

// ─────────────────────────────────────────────
//  Gem / Class Enums
// ─────────────────────────────────────────────

public enum GemColor
{
    Red,
    Black,
    Green,
    Blue,
    Purple
}

public enum ArtistClass
{
    Powerhouse,   // Vocalists  – Red
    Striker,      // Heavyweights – Black
    Technician,   // Songwriters  – Green
    Acrobat,      // Performers   – Blue
    Trickster     // Trendsetters – Purple
}

// ─────────────────────────────────────────────
//  ScriptableObject
//  Create via: Right-click → Create → ArtistRPG → Artist Data
// ─────────────────────────────────────────────

[CreateAssetMenu(fileName = "NewArtist", menuName = "ArtistRPG/Artist Data")]
public class ArtistData : ScriptableObject
{
    [Header("Identity")]
    public string artistName;
    public Sprite portrait;
    public ArtistClass artistClass;

    // Derived automatically — no need to set manually in the Inspector
    public GemColor FavoriteGem => ClassToGem(artistClass);

    [Header("Base Stats")]
    [Min(1)] public int maxHP = 100;
    [Min(1)] public int attackPower = 15;
    [Range(0f, 1f)] public float critChance = 0.10f;   // 0–1  (10% default)

    [Header("Signature Move")]
    public string signatureMoveName = "Signature Move";

    // ── Helpers ──────────────────────────────

    /// <summary>Maps an ArtistClass to its matching GemColor.</summary>
    public static GemColor ClassToGem(ArtistClass cls)
    {
        return cls switch
        {
            ArtistClass.Powerhouse => GemColor.Red,
            ArtistClass.Striker => GemColor.Black,
            ArtistClass.Technician => GemColor.Green,
            ArtistClass.Acrobat => GemColor.Blue,
            ArtistClass.Trickster => GemColor.Purple,
            _ => GemColor.Red
        };
    }

    /// <summary>
    /// Returns the signature-bar charge multiplier for this class.
    /// Strikers (Heavyweights) charge 20 % faster.
    /// </summary>
    public float SignatureChargeMultiplier =>
        artistClass == ArtistClass.Striker ? 1.20f : 1.00f;
}