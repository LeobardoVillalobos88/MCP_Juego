using UnityEngine;

public enum TipoGlitch { Rastreador, Emboscador, Erratico, Timido }

public enum EstadoGlitch { EnCasa, Saliendo, Normal, Asustado, Regresando }

public class Enemigo : MonoBehaviour
{
    public TipoGlitch tipo;
    public Color color = Color.red;
    public Vector2Int celdaInicio;
    public Vector2Int esquina;
    public float tiempoEnCasa = 0f;

    [Header("Velocidades")]
    public float velocidad = 4.6f;
    public float velocidadAsustado = 2.5f;
    public float velocidadRegreso = 10f;
    public float velocidadSalida = 3f;

    [Header("Sprites")]
    public SpriteRenderer cuerpo;
    public SpriteRenderer ojos;
    public Sprite[] spritesCuerpo;
    public Sprite[] spritesAsustado;
    public Sprite[] spritesAsustadoBlanco;
    public Sprite ojosDerecha;
    public Sprite ojosIzquierda;
    public Sprite ojosArriba;
    public Sprite ojosAbajo;

    [HideInInspector] public EstadoGlitch estado;

    private MovimientoLaberinto movimiento;
    private Laberinto laberinto;
    private GameManager gameManager;
    private Jugador jugador;
    private float relojCasa;

    static readonly Vector2Int[] Direcciones = { Vector2Int.up, Vector2Int.left, Vector2Int.down, Vector2Int.right };

    void Awake()
    {
        movimiento = GetComponent<MovimientoLaberinto>();
        laberinto = FindAnyObjectByType<Laberinto>();
        gameManager = FindAnyObjectByType<GameManager>();
        jugador = FindAnyObjectByType<Jugador>();
    }

    public void Reiniciar()
    {
        gameObject.SetActive(true);
        movimiento.Colocar(celdaInicio);
        relojCasa = 0f;
        estado = laberinto.EnCasa(celdaInicio) ? EstadoGlitch.EnCasa : EstadoGlitch.Normal;
        Dibujar();
    }

    void Update()
    {
        if (!gameManager.enJuego || Time.timeScale == 0f) return;

        if (estado == EstadoGlitch.EnCasa)
        {
            relojCasa += Time.deltaTime;
            if (relojCasa >= tiempoEnCasa) estado = EstadoGlitch.Saliendo;
        }
        else
        {
            if (estado == EstadoGlitch.Asustado && !gameManager.HaySusto()) estado = EstadoGlitch.Normal;

            movimiento.velocidad = VelocidadActual();
            movimiento.puedeCruzarPuerta = estado == EstadoGlitch.Saliendo || estado == EstadoGlitch.Regresando;
            if (movimiento.Avanzar()) LlegarACelda();

            RevisarJugador();
        }

        Dibujar();
    }

    float VelocidadActual()
    {
        if (estado == EstadoGlitch.Asustado) return velocidadAsustado * gameManager.MultiplicadorVelocidad();
        if (estado == EstadoGlitch.Regresando) return velocidadRegreso;
        if (estado == EstadoGlitch.Saliendo) return velocidadSalida;
        return velocidad * gameManager.MultiplicadorVelocidad();
    }

    void LlegarACelda()
    {
        if (estado == EstadoGlitch.Saliendo && !laberinto.EnCasa(movimiento.celda)) estado = EstadoGlitch.Normal;
        if (estado == EstadoGlitch.Regresando && movimiento.celda == laberinto.centroCasa) estado = EstadoGlitch.Saliendo;

        movimiento.direccion = ElegirDireccion();
    }

    // En cada celda toma, sin dar media vuelta, el camino que más lo acerca a su objetivo
    Vector2Int ElegirDireccion()
    {
        bool alAzar = estado == EstadoGlitch.Asustado
            || (tipo == TipoGlitch.Erratico && estado == EstadoGlitch.Normal && gameManager.persecucion && Random.value < 0.5f);
        Vector2Int objetivo = Objetivo();

        Vector2Int mejor = Vector2Int.zero;
        float menorDistancia = float.MaxValue;
        foreach (Vector2Int direccion in Direcciones)
        {
            if (direccion == -movimiento.direccion || !movimiento.PuedeIr(direccion)) continue;

            // Al azar: cada opción recibe una distancia aleatoria
            Vector2Int siguiente = movimiento.celda + direccion;
            float distancia = alAzar ? Random.value : (siguiente - objetivo).sqrMagnitude;
            if (distancia < menorDistancia)
            {
                menorDistancia = distancia;
                mejor = direccion;
            }
        }

        // Callejón sin salida: solo queda regresar
        if (mejor == Vector2Int.zero && movimiento.PuedeIr(-movimiento.direccion)) mejor = -movimiento.direccion;
        return mejor;
    }

    Vector2Int Objetivo()
    {
        if (estado == EstadoGlitch.Saliendo) return laberinto.salidaCasa;
        if (estado == EstadoGlitch.Regresando) return laberinto.centroCasa;
        if (!gameManager.persecucion) return esquina;

        Vector2Int celdaJugador = jugador.Celda();
        switch (tipo)
        {
            case TipoGlitch.Emboscador:
                return celdaJugador + jugador.mirando * 4;
            case TipoGlitch.Timido:
                // Si está cerca de Bit se va a su esquina
                return Vector2.Distance(movimiento.celda, celdaJugador) > 8f ? celdaJugador : esquina;
            default:
                return celdaJugador;
        }
    }

    void RevisarJugador()
    {
        if (Vector2.Distance(transform.position, jugador.transform.position) > 0.6f) return;

        if (estado == EstadoGlitch.Asustado)
        {
            estado = EstadoGlitch.Regresando;
            gameManager.ComerGlitch(transform.position);
        }
        else if (estado == EstadoGlitch.Normal || estado == EstadoGlitch.Saliendo)
        {
            gameManager.TocarGlitch();
        }
    }

    public void Asustar()
    {
        if (estado != EstadoGlitch.Normal && estado != EstadoGlitch.Asustado) return;
        if (estado == EstadoGlitch.Normal) movimiento.DarVuelta();
        estado = EstadoGlitch.Asustado;
    }

    public void CambiarModo()
    {
        if (estado == EstadoGlitch.Normal) movimiento.DarVuelta();
    }

    void Dibujar()
    {
        int cuadro = (int)(Time.time * 6f) % 2;
        cuerpo.enabled = estado != EstadoGlitch.Regresando;
        ojos.enabled = estado != EstadoGlitch.Asustado;

        if (estado == EstadoGlitch.Asustado)
        {
            // Parpadea en blanco cuando el susto está por acabar
            bool blanco = gameManager.TiempoSusto() < 2f && cuadro == 0;
            cuerpo.sprite = blanco ? spritesAsustadoBlanco[cuadro] : spritesAsustado[cuadro];
            cuerpo.color = Color.white;
            return;
        }

        cuerpo.sprite = spritesCuerpo[cuadro];
        cuerpo.color = color;

        Vector2Int direccion = movimiento.direccion;
        if (direccion == Vector2Int.right) ojos.sprite = ojosDerecha;
        else if (direccion == Vector2Int.left) ojos.sprite = ojosIzquierda;
        else if (direccion == Vector2Int.up) ojos.sprite = ojosArriba;
        else ojos.sprite = ojosAbajo;
    }
}
