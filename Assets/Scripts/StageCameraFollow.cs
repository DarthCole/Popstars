using UnityEngine;

/// <summary>
/// Smooth camera that follows the player in the Stage scene.
/// Attached to Main Camera by StageSceneBuilder.
/// </summary>
public class StageCameraFollow : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float     smoothSpeed = 8f;
    [SerializeField] Vector3   offset      = new Vector3(0f, 0f, -10f);

    void LateUpdate()
    {
        if (target == null) return;
        var desired = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desired,
                                          smoothSpeed * Time.deltaTime);
    }
}
