using UnityEngine;
using UnityEngine.InputSystem;

public class MenuPausa : MonoBehaviour
{
    public GameObject panel;

    private GameManager gameManager;

    void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        panel.SetActive(false);
    }

    void Update()
    {
        if (gameManager.juegoTerminado || Keyboard.current == null) return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (panel.activeSelf) Continuar();
            else Pausar();
        }
    }

    public void Pausar()
    {
        panel.SetActive(true);
        Time.timeScale = 0f;
        AudioListener.pause = true;
    }

    public void Continuar()
    {
        panel.SetActive(false);
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }
}
