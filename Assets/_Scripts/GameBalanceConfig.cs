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
public sealed class ShopBalanceSettings
{
    [Header("Offerte e ritmo della bottega")]
    [Range(3, 4)] public int numeroOfferte = 4;
    [Min(0)] public int costoRerollBase = 1;
    [Min(0)] public int incrementoCostoReroll = 1;
    [Min(0)] public int bonusCompletamentoOnda = 1;

    [Header("Prezzi per livello")]
    public int[] costiMovimento = { 3, 5, 8 };
    public int[] costiResistenza = { 4, 7, 10 };
    public int[] costiSalute = { 4, 7, 10 };
    public int[] costiDanno = { 8, 14 };
    public int[] costiCadenza = { 3, 6, 9 };
    public int[] costiPenetrazione = { 5, 8, 12 };
    [Min(0)] public int costoCura = 2;

    [Header("Prezzi modificatori speciali")]
    public int[] costiColpoAggiuntivo = { 6, 10 };
    public int[] costiRafficaRaccolto = { 11 };
    public int[] costiPatataGigante = { 4, 7 };
    public int[] costiPatataEsplosiva = { 12 };
    public int[] costiCritico = { 4, 7, 10 };
    public int[] costiRimbalzo = { 6, 10 };
    public int[] costiRallentamento = { 3, 6 };
    public int[] costiSpinta = { 3, 6, 9 };

    [Header("Effetti")]
    [Min(0f)] public float incrementoMovimento = 0.5f;
    public int[] frequenzeBlocco = { 5, 4, 3 };
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
    [Min(0.2f)] public float raggioEsplosione = 1.25f;
    [Range(0.1f, 2f)] public float moltiplicatoreDannoEsplosione = 0.5f;

    [Header("Effetti build Perforazione")]
    [Range(0f, 1f)] public float probabilitaCriticoPerLivello = 0.12f;
    [Min(1f)] public float moltiplicatoreDannoCritico = 2f;
    [Min(0.5f)] public float raggioRicercaRimbalzo = 3.8f;

    [Header("Effetti build Controllo")]
    [Range(0.1f, 0.95f)] public float rallentamentoPrimoLivello = 0.72f;
    [Range(0f, 0.4f)] public float riduzioneRallentamentoPerLivello = 0.12f;
    [Min(0.1f)] public float durataRallentamentoBase = 1.1f;
    [Min(0f)] public float durataRallentamentoPerLivello = 0.35f;
    [Min(0f)] public float forzaSpintaPerLivello = 0.85f;
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
                nomeOndata = "Primi intrusi",
                numeroNemici = 4,
                sequenzaVolpi = new[]
                {
                    TipoVolpe.Comune,
                    TipoVolpe.Comune,
                    TipoVolpe.Comune,
                    TipoVolpe.Agile
                },
                intervalloTraNemici = 0.7f,
                dimensioneMassimaGruppo = 2,
                intervalloTraGruppi = 3.25f,
                numeroMaialiniBonus = 1,
                vitaMaialinoBonus = 2,
                moneteMaialinoBonus = 2
            },
            new Wave
            {
                nomeOndata = "Branco in arrivo",
                numeroNemici = 5,
                sequenzaVolpi = new[]
                {
                    TipoVolpe.Comune,
                    TipoVolpe.Agile,
                    TipoVolpe.Comune,
                    TipoVolpe.Agile,
                    TipoVolpe.Robusta
                },
                intervalloTraNemici = 0.7f,
                dimensioneMassimaGruppo = 2,
                intervalloTraGruppi = 2f,
                numeroMaialiniBonus = 1,
                vitaMaialinoBonus = 2,
                moneteMaialinoBonus = 2
            },
            new Wave
            {
                nomeOndata = "Assalto alla fattoria",
                numeroNemici = 6,
                sequenzaVolpi = new[]
                {
                    TipoVolpe.Comune,
                    TipoVolpe.Agile,
                    TipoVolpe.Robusta,
                    TipoVolpe.Comune,
                    TipoVolpe.Robusta,
                    TipoVolpe.Ladra
                },
                intervalloTraNemici = 0.65f,
                dimensioneMassimaGruppo = 3,
                intervalloTraGruppi = 3.4f,
                numeroMaialiniBonus = 1,
                vitaMaialinoBonus = 3,
                moneteMaialinoBonus = 3
            },
            new Wave
            {
                nomeOndata = "Furia della campagna",
                numeroNemici = 7,
                sequenzaVolpi = new[]
                {
                    TipoVolpe.Agile,
                    TipoVolpe.Robusta,
                    TipoVolpe.Ladra,
                    TipoVolpe.Agile,
                    TipoVolpe.Robusta,
                    TipoVolpe.Comune,
                    TipoVolpe.Alfa
                },
                intervalloTraNemici = 0.6f,
                dimensioneMassimaGruppo = 3,
                intervalloTraGruppi = 2.1f,
                numeroMaialiniBonus = 2,
                vitaMaialinoBonus = 3,
                moneteMaialinoBonus = 3
            },
            new Wave
            {
                nomeOndata = "Ultima difesa",
                numeroNemici = 8,
                sequenzaVolpi = new[]
                {
                    TipoVolpe.Comune,
                    TipoVolpe.Agile,
                    TipoVolpe.Robusta,
                    TipoVolpe.Agile,
                    TipoVolpe.Ladra,
                    TipoVolpe.Robusta,
                    TipoVolpe.Ladra,
                    TipoVolpe.Alfa
                },
                intervalloTraNemici = 0.55f,
                dimensioneMassimaGruppo = 3,
                intervalloTraGruppi = 2.3f,
                numeroMaialiniBonus = 2,
                vitaMaialinoBonus = 4,
                moneteMaialinoBonus = 5
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
        "Blocco 4 - Shop e sistema di build - 2026-07-16";

    [SerializeField] private PlayerBalanceSettings giocatore =
        new PlayerBalanceSettings();
    [SerializeField] private FoxBalanceSettings volpe =
        new FoxBalanceSettings();
    [SerializeField] private FoxVariantsBalanceSettings variantiVolpe =
        new FoxVariantsBalanceSettings();
    [SerializeField] private PigBalanceSettings maialino =
        new PigBalanceSettings();
    [SerializeField] private ShopBalanceSettings shop =
        new ShopBalanceSettings();
    [SerializeField] private WaveBalanceSettings ondate =
        new WaveBalanceSettings();
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
    public ShopBalanceSettings Shop => shop;
    public WaveBalanceSettings Ondate => ondate;
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

        giocatore.intervalloSparoMinimo = Mathf.Max(
            0.01f,
            giocatore.intervalloSparoMinimo
        );
        giocatore.intervalloSparo = Mathf.Max(
            giocatore.intervalloSparoMinimo,
            giocatore.intervalloSparo
        );
        maialino.cambioDirezioneMassimo = Mathf.Max(
            maialino.cambioDirezioneMinimo,
            maialino.cambioDirezioneMassimo
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
        shop.costoRerollBase = Mathf.Max(0, shop.costoRerollBase);
        shop.incrementoCostoReroll = Mathf.Max(
            0,
            shop.incrementoCostoReroll
        );
        shop.bonusCompletamentoOnda = Mathf.Max(
            0,
            shop.bonusCompletamentoOnda
        );
        shop.colpiPerRafficaRaccolto = Mathf.Max(
            2,
            shop.colpiPerRafficaRaccolto
        );
        shop.raggioEsplosione = Mathf.Max(0.2f, shop.raggioEsplosione);
        shop.moltiplicatoreDannoCritico = Mathf.Max(
            1f,
            shop.moltiplicatoreDannoCritico
        );
        shop.raggioRicercaRimbalzo = Mathf.Max(
            0.5f,
            shop.raggioRicercaRimbalzo
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
        NormalizzaVariante(variantiVolpe.ladra);
        NormalizzaVariante(variantiVolpe.alfa);
        variantiVolpe.intervalloRicercaGallina = Mathf.Max(
            0.05f,
            variantiVolpe.intervalloRicercaGallina
        );
        variantiVolpe.distanzaPrelievoGallina = Mathf.Max(
            0.1f,
            variantiVolpe.distanzaPrelievoGallina
        );
        variantiVolpe.distanzaFugaLadra = Mathf.Max(
            2f,
            variantiVolpe.distanzaFugaLadra
        );
        variantiVolpe.moltiplicatoreFugaLadra = Mathf.Max(
            1f,
            variantiVolpe.moltiplicatoreFugaLadra
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
        variantiVolpe.volumeVersi = Mathf.Clamp01(
            variantiVolpe.volumeVersi
        );

        if (shop.frequenzeBlocco != null)
        {
            for (int i = 0; i < shop.frequenzeBlocco.Length; i++)
            {
                shop.frequenzeBlocco[i] = Mathf.Max(
                    1,
                    shop.frequenzeBlocco[i]
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
            valori[i] = Mathf.Max(0, valori[i]);
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
