using UnityEngine;

public class FogDrift : MonoBehaviour
{
    [SerializeField] private Vector2 speed = new Vector2(0.15f, 0.02f);

    private void Update()
    {
        transform.position += (Vector3)(speed * Time.deltaTime);
    }
}