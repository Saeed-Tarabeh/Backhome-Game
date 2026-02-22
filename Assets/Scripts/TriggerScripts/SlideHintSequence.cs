using System.Collections;
using UnityEngine;

public class SlideHintSequence : MonoBehaviour
{
    [Header("Panels (RectTransforms)")]
    [SerializeField] private RectTransform firstPanel;   // knight
    [SerializeField] private RectTransform secondPanel;  // text

    [Header("Optional Fade")]
    [SerializeField] private CanvasGroup firstGroup;
    [SerializeField] private CanvasGroup secondGroup;

    [Header("Positions")]
    [Tooltip("Where the panel rests when hidden (off-screen).")]
    [SerializeField] private Vector2 hiddenOffset = new Vector2(0f, -220f);

    [Tooltip("Where the panel is when visible (on-screen). Uses its current anchoredPosition as 'visible'.")]
    [SerializeField] private bool useCurrentAsVisible = true;

    [Header("Timing")]
    [SerializeField] private float slideUpTime = 0.35f;
    [SerializeField] private float stayTime = 2.2f;
    [SerializeField] private float slideDownTime = 0.35f;
    [SerializeField] private float gapBetweenPanels = 0.15f;

    [Header("Behavior")]
    [SerializeField] private bool playOnce = true;

    private bool played;
    private Vector2 firstVisiblePos;
    private Vector2 secondVisiblePos;

    private void Awake()
    {
        if (useCurrentAsVisible)
        {
            if (firstPanel) firstVisiblePos = firstPanel.anchoredPosition;
            if (secondPanel) secondVisiblePos = secondPanel.anchoredPosition;
        }

        // Start hidden
        SetHidden(firstPanel, firstGroup, firstVisiblePos);
        SetHidden(secondPanel, secondGroup, secondVisiblePos);
    }

    public void Play()
    {
        if (playOnce && played) return;
        played = true;

        StopAllCoroutines();
        StartCoroutine(Sequence());
    }

    private IEnumerator Sequence()
    {
        // Slide knight up
        yield return SlideIn(firstPanel, firstGroup, firstVisiblePos);

        yield return new WaitForSeconds(0.15f);

        // Slide text up
        yield return SlideIn(secondPanel, secondGroup, secondVisiblePos);

        // Both stay visible
        yield return new WaitForSeconds(stayTime);

        // Slide BOTH down together
        StartCoroutine(SlideOut(firstPanel, firstGroup, firstVisiblePos));
        yield return SlideOut(secondPanel, secondGroup, secondVisiblePos);
    }

    private IEnumerator SlideInHoldOut(RectTransform panel, CanvasGroup group, Vector2 visiblePos)
    {
        if (!panel) yield break;

        Vector2 hiddenPos = visiblePos + hiddenOffset;

        // Slide up (hidden -> visible)
        yield return Slide(panel, group, hiddenPos, visiblePos, slideUpTime, fadeIn: true);

        // Stay
        yield return new WaitForSeconds(stayTime);

        // Slide down (visible -> hidden)
        yield return Slide(panel, group, visiblePos, hiddenPos, slideDownTime, fadeIn: false);
    }

    private IEnumerator Slide(RectTransform panel, CanvasGroup group, Vector2 from, Vector2 to, float time, bool fadeIn)
    {
        float t = 0f;

        if (group != null)
            group.alpha = fadeIn ? 0f : 1f;

        panel.anchoredPosition = from;

        while (t < time)
        {
            t += Time.unscaledDeltaTime;
            float u = Mathf.Clamp01(t / time);

            // Smoothstep for nicer motion
            u = u * u * (3f - 2f * u);

            panel.anchoredPosition = Vector2.Lerp(from, to, u);

            if (group != null)
                group.alpha = fadeIn ? u : (1f - u);

            yield return null;
        }

        panel.anchoredPosition = to;

        if (group != null)
            group.alpha = fadeIn ? 1f : 0f;
    }
    
    private IEnumerator SlideIn(RectTransform panel, CanvasGroup group, Vector2 visiblePos)
    {
        Vector2 hiddenPos = visiblePos + hiddenOffset;
        yield return Slide(panel, group, hiddenPos, visiblePos, slideUpTime, fadeIn: true);
    }

    private IEnumerator SlideOut(RectTransform panel, CanvasGroup group, Vector2 visiblePos)
    {
        Vector2 hiddenPos = visiblePos + hiddenOffset;
        yield return Slide(panel, group, visiblePos, hiddenPos, slideDownTime, fadeIn: false);
    }

    private void SetHidden(RectTransform panel, CanvasGroup group, Vector2 visiblePos)
    {
        if (!panel) return;

        Vector2 hiddenPos = visiblePos + hiddenOffset;
        panel.anchoredPosition = hiddenPos;

        if (group != null) group.alpha = 0f;
    }
    
    public float TotalSequenceTime()
    {
        return slideUpTime
            + 0.15f
            + slideUpTime
            + stayTime
            + slideDownTime;
    }
}