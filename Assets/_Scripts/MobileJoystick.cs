using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Ogni gesto appartiene a un solo dito; un secondo dito può schivare.</summary>
public sealed class MobileJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public RectTransform Manopola { get; set; }
    public int? PuntatoreAttivo { get; private set; }
    public Vector2 Valore { get; private set; }

    public void OnPointerDown(PointerEventData evento)
    {
        if (PuntatoreAttivo.HasValue || !MobileExperience.GameplayTouchAttivo) return;
        PuntatoreAttivo = evento.pointerId;
        OnDrag(evento);
    }

    public void OnDrag(PointerEventData evento)
    {
        if (PuntatoreAttivo != evento.pointerId) return;
        RectTransform rect = (RectTransform)transform;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rect, evento.position, evento.pressEventCamera, out Vector2 punto)) return;
        float raggio = Mathf.Min(rect.rect.width, rect.rect.height) * 0.34f;
        Valore = Normalizza(punto, raggio);
        if (Manopola != null) Manopola.anchoredPosition = Valore * raggio;
        FarmerInputController.CreaOTrova().ImpostaMovimentoTouch(Valore);
    }

    public static Vector2 Normalizza(Vector2 punto, float raggio)
    {
        Vector2 valore = Vector2.ClampMagnitude(punto / Mathf.Max(1f, raggio), 1f);
        return valore.sqrMagnitude < 0.01f ? Vector2.zero : valore;
    }

    public void OnPointerUp(PointerEventData evento)
    {
        if (PuntatoreAttivo == evento.pointerId) Azzera();
    }

    public void Azzera()
    {
        PuntatoreAttivo = null;
        Valore = Vector2.zero;
        if (Manopola != null) Manopola.anchoredPosition = Vector2.zero;
        if (FarmerInputController.Instance != null)
            FarmerInputController.Instance.ImpostaMovimentoTouch(Vector2.zero);
    }

    private void OnDisable() => Azzera();
    private void OnApplicationFocus(bool focus) { if (!focus) Azzera(); }
    private void OnApplicationPause(bool pausa) { if (pausa) Azzera(); }
}
