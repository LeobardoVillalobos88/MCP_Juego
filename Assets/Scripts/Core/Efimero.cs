using UnityEngine;

// Se destruye solo después de un tiempo
public class Efimero : MonoBehaviour
{
    public float duracion = 1f;

    void Start()
    {
        Destroy(gameObject, duracion);
    }
}
