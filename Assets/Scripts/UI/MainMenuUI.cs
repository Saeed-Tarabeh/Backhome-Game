using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private string levelSceneName = "CastleLevel";

    public void Play()
    {
        Time.timeScale = 1f; // Safety reset
        SceneManager.LoadScene(levelSceneName);
    }

    public void Quit()
    {
        Application.Quit();
        Debug.Log("Quit Game"); // So it shows in editor
    }
}