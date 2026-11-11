using UnityEngine;

// Mueve de centro a centro de celda. "celda" es la celda de la que sale y
// "direccion" hacia dónde va; al llegar, quien lo usa decide la siguiente dirección.
public class MovimientoLaberinto : MonoBehaviour
{
    public float velocidad = 5f;
    public bool puedeCruzarPuerta = false;
    [HideInInspector] public Vector2Int celda;
    [HideInInspector] public Vector2Int direccion;

    private Laberinto laberinto;

    void Awake()
    {
        laberinto = FindAnyObjectByType<Laberinto>();
    }

    public void Colocar(Vector2Int nuevaCelda)
    {
        celda = nuevaCelda;
        direccion = Vector2Int.zero;
        transform.position = laberinto.CeldaAMundo(celda);
    }

    public bool PuedeIr(Vector2Int nuevaDireccion)
    {
        return nuevaDireccion != Vector2Int.zero && laberinto.EsLibre(celda + nuevaDireccion, puedeCruzarPuerta);
    }

    public void DarVuelta()
    {
        if (direccion == Vector2Int.zero) return;
        celda += direccion;
        direccion = -direccion;
    }

    // Devuelve true cuando está en el centro de una celda
    public bool Avanzar()
    {
        if (direccion == Vector2Int.zero) return true;

        Vector3 destino = laberinto.CeldaAMundo(celda + direccion);
        transform.position = Vector3.MoveTowards(transform.position, destino, velocidad * Time.deltaTime);
        if (transform.position != destino) return false;

        celda += direccion;

        // Túnel: sale por un lado y aparece por el otro
        if (celda.x < 0 || celda.x >= laberinto.ancho)
        {
            celda.x = celda.x < 0 ? laberinto.ancho : -1;
            transform.position = laberinto.CeldaAMundo(celda);
        }
        return true;
    }
}
