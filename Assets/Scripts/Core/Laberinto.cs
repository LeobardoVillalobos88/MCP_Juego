using System.Collections;
using UnityEngine;

public class Laberinto : MonoBehaviour
{
    public TextAsset mapa;
    public GameObject prefabPunto;
    public GameObject prefabNucleo;
    public SpriteRenderer paredes;
    public Color colorParedes = new Color(0.25f, 0.45f, 1f);
    public Vector2Int salidaCasa;
    public Vector2Int centroCasa;

    [HideInInspector] public int ancho;
    [HideInInspector] public int alto;
    [HideInInspector] public int puntosRestantes;

    private char[,] celdas;
    private GameObject[,] puntos;

    void Awake()
    {
        LeerMapa();
        paredes.color = colorParedes;
    }

    void LeerMapa()
    {
        string[] filas = mapa.text.Replace("\r", "").TrimEnd('\n').Split('\n');
        alto = filas.Length;
        ancho = 0;
        foreach (string fila in filas) ancho = Mathf.Max(ancho, fila.Length);

        // La primera fila del texto es la de arriba (y más alta)
        celdas = new char[ancho, alto];
        for (int y = 0; y < alto; y++)
        {
            string fila = filas[alto - 1 - y].PadRight(ancho);
            for (int x = 0; x < ancho; x++) celdas[x, y] = fila[x];
        }
    }

    public void ColocarPuntos()
    {
        // Los que quedaron del nivel anterior
        if (puntos != null)
        {
            foreach (GameObject punto in puntos) if (punto != null) Destroy(punto);
        }

        puntos = new GameObject[ancho, alto];
        puntosRestantes = 0;
        for (int y = 0; y < alto; y++)
        {
            for (int x = 0; x < ancho; x++)
            {
                char c = celdas[x, y];
                if (c != '.' && c != 'o') continue;

                GameObject prefab = c == '.' ? prefabPunto : prefabNucleo;
                puntos[x, y] = Instantiate(prefab, CeldaAMundo(new Vector2Int(x, y)), Quaternion.identity, transform);
                puntosRestantes++;
            }
        }
    }

    // Devuelve '.' si comió un punto, 'o' si fue un núcleo y ' ' si no había nada
    public char Comer(Vector2Int celda)
    {
        if (!Dentro(celda) || puntos[celda.x, celda.y] == null) return ' ';

        Destroy(puntos[celda.x, celda.y]);
        puntos[celda.x, celda.y] = null;
        puntosRestantes--;
        return celdas[celda.x, celda.y];
    }

    public bool Dentro(Vector2Int celda)
    {
        return celda.x >= 0 && celda.x < ancho && celda.y >= 0 && celda.y < alto;
    }

    public bool EsLibre(Vector2Int celda, bool puedeCruzarPuerta)
    {
        if (celda.y < 0 || celda.y >= alto) return false;

        // Fuera de los lados solo se puede pasar por el túnel
        if (celda.x < 0) return celdas[0, celda.y] != '#';
        if (celda.x >= ancho) return celdas[ancho - 1, celda.y] != '#';

        char c = celdas[celda.x, celda.y];
        if (c == '#') return false;
        if (c == '-') return puedeCruzarPuerta;
        return true;
    }

    public bool EnCasa(Vector2Int celda)
    {
        return Dentro(celda) && (celdas[celda.x, celda.y] == 'C' || celdas[celda.x, celda.y] == '-');
    }

    public Vector3 CeldaAMundo(Vector2Int celda)
    {
        return transform.position + new Vector3(celda.x + 0.5f, celda.y + 0.5f, 0f);
    }

    public IEnumerator Parpadear()
    {
        for (int i = 0; i < 4; i++)
        {
            paredes.color = Color.white;
            yield return new WaitForSeconds(0.2f);
            paredes.color = colorParedes;
            yield return new WaitForSeconds(0.2f);
        }
    }
}
