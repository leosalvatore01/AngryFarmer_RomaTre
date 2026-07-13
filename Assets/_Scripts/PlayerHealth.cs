using System;
using UnityEngine;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    public int vitaMassima = 5;

    private int vitaCorrente;
    private TMP_Text testoVita;
    private int frequenzaBlocco;
    private int colpiContati;

    public int VitaCorrente => vitaCorrente;
    public int VitaMassima => vitaMassima;
    public bool VitaPiena => vitaCorrente >= vitaMassima;
    public int FrequenzaBlocco => frequenzaBlocco;

    public event Action VitaCambiata;

    void Start()
    {
        vitaCorrente = vitaMassima;
        testoVita = GameManager.TrovaTestoInterfaccia("VitaText");
        AggiornaInterfaccia();
    }

    public void SubisciDanno(int danno)
    {
        if (danno <= 0 || vitaCorrente <= 0) return;

        if (frequenzaBlocco > 0)
        {
            colpiContati++;
            if (colpiContati >= frequenzaBlocco)
            {
                colpiContati = 0;
                Debug.Log("Il contadino ha resistito al colpo.", this);
                return;
            }
        }

        vitaCorrente = Mathf.Max(0, vitaCorrente - danno);
        AggiornaInterfaccia();
        VitaCambiata?.Invoke();

        if (vitaCorrente <= 0 && GameManager.instance != null)
        {
            GameManager.instance.GameOverGiocatore();
        }
    }

    public void Cura(int quantita)
    {
        if (quantita <= 0 || VitaPiena) return;

        int vitaPrecedente = vitaCorrente;
        vitaCorrente = Mathf.Min(vitaMassima, vitaCorrente + quantita);
        AggiornaInterfaccia();
        if (vitaCorrente != vitaPrecedente)
        {
            VitaCambiata?.Invoke();
        }
    }

    public void AumentaVitaMassima(int quantita, int curaBonus = 1)
    {
        if (quantita <= 0) return;

        vitaMassima += quantita;
        vitaCorrente = Mathf.Min(
            vitaMassima,
            vitaCorrente + Mathf.Max(0, curaBonus)
        );
        AggiornaInterfaccia();
        VitaCambiata?.Invoke();
    }

    public void ImpostaFrequenzaBlocco(int ogniQuantiColpi)
    {
        frequenzaBlocco = Mathf.Max(0, ogniQuantiColpi);
        colpiContati = 0;
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
