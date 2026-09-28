using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Render fuori schermo per verificare le UI anche nei test batch.</summary>
public static class MobileVisualCapture
{
    public static void Salva(string nome, int larghezza = 1920, int altezza = 1080)
    {
        Camera camera = Camera.main;
        bool temporanea = camera == null;
        if (temporanea) camera = new GameObject("CameraVerificaUI", typeof(Camera)).GetComponent<Camera>();
        RenderTexture precedente = camera.targetTexture;
        RenderTexture attiva = RenderTexture.active;
        var rt = new RenderTexture(larghezza, altezza, 24);
        var immagine = new Texture2D(larghezza, altezza, TextureFormat.RGB24, false);
        var canvasSalvati = new List<(Canvas canvas, RenderMode modo, Camera camera, float distanza, int layer, int ordine)>();
        try
        {
            camera.targetTexture = rt;
            foreach (Canvas canvas in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            {
                if (!canvas.isRootCanvas || canvas.renderMode != RenderMode.ScreenSpaceOverlay) continue;
                canvasSalvati.Add((canvas, canvas.renderMode, canvas.worldCamera, canvas.planeDistance,
                    canvas.sortingLayerID, canvas.sortingOrder));
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera;
                canvas.planeDistance = 1;
                canvas.sortingLayerID = SortingLayer.layers[SortingLayer.layers.Length - 1].id;
                canvas.sortingOrder += 20000;
                canvas.GetComponent<CanvasScaler>()?.SendMessage("Update", SendMessageOptions.DontRequireReceiver);
            }
            Canvas.ForceUpdateCanvases();
            camera.Render();
            RenderTexture.active = rt;
            immagine.ReadPixels(new Rect(0, 0, larghezza, altezza), 0, 0);
            immagine.Apply();
            Directory.CreateDirectory("Logs/MobilePreview");
            File.WriteAllBytes("Logs/MobilePreview/" + nome + ".png", immagine.EncodeToPNG());
        }
        finally
        {
            foreach (var salvato in canvasSalvati)
            {
                salvato.canvas.renderMode = salvato.modo;
                salvato.canvas.worldCamera = salvato.camera;
                salvato.canvas.planeDistance = salvato.distanza;
                salvato.canvas.sortingLayerID = salvato.layer;
                salvato.canvas.sortingOrder = salvato.ordine;
            }
            camera.targetTexture = precedente;
            RenderTexture.active = attiva;
            Object.DestroyImmediate(immagine);
            Object.DestroyImmediate(rt);
            if (temporanea) Object.DestroyImmediate(camera.gameObject);
            Canvas.ForceUpdateCanvases();
        }
    }
}
