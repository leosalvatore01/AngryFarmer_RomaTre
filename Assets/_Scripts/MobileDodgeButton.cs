using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public sealed class MobileDodgeButton : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData evento)
    {
        // La schivata parte alla pressione, non quando il dito viene sollevato.
        if (evento.button == PointerEventData.InputButton.Left && GetComponent<Button>().IsInteractable())
            FarmerInputController.CreaOTrova().RichiediSchivataTouch();
    }
}
