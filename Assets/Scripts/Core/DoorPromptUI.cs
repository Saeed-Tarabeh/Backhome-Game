using UnityEngine;
using UnityEngine.UI;

public class DoorPromptUI : MonoBehaviour
{
    [Header("Images")]
    [SerializeField] private Image keyImage;
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite pressedSprite;

    [Header("Timing")]
    [SerializeField] private float loopSpeed = 0.4f;

    [Header("Fade")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeSpeed = 6f;

    private bool visible;
    private float timer;

    private void Update()
    {
        // Smooth fade
        float target = visible ? 1f : 0f;
        canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, target, fadeSpeed * Time.deltaTime);

        // Animate only if visible
        if (!visible) return;

        timer += Time.deltaTime;
        if (timer >= loopSpeed)
        {
            timer = 0f;

            // Toggle sprite
            keyImage.sprite = 
                keyImage.sprite == idleSprite ? pressedSprite : idleSprite;
        }
    }

    public void Show()
    {
        visible = true;
    }

    public void Hide()
    {
        visible = false;
        timer = 0f;
        keyImage.sprite = idleSprite;
    }
}