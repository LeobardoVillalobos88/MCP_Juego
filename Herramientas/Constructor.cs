using System.IO;
using UnityEditor;
using UnityEngine;

// Construcción de escenas, prefabs e importación. Se ejecuta con:
// unity command script run --file Herramientas/Constructor.cs --entry Constructor.Todo
public static class Constructor
{
    const string Arte = "Assets/Art/Propios/";

    public static string Todo()
    {
        ImportarArte();
        return "Listo";
    }

    // ---------- Utilidades ----------

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
}
