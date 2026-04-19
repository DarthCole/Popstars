using UnityEngine;
using UnityEngine.InputSystem;

public class TestCombat : MonoBehaviour
{
    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            CombatManager.Instance.OnGemsCleared(GemColor.Black, 3);
        }
    }
}