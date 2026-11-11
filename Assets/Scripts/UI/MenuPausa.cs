using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MenuPausa : MonoBehaviour
{
    public GameObject panel;
    public GameObject botonContinuar;

    private GameManager gameManager;

    void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        panel.SetActive(false);
    }

    void Update()
    {
        if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame) return;

        // Solo se pausa con el juego en marcha (no durante una muerte ni al final)
        if (panel.activeSelf) Continuar();
        else if (gameManager.enJuego) Pausar();
    }

    public void Pausar()
    {
        panel.SetActive(true);
        Time.timeScale = 0f;
        AudioListener.pause = true;
        EventSystem.current.SetSelectedGameObject(botonContinuar);
    }

    public void Continuar()
    {
        panel.SetActive(false);
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }
}
