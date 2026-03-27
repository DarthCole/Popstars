using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class RoundedCorners : MonoBehaviour
{
    [Range(0f, 0.9f)]
    public float radius = 0.3f;

    void Start()
    {
        GetComponent<Image>().pixelsPerUnitMultiplier = radius * 100;
    }
}