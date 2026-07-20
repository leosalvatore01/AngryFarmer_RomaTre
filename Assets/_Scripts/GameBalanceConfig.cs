using System;
using UnityEngine;

[Serializable]
public sealed class PlayerBalanceSettings
{
    [Header("Movimento")]
    [Min(0f)] public float velocitaMovimento = 8f;
    [Min(0f)] public float accelerazione = 45f;
    [Min(0f)] public float decelerazione = 60f;
    [Min(1f)] public float moltiplicatoreInversione = 1.35f;

    [Header("Salute")]
    [Min(1)] public int vitaMassima = 5;
    [Min(0)] public int frequenzaBloccoBase;
    [Range(0f, 2f)] public float durataInvulnerabilitaDopoColpo = 0.65f;

    [Header("Sparo")]
    [Min(0.01f)] public float intervalloSparo = 0.4f;
    [Min(0.01f)] public float intervalloSparoMinimo = 0.12f;
    [Min(0f)] public float velocitaProiettile = 10f;
    [Min(1)] public int dannoProiettile = 1;
    [Min(0)] public int penetrazioneProiettile;
    [Min(0.05f)] public float durataProiettile = 3f;
    [Min(0f)] public float distanzaMinimaMira = 0.2f;
    [Min(0f)] public float distanzaUscitaProiettile = 0.44f;

    [Header("Power-up temporanei")]
    [Min(0f)] public float durataBoostVelocita = 5f;
    [Min(1f)] public float moltiplicatoreBoostVelocita = 2f;
    [Min(0f)] public float durataTriploSparo = 5f;
    [Range(0f, 45f)] public float angoloLateraleTriploSparo = 10f;
}

[Serializable]
public sealed class FoxBalanceSettings
{
    [Header("Movimento e attacco")]
    [Min(0f)] public float velocita = 2.4f;
    [Min(0f)] public float accelerazione = 12f;
    [Min(0f)] public float decelerazione = 18f;
    [Min(1f)] public float moltiplicatoreInversione = 1.3f;
    [Min(0f)] public float distanzaRipresaInseguimento = 0.95f;
    [Min(0f)] public float distanzaAttacco = 0.8f;
    [Min(0)] public int danno = 1;
    [Min(0.01f)] public float intervalloAttacco = 1f;
    [Range(0.05f, 1f)] public float moltiplicatoreRallentamento = 0.5f;

    [Header("Progressione e ricompense")]
    [Min(1)] public int vitaPrimaOndata = 2;
    [Min(0)] public int vitaAggiuntivaPerOndata = 1;
    [Min(0)] public int monetePerEliminazione = 1;
    [Range(0f, 100f)] public float probabilitaDrop = 30f;
    [Range(0f, 1f)] public float probabilitaDenteSulDrop = 0.5f;
}

[Serializable]
public sealed class PigBalanceSettings
{
    [Min(0f)] public float velocitaPasseggio = 1.45f;
    [Min(0f)] public float velocitaFuga = 3.1f;
    [Min(0f)] public float accelerazione = 10f;
    [Min(0f)] public float decelerazione = 14f;
    [Min(1f)] public float moltiplicatoreInversione = 1.25f;
    [Min(0f)] public float raggioFuga = 2.6f;
    [Min(0.1f)] public float durataSullaMappa = 14f;
    [Min(1)] public int vitaBase = 1;
    [Min(0)] public int moneteBase = 3;
    [Min(0.05f)] public float cambioDirezioneMinimo = 1.4f;
    [Min(0.05f)] public float cambioDirezioneMassimo = 2.8f;
    [Min(0f)] public float ritardoDirezioneDopoFuga = 0.5f;
}

[Serializable]
public sealed class FarmObjectivesBalanceSettings
{
    [Header("Recupero delle uova")]
    [Min(1f)] public float durataRecuperoUovo = 7f;
    [Min(0.2f)] public float raggioRecuperoUovo = 0.85f;
    [Min(0)] public int uovaPerRecupero = 1;

    [Header("Ricompense")]
    [Min(0)] public int uovaPerMaialino = 1;
    [Min(0)] public int uovaPerObiettivo = 2;

    [Header("Serie di difesa")]
    [Min(1)] public int salvataggiPerBonusSerie = 2;
    [Min(0)] public int uovaBonusSeriePerLivello = 1;
    [Min(0)] public int bonusMassimoSerie = 2;
}

[Serializable]
public sealed class ShopBalanceSettings
{
    [Header("Offerte e ritmo della bottega")]
    [Range(3, 4)] public int numeroOfferte = 4;
    [Min(1)] public int costoRerollBase = 1;
    [Min(0)] public int incrementoCostoReroll = 1;
    [Min(0)] public int bonusCompletamentoOnda = 1;

    [Header("Prezzi per livello")]
    public int[] costiMovimento = { 3, 5, 8 };
    public int[] costiResistenza = { 4, 7, 10 };
    public int[] costiSalute = { 4, 7, 10 };
    public int[] costiDanno = { 10, 16 };
    public int[] costiCadenza = { 3, 6, 9 };
    public int[] costiPenetrazione = { 3, 5, 7 };
    [Min(1)] public int costoCura = 2;

    [Header("Prezzi modificatori speciali")]
    public int[] costiColpoAggiuntivo = { 6, 10 };
    public int[] costiRafficaRaccolto = { 11 };
    public int[] costiPatataGigante = { 4, 7 };
    public int[] costiPatataEsplosiva = { 12 };
    public int[] costiCritico = { 3, 5, 7 };
    public int[] costiRimbalzo = { 8, 14 };
    public int[] costiRallentamento = { 3, 6 };
    public int[] costiSpinta = { 3, 6, 9 };

    [Header("Effetti")]
    [Min(0f)] public float incrementoMovimento = 0.5f;
    [Min(2)] public int[] frequenzeBlocco = { 5, 4, 3 };
    [Min(0)] public int incrementoSaluteMassima = 1;
    [Min(0)] public int curaSuIncrementoSalute = 1;
    [Min(0)] public int quantitaCura = 2;
    [Min(0)] public int incrementoDanno = 1;
    [Min(0f)] public float riduzioneIntervalloSparo = 0.04f;
    [Min(0)] public int incrementoPenetrazione = 1;
    [Min(0)] public int moneteIniziali;

    [Header("Effetti build Raffica")]
    [Range(0f, 1f)] public float probabilitaColpoAggiuntivoPerLivello = 0.18f;
    [Min(2)] public int colpiPerRafficaRaccolto = 5;
    [Range(0f, 30f)] public float angoloColpoAggiuntivo = 7f;

    [Header("Effetti build Artiglieria")]
    [Range(0f, 1f)] public float incrementoScalaPatataGigante = 0.2f;
    [Range(0f, 0.4f)] public float riduzioneVelocitaPatataGigante = 0.06f;
    [Range(0f, 2f)] public float forzaSpintaPatataGigantePerLivello = 1.1f;
    [Min(0.2f)] public float raggioEsplosione = 1.25f;
    [Range(0.1f, 2f)] public float moltiplicatoreDannoEsplosione = 0.5f;

    [Header("Effetti build Perforazione")]
    [Range(0f, 1f)] public float probabilitaCriticoPerLivello = 0.12f;
    [Min(1f)] public float moltiplicatoreDannoCritico = 2f;
    [Range(1.5f, 3.2f)] public float raggioRicercaRimbalzo = 2.4f;
    [Range(0.5f, 1f)] public float moltiplicatoreDannoRimbalzo = 0.65f;

    [Header("Effetti build Controllo")]
    [Range(0.1f, 0.95f)] public float rallentamentoPrimoLivello = 0.72f;
    [Range(0f, 0.4f)] public float riduzioneRallentamentoPerLivello = 0.12f;
    [Min(0.1f)] public float durataRallentamentoBase = 1.1f;
    [Min(0f)] public float durataRallentamentoPerLivello = 0.35f;
    [Min(0f)] public float forzaSpintaPerLivello = 0.85f;
}

[Serializable]
public sealed class FarmInteractiveBalanceSettings
{
    [Header("Distribuzione deterministica")]
    public int seedArena = 51027;
    [Range(1, 5)] public int numeroZoneFango = 3;
    [Range(0, 4)] public int numeroBallePaglia = 2;
    [Range(0, 4)] public int numeroZuccheEsplosive = 2;
    [Range(0, 3)] public int numeroCasseMonete = 1;
    [Range(0, 3)] public int numeroCasseCura = 1;
    [Min(2f)] public float raggioMinimoArena = 4.1f;
    [Min(3f)] public float raggioMassimoArena = 8f;
    [Range(0.4f, 1f)] public float scalaVerticaleArena = 0.68f;
    [Min(0.5f)] public float distanzaMinimaElementi = 2.15f;
    [Min(0.5f)] public float raggioLiberoGiocatore = 2.8f;
    [Min(0.5f)] public float raggioLiberoGalline = 1.6f;

    [Header("Fango")]
    [Min(0.5f)] public float raggioFango = 1.35f;
    [Range(0.2f, 1f)] public float velocitaGiocatoreNelFango = 0.72f;
    [Range(0.2f, 1f)] public float velocitaVolpiNelFango = 0.52f;

    [Header("Oggetti colpibili")]
    [Min(1)] public int vitaBallaPaglia = 2;
    [Min(1)] public int vitaZucca = 1;
    [Min(0.5f)] public float raggioEsplosioneZucca = 2.15f;
    [Min(1)] public int dannoEsplosioneZucca = 3;
    [Min(0f)] public float spintaEsplosioneZucca = 2.4f;
    [Min(1)] public int vitaCassa = 1;
    [Min(0)] public int moneteCassa = 2;
    [Min(0)] public int curaCassa = 2;

    [Header("Leggibilita")]
    [Min(0f)] public float durataSuggerimentoIniziale = 5.5f;
}

[Serializable]
public sealed class WaveBalanceSettings
{
    [Header("Spawn e ritmo")]
    [Min(0f)] public float distanzaSpawnVolpi = 10f;
    [Min(0f)] public float distanzaSpawnMaialini = 5.2f;
    [Min(0f)] public float durataBannerOndata = 0.85f;
    [Min(0.02f)] public float intervalloControlloFineOndata = 0.2f;
    [Min(0.05f)] public float durataMinimaDistribuzioneMaialini = 1f;

    [Header("Leggibilita e gruppi")]
    [Range(0.15f, 1f)] public float durataPreavvisoSpawn = 0.5f;
    [Range(1, 4)] public int sogliaUltimiNemici = 2;
    [Range(0f, 1f)] public float volumeSegnaleUltimiNemici = 0.26f;

    [Header("Sequenza di riferimento")]
    public Wave[] ondate = CreaOndateRiferimento();

    private static Wave[] CreaOndateRiferimento()
    {
        return new[]
        {
            new Wave
            {
                nomeOndata = "Riscaldamento",
                numeroNemici = 3,
                sequenzaVolpi = new[]
                {
                    TipoVolpe.Comune,
                    TipoVolpe.Comune,
                    TipoVolpe.Comune
                },
                intervalloTraNemici = 0.7f,
                dimensioneMassimaGruppo = 2,
                intervalloTraGruppi = 2.9f,
                numeroMaialiniBonus = 0,
                vitaMaialinoBonus = 1,
                moneteMaialinoBonus = 0
            },
            new Wave
            {
                nomeOndata = "Passi veloci",
                numeroNemici = 4,
                sequenzaVolpi = new[]
                {
                    TipoVolpe.Comune,
                    TipoVolpe.Comune,
                    TipoVolpe.Agile,
                    TipoVolpe.Comune
                },
                intervalloTraNemici = 0.7f,
                dimensioneMassimaGruppo = 2,
                intervalloTraGruppi = 3f,
                numeroMaialiniBonus = 0,
                vitaMaialinoBonus = 1,
                moneteMaialinoBonus = 0
            },
            new Wave
            {
                nomeOndata = "Pelle dura",
                numeroNemici = 5,
                sequenzaVolpi = new[]
                {
                    TipoVolpe.Comune,
                    TipoVolpe.Agile,
                    TipoVolpe.Comune,
                    TipoVolpe.Robusta,
                    TipoVolpe.Comune
                },
                intervalloTraNemici = 0.68f,
                dimensioneMassimaGruppo = 2,
                intervalloTraGruppi = 2.7f,
                numeroMaialiniBonus = 0,
                vitaMaialinoBonus = 1,
                moneteMaialinoBonus = 0
            },
            new Wave
            {
                nomeOndata = "Salti imprevedibili",
                numeroNemici = 6,
                sequenzaVolpi = new[]
                {
                    TipoVolpe.Comune,
                    TipoVolpe.Agile,
                    TipoVolpe.Comune,
                    TipoVolpe.Robusta,
                    TipoVolpe.Schivatrice,
                    TipoVolpe.Comune
                },
                intervalloTraNemici = 0.66f,
                dimensioneMassimaGruppo = 2,
                intervalloTraGruppi = 2.55f,
                numeroMaialiniBonus = 0,
                vitaMaialinoBonus = 1,
                moneteMaialinoBonus = 0
            },
            new Wave
            {
                nomeOndata = "Carica alfa",
                numeroNemici = 7,
                sequenzaVolpi = new[]
                {
                    TipoVolpe.Comune,
                    TipoVolpe.Agile,
                    TipoVolpe.Robusta,
                    TipoVolpe.Comune,
                    TipoVolpe.Schivatrice,
                    TipoVolpe.Comune,
                    TipoVolpe.Alfa
                },
                intervalloTraNemici = 0.64f,
                dimensioneMassimaGruppo = 3,
                intervalloTraGruppi = 2.6f,
                numeroMaialiniBonus = 0,
                vitaMaialinoBonus = 1,
                moneteMaialinoBonus = 0
            },
            new Wave
            {
                nomeOndata = "Richiamo del branco",
                numeroNemici = 8,
                sequenzaVolpi = new[]
                {
                    TipoVolpe.Comune,
                    TipoVolpe.Agile,
                    TipoVolpe.Robusta,
                    TipoVolpe.Comune,
                    TipoVolpe.Schivatrice,
                    TipoVolpe.Ululatrice,
                    TipoVolpe.Comune,
                    TipoVolpe.Alfa
                },
                intervalloTraNemici = 0.62f,
                dimensioneMassimaGruppo = 3,
                intervalloTraGruppi = 2.5f,
                numeroMaialiniBonus = 0,
                vitaMaialinoBonus = 1,
                moneteMaialinoBonus = 0
            },
            new Wave
            {
                nomeOndata = "Branco misto",
                numeroNemici = 10,
                sequenzaVolpi = new[]
                {
                    TipoVolpe.Comune,
                    TipoVolpe.Agile,
                    TipoVolpe.Robusta,
                    TipoVolpe.Comune,
                    TipoVolpe.Schivatrice,
                    TipoVolpe.Alfa,
                    TipoVolpe.Comune,
                    TipoVolpe.Agile,
                    TipoVolpe.Ululatrice,
                    TipoVolpe.Robusta
                },
                intervalloTraNemici = 0.6f,
                dimensioneMassimaGruppo = 3,
                intervalloTraGruppi = 2.35f,
                numeroMaialiniBonus = 0,
                vitaMaialinoBonus = 1,
                moneteMaialinoBonus = 0
            },
            new Wave
            {
                nomeOndata = "Fango in arrivo",
                numeroNemici = 12,
                sequenzaVolpi = new[]
                {
                    TipoVolpe.Comune,
                    TipoVolpe.Agile,
                    TipoVolpe.Robusta,
                    TipoVolpe.Comune,
                    TipoVolpe.Schivatrice,
                    TipoVolpe.Ululatrice,
                    TipoVolpe.Comune,
                    TipoVolpe.Agile,
                    TipoVolpe.Alfa,
                    TipoVolpe.Robusta,
                    TipoVolpe.Comune,
                    TipoVolpe.Sputafango
                },
                intervalloTraNemici = 0.58f,
                dimensioneMassimaGruppo = 3,
                intervalloTraGruppi = 2.25f,
                numeroMaialiniBonus = 0,
                vitaMaialinoBonus = 1,
                moneteMaialinoBonus = 0
            },
            new Wave
            {
                nomeOndata = "Assedio del branco",
                numeroNemici = 14,
                sequenzaVolpi = new[]
                {
                    TipoVolpe.Comune,
                    TipoVolpe.Agile,
                    TipoVolpe.Robusta,
                    TipoVolpe.Comune,
                    TipoVolpe.Schivatrice,
                    TipoVolpe.Sputafango,
                    TipoVolpe.Comune,
                    TipoVolpe.Agile,
                    TipoVolpe.Alfa,
                    TipoVolpe.Robusta,
                    TipoVolpe.Comune,
                    TipoVolpe.Schivatrice,
                    TipoVolpe.Ululatrice,
                    TipoVolpe.Agile
                },
                intervalloTraNemici = 0.56f,
                dimensioneMassimaGruppo = 3,
                intervalloTraGruppi = 2.15f,
                numeroMaialiniBonus = 0,
                vitaMaialinoBonus = 1,
                moneteMaialinoBonus = 0
            },
            new Wave
            {
                nomeOndata = "Terra in movimento",
                numeroNemici = 16,
                sequenzaVolpi = new[]
                {
                    TipoVolpe.Comune,
                    TipoVolpe.Agile,
                    TipoVolpe.Robusta,
                    TipoVolpe.Comune,
                    TipoVolpe.Schivatrice,
                    TipoVolpe.Ululatrice,
                    TipoVolpe.Comune,
                    TipoVolpe.Agile,
                    TipoVolpe.Alfa,
                    TipoVolpe.Robusta,
                    TipoVolpe.Comune,
                    TipoVolpe.Schivatrice,
                    TipoVolpe.Sputafango,
                    TipoVolpe.Agile,
                    TipoVolpe.Comune,
                    TipoVolpe.Scavatrice
                },
                intervalloTraNemici = 0.54f,
                dimensioneMassimaGruppo = 3,
                intervalloTraGruppi = 2.05f,
                numeroMaialiniBonus = 0,
                vitaMaialinoBonus = 1,
                moneteMaialinoBonus = 0
            }
        };
    }
}

[Serializable]
public sealed class CombatFeedbackSettings
{
    [Header("Mirino")]
    public bool mirinoPixelAttivo = true;
    [Range(14f, 48f)] public float dimensioneMirino = 24f;

    [Header("Effetti visivi")]
    public bool effettiVisiviAttivi = true;
    [Range(3, 8)] public int particelleImpatto = 5;
    [Range(0.08f, 0.35f)] public float durataParticelle = 0.18f;
    [Range(0f, 0.3f)] public float distanzaRinculoBersaglio = 0.12f;
    [Range(0.03f, 0.25f)] public float durataRinculoBersaglio = 0.09f;
    [Range(0.03f, 0.2f)] public float durataFlashBersaglio = 0.075f;

    [Header("Audio")]
    public bool audioAttivo = true;
    [Range(0f, 1f)] public float volumeSparo = 0.22f;
    [Range(0f, 1f)] public float volumeImpatto = 0.28f;
    [Range(0f, 0.2f)] public float variazioneIntonazione = 0.055f;

    [Header("Vibrazione camera")]
    public bool vibrazioneCameraAttiva = true;
    [Range(0f, 0.15f)] public float intensitaVibrazione = 0.045f;
    [Range(0.02f, 0.25f)] public float durataVibrazione = 0.075f;
}

[CreateAssetMenu(
    fileName = "GameBalanceConfig",
    menuName = "Angry Farmer/Bilanciamento di riferimento"
)]
public sealed class GameBalanceConfig : ScriptableObject
{
    public const string PercorsoResources = "GameBalanceConfig";

    [SerializeField]
    private string versioneRiferimento =
        "Blocco 8 - Fine partita e bilanciamento - 2026-07-20";

    [SerializeField] private PlayerBalanceSettings giocatore =
        new PlayerBalanceSettings();
    [SerializeField] private FoxBalanceSettings volpe =
        new FoxBalanceSettings();
    [SerializeField] private FoxVariantsBalanceSettings variantiVolpe =
        new FoxVariantsBalanceSettings();
    [SerializeField] private PigBalanceSettings maialino =
        new PigBalanceSettings();
    [SerializeField] private FarmObjectivesBalanceSettings obiettiviFattoria =
        new FarmObjectivesBalanceSettings();
    [SerializeField] private ShopBalanceSettings shop =
        new ShopBalanceSettings();
    [SerializeField] private FarmInteractiveBalanceSettings fattoria =
        new FarmInteractiveBalanceSettings();
    [SerializeField] private WaveBalanceSettings ondate =
        new WaveBalanceSettings();
    [SerializeField] private BilanciamentoDifficolta difficolta =
        new BilanciamentoDifficolta();
    [SerializeField] private CombatFeedbackSettings feedbackCombattimento =
        new CombatFeedbackSettings();

    private static GameBalanceConfig corrente;
    private static bool avvisoFallbackMostrato;

    public string VersioneRiferimento => versioneRiferimento;
    public PlayerBalanceSettings Giocatore => giocatore;
    public FoxBalanceSettings Volpe => volpe;
    public FoxVariantsBalanceSettings VariantiVolpe =>
        variantiVolpe ??
        (variantiVolpe = new FoxVariantsBalanceSettings());
    public PigBalanceSettings Maialino => maialino;
    public FarmObjectivesBalanceSettings ObiettiviFattoria =>
        obiettiviFattoria ??
        (obiettiviFattoria = new FarmObjectivesBalanceSettings());
    public ShopBalanceSettings Shop => shop;
    public FarmInteractiveBalanceSettings Fattoria =>
        fattoria ?? (fattoria = new FarmInteractiveBalanceSettings());
    public WaveBalanceSettings Ondate => ondate;
    public BilanciamentoDifficolta Difficolta =>
        difficolta ?? (difficolta = new BilanciamentoDifficolta());
    public CombatFeedbackSettings FeedbackCombattimento =>
        feedbackCombattimento ??
        (feedbackCombattimento = new CombatFeedbackSettings());

    public static GameBalanceConfig Corrente
    {
        get
        {
            if (corrente != null) return corrente;

            corrente = Resources.Load<GameBalanceConfig>(PercorsoResources);
            if (corrente != null) return corrente;

            corrente = CreateInstance<GameBalanceConfig>();
            corrente.name = "GameBalanceConfig_FallbackRuntime";
            corrente.hideFlags = HideFlags.HideAndDontSave;

            if (!avvisoFallbackMostrato)
            {
                Debug.LogWarning(
                    "GameBalanceConfig.asset non trovato in Resources: " +
                    "uso i valori di riferimento incorporati nel codice."
                );
                avvisoFallbackMostrato = true;
            }
            return corrente;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void AzzeraCache()
    {
        corrente = null;
        avvisoFallbackMostrato = false;
    }

    void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(versioneRiferimento))
        {
            versioneRiferimento = "Baseline senza nome";
        }

        if (feedbackCombattimento == null)
        {
            feedbackCombattimento = new CombatFeedbackSettings();
        }
        if (variantiVolpe == null)
        {
            variantiVolpe = new FoxVariantsBalanceSettings();
        }
        if (fattoria == null)
        {
            fattoria = new FarmInteractiveBalanceSettings();
        }
        if (obiettiviFattoria == null)
        {
            obiettiviFattoria = new FarmObjectivesBalanceSettings();
        }
        if (difficolta == null)
        {
            difficolta = new BilanciamentoDifficolta();
        }
        difficolta.Normalizza();

        giocatore.intervalloSparoMinimo = Mathf.Max(
            0.01f,
            giocatore.intervalloSparoMinimo
        );
        giocatore.intervalloSparo = Mathf.Max(
            giocatore.intervalloSparoMinimo,
            giocatore.intervalloSparo
        );
        giocatore.durataInvulnerabilitaDopoColpo = Mathf.Clamp(
            giocatore.durataInvulnerabilitaDopoColpo,
            0f,
            2f
        );
        maialino.cambioDirezioneMassimo = Mathf.Max(
            maialino.cambioDirezioneMinimo,
            maialino.cambioDirezioneMassimo
        );
        obiettiviFattoria.durataRecuperoUovo = Mathf.Max(
            1f,
            obiettiviFattoria.durataRecuperoUovo
        );
        obiettiviFattoria.raggioRecuperoUovo = Mathf.Max(
            0.2f,
            obiettiviFattoria.raggioRecuperoUovo
        );
        obiettiviFattoria.uovaPerRecupero = Mathf.Max(
            0,
            obiettiviFattoria.uovaPerRecupero
        );
        obiettiviFattoria.uovaPerMaialino = Mathf.Max(
            0,
            obiettiviFattoria.uovaPerMaialino
        );
        obiettiviFattoria.uovaPerObiettivo = Mathf.Max(
            0,
            obiettiviFattoria.uovaPerObiettivo
        );
        obiettiviFattoria.salvataggiPerBonusSerie = Mathf.Max(
            1,
            obiettiviFattoria.salvataggiPerBonusSerie
        );
        obiettiviFattoria.uovaBonusSeriePerLivello = Mathf.Max(
            0,
            obiettiviFattoria.uovaBonusSeriePerLivello
        );
        obiettiviFattoria.bonusMassimoSerie = Mathf.Max(
            0,
            obiettiviFattoria.bonusMassimoSerie
        );
        NormalizzaArrayNonNegativo(shop.costiMovimento);
        NormalizzaArrayNonNegativo(shop.costiResistenza);
        NormalizzaArrayNonNegativo(shop.costiSalute);
        NormalizzaArrayNonNegativo(shop.costiDanno);
        NormalizzaArrayNonNegativo(shop.costiCadenza);
        NormalizzaArrayNonNegativo(shop.costiPenetrazione);
        NormalizzaArrayNonNegativo(shop.costiColpoAggiuntivo);
        NormalizzaArrayNonNegativo(shop.costiRafficaRaccolto);
        NormalizzaArrayNonNegativo(shop.costiPatataGigante);
        NormalizzaArrayNonNegativo(shop.costiPatataEsplosiva);
        NormalizzaArrayNonNegativo(shop.costiCritico);
        NormalizzaArrayNonNegativo(shop.costiRimbalzo);
        NormalizzaArrayNonNegativo(shop.costiRallentamento);
        NormalizzaArrayNonNegativo(shop.costiSpinta);
        shop.numeroOfferte = Mathf.Clamp(shop.numeroOfferte, 3, 4);
        shop.costoRerollBase = Mathf.Max(1, shop.costoRerollBase);
        shop.incrementoCostoReroll = Mathf.Max(
            1,
            shop.incrementoCostoReroll
        );
        shop.costoCura = Mathf.Max(1, shop.costoCura);
        shop.bonusCompletamentoOnda = Mathf.Max(
            0,
            shop.bonusCompletamentoOnda
        );
        shop.colpiPerRafficaRaccolto = Mathf.Max(
            2,
            shop.colpiPerRafficaRaccolto
        );
        shop.raggioEsplosione = Mathf.Max(0.2f, shop.raggioEsplosione);
        shop.forzaSpintaPatataGigantePerLivello = Mathf.Clamp(
            shop.forzaSpintaPatataGigantePerLivello,
            0f,
            2f
        );
        shop.moltiplicatoreDannoCritico = Mathf.Max(
            1f,
            shop.moltiplicatoreDannoCritico
        );
        shop.raggioRicercaRimbalzo = Mathf.Clamp(
            shop.raggioRicercaRimbalzo,
            1.5f,
            3.2f
        );
        shop.moltiplicatoreDannoRimbalzo = Mathf.Clamp(
            shop.moltiplicatoreDannoRimbalzo,
            0.5f,
            1f
        );
        shop.durataRallentamentoBase = Mathf.Max(
            0.1f,
            shop.durataRallentamentoBase
        );
        shop.durataRallentamentoPerLivello = Mathf.Max(
            0f,
            shop.durataRallentamentoPerLivello
        );
        shop.forzaSpintaPerLivello = Mathf.Max(
            0f,
            shop.forzaSpintaPerLivello
        );

        fattoria.numeroZoneFango = Mathf.Clamp(
            fattoria.numeroZoneFango,
            1,
            5
        );
        fattoria.numeroBallePaglia = Mathf.Clamp(
            fattoria.numeroBallePaglia,
            0,
            4
        );
        fattoria.numeroZuccheEsplosive = Mathf.Clamp(
            fattoria.numeroZuccheEsplosive,
            0,
            4
        );
        fattoria.numeroCasseMonete = Mathf.Clamp(
            fattoria.numeroCasseMonete,
            0,
            3
        );
        fattoria.numeroCasseCura = Mathf.Clamp(
            fattoria.numeroCasseCura,
            0,
            3
        );
        fattoria.raggioMinimoArena = Mathf.Max(
            2f,
            fattoria.raggioMinimoArena
        );
        fattoria.raggioMassimoArena = Mathf.Max(
            fattoria.raggioMinimoArena + 0.5f,
            fattoria.raggioMassimoArena
        );
        fattoria.scalaVerticaleArena = Mathf.Clamp(
            fattoria.scalaVerticaleArena,
            0.4f,
            1f
        );
        fattoria.distanzaMinimaElementi = Mathf.Max(
            0.5f,
            fattoria.distanzaMinimaElementi
        );
        fattoria.raggioLiberoGiocatore = Mathf.Max(
            0.5f,
            fattoria.raggioLiberoGiocatore
        );
        fattoria.raggioLiberoGalline = Mathf.Max(
            0.5f,
            fattoria.raggioLiberoGalline
        );
        fattoria.raggioFango = Mathf.Max(0.5f, fattoria.raggioFango);
        fattoria.velocitaGiocatoreNelFango = Mathf.Clamp(
            fattoria.velocitaGiocatoreNelFango,
            0.2f,
            1f
        );
        fattoria.velocitaVolpiNelFango = Mathf.Clamp(
            fattoria.velocitaVolpiNelFango,
            0.2f,
            1f
        );
        fattoria.vitaBallaPaglia = Mathf.Max(
            1,
            fattoria.vitaBallaPaglia
        );
        fattoria.vitaZucca = Mathf.Max(1, fattoria.vitaZucca);
        fattoria.raggioEsplosioneZucca = Mathf.Max(
            0.5f,
            fattoria.raggioEsplosioneZucca
        );
        fattoria.dannoEsplosioneZucca = Mathf.Max(
            1,
            fattoria.dannoEsplosioneZucca
        );
        fattoria.spintaEsplosioneZucca = Mathf.Max(
            0f,
            fattoria.spintaEsplosioneZucca
        );
        fattoria.vitaCassa = Mathf.Max(1, fattoria.vitaCassa);
        fattoria.moneteCassa = Mathf.Max(0, fattoria.moneteCassa);
        fattoria.curaCassa = Mathf.Max(0, fattoria.curaCassa);
        fattoria.durataSuggerimentoIniziale = Mathf.Max(
            0f,
            fattoria.durataSuggerimentoIniziale
        );

        feedbackCombattimento.dimensioneMirino = Mathf.Clamp(
            feedbackCombattimento.dimensioneMirino,
            14f,
            48f
        );
        feedbackCombattimento.particelleImpatto = Mathf.Clamp(
            feedbackCombattimento.particelleImpatto,
            3,
            8
        );
        ondate.durataPreavvisoSpawn = Mathf.Clamp(
            ondate.durataPreavvisoSpawn,
            0.15f,
            1f
        );
        ondate.sogliaUltimiNemici = Mathf.Clamp(
            ondate.sogliaUltimiNemici,
            1,
            4
        );
        ondate.volumeSegnaleUltimiNemici = Mathf.Clamp01(
            ondate.volumeSegnaleUltimiNemici
        );

        NormalizzaVariante(variantiVolpe.comune);
        NormalizzaVariante(variantiVolpe.agile);
        NormalizzaVariante(variantiVolpe.robusta);
        NormalizzaVariante(variantiVolpe.schivatrice);
        NormalizzaVariante(variantiVolpe.alfa);
        NormalizzaVariante(variantiVolpe.ululatrice);
        NormalizzaVariante(variantiVolpe.sputafango);
        NormalizzaVariante(variantiVolpe.scavatrice);
        variantiVolpe.distanzaAttivazioneSchivata = Mathf.Max(
            0.25f,
            variantiVolpe.distanzaAttivazioneSchivata
        );
        variantiVolpe.durataSchivata = Mathf.Max(
            0.05f,
            variantiVolpe.durataSchivata
        );
        variantiVolpe.recuperoSchivata = Mathf.Max(
            0.1f,
            variantiVolpe.recuperoSchivata
        );
        variantiVolpe.moltiplicatoreVelocitaSchivata = Mathf.Max(
            1f,
            variantiVolpe.moltiplicatoreVelocitaSchivata
        );
        variantiVolpe.distanzaPreparazioneAlfa = Mathf.Max(
            0.5f,
            variantiVolpe.distanzaPreparazioneAlfa
        );
        variantiVolpe.durataPreparazioneAlfa = Mathf.Max(
            0.1f,
            variantiVolpe.durataPreparazioneAlfa
        );
        variantiVolpe.durataScattoAlfa = Mathf.Max(
            0.1f,
            variantiVolpe.durataScattoAlfa
        );
        variantiVolpe.moltiplicatoreScattoAlfa = Mathf.Max(
            1f,
            variantiVolpe.moltiplicatoreScattoAlfa
        );
        variantiVolpe.recuperoScattoAlfa = Mathf.Max(
            0.1f,
            variantiVolpe.recuperoScattoAlfa
        );
        variantiVolpe.raggioUlulato = Mathf.Max(
            0.5f,
            variantiVolpe.raggioUlulato
        );
        variantiVolpe.recuperoUlulato = Mathf.Max(
            0.1f,
            variantiVolpe.recuperoUlulato
        );
        variantiVolpe.durataPreparazioneUlulato = Mathf.Max(
            0.1f,
            variantiVolpe.durataPreparazioneUlulato
        );
        variantiVolpe.durataRallentamentoUlulato = Mathf.Max(
            0.1f,
            variantiVolpe.durataRallentamentoUlulato
        );
        variantiVolpe.moltiplicatoreRallentamentoUlulato = Mathf.Clamp(
            variantiVolpe.moltiplicatoreRallentamentoUlulato,
            0.1f,
            1f
        );
        variantiVolpe.distanzaTiroFango = Mathf.Max(
            1f,
            variantiVolpe.distanzaTiroFango
        );
        variantiVolpe.recuperoTiroFango = Mathf.Max(
            0.1f,
            variantiVolpe.recuperoTiroFango
        );
        variantiVolpe.velocitaProiettileFango = Mathf.Max(
            0.5f,
            variantiVolpe.velocitaProiettileFango
        );
        variantiVolpe.durataPozzaFango = Mathf.Max(
            0.25f,
            variantiVolpe.durataPozzaFango
        );
        variantiVolpe.raggioPozzaFango = Mathf.Max(
            0.25f,
            variantiVolpe.raggioPozzaFango
        );
        variantiVolpe.moltiplicatoreRallentamentoFango = Mathf.Clamp(
            variantiVolpe.moltiplicatoreRallentamentoFango,
            0.1f,
            1f
        );
        variantiVolpe.dannoFango = Mathf.Max(
            1,
            variantiVolpe.dannoFango
        );
        variantiVolpe.distanzaInizioScavo = Mathf.Max(
            1f,
            variantiVolpe.distanzaInizioScavo
        );
        variantiVolpe.durataScavo = Mathf.Max(
            0.1f,
            variantiVolpe.durataScavo
        );
        variantiVolpe.durataEmersione = Mathf.Max(
            0.1f,
            variantiVolpe.durataEmersione
        );
        variantiVolpe.recuperoScavo = Mathf.Max(
            0.1f,
            variantiVolpe.recuperoScavo
        );
        variantiVolpe.moltiplicatoreVelocitaScavo = Mathf.Max(
            1f,
            variantiVolpe.moltiplicatoreVelocitaScavo
        );
        variantiVolpe.volumeVersi = Mathf.Clamp01(
            variantiVolpe.volumeVersi
        );

        if (shop.frequenzeBlocco != null)
        {
            for (int i = 0; i < shop.frequenzeBlocco.Length; i++)
            {
                int minimo = 2 + shop.frequenzeBlocco.Length - 1 - i;
                int massimo = i == 0
                    ? int.MaxValue
                    : shop.frequenzeBlocco[i - 1] - 1;
                shop.frequenzeBlocco[i] = Mathf.Clamp(
                    shop.frequenzeBlocco[i],
                    minimo,
                    massimo
                );
            }
        }

        if (ondate.ondate == null) return;
        foreach (Wave onda in ondate.ondate)
        {
            if (onda == null) continue;
            onda.numeroNemici = Mathf.Max(0, onda.numeroNemici);
            if (onda.sequenzaVolpi != null)
            {
                for (int i = 0; i < onda.sequenzaVolpi.Length; i++)
                {
                    onda.sequenzaVolpi[i] = FoxVariantStyle.Normalizza(
                        onda.sequenzaVolpi[i]
                    );
                }
            }
            onda.intervalloTraNemici = Mathf.Max(
                0.05f,
                onda.intervalloTraNemici
            );
            onda.dimensioneMassimaGruppo = Mathf.Clamp(
                onda.dimensioneMassimaGruppo,
                1,
                4
            );
            onda.intervalloTraGruppi = Mathf.Max(
                onda.intervalloTraNemici,
                onda.intervalloTraGruppi
            );
            onda.numeroMaialiniBonus = Mathf.Max(
                0,
                onda.numeroMaialiniBonus
            );
            onda.vitaMaialinoBonus = Mathf.Max(1, onda.vitaMaialinoBonus);
            onda.moneteMaialinoBonus = Mathf.Max(
                0,
                onda.moneteMaialinoBonus
            );
        }
    }

    private static void NormalizzaArrayNonNegativo(int[] valori)
    {
        if (valori == null) return;
        for (int i = 0; i < valori.Length; i++)
        {
            int minimo = i == 0 ? 1 : valori[i - 1] + 1;
            valori[i] = Mathf.Max(minimo, valori[i]);
        }
    }

    private static void NormalizzaVariante(FoxVariantStats variante)
    {
        if (variante == null) return;
        variante.moltiplicatoreVelocita = Mathf.Max(
            0.1f,
            variante.moltiplicatoreVelocita
        );
        variante.moltiplicatoreAccelerazione = Mathf.Max(
            0.1f,
            variante.moltiplicatoreAccelerazione
        );
        variante.moltiplicatoreDecelerazione = Mathf.Max(
            0.1f,
            variante.moltiplicatoreDecelerazione
        );
        variante.moltiplicatoreVita = Mathf.Max(
            0.1f,
            variante.moltiplicatoreVita
        );
        variante.moltiplicatoreIntervalloAttacco = Mathf.Max(
            0.1f,
            variante.moltiplicatoreIntervalloAttacco
        );
        variante.scala = Mathf.Max(0.1f, variante.scala);
        variante.moltiplicatoreRinculo = Mathf.Clamp(
            variante.moltiplicatoreRinculo,
            0f,
            2f
        );
        variante.monetePerEliminazione = Mathf.Max(
            0,
            variante.monetePerEliminazione
        );
        variante.ampiezzaSerpentina = Mathf.Clamp(
            variante.ampiezzaSerpentina,
            0f,
            0.8f
        );
        variante.frequenzaSerpentina = Mathf.Max(
            0f,
            variante.frequenzaSerpentina
        );
    }
}
