using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuSceneLoader : MonoBehaviour
{
    [SerializeField] private string sceneName = "AlternateRoom";
    
    public void LoadScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}

