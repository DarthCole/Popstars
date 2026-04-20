using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach to the Back button in StageScene.
/// Wire the Button onClick to GoBack().
/// </summary>
public class StageBackButton : MonoBehaviour
{
    public void GoBack()
    {
        SceneManager.LoadScene("CharacterCustomisation");
    }
}
