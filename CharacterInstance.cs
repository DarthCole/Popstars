using UnityEngine;
using System;

public class CharacterInstance : MonoBehaviour
{
    public string UniqueID { get; private set; }
    public CharacterData CharacterData { get; private set; }
    public CostumeData CurrentCostumeData { get; private set; }
    public SpriteRenderer[] SpriteLayers;

    private void Awake()
    {
        UniqueID = Guid.NewGuid().ToString();
        SpriteLayers = new SpriteRenderer[7]; // 7 layers for modular pieces
    }

    public void Initialize(CharacterData characterData, CostumeData costumeData)
    {
        CharacterData = characterData;
        CurrentCostumeData = costumeData;
        ApplyCostume(costumeData);
    }

    private void ApplyCostume(CostumeData costumeData)
    {
        // Set each layer to the corresponding sprite from the costume data.
        // Assuming costumeData provides sprites in the correct order
        for (int i = 0; i < SpriteLayers.Length; i++)
        {
            if (SpriteLayers[i] != null && i < costumeData.Sprites.Length)
            {
                SpriteLayers[i].sprite = costumeData.Sprites[i];
            }
        }
    }

    public void SwapCostume(CostumeData newCostumeData)
    {
        CurrentCostumeData = newCostumeData;
        ApplyCostume(newCostumeData);
        // Trigger an event for costume swap
        OnCostumeSwapped?.Invoke(this, newCostumeData);
    }

    public event Action<CharacterInstance, CostumeData> OnCostumeSwapped;

    public void CustomizeAppearance(Color color, float scale)
    {
        // Example: Customize sprite layers based on parameters
        foreach (var layer in SpriteLayers)
        {
            if (layer != null)
            {
                layer.color = color;
                layer.transform.localScale = new Vector3(scale, scale, scale);
            }
        }
    }
}

[System.Serializable]
public class CharacterData
{
    // Add fields for character data
}

[System.Serializable]
public class CostumeData
{
    public Sprite[] Sprites; // Layered sprites for the costume
}