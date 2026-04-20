using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

/// <summary>
/// Builds the TriviaScene from scratch:
///   1. Creates (or clears) a scene called "TriviaScene"
///   2. Adds an empty GameObject with TriviaGameUI attached
///   3. Sets the camera background to dark purple
///
/// Menu: PopstarHub ▶ Build Trivia Scene
/// After running, add "TriviaScene" to your Build Profiles.
/// </summary>
public static class TriviaSceneBuilder
{
    [MenuItem("PopstarHub/Build Trivia Scene")]
    static void Build()
    {
        // ── 1. Find or create the scene asset ────────────────────────────────
        const string SCENE_PATH = "Assets/Scenes/TriviaScene.unity";

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ── 2. Camera with dark-purple background ─────────────────────────────
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.clearFlags       = CameraClearFlags.SolidColor;
        cam.backgroundColor  = new Color(0.039f, 0.016f, 0.075f, 1f);
        cam.orthographic     = false;
        camGO.AddComponent<AudioListener>();

        // ── 3. EventSystem (InputSystem) ──────────────────────────────────────
        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<EventSystem>();
        esGO.AddComponent<InputSystemUIInputModule>();

        // ── 4. TriviaManager host ─────────────────────────────────────────────
        var host = new GameObject("TriviaManager");
        host.AddComponent<TriviaGameUI>();
        // TriviaGameUI.Start() builds the whole canvas at runtime — nothing else needed.

        // ── 5. Save scene ─────────────────────────────────────────────────────
        EditorSceneManager.SaveScene(scene, SCENE_PATH);
        AssetDatabase.Refresh();

        Debug.Log("[TriviaSceneBuilder] TriviaScene built and saved to " + SCENE_PATH +
                  "\nRemember to add it to File → Build Profiles!");
    }
}
