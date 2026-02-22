using UnityEngine;

public class InfiniteFogLocal : MonoBehaviour
{
    [SerializeField] private float scrollSpeed = 0.2f;

    private Transform a;
    private Transform b;
    private float width;

    private void Awake()
    {
        a = transform.GetChild(0);
        b = transform.GetChild(1);

        var sr = a.GetComponent<SpriteRenderer>();
        width = sr.bounds.size.x; // world-space width
    }

    private void Update()
    {
        // Move in local space (camera moves, fog stays stable)
        a.localPosition += Vector3.right * scrollSpeed * Time.deltaTime;
        b.localPosition += Vector3.right * scrollSpeed * Time.deltaTime;

        // Wrap when a piece moves one width
        if (a.localPosition.x >= width) a.localPosition -= new Vector3(width * 2f, 0f, 0f);
        if (b.localPosition.x >= width) b.localPosition -= new Vector3(width * 2f, 0f, 0f);
    }
}