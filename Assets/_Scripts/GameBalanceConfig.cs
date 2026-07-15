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
    [Header("Prezzi per livello")]
    public int[] costiMovimento = { 3, 6, 10 };
    public int[] costiResistenza = { 4, 8, 13 };
    public int[] costiSalute = { 5, 9, 14 };
    public int[] costiDanno = { 9, 16 };
    public int[] costiCadenza = { 4, 7, 11 };
    public int[] costiPenetrazione = { 6, 10, 15 };
    [Min(0)] public int costoCura = 3;

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
                intervalloTraNemici = 1.8f,
                numeroMaialiniBonus = 0,
                vitaMaialinoBonus = 1,
                moneteMaialinoBonus = 0
            },
            new Wave
            {
                nomeOndata = "Primi intrusi",
                numeroNemici = 4,
                intervalloTraNemici = 1.55f,
                numeroMaialiniBonus = 1,
                vitaMaialinoBonus = 2,
                moneteMaialinoBonus = 3
            },
            new Wave
            {
                nomeOndata = "Branco in arrivo",
                numeroNemici = 5,
                intervalloTraNemici = 1.35f,
                numeroMaialiniBonus = 1,
                vitaMaialinoBonus = 2,
                moneteMaialinoBonus = 3
            },
            new Wave
            {
                nomeOndata = "Assalto alla fattoria",
                numeroNemici = 6,
                intervalloTraNemici = 1.2f,
                numeroMaialiniBonus = 1,
                vitaMaialinoBonus = 3,
                moneteMaialinoBonus = 4
            },
            new Wave
            {
                nomeOndata = "Furia della campagna",
                numeroNemici = 7,
                intervalloTraNemici = 1.1f,
                numeroMaialiniBonus = 2,
                vitaMaialinoBonus = 3,
                moneteMaialinoBonus = 4
            },
            new Wave
            {
                nomeOndata = "Ultima difesa",
                numeroNemici = 8,
                intervalloTraNemici = 1.05f,
                numeroMaialiniBonus = 2,
                vitaMaialinoBonus = 4,
                moneteMaialinoBonus = 5
            }
        };
    }
}

[CreateAssetMenu(
    fileName = "GameBalanceConfig",
    menuName = "Angry Farmer/Bilanciamento di riferimento"
)]
public sealed class GameBalanceConfig : ScriptableObject
{
    public const string PercorsoResources = "GameBalanceConfig";

    [SerializeField]
    private string versioneRiferimento = "Baseline 1 - 2026-07-15";

    [SerializeField] private PlayerBalanceSettings giocatore =
        new PlayerBalanceSettings();
    [SerializeField] private FoxBalanceSettings volpe =
        new FoxBalanceSettings();
    [SerializeField] private PigBalanceSettings maialino =
        new PigBalanceSettings();
    [SerializeField] private ShopBalanceSettings shop =
        new ShopBalanceSettings();
    [SerializeField] private WaveBalanceSettings ondate =
        new WaveBalanceSettings();

    private static GameBalanceConfig corrente;
    private static bool avvisoFallbackMostrato;

    public string VersioneRiferimento => versioneRiferimento;
    public PlayerBalanceSettings Giocatore => giocatore;
    public FoxBalanceSettings Volpe => volpe;
    public PigBalanceSettings Maialino => maialino;
    public ShopBalanceSettings Shop => shop;
    public WaveBalanceSettings Ondate => ondate;

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
            onda.intervalloTraNemici = Mathf.Max(
                0.05f,
                onda.intervalloTraNemici
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
}
