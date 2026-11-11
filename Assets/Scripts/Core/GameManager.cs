using UnityEngine;

public class GameManager : MonoBehaviour
{
    public float velocidadJugador = 5f;

    [HideInInspector] public int puntaje = 0;
    [HideInInspector] public int nivel = 1;
    [HideInInspector] public bool enJuego = false;

    private Laberinto laberinto;
    private Jugador jugador;

    void Start()
    {
        laberinto = FindAnyObjectByType<Laberinto>();
        jugador = FindAnyObjectByType<Jugador>();

        laberinto.ColocarPuntos();
        jugador.Reiniciar();
        enJuego = true;
    }

    // Cada nivel es 5 % más rápido, hasta +30 %
    public float MultiplicadorVelocidad()
    {
        return 1f + Mathf.Min(0.3f, 0.05f * (nivel - 1));
    }

    public void Comer(Vector2Int celda)
    {
        char comido = laberinto.Comer(celda);
        if (comido == '.') puntaje += 10;
        else if (comido == 'o') puntaje += 50;
    }
}
