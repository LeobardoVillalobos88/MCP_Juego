using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipal : MonoBehaviour
{
    public TMP_Text textoRecord;
    public AudioClip clic;

    private AudioSource fuenteAudio;
    private bool cargando = false;

    void Start()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        fuenteAudio = gameObject.AddComponent<AudioSource>();
        textoRecord.text = "RÉCORD  " + PlayerPrefs.GetInt("Record", 0);
    }

    public void Jugar()
    {
        if (!cargando) StartCoroutine(Cargar());
    }

    public void Salir()
    {
        Application.Quit();
    }

    // Espera un momento para que se escuche el clic
    IEnumerator Cargar()
    {
        cargando = true;
        fuenteAudio.PlayOneShot(clic);
        yield return new WaitForSeconds(0.15f);
        SceneManager.LoadScene("Game");
    }
}
