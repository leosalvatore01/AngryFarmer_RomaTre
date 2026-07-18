using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerHealth : MonoBehaviour
{
    [Header("Statistiche di salute")]
    [FormerlySerializedAs("vitaMassima")]
    [SerializeField, Min(1)] private int vitaMassimaBase = 5;

    private int vitaCorrente;
    private TMP_Text testoVita;
    private int bonusVitaMassima;
    private int frequenzaBloccoBase;
    private int frequenzaBloccoBonus;
    private int colpiContati;
    private float durataInvulnerabilita;
    private float invulnerabileFinoA;
    private bool invulnerabilitaSegnalata;
    private PlayerInvulnerabilityFeedback feedbackInvulnerabilita;

    public int VitaCorrente => vitaCorrente;
    public int VitaMassimaBase => vitaMassimaBase;
    public int BonusVitaMassima => bonusVitaMassima;
    public int VitaMassimaFinale =>
        Mathf.Max(1, vitaMassimaBase + bonusVitaMassima);
    public int VitaMassima => VitaMassimaFinale;
    public bool VitaPiena => vitaCorrente >= VitaMassimaFinale;
    public bool Invulnerabile =>
        durataInvulnerabilita > 0f && Time.time < invulnerabileFinoA;
    public float TempoInvulnerabilitaRimasto => Invulnerabile
        ? Mathf.Max(0f, invulnerabileFinoA - Time.time)
        : 0f;

    public int FrequenzaBloccoBase => frequenzaBloccoBase;
    public int BonusFrequenzaBlocco => frequenzaBloccoBonus;
    public int FrequenzaBloccoFinale
    {
        get
        {
            if (frequenzaBloccoBase <= 0) return frequenzaBloccoBonus;
            if (frequenzaBloccoBonus <= 0) return frequenzaBloccoBase;
            return Mathf.Min(frequenzaBloccoBase, frequenzaBloccoBonus);
        }
    }
    public int FrequenzaBlocco => FrequenzaBloccoFinale;

    [Obsolete("Usa VitaMassimaFinale.")]
    public int vitaMassima => VitaMassimaFinale;

    public event Action VitaCambiata;
    public event Action<int> DannoSubito;
    public event Action<bool> InvulnerabilitaCambiata;

    void Awake()
    {
        PlayerBalanceSettings configurazione =
            GameBalanceConfig.Corrente.Giocatore;
        vitaMassimaBase = Mathf.Max(1, configurazione.vitaMassima);
        frequenzaBloccoBase = Mathf.Max(
            0,
            configurazione.frequenzaBloccoBase
        );
        durataInvulnerabilita = Mathf.Clamp(
            configurazione.durataInvulnerabilitaDopoColpo,
            0f,
            2f
        );
    }

    void Start()
    {
        vitaCorrente = VitaMassimaFinale;
        testoVita = GameManager.TrovaTestoInterfaccia("VitaText");
        feedbackInvulnerabilita =
            PlayerInvulnerabilityFeedback.AggiungiOTrova(gameObject);
        AggiornaInterfaccia();
    }

    void Update()
    {
        if (invulnerabilitaSegnalata && !Invulnerabile)
        {
            invulnerabilitaSegnalata = false;
            InvulnerabilitaCambiata?.Invoke(false);
        }
    }

    public void SubisciDanno(int danno)
    {
        ProvaSubireDanno(danno);
    }

    public bool ProvaSubireDanno(int danno)
    {
        if (danno <= 0 || vitaCorrente <= 0 || Invulnerabile) return false;

        int frequenzaBlocco = FrequenzaBloccoFinale;
        if (frequenzaBlocco > 0)
        {
            colpiContati++;
            if (colpiContati >= frequenzaBlocco)
            {
                colpiContati = 0;
                Debug.Log("Il contadino ha resistito al colpo.", this);
                return false;
            }
        }

        int vitaPrecedente = vitaCorrente;
        vitaCorrente = Mathf.Max(0, vitaCorrente - danno);
        int dannoEffettivo = vitaPrecedente - vitaCorrente;
        AggiornaInterfaccia();
        VitaCambiata?.Invoke();
        if (dannoEffettivo > 0)
        {
            if (durataInvulnerabilita > 0f && vitaCorrente > 0)
            {
                invulnerabileFinoA = Time.time + durataInvulnerabilita;
                invulnerabilitaSegnalata = true;
                feedbackInvulnerabilita?.Avvia(durataInvulnerabilita);
                InvulnerabilitaCambiata?.Invoke(true);
            }
            DannoSubito?.Invoke(dannoEffettivo);
            DamageNumberFeedback.MostraGiocatore(
                transform.position,
                dannoEffettivo
            );
            FarmAudioController.RiproduciPericolo();
        }

        if (vitaCorrente <= 0 && GameManager.instance != null)
        {
            GameManager.instance.GameOverGiocatore();
        }
        return dannoEffettivo > 0;
    }

    public void Cura(int quantita)
    {
        if (quantita <= 0 || VitaPiena) return;

        int vitaPrecedente = vitaCorrente;
        vitaCorrente = Mathf.Min(
            VitaMassimaFinale,
            vitaCorrente + quantita
        );
        AggiornaInterfaccia();
        if (vitaCorrente != vitaPrecedente)
        {
            VitaCambiata?.Invoke();
        }
    }

    public void ImpostaBonusVitaMassima(
        int nuovoBonus,
        int curaBonus = 0
    )
    {
        int bonusValido = Mathf.Max(0, nuovoBonus);
        int curaValida = Mathf.Max(0, curaBonus);
        if (bonusVitaMassima == bonusValido && curaValida == 0) return;

        bonusVitaMassima = bonusValido;
        vitaCorrente = Mathf.Min(
            VitaMassimaFinale,
            vitaCorrente + curaValida
        );
        AggiornaInterfaccia();
        VitaCambiata?.Invoke();
    }

    public void AggiungiBonusVitaMassima(
        int quantita,
        int curaBonus = 1
    )
    {
        if (quantita <= 0) return;
        ImpostaBonusVitaMassima(
            bonusVitaMassima + quantita,
            curaBonus
        );
    }

    [Obsolete("Usa AggiungiBonusVitaMassima.")]
    public void AumentaVitaMassima(int quantita, int curaBonus = 1)
    {
        AggiungiBonusVitaMassima(quantita, curaBonus);
    }

    public void ImpostaBonusFrequenzaBlocco(int ogniQuantiColpi)
    {
        frequenzaBloccoBonus = Mathf.Max(0, ogniQuantiColpi);
        colpiContati = 0;
    }

    [Obsolete("Usa ImpostaBonusFrequenzaBlocco.")]
    public void ImpostaFrequenzaBlocco(int ogniQuantiColpi)
    {
        ImpostaBonusFrequenzaBlocco(ogniQuantiColpi);
    }

    void AggiornaInterfaccia()
    {
        if (testoVita != null)
        {
            int vitaMassima = VitaMassimaFinale;
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

            testoVita.text = vitaCorrente + " / " + vitaMassima;
        }
    }
}
