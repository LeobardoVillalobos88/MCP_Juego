using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

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
        CrearFuente();
        CrearPrefabs();
        CrearEscenaJuego();
        CrearEscenaMenu();
        ConfigurarBuild();
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

    // Borra los objetos raíz con ese nombre, aunque estén desactivados
    static void Borrar(string nombre)
    {
        foreach (GameObject go in EditorSceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (go.name == nombre) Object.DestroyImmediate(go);
        }
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
        texto.font = Fuente();
        texto.fontSize = 5f;
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
        audio.sirena = Sonido("Sirena");
        audio.alarma = Sonido("Alarma");

        CrearHUD(gameManager);
        AgregarBrillo();

        GuardarEscena();
        return "Escena de juego creada";
    }

    // ---------- Interfaz ----------

    static readonly Color Cian = new Color(0.4f, 0.95f, 1f);
    static readonly Color Rosa = new Color(1f, 0.4f, 0.85f);
    static readonly Color Amarillo = new Color(1f, 0.9f, 0.3f);
    static readonly Color AzulOscuro = new Color(0.04f, 0.06f, 0.2f);

    // Fuente pixel propia, con los caracteres del juego ya dibujados en el atlas.
    // Si el asset ya existe solo se vuelve a dibujar, para no romper las referencias.
    public static string CrearFuente()
    {
        string ruta = "Assets/Fonts/PixelNeon SDF.asset";
        AssetDatabase.ImportAsset("Assets/Fonts/PixelNeon.ttf", ImportAssetOptions.ForceUpdate);
        TMP_FontAsset asset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(ruta);

        if (asset == null)
        {
            Font fuente = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/PixelNeon.ttf");
            asset = TMP_FontAsset.CreateFontAsset(fuente, 70, 7, GlyphRenderMode.SDFAA, 512, 512);
            AssetDatabase.CreateAsset(asset, ruta);
            asset.atlasTexture.name = "PixelNeon Atlas";
            AssetDatabase.AddObjectToAsset(asset.atlasTexture, asset);
            asset.material.name = "PixelNeon Material";
            AssetDatabase.AddObjectToAsset(asset.material, asset);
        }

        asset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
        asset.ClearFontAssetData();
        if (!asset.TryAddCharacters("ABCDEFGHIJKLMNOPQRSTUVWXYZÁÉÍÓÚÑabcdefghijklmnopqrstuvwxyzáéíóúñ0123456789!¡?.,:-/+ ", out string faltantes))
        {
            throw new System.Exception("La fuente no tiene los caracteres: " + faltantes);
        }
        asset.atlasPopulationMode = AtlasPopulationMode.Static;
        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
        return "Fuente lista";
    }

    static TMP_FontAsset Fuente()
    {
        return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/PixelNeon SDF.asset");
    }

    static RectTransform CrearRect(string nombre, Transform padre, Vector2 ancla, Vector2 posicion, Vector2 tamano)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform));
        go.transform.SetParent(padre, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = ancla;
        rect.anchorMax = ancla;
        rect.anchoredPosition = posicion;
        rect.sizeDelta = tamano;
        return rect;
    }

    static TMP_Text Texto(string nombre, Transform padre, string contenido, float tamano, Color color, Vector2 ancla, Vector2 posicion)
    {
        RectTransform rect = CrearRect(nombre, padre, ancla, posicion, new Vector2(900f, tamano * 1.4f));
        TextMeshProUGUI texto = rect.gameObject.AddComponent<TextMeshProUGUI>();
        texto.font = Fuente();
        texto.text = contenido;
        texto.fontSize = tamano;
        texto.color = color;
        texto.alignment = TextAlignmentOptions.Center;
        texto.textWrappingMode = TextWrappingModes.NoWrap;
        return texto;
    }

    static Image Imagen(string nombre, Transform padre, string sprite, Vector2 ancla, Vector2 posicion, float tamano, Color color)
    {
        Image imagen = CrearRect(nombre, padre, ancla, posicion, new Vector2(tamano, tamano)).gameObject.AddComponent<Image>();
        imagen.sprite = SpriteDe(sprite);
        imagen.color = color;
        imagen.preserveAspect = true;
        return imagen;
    }

    static GameObject Boton(string nombre, Transform padre, string contenido, Vector2 posicion, UnityAction accion)
    {
        // Marco cian con el fondo encima, 4 px más chico por lado
        RectTransform rect = CrearRect(nombre, padre, new Vector2(0.5f, 0.5f), posicion, new Vector2(420f, 80f));
        rect.gameObject.AddComponent<Image>().color = Cian;
        RectTransform rectFondo = CrearRect("Fondo", rect, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(412f, 72f));
        Image fondo = rectFondo.gameObject.AddComponent<Image>();

        Button boton = rect.gameObject.AddComponent<Button>();
        ColorBlock colores = boton.colors;
        colores.normalColor = AzulOscuro;
        colores.highlightedColor = new Color(0.15f, 0.25f, 0.7f);
        colores.selectedColor = colores.highlightedColor;
        colores.pressedColor = new Color(0.3f, 0.7f, 0.9f);
        boton.colors = colores;
        boton.targetGraphic = fondo;
        UnityEventTools.AddPersistentListener(boton.onClick, accion);

        TMP_Text texto = Texto("Texto", rect, contenido, 40f, Cian, new Vector2(0.5f, 0.5f), Vector2.zero);
        texto.rectTransform.sizeDelta = new Vector2(420f, 80f);
        texto.alignment = TextAlignmentOptions.Center;
        return rect.gameObject;
    }

    static GameObject PanelCompleto(string nombre, Transform padre)
    {
        RectTransform rect = CrearRect(nombre, padre, Vector2.zero, Vector2.zero, Vector2.zero);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0.03f, 0.85f);
        return rect.gameObject;
    }

    static Canvas CrearCanvas()
    {
        GameObject go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler escala = go.GetComponent<CanvasScaler>();
        escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escala.referenceResolution = new Vector2(1920f, 1080f);
        escala.matchWidthOrHeight = 1f;

        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        return canvas;
    }

    // Marcador a los lados del laberinto, mensaje de inicio, pausa y fin de partida
    static void CrearHUD(GameManager gameManager)
    {
        Borrar("Canvas");
        Borrar("EventSystem");
        Transform canvas = CrearCanvas().transform;
        HUD hud = canvas.gameObject.AddComponent<HUD>();

        Vector2 izquierda = new Vector2(0f, 1f);
        Vector2 derecha = new Vector2(1f, 1f);
        Texto("EtiquetaPuntaje", canvas, "PUNTAJE", 40f, Rosa, izquierda, new Vector2(230f, -160f));
        hud.textoPuntaje = Texto("Puntaje", canvas, "0", 50f, Color.white, izquierda, new Vector2(230f, -230f));
        Texto("EtiquetaRecord", canvas, "RÉCORD", 40f, Rosa, izquierda, new Vector2(230f, -360f));
        hud.textoRecord = Texto("Record", canvas, "0", 50f, Color.white, izquierda, new Vector2(230f, -430f));

        Texto("EtiquetaNivel", canvas, "NIVEL", 40f, Rosa, derecha, new Vector2(-230f, -160f));
        hud.textoNivel = Texto("Nivel", canvas, "1", 50f, Color.white, derecha, new Vector2(-230f, -230f));
        Texto("EtiquetaVidas", canvas, "VIDAS", 40f, Rosa, derecha, new Vector2(-230f, -360f));
        hud.iconosVidas = new Image[5];
        for (int i = 0; i < hud.iconosVidas.Length; i++)
        {
            hud.iconosVidas[i] = Imagen("Vida" + i, canvas, "BitIzquierda0", derecha, new Vector2(-350f + i * 60f, -430f), 56f, Color.white);
        }

        // Debajo de la casa de los Glitches
        hud.textoMensaje = Texto("Mensaje", canvas, "", 40f, Amarillo, new Vector2(0.5f, 0.5f), new Vector2(0f, -104f));

        GameObject pausa = PanelCompleto("PanelPausa", canvas);
        MenuPausa menuPausa = canvas.gameObject.AddComponent<MenuPausa>();
        menuPausa.panel = pausa;
        Texto("Titulo", pausa.transform, "PAUSA", 90f, Cian, new Vector2(0.5f, 0.5f), new Vector2(0f, 180f));
        menuPausa.botonContinuar = Boton("Continuar", pausa.transform, "CONTINUAR", new Vector2(0f, 0f), menuPausa.Continuar);
        Boton("Menu", pausa.transform, "MENÚ", new Vector2(0f, -120f), gameManager.IrAlMenu);

        GameObject fin = PanelCompleto("PanelFinPartida", canvas);
        hud.panelFinPartida = fin;
        Texto("Titulo", fin.transform, "FIN DEL JUEGO", 90f, Rosa, new Vector2(0.5f, 0.5f), new Vector2(0f, 250f));
        hud.textoFinal = Texto("Resultado", fin.transform, "", 50f, Color.white, new Vector2(0.5f, 0.5f), new Vector2(0f, 90f));
        hud.textoFinal.rectTransform.sizeDelta = new Vector2(900f, 160f);
        hud.botonReintentar = Boton("Reintentar", fin.transform, "REINTENTAR", new Vector2(0f, -80f), gameManager.Reiniciar);
        Boton("Menu", fin.transform, "MENÚ", new Vector2(0f, -200f), gameManager.IrAlMenu);
    }

    // ---------- Menú principal ----------

    public static string CrearEscenaMenu()
    {
        AbrirEscena("Assets/Scenes/MainMenu.unity");
        Borrar("Canvas");
        Borrar("EventSystem");

        Camera camara = Camera.main;
        camara.clearFlags = CameraClearFlags.SolidColor;
        camara.backgroundColor = Color.black;

        Transform canvas = CrearCanvas().transform;
        MenuPrincipal menu = canvas.gameObject.AddComponent<MenuPrincipal>();
        menu.clic = Sonido("Clic");

        Vector2 centro = new Vector2(0.5f, 0.5f);
        Texto("Titulo", canvas, "LABERINTO", 130f, Cian, centro, new Vector2(0f, 360f));
        Texto("Subtitulo", canvas, "NEÓN", 130f, Rosa, centro, new Vector2(0f, 220f));
        menu.textoRecord = Texto("Record", canvas, "RÉCORD  0", 40f, Amarillo, centro, new Vector2(0f, 110f));

        // Desfile de personajes
        Imagen("Bit", canvas, "BitDerecha0", centro, new Vector2(-400f, 0f), 96f, Color.white);
        Texto("NombreBit", canvas, "BIT", 30f, Cian, centro, new Vector2(-400f, -75f));
        string[] nombres = { "RASTREADOR", "EMBOSCADOR", "ERRÁTICO", "TÍMIDO" };
        Color[] colores = { new Color(1f, 0.25f, 0.3f), new Color(1f, 0.5f, 0.85f), new Color(0.65f, 0.45f, 1f), new Color(1f, 0.6f, 0.2f) };
        for (int i = 0; i < nombres.Length; i++)
        {
            Vector2 posicion = new Vector2(-200f + i * 200f, 0f);
            Image cuerpo = Imagen("Glitch" + i, canvas, "Glitch0", centro, posicion, 96f, colores[i]);
            Imagen("Ojos", cuerpo.transform, "OjosIzquierda", centro, Vector2.zero, 96f, Color.white);
            Texto("Nombre" + i, canvas, nombres[i], 24f, colores[i], centro, posicion + new Vector2(0f, -75f));
        }

        // Jugar queda seleccionado para empezar con Enter
        GameObject jugar = Boton("Jugar", canvas, "JUGAR", new Vector2(0f, -200f), menu.Jugar);
        Boton("Salir", canvas, "SALIR", new Vector2(0f, -320f), menu.Salir);
        Object.FindAnyObjectByType<EventSystem>().firstSelectedGameObject = jugar;
        Texto("Controles", canvas, "WASD / FLECHAS: MOVERSE     ESC: PAUSA     ENTER: ELEGIR", 26f, new Color(0.6f, 0.7f, 1f), centro, new Vector2(0f, -460f));

        AgregarBrillo();

        GuardarEscena();
        return "Menú creado";
    }

    // ---------- Brillo de neón ----------

    static VolumeProfile PerfilNeon()
    {
        string ruta = "Assets/Settings/Neon.asset";
        VolumeProfile perfil = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ruta);
        if (perfil == null)
        {
            perfil = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(perfil, ruta);
        }

        if (!perfil.TryGet(out Bloom bloom))
        {
            bloom = perfil.Add<Bloom>(true);
            AssetDatabase.AddObjectToAsset(bloom, perfil);
        }
        bloom.threshold.Override(0.55f);
        bloom.intensity.Override(1.6f);
        bloom.scatter.Override(0.65f);
        EditorUtility.SetDirty(perfil);
        AssetDatabase.SaveAssets();
        return perfil;
    }

    // Activa el posprocesado en la cámara y agrega el volumen con el Bloom
    static void AgregarBrillo()
    {
        Borrar("Brillo");
        Camera.main.GetUniversalAdditionalCameraData().renderPostProcessing = true;
        Volume volumen = new GameObject("Brillo").AddComponent<Volume>();
        volumen.isGlobal = true;
        volumen.sharedProfile = PerfilNeon();
    }

    // Escenas del build: primero el menú y luego el juego
    public static string ConfigurarBuild()
    {
        if (File.Exists("Assets/Scenes/SampleScene.unity")) AssetDatabase.DeleteAsset("Assets/Scenes/SampleScene.unity");
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Game.unity", true),
        };
        PlayerSettings.productName = "Laberinto Neón";
        return "Build configurado";
    }
}
