// File: Assets/Scripts/GemMatch/GemInputHandler.cs
using UnityEngine;
using UnityEngine.UI;

public class GemInputHandler : MonoBehaviour
{
    [SerializeField] private GemBoard board;

    private Gem selectedGem;

    // Highlight color when a gem is selected
    private readonly Color selectedTint = new Color(0.7f, 0.7f, 0.7f, 1f);
    private Color originalColor;


    private bool interactable = true;

    public void OnGemClicked(Gem clickedGem)
    {
        if (!interactable) return;
        if (board.IsProcessing) return;
        // First click — select the gem
        if (selectedGem == null)
        {
            SelectGem(clickedGem);
            return;
        }

        // Clicked the same gem — deselect it
        if (clickedGem == selectedGem)
        {
            DeselectGem();
            return;
        }

        // Clicked a neighbor — try to swap
        if (AreNeighbors(selectedGem, clickedGem))
        {
            board.TrySwapGems(selectedGem, clickedGem);
            DeselectGem();
        }
        else
        {
            // Clicked a non-neighbor — select the new gem instead
            DeselectGem();
            SelectGem(clickedGem);
        }
    }

    private void SelectGem(Gem gem)
    {
        selectedGem = gem;
        Image image = gem.GetComponent<Image>();
        originalColor = image.color;
        image.color = originalColor * selectedTint;
    }

    private void DeselectGem()
    {
        if (selectedGem == null) return;

        Image image = selectedGem.GetComponent<Image>();
        image.color = originalColor;
        selectedGem = null;
    }
    public void SetInteractable(bool value)
    {
        interactable = value;

        if (!value)
        {
            DeselectGem();
        }
    }

    private bool AreNeighbors(Gem a, Gem b)
    {
        int rowDiff = Mathf.Abs(a.Row - b.Row);
        int colDiff = Mathf.Abs(a.Column - b.Column);

        // Neighbors share a row or column and are exactly 1 apart
        return (rowDiff + colDiff) == 1;
    }
}

