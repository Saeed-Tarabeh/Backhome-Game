using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif

public class LevelManager : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Time.timeScale = 1f;

#if UNITY_EDITOR
            ClearConsole();
#endif

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            if (PlayerHealth.Instance != null)
                PlayerHealth.Instance.TakeDamage(1);
        }
    }

#if UNITY_EDITOR
    private void ClearConsole()
    {
        var assembly = Assembly.GetAssembly(typeof(SceneView));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(null, null);
    }
#endif
}