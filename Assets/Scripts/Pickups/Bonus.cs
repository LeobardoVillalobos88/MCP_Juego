using UnityEngine;

public class Bonus : MonoBehaviour
{
    public float duracion = 9f;

    private GameManager gameManager;
    private Jugador jugador;
    private float reloj = 0f;

    void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        jugador = FindAnyObjectByType<Jugador>();
    }

    void Update()
    {
        if (!gameManager.enJuego) return;

        reloj += Time.deltaTime;
        if (reloj >= duracion)
        {
            Destroy(gameObject);
            return;
        }

        if (Vector2.Distance(transform.position, jugador.transform.position) < 0.5f)
        {
            gameManager.ComerBonus(transform.position);
            Destroy(gameObject);
        }
    }
}
