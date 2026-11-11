using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Jugador : MonoBehaviour
{
    public Vector2Int celdaInicio;
    public Sprite[] spritesDerecha;
    public Sprite[] spritesIzquierda;
    public Sprite[] spritesArriba;
    public Sprite[] spritesAbajo;
    public Sprite[] spritesMuerte;
    [HideInInspector] public Vector2Int mirando = Vector2Int.left;

    private MovimientoLaberinto movimiento;
    private SpriteRenderer sr;
    private GameManager gameManager;
    private Vector2Int direccionDeseada;

    void Awake()
    {
        movimiento = GetComponent<MovimientoLaberinto>();
        sr = GetComponent<SpriteRenderer>();
        gameManager = FindAnyObjectByType<GameManager>();
    }

    public Vector2Int Celda()
    {
        return movimiento.celda;
    }

    public void Reiniciar()
    {
        movimiento.Colocar(celdaInicio);
        movimiento.velocidad = gameManager.velocidadJugador * gameManager.MultiplicadorVelocidad();
        // Espera quieto hasta que se presione una dirección
        direccionDeseada = Vector2Int.zero;
        mirando = Vector2Int.left;
        sr.enabled = true;
        sr.sprite = spritesIzquierda[0];
    }

    void Update()
    {
        if (!gameManager.enJuego || Time.timeScale == 0f) return;

        LeerTeclado();

        // Media vuelta inmediata, sin esperar a llegar a la celda
        if (direccionDeseada == -movimiento.direccion) movimiento.DarVuelta();

        if (movimiento.Avanzar())
        {
            gameManager.Comer(movimiento.celda);

            // La dirección presionada se aplica en cuanto el pasillo lo permite
            if (movimiento.PuedeIr(direccionDeseada)) movimiento.direccion = direccionDeseada;
            else if (!movimiento.PuedeIr(movimiento.direccion)) movimiento.direccion = Vector2Int.zero;
        }

        Animar();
    }

    void LeerTeclado()
    {
        Keyboard teclado = Keyboard.current;
        if (teclado == null) return;

        if (teclado.leftArrowKey.isPressed || teclado.aKey.isPressed) direccionDeseada = Vector2Int.left;
        if (teclado.rightArrowKey.isPressed || teclado.dKey.isPressed) direccionDeseada = Vector2Int.right;
        if (teclado.upArrowKey.isPressed || teclado.wKey.isPressed) direccionDeseada = Vector2Int.up;
        if (teclado.downArrowKey.isPressed || teclado.sKey.isPressed) direccionDeseada = Vector2Int.down;
    }

    void Animar()
    {
        bool caminando = movimiento.direccion != Vector2Int.zero;
        if (caminando) mirando = movimiento.direccion;

        Sprite[] sprites = spritesIzquierda;
        if (mirando == Vector2Int.right) sprites = spritesDerecha;
        else if (mirando == Vector2Int.up) sprites = spritesArriba;
        else if (mirando == Vector2Int.down) sprites = spritesAbajo;

        int cuadro = caminando ? (int)(Time.time * 8f) % sprites.Length : 0;
        sr.sprite = sprites[cuadro];
    }

    public IEnumerator Morir()
    {
        foreach (Sprite sprite in spritesMuerte)
        {
            sr.sprite = sprite;
            yield return new WaitForSeconds(0.12f);
        }
        sr.enabled = false;
    }
}
