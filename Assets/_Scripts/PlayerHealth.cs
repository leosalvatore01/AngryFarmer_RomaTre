using UnityEngine;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    public int vitaMassima = 5;

    private int vitaCorrente;
    private TMP_Text testoVita;

    void Start()
    {
        vitaCorrente = vitaMassima;
        testoVita = GameObject.Find("VitaText")?.GetComponent<TMP_Text>();
        AggiornaInterfaccia();
    }

    public void SubisciDanno(int danno)
    {
        vitaCorrente = Mathf.Max(0, vitaCorrente - danno);
        AggiornaInterfaccia();

        if (vitaCorrente <= 0 && GameManager.instance != null)
        {
            GameManager.instance.GameOverGiocatore();
        }
    }

    public void Cura(int quantita)
    {
        vitaCorrente = Mathf.Min(vitaMassima, vitaCorrente + quantita);
        AggiornaInterfaccia();
    }

    void AggiornaInterfaccia()
    {
        if (testoVita != null)
        {
            testoVita.text = "VITA: " + vitaCorrente + " / " + vitaMassima;
        }
    }
}