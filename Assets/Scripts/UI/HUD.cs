using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public TMP_Text textoPuntaje;
    public TMP_Text textoRecord;
    public TMP_Text textoNivel;
    public TMP_Text textoMensaje;
    public Image[] iconosVidas;

    [Header("Fin de partida")]
    public GameObject panelFinPartida;
    public TMP_Text textoFinal;

    private GameManager gameManager;

    void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        panelFinPartida.SetActive(false);
    }

    void Update()
    {
        textoPuntaje.text = gameManager.puntaje.ToString();
        textoRecord.text = gameManager.record.ToString();
        textoNivel.text = gameManager.nivel.ToString();
        textoMensaje.text = gameManager.mensaje;

        for (int i = 0; i < iconosVidas.Length; i++)
        {
            iconosVidas[i].enabled = i < gameManager.vidas;
        }

        if (gameManager.juegoTerminado && !panelFinPartida.activeSelf)
        {
            panelFinPartida.SetActive(true);
            textoFinal.text = "PUNTAJE  " + gameManager.puntaje + "\nRÉCORD  " + gameManager.record;
        }

        // Enter para jugar otra vez
        if (panelFinPartida.activeSelf && Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame)
        {
            gameManager.Reiniciar();
        }
    }
}
