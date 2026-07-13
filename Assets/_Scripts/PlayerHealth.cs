using UnityEngine;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    public int vitaMassima = 5;

    private int vitaCorrente;
    private TMP_Text testoVita;

    public int VitaCorrente => vitaCorrente;

    void Start()
    {
        vitaCorrente = vitaMassima;
        testoVita = GameManager.TrovaTestoInterfaccia("VitaText");
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
            float rapporto = vitaMassima > 0
                ? (float)vitaCorrente / vitaMassima
                : 0f;

            if (rapporto > 0.5f)
            {
                testoVita.color = new Color(0.5f, 0.95f, 0.47f, 1f);
            }
            else if (rapporto > 0.25f)
            {
                testoVita.color = new Color(1f, 0.78f, 0.24f, 1f);
            }
            else
            {
                testoVita.color = new Color(1f, 0.28f, 0.2f, 1f);
            }

            testoVita.text = "Salute   " + vitaCorrente + " / " + vitaMassima;
        }
    }
}
