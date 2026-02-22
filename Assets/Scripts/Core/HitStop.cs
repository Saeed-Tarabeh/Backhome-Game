using System.Collections;
using UnityEngine;

public class HitStop : MonoBehaviour
{
    public static HitStop Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Stop(float duration, float timeScale = 0.05f)
    {
        StartCoroutine(DoStop(duration, timeScale));
    }

    private IEnumerator DoStop(float duration, float timeScale)
    {
        float original = Time.timeScale;
        Time.timeScale = timeScale;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = original;
    }
}