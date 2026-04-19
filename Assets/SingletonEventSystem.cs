using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Attach to the EventSystem in your first scene.
/// Destroys any duplicate EventSystems that appear on scene load.
/// </summary>
public class SingletonEventSystem : MonoBehaviour
{
    void Awake()
    {
        EventSystem[] all = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
        if (all.Length > 1)
        {
            // A persisted one already exists — destroy this new duplicate
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }
}
