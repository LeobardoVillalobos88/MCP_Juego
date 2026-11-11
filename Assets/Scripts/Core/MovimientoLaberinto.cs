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
    private float sobrante = 0f;

    void Awake()
    {
        laberinto = FindAnyObjectByType<Laberinto>();
    }

    public void Colocar(Vector2Int nuevaCelda)
    {
        celda = nuevaCelda;
        direccion = Vector2Int.zero;
        sobrante = 0f;
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
        if (direccion == Vector2Int.zero)
        {
            sobrante = 0f;
            return true;
        }

        // Lo que sobró al llegar al centro en el cuadro anterior se recorre ahora,
        // así la velocidad real no depende de los FPS
        float paso = velocidad * Time.deltaTime + sobrante;
        sobrante = 0f;
        Vector3 destino = laberinto.CeldaAMundo(celda + direccion);
        float distancia = Vector3.Distance(transform.position, destino);
        if (paso < distancia)
        {
            transform.position = Vector3.MoveTowards(transform.position, destino, paso);
            return false;
        }

        transform.position = destino;
        sobrante = paso - distancia;
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
