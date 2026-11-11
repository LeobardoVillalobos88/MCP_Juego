using System.IO;
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

    public static string Todo()
    {
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

        return "Prefabs creados";
    }

    // ---------- Escena de juego ----------

    public static string CrearEscenaJuego()
    {
        AbrirEscena("Assets/Scenes/Game.unity");
        Borrar("Laberinto");
        Borrar("Jugador");
        Borrar("GameManager");

        Camera camara = Camera.main;
        camara.orthographicSize = 13f;
        camara.clearFlags = CameraClearFlags.SolidColor;
        camara.backgroundColor = Color.black;
        camara.transform.position = new Vector3(0f, 0f, -10f);

        // El laberinto mide 27 x 24 celdas y queda centrado en la pantalla
        GameObject objetoLaberinto = ObjetoSprite("Laberinto", "Paredes", -1);
        objetoLaberinto.transform.position = new Vector3(-13.5f, -12f, 0f);
        Laberinto laberinto = objetoLaberinto.AddComponent<Laberinto>();
        laberinto.mapa = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Niveles/Laberinto.txt");
        laberinto.prefabPunto = Prefab("Assets/Prefabs/Pickups/Punto.prefab");
        laberinto.prefabNucleo = Prefab("Assets/Prefabs/Pickups/Nucleo.prefab");
        laberinto.paredes = objetoLaberinto.GetComponent<SpriteRenderer>();
        laberinto.salidaCasa = SalidaCasa;
        laberinto.centroCasa = CentroCasa;

        GameObject objetoJugador = ObjetoSprite("Jugador", "BitIzquierda0", 1);
        objetoJugador.transform.position = objetoLaberinto.transform.position + new Vector3(InicioJugador.x + 0.5f, InicioJugador.y + 0.5f, 0f);
        objetoJugador.AddComponent<MovimientoLaberinto>();
        Jugador jugador = objetoJugador.AddComponent<Jugador>();
        jugador.celdaInicio = InicioJugador;
        jugador.spritesDerecha = Sprites("BitDerecha0", "BitDerecha1");
        jugador.spritesIzquierda = Sprites("BitIzquierda0", "BitIzquierda1");
        jugador.spritesArriba = Sprites("BitArriba0", "BitArriba1");
        jugador.spritesAbajo = Sprites("BitAbajo0", "BitAbajo1");
        jugador.spritesMuerte = Sprites("BitMuerte0", "BitMuerte1", "BitMuerte2", "BitMuerte3",
                                        "BitMuerte4", "BitMuerte5", "BitMuerte6", "BitMuerte7");

        new GameObject("GameManager").AddComponent<GameManager>();

        GuardarEscena();
        return "Escena de juego creada";
    }
}
