using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

/// <summary>
/// Builds the Stage scene from scratch in one click.
/// Menu: PopstarHub ▶ Build Stage Scene
///
/// What gets built:
///   Main Camera       — orthographic, dark-purple background, follows player
///   EventSystem       — Input System module
///   StageBackground   — SpriteRenderer for your stage PNG (assign after build)
///   Player            — SpriteRenderer + Rigidbody2D + Animator + StagePlayerController
///   StageSessionData  — DontDestroyOnLoad data bridge
///   CameraFollow      — simple script that tracks the player
///
/// After running:
///   1. Assign your stage PNG to StageBackground > SpriteRenderer > Sprite
///   2. Assign your Animator Controller to Player > Animator > Controller
///   3. Add "StageScene" to File > Build Profiles
/// </summary>
public static class StageSceneBuilder
{
    const string SCENE_PATH = "Assets/Scenes/StageScene.unity";

    [MenuItem("PopstarHub/Build Stage Scene")]
    static void Build()
    {
        // ── 1. New empty scene ────────────────────────────────────────────────
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ── 2. Camera ─────────────────────────────────────────────────────────
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.clearFlags      = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.039f, 0.016f, 0.075f, 1f);
        cam.orthographic    = true;
        cam.orthographicSize = 5f;
        cam.nearClipPlane   = -100f;
        cam.farClipPlane    =  100f;
        camGO.AddComponent<AudioListener>();
        camGO.AddComponent<StageCameraFollow>();   // wired below

        // ── 3. EventSystem ────────────────────────────────────────────────────
        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<EventSystem>();
        esGO.AddComponent<InputSystemUIInputModule>();

        // ── 4. Stage background ───────────────────────────────────────────────
        var bgGO = new GameObject("StageBackground");
        var bgSR = bgGO.AddComponent<SpriteRenderer>();
        bgSR.sortingOrder = -10;
        bgGO.transform.position = Vector3.zero;
        // ▶ After build: drag your stage PNG onto bgSR.sprite in the Inspector

        // ── 5. Player ─────────────────────────────────────────────────────────
        var playerGO = new GameObject("Player");
        playerGO.tag = "Player";
        playerGO.transform.position = Vector3.zero;

        var sr = playerGO.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 0;

        var rb = playerGO.AddComponent<Rigidbody2D>();
        rb.gravityScale    = 0f;
        rb.freezeRotation  = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        playerGO.AddComponent<Animator>();              // assign controller in Inspector
        playerGO.AddComponent<StagePlayerController>(); // handles movement + anim params

        // Stage boundary collider (invisible walls so player can't walk off)
        var boundsGO = new GameObject("StageBounds");
        boundsGO.transform.position = Vector3.zero;
        AddBoundaryWalls(boundsGO);

        // ── 6. Session data bridge ────────────────────────────────────────────
        var sdGO = new GameObject("StageSessionData");
        sdGO.AddComponent<StageSessionData>();

        // ── 7. Wire camera follow to player ───────────────────────────────────
        var follow = camGO.GetComponent<StageCameraFollow>();
        var followSO = new UnityEditor.SerializedObject(follow);
        followSO.FindProperty("target").objectReferenceValue = playerGO.transform;
        followSO.ApplyModifiedProperties();

        // ── 8. Save scene ─────────────────────────────────────────────────────
        EditorSceneManager.SaveScene(scene, SCENE_PATH);
        AssetDatabase.Refresh();

        Debug.Log("[StageSceneBuilder] StageScene built and saved to " + SCENE_PATH +
                  "\n▶ Assign your stage background PNG to StageBackground > SpriteRenderer > Sprite" +
                  "\n▶ Assign your Animator Controller to Player > Animator > Controller" +
                  "\n▶ Add StageScene to File > Build Profiles");
    }

    // ── Invisible boundary walls (box colliders around the stage) ────────────
    static void AddBoundaryWalls(GameObject parent)
    {
        // Half-extents of the play area in world units — tweak to match your stage PNG
        const float HW = 9f;   // half-width
        const float HH = 5f;   // half-height
        const float T  = 0.5f; // wall thickness

        CreateWall(parent, "WallTop",    new Vector2(0,  HH + T), new Vector2(HW * 2 + T * 2, T));
        CreateWall(parent, "WallBottom", new Vector2(0, -HH - T), new Vector2(HW * 2 + T * 2, T));
        CreateWall(parent, "WallLeft",   new Vector2(-HW - T, 0), new Vector2(T, HH * 2 + T * 2));
        CreateWall(parent, "WallRight",  new Vector2( HW + T, 0), new Vector2(T, HH * 2 + T * 2));
    }

    static void CreateWall(GameObject parent, string name, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.transform.localPosition = pos;
        var col = go.AddComponent<BoxCollider2D>();
        col.size = size;
    }
}
