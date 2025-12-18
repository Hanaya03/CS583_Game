using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuSceneLoader : MonoBehaviour
{
    [SerializeField] private string sceneName = "EnemyScene";
    [SerializeField] private string sceneNameHard = "EnemySceneHard";

    public void SelectEasyScene()
    {
        PlayerPrefs.SetString("SelectedDifficulty", sceneName);
        PlayerPrefs.Save();
        LoadScene();
    }

    public void SelectHardScene()
    {
        PlayerPrefs.SetString("SelectedDifficulty", sceneNameHard);
        PlayerPrefs.Save();
        LoadScene();
    }

    public void LoadScene()
    {
        SceneManager.LoadScene(PlayerPrefs.GetString("SelectedDifficulty"));
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
