using UnityEngine;
using System.IO;

public class SpriteScreenshot : MonoBehaviour
{
    [Header("Cámara para capturar")]
    public Camera targetCamera;

    [Header("Tamaño de la imagen")]
    public int width = 512;
    public int height = 512;

    [Header("Nombre del archivo")]
    public string fileName = "sprite";

    void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    [ContextMenu("Tomar Captura")]
    public void TakeScreenshot()
    {
        // Guardar configuración original
        CameraClearFlags originalClearFlags = targetCamera.clearFlags;
        Color originalBackground = targetCamera.backgroundColor;

        // Fondo transparente (usa Color negro con alpha 0)
        targetCamera.clearFlags = CameraClearFlags.SolidColor;
        targetCamera.backgroundColor = new Color(0, 0, 0, 0);

        // Crear textura temporal
        RenderTexture rt = new RenderTexture(width, height, 24);
        targetCamera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGBA32, false);

        // Renderizar la cámara
        targetCamera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenShot.Apply();

        // Restaurar configuración
        targetCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);
        targetCamera.clearFlags = originalClearFlags;
        targetCamera.backgroundColor = originalBackground;

        // Guardar como PNG
        byte[] bytes = screenShot.EncodeToPNG();
        string path = Path.Combine(Application.dataPath, fileName + ".png");
        File.WriteAllBytes(path, bytes);

        Debug.Log("✅ Sprite guardado en: " + path);
    }
}
