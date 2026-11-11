using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioClip punto0;
    public AudioClip punto1;
    public AudioClip nucleo;
    public AudioClip comerGlitch;
    public AudioClip muerte;
    public AudioClip inicio;
    public AudioClip nivelCompleto;
    public AudioClip vidaExtra;
    public AudioClip bonus;
    public AudioClip sirena;
    public AudioClip alarma;

    private AudioSource efectos;
    private AudioSource fondo;
    private bool alternar = false;

    void Awake()
    {
        efectos = gameObject.AddComponent<AudioSource>();
        fondo = gameObject.AddComponent<AudioSource>();
        fondo.loop = true;
        fondo.volume = 0.6f;
    }

    public void Reproducir(AudioClip clip)
    {
        efectos.PlayOneShot(clip);
    }

    // Los puntos alternan entre dos tonos
    public void ReproducirPunto()
    {
        Reproducir(alternar ? punto1 : punto0);
        alternar = !alternar;
    }

    public void Fondo(AudioClip clip)
    {
        if (fondo.clip == clip && fondo.isPlaying) return;
        fondo.clip = clip;
        fondo.Play();
    }

    public void DetenerFondo()
    {
        fondo.Stop();
    }
}
