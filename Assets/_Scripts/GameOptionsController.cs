using System;
using System.Collections.Generic;
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
    public int LarghezzaRisoluzione { get; private set; }
    public int AltezzaRisoluzione { get; private set; }
    public FullScreenMode ModalitaSchermo { get; private set; }
    public bool VSyncAttivo { get; private set; }
    public int LimiteFps { get; private set; }
    public bool TutorialCompletato { get; private set; }

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
        ApplicaImpostazioniVideo();
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

    public void ImpostaRisoluzione(int larghezza, int altezza)
    {
        bool automatica = larghezza <= 0 || altezza <= 0;
        larghezza = automatica ? 0 : Mathf.Clamp(larghezza, 640, 7680);
        altezza = automatica ? 0 : Mathf.Clamp(altezza, 360, 4320);
        if (LarghezzaRisoluzione == larghezza &&
            AltezzaRisoluzione == altezza)
        {
            return;
        }

        LarghezzaRisoluzione = larghezza;
        AltezzaRisoluzione = altezza;
        SaveService.ModificaDispositivo(dati =>
        {
            dati.larghezzaRisoluzione = larghezza;
            dati.altezzaRisoluzione = altezza;
        });
        ApplicaImpostazioniVideo();
        RegistraModifica();
    }

    public void ImpostaModalitaSchermo(FullScreenMode modalita)
    {
        if (!Enum.IsDefined(typeof(FullScreenMode), modalita))
        {
            modalita = FullScreenMode.FullScreenWindow;
        }
        if (ModalitaSchermo == modalita) return;

        ModalitaSchermo = modalita;
        SaveService.ModificaDispositivo(
            dati => dati.modalitaSchermo = (int)modalita
        );
        ApplicaImpostazioniVideo();
        RegistraModifica();
    }

    public void ImpostaVSyncAttivo(bool attivo)
    {
        if (VSyncAttivo == attivo) return;

        VSyncAttivo = attivo;
        SaveService.ModificaDispositivo(
            dati => dati.vSyncAttivo = attivo
        );
        ApplicaImpostazioniVideo();
        RegistraModifica();
    }

    public void ImpostaLimiteFps(int limite)
    {
        limite = NormalizzaLimiteFps(limite);
        if (LimiteFps == limite) return;

        LimiteFps = limite;
        SaveService.ModificaDispositivo(
            dati => dati.limiteFps = limite
        );
        ApplicaImpostazioniVideo();
        RegistraModifica();
    }

    public void ImpostaTutorialCompletato(bool completato)
    {
        if (TutorialCompletato == completato) return;

        TutorialCompletato = completato;
        SaveService.ModificaDispositivo(
            dati => dati.tutorialCompletato = completato
        );
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
        ImpostaRisoluzione(0, 0);
        ImpostaModalitaSchermo(FullScreenMode.FullScreenWindow);
        ImpostaVSyncAttivo(true);
        ImpostaLimiteFps(60);
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
        LarghezzaRisoluzione = dati.larghezzaRisoluzione;
        AltezzaRisoluzione = dati.altezzaRisoluzione;
        ModalitaSchermo = Enum.IsDefined(
            typeof(FullScreenMode),
            dati.modalitaSchermo
        )
            ? (FullScreenMode)dati.modalitaSchermo
            : FullScreenMode.FullScreenWindow;
        VSyncAttivo = dati.vSyncAttivo;
        LimiteFps = NormalizzaLimiteFps(dati.limiteFps);
        TutorialCompletato = dati.tutorialCompletato;
    }

    public void ApplicaImpostazioniVideo()
    {
        QualitySettings.vSyncCount = VSyncAttivo ? 1 : 0;
        Application.targetFrameRate = VSyncAttivo
            ? -1
            : (LimiteFps > 0 ? LimiteFps : -1);

        if (!Application.isPlaying || Application.isEditor) return;

        int larghezza = LarghezzaRisoluzione > 0
            ? LarghezzaRisoluzione
            : Screen.currentResolution.width;
        int altezza = AltezzaRisoluzione > 0
            ? AltezzaRisoluzione
            : Screen.currentResolution.height;
        Screen.SetResolution(larghezza, altezza, ModalitaSchermo);
    }

    public static IReadOnlyList<Vector2Int> OttieniRisoluzioniSupportate()
    {
        List<Vector2Int> risultato = new List<Vector2Int>();
        Resolution[] disponibili = Screen.resolutions;
        for (int i = 0; i < disponibili.Length; i++)
        {
            Vector2Int candidata = new Vector2Int(
                disponibili[i].width,
                disponibili[i].height
            );
            if (candidata.x < 640 || candidata.y < 360 ||
                risultato.Contains(candidata))
            {
                continue;
            }
            risultato.Add(candidata);
        }

        if (risultato.Count == 0)
        {
            risultato.Add(new Vector2Int(1280, 720));
            risultato.Add(new Vector2Int(1600, 900));
            risultato.Add(new Vector2Int(1920, 1080));
        }
        risultato.Sort((a, b) =>
        {
            int confronto = (a.x * a.y).CompareTo(b.x * b.y);
            return confronto != 0 ? confronto : a.x.CompareTo(b.x);
        });
        return risultato;
    }

    private static int NormalizzaLimiteFps(int valore)
    {
        if (valore <= 0) return 0;
        int[] consentiti = { 30, 60, 120, 144, 165, 240 };
        int migliore = consentiti[0];
        int distanzaMigliore = Mathf.Abs(valore - migliore);
        for (int i = 1; i < consentiti.Length; i++)
        {
            int distanza = Mathf.Abs(valore - consentiti[i]);
            if (distanza >= distanzaMigliore) continue;
            migliore = consentiti[i];
            distanzaMigliore = distanza;
        }
        return migliore;
    }

    private void RegistraModifica()
    {
        salvataggioInAttesa = true;
        istanteUltimaModifica = Time.unscaledTime;
        ImpostazioniCambiate?.Invoke();
    }
}
