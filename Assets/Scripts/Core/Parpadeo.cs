using UnityEngine;

public class Parpadeo : MonoBehaviour
{
    public float intervalo = 0.25f;

    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        sr.enabled = (int)(Time.time / intervalo) % 2 == 0;
    }
}
