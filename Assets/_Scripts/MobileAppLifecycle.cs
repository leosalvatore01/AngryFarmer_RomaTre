using UnityEngine;

/// <summary>Salva prima della sospensione e richiede una ripresa esplicita della run.</summary>
public sealed class MobileAppLifecycle : MonoBehaviour
{
    public static MobileAppLifecycle Instance { get; private set; }
    public static bool AppSospesa => Instance != null && Instance.Sospesa;
    public bool Sospesa { get; private set; }
    public bool UltimoSalvataggioRiuscito { get; private set; } = true;
    private bool pausaSistema;
    private bool haFocus = true;
    private bool audioPrecedente;
    private bool mostraPausaAllaRipresa;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Azzera() => Instance = null;

    private void Awake() => Instance = this;

    private void OnDestroy()
    {
        if (Instance != this) return;
        if (Sospesa) AudioListener.pause = audioPrecedente;
        Instance = null;
    }

    private void OnApplicationPause(bool pausa) => NotificaPausa(pausa);
    private void OnApplicationFocus(bool focus) => NotificaFocus(focus);
    private void OnApplicationQuit() => SaveService.Flush();

    public void NotificaPausa(bool pausa)
    {
        pausaSistema = pausa;
        AggiornaSospensione();
    }

    public void NotificaFocus(bool focus)
    {
        haFocus = focus;
        AggiornaSospensione();
    }

    private void AggiornaSospensione()
    {
        bool nuova = pausaSistema || !haFocus;
        if (nuova == Sospesa) return;
        Sospesa = nuova;
        FarmerInputController.Instance?.AzzeraTouch();
        if (Sospesa)
        {
            audioPrecedente = AudioListener.pause;
            AudioListener.pause = true;
            GameManager gestore = GameManager.instance;
            if (gestore != null && !gestore.isGameOver && gestore.DifficoltaConfermata &&
                (gestore.StatoCorrente == StatoPartita.Onda || gestore.StatoCorrente == StatoPartita.Transizione))
            {
                gestore.ImpostaPausaManuale(true);
                mostraPausaAllaRipresa = true;
            }
            GameOptionsController.Instance?.Salva();
            UltimoSalvataggioRiuscito = SaveService.Flush();
            if (!UltimoSalvataggioRiuscito)
                Debug.LogWarning("[MOBILE] Salvataggio prima della sospensione non riuscito. Riprovare dalle opzioni.");
        }
        else
        {
            AudioListener.pause = audioPrecedente;
            GameOptionsController.Instance?.ApplicaImpostazioniVideo();
            MostraPausaSePronta();
        }
    }

    private void Update() => MostraPausaSePronta();

    private void MostraPausaSePronta()
    {
        if (Sospesa || !mostraPausaAllaRipresa) return;
        GameManager gestore = GameManager.instance;
        if (gestore == null || gestore.isGameOver)
        {
            mostraPausaAllaRipresa = false;
            return;
        }
        // Un banner di transizione può terminare al ritorno dal sistema operativo.
        if (gestore.StatoCorrente == StatoPartita.Transizione) return;
        PauseSettingsMenu.CreaOTrova()?.Mostra();
        mostraPausaAllaRipresa = false;
    }
}
