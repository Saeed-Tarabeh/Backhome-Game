using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class DoorToMenu : MonoBehaviour
{
    [Header("Target Scene")]
    [SerializeField] private string menuSceneName = "MainMenu";

    [Header("Interaction")]
    [SerializeField] private KeyCode interactKey = KeyCode.Return;
    [SerializeField] private bool requireButtonPress = true;

    [SerializeField] private DoorPromptUI promptUI;

    private bool playerInside;

    private void Reset()
    {
        // Helpful default: make collider a trigger
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void Update()
    {
        if (!playerInside) return;

        if (!requireButtonPress || Input.GetKeyDown(interactKey))
            GoToMenu();
    }

    private void GoToMenu()
    {
        // If you use Time.timeScale anywhere (pause/slow), reset it:
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            promptUI?.Show();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            promptUI?.Hide();
        }
    }
}