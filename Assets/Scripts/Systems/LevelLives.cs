using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLives : MonoBehaviour
{
    public static LevelLives Instance { get; private set; }

    [SerializeField] private int startingLives = 3;
    public int LivesRemaining { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
        ResetLives();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetLives();
    }

    private void ResetLives()
    {
        LivesRemaining = startingLives;
    }

    /// <summary>
    /// Returns true if player is allowed to respawn (life spent),
    /// false if it's game over.
    /// </summary>
    public bool SpendLife()
    {
        if (LivesRemaining <= 0) return false;
        LivesRemaining--;
        return true;
    }

    public void RestartLevel()
    {
        // Kill the persistent manager so next load starts fresh.
        Destroy(gameObject);
        Instance = null;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}