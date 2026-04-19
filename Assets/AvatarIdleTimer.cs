using System.Collections;
using UnityEngine;

/// <summary>
/// Attach to the avatar GameObject alongside the Animator.
/// Every <idleInterval> seconds it fires the idle animation trigger,
/// then waits for it to finish before resuming the countdown.
///
/// Your Animator Controller needs:
///   • A default state called "Idle" (or whatever you set idleStateName to)
///   • A trigger parameter named "PlayIdle" (or whatever you set idleTriggerName to)
///   • The Idle state transitions back to the standing state when done
/// </summary>
public class AvatarIdleTimer : MonoBehaviour
{
    [Header("References")]
    public Animator avatarAnimator;

    [Header("Timing")]
    [Tooltip("Seconds between each automatic idle animation.")]
    public float idleInterval = 10f;

    [Header("Animator Parameters")]
    public string idleTriggerName = "PlayIdle";   // trigger to fire
    public string idleStateName   = "Idle";       // state name to detect when it finishes

    void OnEnable()
    {
        if (avatarAnimator != null)
            StartCoroutine(IdleLoop());
    }

    void OnDisable() => StopAllCoroutines();

    IEnumerator IdleLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(idleInterval);

            if (avatarAnimator == null || !avatarAnimator.enabled) yield break;

            // Only fire if the parameter actually exists in this controller
            if (!HasParameter(idleTriggerName)) yield break;

            // Fire the idle
            avatarAnimator.SetTrigger(idleTriggerName);

            // Wait one frame for the Animator to enter the idle state
            yield return null;

            // Wait until the idle state finishes (normalizedTime >= 1)
            while (IsPlayingIdle())
                yield return null;
        }
    }

    bool IsPlayingIdle()
    {
        var info = avatarAnimator.GetCurrentAnimatorStateInfo(0);
        return info.IsName(idleStateName) && info.normalizedTime < 1f;
    }

    bool HasParameter(string paramName)
    {
        if (avatarAnimator == null || avatarAnimator.runtimeAnimatorController == null) return false;
        foreach (var p in avatarAnimator.parameters)
            if (p.name == paramName) return true;
        return false;
    }
}
