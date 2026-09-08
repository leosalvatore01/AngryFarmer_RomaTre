using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerTemporaryEffects : MonoBehaviour
{
    private PlayerHealth salute;
    private float scudoFinoA;
    private float calamitaFinoA;
    private float furiaFinoA;
    private int caricheScudo;
    private PlayerBalanceSettings impostazioni;

    private PlayerBalanceSettings Impostazioni =>
        impostazioni ??
        (impostazioni = GameBalanceConfig.Corrente.Giocatore);

    public bool ScudoAttivo => caricheScudo > 0 && Time.time < scudoFinoA;
    public int CaricheScudo => ScudoAttivo ? caricheScudo : 0;
    public float TempoScudoRimasto => TempoRimasto(scudoFinoA);
    public float DurataScudoTotale =>
        Mathf.Max(0f, Impostazioni.durataScudoTemporaneo);

    public bool CalamitaAttiva => Time.time < calamitaFinoA;
    public float TempoCalamitaRimasto => TempoRimasto(calamitaFinoA);
    public float DurataCalamitaTotale =>
        Mathf.Max(0f, Impostazioni.durataCalamita);

    public bool FuriaAttiva => Time.time < furiaFinoA;
    public float TempoFuriaRimasto => TempoRimasto(furiaFinoA);
    public float DurataFuriaTotale =>
        Mathf.Max(0f, Impostazioni.durataFuria);
    public float MoltiplicatoreDanno => FuriaAttiva
        ? Mathf.Max(1f, Impostazioni.moltiplicatoreDannoFuria)
        : 1f;
    public float MoltiplicatoreIntervalloSparo =>
        FuriaAttiva
            ? Mathf.Clamp(Impostazioni.moltiplicatoreIntervalloFuria, 0.2f, 1f)
            : 1f;

    public int UltimiNemiciColpitiDaEsplosione { get; private set; }

    public static PlayerTemporaryEffects AggiungiOTrova(GameObject giocatore)
    {
        if (giocatore == null) return null;
        PlayerTemporaryEffects effetti =
            giocatore.GetComponent<PlayerTemporaryEffects>();
        return effetti != null
            ? effetti
            : giocatore.AddComponent<PlayerTemporaryEffects>();
    }

    void Awake()
    {
        impostazioni = GameBalanceConfig.Corrente.Giocatore;
        salute = GetComponent<PlayerHealth>();
    }

    void Update()
    {
        if (caricheScudo > 0 && Time.time >= scudoFinoA)
            caricheScudo = 0;

        if (CalamitaAttiva)
        {
            PowerUp.AttraiDropVerso(
                transform.position,
                Mathf.Max(0.5f, Impostazioni.raggioCalamita),
                Mathf.Max(0.1f, Impostazioni.velocitaAttrazioneCalamita),
                Time.deltaTime
            );
        }
    }

    public bool Applica(TipoDropTemporaneo tipo)
    {
        switch (tipo)
        {
            case TipoDropTemporaneo.TriploSparo:
                PlayerShooting sparo = GetComponent<PlayerShooting>();
                if (sparo == null) return false;
                sparo.AttivaTriploSparo();
                return true;
            case TipoDropTemporaneo.Velocita:
                PlayerMovement movimento = GetComponent<PlayerMovement>();
                if (movimento == null) return false;
                movimento.AttivaBoostVelocita();
                return true;
            case TipoDropTemporaneo.Scudo:
                AttivaScudo();
                return true;
            case TipoDropTemporaneo.Cura:
                if (salute == null) salute = GetComponent<PlayerHealth>();
                if (salute == null || salute.VitaPiena) return false;
                salute.Cura(Mathf.Max(1, Impostazioni.quantitaCuraDrop));
                return true;
            case TipoDropTemporaneo.Calamita:
                AttivaCalamita();
                return true;
            case TipoDropTemporaneo.Furia:
                AttivaFuria();
                return true;
            case TipoDropTemporaneo.Esplosione:
                ApplicaEsplosioneIstantanea();
                return true;
            default:
                return false;
        }
    }

    public void AttivaScudo()
    {
        float durata = Mathf.Max(0.1f, Impostazioni.durataScudoTemporaneo);
        scudoFinoA = Mathf.Max(scudoFinoA, Time.time) + durata;
        caricheScudo = Mathf.Max(
            caricheScudo,
            Mathf.Max(1, Impostazioni.caricheScudoTemporaneo)
        );
    }

    public bool ProvaAssorbireDanno()
    {
        if (!ScudoAttivo) return false;
        caricheScudo = Mathf.Max(0, caricheScudo - 1);
        if (caricheScudo <= 0) scudoFinoA = 0f;
        if (Application.isPlaying)
        {
            CombatFeedbackController.CreaOTrova()?.RegistraScudo(
                transform.position
            );
        }
        return true;
    }

    public void AttivaCalamita()
    {
        calamitaFinoA = EstendiDurata(
            calamitaFinoA,
            Impostazioni.durataCalamita
        );
    }

    public void AttivaFuria()
    {
        furiaFinoA = EstendiDurata(furiaFinoA, Impostazioni.durataFuria);
    }

    public void ApplicaEsplosioneIstantanea()
    {
        float raggio = Mathf.Max(0.5f, Impostazioni.raggioEsplosioneDrop);
        int danno = Mathf.Max(1, Impostazioni.dannoEsplosioneDrop);
        UltimiNemiciColpitiDaEsplosione = EnemyAI.DanneggiaNelRaggio(
            transform.position,
            raggio,
            danno
        );
        if (Application.isPlaying)
        {
            CombatFeedbackController.CreaOTrova()?.RegistraEsplosioneDrop(
                transform.position,
                raggio
            );
        }
    }

    private static float EstendiDurata(float scadenza, float durata)
    {
        return Mathf.Max(scadenza, Time.time) + Mathf.Max(0.1f, durata);
    }

    private static float TempoRimasto(float scadenza)
    {
        return scadenza > Time.time
            ? Mathf.Max(0f, scadenza - Time.time)
            : 0f;
    }
}
