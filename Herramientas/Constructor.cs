using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Construcción de escenas, prefabs e importación. Se ejecuta con:
// unity command script run --file Herramientas/Constructor.cs --entry Constructor.Todo
public static class Constructor
{
    const string Arte = "Assets/Art/Propios/";
    const string Plantilla = "Assets/Settings/Scenes/URP2DSceneTemplate.unity";

    // Celdas del laberinto (x desde la izquierda, y desde abajo)
    static readonly Vector2Int InicioJugador = new Vector2Int(13, 5);
    static readonly Vector2Int SalidaCasa = new Vector2Int(13, 15);
    static readonly Vector2Int CentroCasa = new Vector2Int(13, 12);
    static readonly Vector2Int CeldaBonus = new Vector2Int(13, 9);
    static readonly Vector3 OrigenLaberinto = new Vector3(-13.5f, -12f, 0f);

    public static string Todo()
    {
        ImportarTMP();
        ImportarArte();
        CrearPrefabs();
        CrearEscenaJuego();
        return "Listo";
    }

    // ---------- Utilidades ----------

    static void CrearCarpeta(string ruta)
    {
        if (!AssetDatabase.IsValidFolder(ruta))
        {
            AssetDatabase.CreateFolder(Path.GetDirectoryName(ruta).Replace('\\', '/'), Path.GetFileName(ruta));
        }
    }

    static void Borrar(string nombre)
    {
        GameObject go = GameObject.Find(nombre);
        if (go != null) Object.DestroyImmediate(go);
    }

    // Abre la escena; si no existe la crea a partir de la plantilla de URP 2D
    static void AbrirEscena(string ruta)
    {
        if (!File.Exists(ruta)) AssetDatabase.CopyAsset(Plantilla, ruta);
        EditorSceneManager.OpenScene(ruta);
    }

    static void GuardarEscena()
    {
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
    }

    static GameObject ObjetoSprite(string nombre, string sprite, int orden, Transform padre = null)
    {
        GameObject go = new GameObject(nombre);
        go.transform.SetParent(padre, false);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteDe(sprite);
        sr.sortingOrder = orden;
        return go;
    }

    static GameObject GuardarPrefab(GameObject go, string ruta)
    {
        PrefabUtility.SaveAsPrefabAsset(go, ruta);
        Object.DestroyImmediate(go);
        return AssetDatabase.LoadAssetAtPath<GameObject>(ruta);
    }

    static GameObject Prefab(string ruta)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ruta);
        if (prefab == null) throw new System.Exception("No existe el prefab " + ruta);
        return prefab;
    }

    static AudioClip Sonido(string nombre)
    {
        AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/" + nombre + ".wav");
        if (clip == null) throw new System.Exception("No existe el sonido " + nombre);
        return clip;
    }

    static Vector3 CeldaAMundo(Vector2Int celda)
    {
        return OrigenLaberinto + new Vector3(celda.x + 0.5f, celda.y + 0.5f, 0f);
    }

    static Sprite SpriteDe(string nombre)
    {
        Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(Arte + nombre + ".png");
        if (s == null) throw new System.Exception("No existe el sprite " + nombre);
        return s;
    }

    static Sprite[] Sprites(params string[] nombres)
    {
        Sprite[] sprites = new Sprite[nombres.Length];
        for (int i = 0; i < nombres.Length; i++) sprites[i] = SpriteDe(nombres[i]);
        return sprites;
    }

    // ---------- Importación ----------

    // Recursos básicos de TextMeshPro (fuente y sombreadores)
    public static string ImportarTMP()
    {
        if (AssetDatabase.IsValidFolder("Assets/TextMesh Pro")) return "TMP ya estaba importado";

        string paquete = Directory.GetDirectories("Library/PackageCache", "com.unity.ugui@*")[0]
            + "/Package Resources/TMP Essential Resources.unitypackage";
        UnityEditor.AssetPackage.Package.Import(Path.GetFullPath(paquete), false);
        return "TMP importado";
    }

    // Pixel art: 16 px por unidad, sin filtro ni compresión
    public static string ImportarArte()
    {
        foreach (string ruta in Directory.GetFiles(Arte, "*.png"))
        {
            TextureImporter importador = (TextureImporter)AssetImporter.GetAtPath(ruta.Replace('\\', '/'));
            importador.textureType = TextureImporterType.Sprite;
            importador.spriteImportMode = SpriteImportMode.Single;
            importador.spritePixelsPerUnit = 16;
            importador.filterMode = FilterMode.Point;
            importador.textureCompression = TextureImporterCompression.Uncompressed;
            importador.mipmapEnabled = false;

            // Las paredes se colocan desde la esquina de abajo a la izquierda
            TextureImporterSettings ajustes = new TextureImporterSettings();
            importador.ReadTextureSettings(ajustes);
            ajustes.spriteAlignment = Path.GetFileName(ruta) == "Paredes.png"
                ? (int)SpriteAlignment.BottomLeft
                : (int)SpriteAlignment.Center;
            importador.SetTextureSettings(ajustes);
            importador.SaveAndReimport();
        }
        return "Arte importado";
    }

    // ---------- Prefabs ----------

    public static string CrearPrefabs()
    {
        CrearCarpeta("Assets/Prefabs");
        CrearCarpeta("Assets/Prefabs/Pickups");

        GuardarPrefab(ObjetoSprite("Punto", "Punto", 0), "Assets/Prefabs/Pickups/Punto.prefab");

        GameObject nucleo = ObjetoSprite("Nucleo", "Nucleo", 0);
        nucleo.AddComponent<Parpadeo>().intervalo = 0.2f;
        GuardarPrefab(nucleo, "Assets/Prefabs/Pickups/Nucleo.prefab");

        GameObject bonus = ObjetoSprite("Bonus", "Cristal", 0);
        bonus.AddComponent<Bonus>();
        GuardarPrefab(bonus, "Assets/Prefabs/Pickups/Bonus.prefab");

        // Puntos que aparecen al comer un Glitch o el bonus
        GameObject textoPuntos = new GameObject("TextoPuntos");
        TextMeshPro texto = textoPuntos.AddComponent<TextMeshPro>();
        texto.fontSize = 5f;
        texto.fontStyle = FontStyles.Bold;
        texto.alignment = TextAlignmentOptions.Center;
        texto.color = new Color(0.5f, 1f, 1f);
        texto.sortingOrder = 5;
        texto.rectTransform.sizeDelta = new Vector2(4f, 1f);
        textoPuntos.AddComponent<Efimero>().duracion = 1f;
        GuardarPrefab(textoPuntos, "Assets/Prefabs/Pickups/TextoPuntos.prefab");

        CrearCarpeta("Assets/Prefabs/Enemigos");
        GameObject glitch = new GameObject("Glitch");
        glitch.AddComponent<MovimientoLaberinto>();
        Enemigo enemigo = glitch.AddComponent<Enemigo>();
        enemigo.cuerpo = ObjetoSprite("Cuerpo", "Glitch0", 2, glitch.transform).GetComponent<SpriteRenderer>();
        enemigo.ojos = ObjetoSprite("Ojos", "OjosAbajo", 3, glitch.transform).GetComponent<SpriteRenderer>();
        enemigo.spritesCuerpo = Sprites("Glitch0", "Glitch1");
        enemigo.spritesAsustado = Sprites("GlitchAsustado0", "GlitchAsustado1");
        enemigo.spritesAsustadoBlanco = Sprites("GlitchAsustadoBlanco0", "GlitchAsustadoBlanco1");
        enemigo.ojosDerecha = SpriteDe("OjosDerecha");
        enemigo.ojosIzquierda = SpriteDe("OjosIzquierda");
        enemigo.ojosArriba = SpriteDe("OjosArriba");
        enemigo.ojosAbajo = SpriteDe("OjosAbajo");
        GuardarPrefab(glitch, "Assets/Prefabs/Enemigos/Glitch.prefab");

        return "Prefabs creados";
    }

    static void CrearGlitch(Transform padre, TipoGlitch tipo, Color color, Vector2Int inicio, Vector2Int esquina, float tiempoEnCasa)
    {
        GameObject glitch = (GameObject)PrefabUtility.InstantiatePrefab(Prefab("Assets/Prefabs/Enemigos/Glitch.prefab"), padre);
        glitch.name = tipo.ToString();
        glitch.transform.position = CeldaAMundo(inicio);
        Enemigo enemigo = glitch.GetComponent<Enemigo>();
        enemigo.tipo = tipo;
        enemigo.color = color;
        enemigo.celdaInicio = inicio;
        enemigo.esquina = esquina;
        enemigo.tiempoEnCasa = tiempoEnCasa;
        enemigo.cuerpo.color = color;
    }

    // ---------- Escena de juego ----------

    public static string CrearEscenaJuego()
    {
        AbrirEscena("Assets/Scenes/Game.unity");
        Borrar("Laberinto");
        Borrar("Jugador");
        Borrar("GameManager");
        Borrar("Glitches");

        Camera camara = Camera.main;
        camara.orthographicSize = 13f;
        camara.clearFlags = CameraClearFlags.SolidColor;
        camara.backgroundColor = Color.black;
        camara.transform.position = new Vector3(0f, 0f, -10f);

        // El laberinto mide 27 x 24 celdas y queda centrado en la pantalla
        GameObject objetoLaberinto = ObjetoSprite("Laberinto", "Paredes", -1);
        objetoLaberinto.transform.position = OrigenLaberinto;
        Laberinto laberinto = objetoLaberinto.AddComponent<Laberinto>();
        laberinto.mapa = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Niveles/Laberinto.txt");
        laberinto.prefabPunto = Prefab("Assets/Prefabs/Pickups/Punto.prefab");
        laberinto.prefabNucleo = Prefab("Assets/Prefabs/Pickups/Nucleo.prefab");
        laberinto.paredes = objetoLaberinto.GetComponent<SpriteRenderer>();
        laberinto.salidaCasa = SalidaCasa;
        laberinto.centroCasa = CentroCasa;

        GameObject objetoJugador = ObjetoSprite("Jugador", "BitIzquierda0", 1);
        objetoJugador.transform.position = CeldaAMundo(InicioJugador);
        objetoJugador.AddComponent<MovimientoLaberinto>();
        Jugador jugador = objetoJugador.AddComponent<Jugador>();
        jugador.celdaInicio = InicioJugador;
        jugador.spritesDerecha = Sprites("BitDerecha0", "BitDerecha1");
        jugador.spritesIzquierda = Sprites("BitIzquierda0", "BitIzquierda1");
        jugador.spritesArriba = Sprites("BitArriba0", "BitArriba1");
        jugador.spritesAbajo = Sprites("BitAbajo0", "BitAbajo1");
        jugador.spritesMuerte = Sprites("BitMuerte0", "BitMuerte1", "BitMuerte2", "BitMuerte3",
                                        "BitMuerte4", "BitMuerte5", "BitMuerte6", "BitMuerte7");

        // Esquinas fuera del laberinto para que la dispersión los lleve a cada orilla
        Transform glitches = new GameObject("Glitches").transform;
        CrearGlitch(glitches, TipoGlitch.Rastreador, new Color(1f, 0.25f, 0.3f), SalidaCasa, new Vector2Int(25, 25), 0f);
        CrearGlitch(glitches, TipoGlitch.Emboscador, new Color(1f, 0.5f, 0.85f), new Vector2Int(13, 12), new Vector2Int(1, 25), 2f);
        CrearGlitch(glitches, TipoGlitch.Erratico, new Color(0.65f, 0.45f, 1f), new Vector2Int(11, 12), new Vector2Int(25, -2), 5f);
        CrearGlitch(glitches, TipoGlitch.Timido, new Color(1f, 0.6f, 0.2f), new Vector2Int(15, 12), new Vector2Int(1, -2), 8f);

        GameObject objetoManager = new GameObject("GameManager");
        GameManager gameManager = objetoManager.AddComponent<GameManager>();
        gameManager.prefabBonus = Prefab("Assets/Prefabs/Pickups/Bonus.prefab");
        gameManager.prefabTextoPuntos = Prefab("Assets/Prefabs/Pickups/TextoPuntos.prefab");
        gameManager.celdaBonus = CeldaBonus;

        AudioManager audio = objetoManager.AddComponent<AudioManager>();
        audio.punto0 = Sonido("Punto0");
        audio.punto1 = Sonido("Punto1");
        audio.nucleo = Sonido("Nucleo");
        audio.comerGlitch = Sonido("ComerGlitch");
        audio.muerte = Sonido("Muerte");
        audio.inicio = Sonido("Inicio");
        audio.nivelCompleto = Sonido("NivelCompleto");
        audio.vidaExtra = Sonido("VidaExtra");
        audio.bonus = Sonido("Bonus");
        audio.clic = Sonido("Clic");
        audio.sirena = Sonido("Sirena");
        audio.alarma = Sonido("Alarma");

        GuardarEscena();
        return "Escena de juego creada";
    }
}
