using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public int vidasIniciales = 3;
    public int puntosVidaExtra = 10000;
    public float velocidadJugador = 5f;
    public float duracionSusto = 7f;
    public float duracionDispersion = 7f;
    public float duracionPersecucion = 20f;

    [Header("Bonus")]
    public GameObject prefabBonus;
    public Vector2Int celdaBonus;
    public GameObject prefabTextoPuntos;

    [HideInInspector] public int puntaje = 0;
    [HideInInspector] public int record = 0;
    [HideInInspector] public int vidas = 0;
    [HideInInspector] public int nivel = 1;
    [HideInInspector] public bool enJuego = false;
    [HideInInspector] public bool juegoTerminado = false;
    [HideInInspector] public bool persecucion = false;
    [HideInInspector] public string mensaje = "";

    private Laberinto laberinto;
    private Jugador jugador;
    private Enemigo[] enemigos;
    private AudioManager audioManager;
    private float finSusto = 0f;
    private float relojModo = 0f;
    private int racha = 0;
    private int puntosComidos = 0;
    private GameObject bonus;

    void Awake()
    {
        // Partida nueva
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    void Start()
    {
        laberinto = FindAnyObjectByType<Laberinto>();
        jugador = FindAnyObjectByType<Jugador>();
        enemigos = FindObjectsByType<Enemigo>();
        audioManager = FindAnyObjectByType<AudioManager>();

        record = PlayerPrefs.GetInt("Record", 0);
        vidas = vidasIniciales;
        laberinto.ColocarPuntos();
        audioManager.Reproducir(audioManager.inicio);
        StartCoroutine(EmpezarVida(audioManager.inicio.length));
    }

    void Update()
    {
        if (!enJuego) return;

        // Dispersión y persecución se alternan; el reloj se detiene durante el susto
        if (!HaySusto())
        {
            relojModo += Time.deltaTime;
            float duracion = persecucion ? duracionPersecucion : duracionDispersion;
            if (relojModo >= duracion)
            {
                relojModo = 0f;
                persecucion = !persecucion;
                foreach (Enemigo enemigo in enemigos) enemigo.CambiarModo();
            }
        }

        audioManager.Fondo(HaySusto() ? audioManager.alarma : audioManager.sirena);
    }

    // Cada nivel es 5 % más rápido, hasta +30 %
    public float MultiplicadorVelocidad()
    {
        return 1f + Mathf.Min(0.3f, 0.05f * (nivel - 1));
    }

    public bool HaySusto()
    {
        return Time.time < finSusto;
    }

    public float TiempoSusto()
    {
        return finSusto - Time.time;
    }

    public void Comer(Vector2Int celda)
    {
        char comido = laberinto.Comer(celda);
        if (comido == '.')
        {
            Sumar(10);
            audioManager.ReproducirPunto();
        }
        else if (comido == 'o')
        {
            Sumar(50);
            Asustar();
        }
        else
        {
            return;
        }

        puntosComidos++;
        if (puntosComidos == 70 || puntosComidos == 170) AparecerBonus();
        if (laberinto.puntosRestantes == 0) StartCoroutine(CompletarNivel());
    }

    void Asustar()
    {
        // El susto dura un segundo menos por nivel, mínimo 2 s
        racha = 0;
        finSusto = Time.time + Mathf.Max(2f, duracionSusto - (nivel - 1));
        foreach (Enemigo enemigo in enemigos) enemigo.Asustar();
        audioManager.Reproducir(audioManager.nucleo);
    }

    // 200, 400, 800 y 1600 en la misma racha
    public void ComerGlitch(Vector3 posicion)
    {
        racha++;
        int puntos = 200 * (int)Mathf.Pow(2, racha - 1);
        Sumar(puntos);
        MostrarPuntos(puntos, posicion);
        audioManager.Reproducir(audioManager.comerGlitch);
        StartCoroutine(PausaBreve(0.5f));
    }

    void AparecerBonus()
    {
        if (bonus != null) Destroy(bonus);
        bonus = Instantiate(prefabBonus, laberinto.CeldaAMundo(celdaBonus), Quaternion.identity);
    }

    // 100 en el nivel 1, +200 por nivel, máximo 2000
    public void ComerBonus(Vector3 posicion)
    {
        int puntos = Mathf.Min(2000, 100 + 200 * (nivel - 1));
        Sumar(puntos);
        MostrarPuntos(puntos, posicion);
        audioManager.Reproducir(audioManager.bonus);
    }

    void MostrarPuntos(int puntos, Vector3 posicion)
    {
        GameObject texto = Instantiate(prefabTextoPuntos, posicion, Quaternion.identity);
        texto.GetComponent<TMP_Text>().text = puntos.ToString();
    }

    void Sumar(int puntos)
    {
        int antes = puntaje;
        puntaje += puntos;
        if (antes < puntosVidaExtra && puntaje >= puntosVidaExtra)
        {
            vidas++;
            audioManager.Reproducir(audioManager.vidaExtra);
        }
        if (puntaje > record) record = puntaje;
    }

    public void TocarGlitch()
    {
        if (enJuego) StartCoroutine(PerderVida());
    }

    IEnumerator EmpezarVida(float espera)
    {
        enJuego = false;
        finSusto = 0f;
        relojModo = 0f;
        persecucion = false;
        jugador.Reiniciar();
        foreach (Enemigo enemigo in enemigos) enemigo.Reiniciar();

        mensaje = "¡LISTO!";
        yield return new WaitForSeconds(espera);
        mensaje = "";
        enJuego = true;
    }

    // El susto no corre durante la pausa
    IEnumerator PausaBreve(float segundos)
    {
        enJuego = false;
        finSusto += segundos;
        yield return new WaitForSeconds(segundos);
        if (!juegoTerminado) enJuego = true;
    }

    IEnumerator PerderVida()
    {
        enJuego = false;
        audioManager.DetenerFondo();
        yield return new WaitForSeconds(1f);

        foreach (Enemigo enemigo in enemigos) enemigo.gameObject.SetActive(false);
        if (bonus != null) Destroy(bonus);
        audioManager.Reproducir(audioManager.muerte);
        yield return jugador.Morir();
        yield return new WaitForSeconds(0.5f);

        vidas--;
        if (vidas <= 0)
        {
            TerminarPartida();
            yield break;
        }
        yield return EmpezarVida(2f);
    }

    IEnumerator CompletarNivel()
    {
        enJuego = false;
        audioManager.DetenerFondo();
        yield return new WaitForSeconds(1f);

        foreach (Enemigo enemigo in enemigos) enemigo.gameObject.SetActive(false);
        if (bonus != null) Destroy(bonus);
        audioManager.Reproducir(audioManager.nivelCompleto);
        yield return laberinto.Parpadear();

        nivel++;
        puntosComidos = 0;
        laberinto.ColocarPuntos();
        yield return EmpezarVida(2f);
    }

    void TerminarPartida()
    {
        juegoTerminado = true;
        GuardarRecord();
    }

    // También al salir a media partida, para no perder un récord nuevo
    void GuardarRecord()
    {
        PlayerPrefs.SetInt("Record", record);
        PlayerPrefs.Save();
    }

    public void Reiniciar()
    {
        GuardarRecord();
        SceneManager.LoadScene("Game");
    }

    public void IrAlMenu()
    {
        GuardarRecord();
        SceneManager.LoadScene("MainMenu");
    }
}
