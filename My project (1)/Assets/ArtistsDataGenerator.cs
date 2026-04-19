using UnityEngine;
using UnityEditor;

public class ArtistDataGenerator
{
    [MenuItem("ArtistRPG/Generate All Artists")]
    public static void GenerateArtists()
    {
        string folder = "Assets/Artists";

        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets", "Artists");

        var artists = new[]
        {
            // ── Vocalists (Red / Powerhouse) ──
            ("Adele-icious",           ArtistClass.Powerhouse, 130, 18, 0.10f, "Hello From the Other Side"),
            ("Witney Tooting",         ArtistClass.Powerhouse, 140, 20, 0.12f, "I Will Always Toot You"),
            ("Temz McGee",             ArtistClass.Powerhouse, 110, 16, 0.14f, "Free McGee"),
            ("Freddie Mercurial",      ArtistClass.Powerhouse, 125, 22, 0.15f, "Bohemian Wrap-sody"),

            // ── Heavyweights (Black / Striker) ──
            ("Pop Foggy",              ArtistClass.Striker,    120, 20, 0.15f, "Woo Smash"),
            ("Travis Splat",           ArtistClass.Striker,    115, 21, 0.13f, "Highest in the Room Temperature"),
            ("Central Pee",            ArtistClass.Striker,    110, 19, 0.14f, "Loading... Please Wait"),
            ("Burna Boi",              ArtistClass.Striker,    125, 20, 0.12f, "Ye Olde Banger"),

            // ── Songwriters (Green / Technician) ──
            ("M&Nem",                  ArtistClass.Technician, 120, 24, 0.18f, "Rap Nerd"),
            ("Jay-Zzz",                ArtistClass.Technician, 130, 22, 0.16f, "Empire Nap of Mind"),
            ("Dave From Accounting",   ArtistClass.Technician, 110, 20, 0.17f, "Spreadsheet of Doom"),
            ("Kendrick LlamaFarmer",   ArtistClass.Technician, 125, 25, 0.20f, "HUMBLE Llama"),

            // ── Performers (Blue / Acrobat) ──
            ("Chris Brownie",          ArtistClass.Acrobat,    115, 19, 0.16f, "With Sprinkles"),
            ("Beyonslay",              ArtistClass.Acrobat,    135, 21, 0.14f, "Crazy in Glove"),
            ("Tyla Tyla Pumpkin Fila", ArtistClass.Acrobat,    105, 17, 0.15f, "Waterfall Splash"),
            ("Ushurr",                 ArtistClass.Acrobat,    120, 20, 0.13f, "Yeah! Yeah! Yeah!"),

            // ── Trendsetters (Purple / Trickster) ──
            ("Nay (Kanye)",            ArtistClass.Trickster,  125, 23, 0.17f, "Stronger Coffee"),
            ("Ice Splice",             ArtistClass.Trickster,  100, 18, 0.19f, "Munch and Crunch"),
            ("ZSA",                    ArtistClass.Trickster,  115, 19, 0.16f, "Kill Bill Nye"),
            ("Ayra Shooting Starr",    ArtistClass.Trickster,  108, 17, 0.15f, "Rush Hour"),
        };

        int created = 0;

        foreach (var (name, cls, hp, atk, crit, sig) in artists)
        {
            string path = $"{folder}/{name.Replace(" ", "_").Replace("&", "and").Replace("(", "").Replace(")", "").Replace("!", "").Replace(".", "").Replace("...", "")}.asset";

            if (AssetDatabase.LoadAssetAtPath<ArtistData>(path) != null)
            {
                Debug.Log($"[Generator] Skipped (already exists): {name}");
                continue;
            }

            ArtistData data = ScriptableObject.CreateInstance<ArtistData>();
            data.artistName = name;
            data.artistClass = cls;
            data.maxHP = hp;
            data.attackPower = atk;
            data.critChance = crit;
            data.signatureMoveName = sig;

            AssetDatabase.CreateAsset(data, path);
            created++;
            Debug.Log($"[Generator] Created: {name}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[Generator] Done! {created} artists created in {folder}");
    }
}