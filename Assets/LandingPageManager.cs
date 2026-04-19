using UnityEngine;
using UnityEngine.SceneManagement;

public class LandingPageManager : MonoBehaviour
{
    public void LoadMode(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}