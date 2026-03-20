// File: Assets/Scripts/GemMatch/Gem.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class Gem : MonoBehaviour, IPointerClickHandler
{
    public enum GemType
    {
        Red,
        Blue,
        Green,
        Yellow,
        Purple
    }

    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI iconText;

    private GemType type;
    private int row;
    private int column;
    private GemInputHandler inputHandler;
    private PowerUpType currentPowerUp = PowerUpType.None;

    private static readonly Color[] gemColors = new Color[]
    {
        new Color(0.9f, 0.2f, 0.3f),
        new Color(0.2f, 0.4f, 0.9f),
        new Color(0.2f, 0.8f, 0.4f),
        new Color(1.0f, 0.8f, 0.1f),
        new Color(0.6f, 0.2f, 0.9f)
    };

    private static readonly string[] gemIcons = new string[]
    {
        "\u2665",   // Heart
        "\u2605",   // Star
        "\u266B",   // Music
        "\u2726",   // Diamond
        "\u26A1"    // Lightning
    };

    // Power-up display icons
    private static readonly string[] powerUpIcons = new string[]
    {
        "",         // None
        "\u2194",   // ↔ Striped Horizontal
        "\u2195",   // ↕ Striped Vertical
        "\u25CE",   // ◎ Wrapped
        "\u2738"    // ✸ Color Bomb
    };

    public GemType Type => type;
    public int Row => row;
    public int Column => column;
    public PowerUpType CurrentPowerUp => currentPowerUp;

    public void Initialize(GemType gemType, int row, int column)
    {
        this.type = gemType;
        this.row = row;
        this.column = column;
        this.currentPowerUp = PowerUpType.None;

        image.color = gemColors[(int)gemType];

        if (iconText != null)
        {
            iconText.text = gemIcons[(int)gemType];
        }
    }

    public void SetPowerUp(PowerUpType powerUp)
    {
        currentPowerUp = powerUp;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (currentPowerUp == PowerUpType.None)
        {
            iconText.text = gemIcons[(int)type];
            return;
        }

        if (currentPowerUp == PowerUpType.ColorBomb)
        {
            // Color bomb is a special rainbow-ish look
            image.color = new Color(0.1f, 0.1f, 0.1f);
            iconText.text = powerUpIcons[(int)currentPowerUp];
            iconText.fontSize = 50;
            return;
        }

        // Striped and Wrapped — keep gem color but show power-up icon
        iconText.text = powerUpIcons[(int)currentPowerUp];
        iconText.fontSize = 50;

        // Make the gem slightly brighter to stand out
        Color baseColor = gemColors[(int)type];
        image.color = Color.Lerp(baseColor, Color.white, 0.3f);
    }

    public void SetInputHandler(GemInputHandler handler)
    {
        inputHandler = handler;
    }

    public void SetPosition(int newRow, int newColumn)
    {
        row = newRow;
        column = newColumn;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (inputHandler != null)
        {
            inputHandler.OnGemClicked(this);
        }
    }
}