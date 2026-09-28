using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Adatta gli ancoraggi senza cambiare la gerarchia usata dagli HUD esistenti.</summary>
[RequireComponent(typeof(Canvas))]
public sealed class MobileSafeAreaCanvas : MonoBehaviour
{
    private readonly Dictionary<RectTransform, Vector4> ancoraggi = new Dictionary<RectTransform, Vector4>();
    private CanvasScaler scaler;
    private Vector2 risoluzioneBase;
    private Rect ultimaArea;
    private Vector2 ultimaDimensione;
    private int numeroFigli = -1;

    private void Awake()
    {
        scaler = GetComponent<CanvasScaler>();
        if (scaler != null) risoluzioneBase = scaler.referenceResolution;
    }

    public static Rect Normalizza(Rect area, float larghezza, float altezza)
    {
        if (larghezza <= 0 || altezza <= 0 || area.width <= 0 || area.height <= 0)
            return new Rect(0, 0, 1, 1);
        float x = Mathf.Clamp01(area.xMin / larghezza);
        float y = Mathf.Clamp01(area.yMin / altezza);
        return Rect.MinMaxRect(x, y, Mathf.Clamp(area.xMax / larghezza, x, 1),
            Mathf.Clamp(area.yMax / altezza, y, 1));
    }

    public static Vector2 MappaAncora(Vector2 ancora, Rect area)
        => area.min + Vector2.Scale(ancora, area.size);

    private void LateUpdate()
    {
        if (!MobileExperience.Attiva) return;
        Rect area = Normalizza(Screen.safeArea, Screen.width, Screen.height);
        Vector2 dimensione = new Vector2(Screen.width, Screen.height);
        if (area == ultimaArea && dimensione == ultimaDimensione && numeroFigli == transform.childCount) return;
        ultimaArea = area;
        ultimaDimensione = dimensione;
        numeroFigli = transform.childCount;
        if (scaler != null)
        {
            // La superficie utile mantiene almeno lo spazio di progetto anche sui tablet.
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            scaler.referenceResolution = new Vector2(risoluzioneBase.x / Mathf.Max(.1f, area.width),
                risoluzioneBase.y / Mathf.Max(.1f, area.height));
        }
        foreach (Transform figlio in transform)
        {
            RectTransform rect = figlio as RectTransform;
            if (rect == null) continue;
            if (!ancoraggi.TryGetValue(rect, out Vector4 originale))
            {
                originale = new Vector4(rect.anchorMin.x, rect.anchorMin.y, rect.anchorMax.x, rect.anchorMax.y);
                ancoraggi.Add(rect, originale);
            }
            rect.anchorMin = MappaAncora(new Vector2(originale.x, originale.y), area);
            rect.anchorMax = MappaAncora(new Vector2(originale.z, originale.w), area);
        }
    }

    private void OnTransformChildrenChanged() => numeroFigli = -1;
}
