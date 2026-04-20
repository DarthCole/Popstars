using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Top-down 2D player controller for the Stage scene.
/// Attach to the player SpriteRenderer GameObject alongside a Rigidbody2D.
///
/// Animator contract (Blend Tree recommended):
///   float MoveX  — horizontal input (-1 to 1)
///   float MoveY  — vertical   input (-1 to 1)
///   bool  Moving — true while any input is active
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class StagePlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 5f;

    [Header("Sprites")]
    public Sprite overrideSprite;   // walk spritesheet default — overridden by StageSessionData
    public Sprite idleSprite;       // static PNG shown when not moving

    // ── Cached components ────────────────────────────────────────────────────
    Rigidbody2D  _rb;
    Animator     _anim;
    SpriteRenderer _sr;

    // ── Input ────────────────────────────────────────────────────────────────
    Vector2 _input;

    // ── Animator param hashes ─────────────────────────────────────────────────
    static readonly int H_MoveX  = Animator.StringToHash("MoveX");
    static readonly int H_MoveY  = Animator.StringToHash("MoveY");
    static readonly int H_Moving = Animator.StringToHash("Moving");

    // ════════════════════════════════════════════════════════════════════════
    void Awake()
    {
        _rb   = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
        _sr   = GetComponent<SpriteRenderer>();

        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void Start()
    {
        // Apply character sprite chosen in Avatar customisation screen
        var data = StageSessionData.Instance;
        if (data != null && data.characterSprite != null)
            _sr.sprite = data.characterSprite;
        else if (overrideSprite != null)
            _sr.sprite = overrideSprite;

        // Apply idle sprite from session data (overrides Inspector field)
        if (data != null && data.idleSprite != null)
            idleSprite = data.idleSprite;

        // Apply animator controller if passed through session data
        if (data != null && data.animatorController != null)
            _anim.runtimeAnimatorController = data.animatorController;

        // Start in idle state
        _anim.enabled = false;
        if (idleSprite != null) _sr.sprite = idleSprite;
    }

    // ════════════════════════════════════════════════════════════════════════
    void Update()
    {
        // ── Read input (works with both old and new Input System) ────────────
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null)
        {
            float x = (kb.dKey.isPressed || kb.rightArrowKey.isPressed ? 1f : 0f)
                    - (kb.aKey.isPressed || kb.leftArrowKey.isPressed  ? 1f : 0f);
            float y = (kb.wKey.isPressed || kb.upArrowKey.isPressed    ? 1f : 0f)
                    - (kb.sKey.isPressed || kb.downArrowKey.isPressed   ? 1f : 0f);
            _input = new Vector2(x, y);
        }
#else
        _input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#endif

        if (_input.sqrMagnitude > 0.01f)
            _input = _input.normalized;

        UpdateAnimator();
    }

    void FixedUpdate()
    {
        _rb.MovePosition(_rb.position + _input * moveSpeed * Time.fixedDeltaTime);
    }

    // ════════════════════════════════════════════════════════════════════════
    void UpdateAnimator()
    {
        bool moving = _input.sqrMagnitude > 0.01f;

        if (moving)
        {
            // Re-enable animator to drive walk animation
            if (!_anim.enabled) _anim.enabled = true;
            _anim.SetBool(H_Moving, true);
            _anim.SetFloat(H_MoveX, _input.x);
            _anim.SetFloat(H_MoveY, _input.y);
        }
        else
        {
            // Disable animator and show idle sprite directly
            _anim.enabled = false;
            if (idleSprite != null)
                _sr.sprite = idleSprite;
        }
    }
}
