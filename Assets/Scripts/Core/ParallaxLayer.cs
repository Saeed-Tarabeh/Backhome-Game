using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [SerializeField] private float parallaxStrength = 0.2f;
    private float startpos, length;
    public GameObject cam;

    private void Start()
    {
        startpos = transform.position.x;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    private void LateUpdate()
    {
        float dist = cam.transform.position.x * parallaxStrength;
        float movement = cam.transform.position.x * (1 - parallaxStrength);

        transform.position = new Vector3(startpos + dist, transform.position.y, transform.position.z);

        if(movement > startpos + length) startpos += length;
        else if(movement < startpos - length) startpos -= length;
    }
}