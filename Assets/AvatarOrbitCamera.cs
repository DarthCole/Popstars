using UnityEngine;

/// <summary>
/// Attach to Main Camera. Drag left/right to spin the active avatar on its Y axis.
/// Assign avatarTransform in the Inspector, or let AvatarGenderSelector set it at runtime.
/// </summary>
public class AvatarOrbitCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform avatarTransform;

    [Header("Feel")]
    public float sensitivity  = 0.35f;  // degrees rotated per pixel dragged
    public float inertiaDecay = 7f;     // higher = snappier stop after release

    float _velocity;
    float _lastX;
    bool  _dragging;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _dragging  = true;
            _lastX     = Input.mousePosition.x;
            _velocity  = 0f;
        }

        if (Input.GetMouseButtonUp(0))
            _dragging = false;

        if (avatarTransform == null) return;

        if (_dragging)
        {
            float dx  = Input.mousePosition.x - _lastX;
            _velocity = -dx * sensitivity;
            avatarTransform.Rotate(Vector3.up, _velocity, Space.World);
            _lastX    = Input.mousePosition.x;
        }
        else
        {
            // Inertia: gradually bleed off spin after finger/mouse lifts
            _velocity = Mathf.Lerp(_velocity, 0f, Time.deltaTime * inertiaDecay);
            avatarTransform.Rotate(Vector3.up, _velocity, Space.World);
        }
    }
}
