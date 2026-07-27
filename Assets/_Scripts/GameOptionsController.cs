using System;
using UnityEngine;

/// <summary>
/// Conserva le preferenze di comfort del giocatore e le rende disponibili
/// a tutti i sistemi del gioco. Le preferenze sopravvivono ai cambi scena.
/// </summary>
public sealed class GameOptionsController : MonoBehaviour
{
    public const float DimensioneMirinoMinima = 14f;
    public const float DimensioneMirinoMassima = 48f;

    private const float RitardoSalvataggio = 0.35f;

    private bool salvataggioInAttesa;
    private float istanteUltimaModifica;

    public static GameOptionsController Instance { get; private set; }

    public float VolumeMusica { get; private set; }
    public float VolumeEffetti { get; private set; }
    public bool VibrazioneAttiva { get; private set; }
    public bool FlashAttivi { get; private set; }
    public bool NumeriDannoAttivi { get; private set; }
    public float DimensioneMirino { get; private set; }

    public event Action ImpostazioniCambiate;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void AzzeraStatoStatico()
    {
        Instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreaPrimaDellaScena()
    {
        CreaOTrova();
    }

    public static GameOptionsController CreaOTrova()
    {
        if (Instance != null) return Instance;

        GameOptionsController esistente =
            FindFirstObjectByType<GameOptionsController>();
        if (esistente != null)
        {
            Instance = esistente;
            return esistente;
        }

        GameObject radice = new GameObject("OpzioniGiocatore");
        return radice.AddComponent<GameOptionsController>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Carica();
    }

    private void Update()
    {
        if (!salvataggioInAttesa) return;

        if (Time.unscaledTime - istanteUltimaModifica >= RitardoSalvataggio)
        {
            Salva();
        }
    }

    private void OnApplicationPause(bool inPausa)
    {
        if (inPausa) Salva();
    }

    private void OnApplicationQuit()
    {
        Salva();
    }

    private void OnDestroy()
    {
        if (Instance != this) return;

        Salva();
        Instance = null;
    }

    public void ImpostaVolumeMusica(float valore)
    {
        valore = Mathf.Clamp01(valore);
        if (Mathf.Approximately(VolumeMusica, valore)) return;

        VolumeMusica = valore;
        SaveService.ModificaDispositivo(dati =>
        {
            dati.volumeMusica = VolumeMusica;
        });
        RegistraModifica();
    }

    public void ImpostaVolumeEffetti(float valore)
    {
        valore = Mathf.Clamp01(valore);
        if (Mathf.Approximately(VolumeEffetti, valore)) return;

        VolumeEffetti = valore;
        SaveService.ModificaDispositivo(dati =>
        {
            dati.volumeEffetti = VolumeEffetti;
        });
        RegistraModifica();
    }

    public void ImpostaVibrazioneAttiva(bool attiva)
    {
        if (VibrazioneAttiva == attiva) return;

        VibrazioneAttiva = attiva;
        SaveService.ModificaDispositivo(dati =>
        {
            dati.vibrazioneAttiva = VibrazioneAttiva;
        });
        RegistraModifica();
    }

    public void ImpostaFlashAttivi(bool attivi)
    {
        if (FlashAttivi == attivi) return;

        FlashAttivi = attivi;
        SaveService.ModificaDispositivo(dati =>
        {
            dati.flashAttivi = FlashAttivi;
        });
        RegistraModifica();
    }

    public void ImpostaNumeriDannoAttivi(bool attivi)
    {
        if (NumeriDannoAttivi == attivi) return;

        NumeriDannoAttivi = attivi;
        SaveService.ModificaDispositivo(dati =>
        {
            dati.numeriDannoAttivi = NumeriDannoAttivi;
        });
        RegistraModifica();
    }

    public void ImpostaDimensioneMirino(float dimensione)
    {
        dimensione = Mathf.Clamp(
            Mathf.Round(dimensione),
            DimensioneMirinoMinima,
            DimensioneMirinoMassima
        );
        if (Mathf.Approximately(DimensioneMirino, dimensione)) return;

        DimensioneMirino = dimensione;
        SaveService.ModificaDispositivo(dati =>
        {
            dati.dimensioneMirino = DimensioneMirino;
        });
        RegistraModifica();
    }

    public void RipristinaPredefiniti()
    {
        CombatFeedbackSettings feedback =
            GameBalanceConfig.Corrente.FeedbackCombattimento;

        ImpostaVolumeMusica(feedback.audioAttivo ? 0.55f : 0f);
        ImpostaVolumeEffetti(feedback.audioAttivo ? 1f : 0f);
        ImpostaVibrazioneAttiva(feedback.vibrazioneCameraAttiva);
        ImpostaFlashAttivi(feedback.effettiVisiviAttivi);
        ImpostaNumeriDannoAttivi(true);
        ImpostaDimensioneMirino(feedback.dimensioneMirino);
        Salva();
    }

    public void Salva()
    {
        if (!salvataggioInAttesa) return;

        if (SaveService.SalvaDispositivoOra())
        {
            salvataggioInAttesa = false;
        }
    }

    private void Carica()
    {
        DeviceSettingsData dati = SaveService.Dispositivo;
        VolumeMusica = Mathf.Clamp01(dati.volumeMusica);
        VolumeEffetti = Mathf.Clamp01(dati.volumeEffetti);
        VibrazioneAttiva = dati.vibrazioneAttiva;
        FlashAttivi = dati.flashAttivi;
        NumeriDannoAttivi = dati.numeriDannoAttivi;
        DimensioneMirino = Mathf.Clamp(
            Mathf.Round(dati.dimensioneMirino),
            DimensioneMirinoMinima,
            DimensioneMirinoMassima
        );
    }

    private void RegistraModifica()
    {
        salvataggioInAttesa = true;
        istanteUltimaModifica = Time.unscaledTime;
        ImpostazioniCambiate?.Invoke();
    }
}
