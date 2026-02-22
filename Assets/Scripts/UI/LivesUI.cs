using UnityEngine;
using UnityEngine.UI;

public class LivesUI : MonoBehaviour
{
    [Header("Assign 3 heart Images in order (left -> right)")]
    [SerializeField] private Image[] hearts;

    [Header("Visuals")]
    [Range(0f, 1f)]
    [SerializeField] private float emptyAlpha = 0.2f;
    [SerializeField] private bool disableEmptyHearts = false;

    private void OnEnable()
    {
        Refresh();
    }

    private void Update()
    {
        // Cheap + simple: just refresh each frame (3 images only)
        Refresh();
    }

    private void Refresh()
    {
        if (LevelLives.Instance == null || hearts == null) return;

        int lives = LevelLives.Instance.LivesRemaining;

        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null) continue;

            bool filled = i < lives;

            if (disableEmptyHearts)
            {
                hearts[i].enabled = filled;
            }
            else
            {
                hearts[i].enabled = true;
                Color c = hearts[i].color;
                c.a = filled ? 1f : emptyAlpha;
                hearts[i].color = c;
            }
        }
    }
}