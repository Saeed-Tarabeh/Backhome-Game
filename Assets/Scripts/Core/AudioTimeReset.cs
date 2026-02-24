using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioTimeReset : MonoBehaviour
{
    [Header("Mixer Snapshots")]
    [SerializeField] private AudioMixerSnapshot normalSnapshot;
    [SerializeField] private float transitionTime = 0.01f;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetNow();
    }

    public void ResetNow()
    {
        // Reset time
        Time.timeScale = 1f;

        // Reset audio snapshot
        if (normalSnapshot != null)
            normalSnapshot.TransitionTo(transitionTime);

        AudioListener.pause = false;
    }
}