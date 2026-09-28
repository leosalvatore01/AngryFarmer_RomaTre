using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Su touch tutta la carta è acquistabile, sempre rispettando il pulsante disabilitato.</summary>
public sealed class MobileCardPurchase : MonoBehaviour, IPointerClickHandler
{
    public Button Pulsante { get; set; }
    public void OnPointerClick(PointerEventData evento)
    {
        if (MobileExperience.Attiva && evento.button == PointerEventData.InputButton.Left &&
            Pulsante != null && Pulsante.isActiveAndEnabled && Pulsante.IsInteractable())
            Pulsante.onClick.Invoke();
    }
}
