using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Pannello PC dedicato a video e rimappatura dei comandi. Si apre dal
/// menu di pausa, quindi non cambia autonomamente lo stato della partita.
/// </summary>
public sealed class PcSettingsMenu : MonoBehaviour
{
    private static readonly FullScreenMode[] ModalitaDisponibili =
    {
        FullScreenMode.Windowed,
        FullScreenMode.FullScreenWindow,
        FullScreenMode.ExclusiveFullScreen
    };

    private static readonly int[] LimitiFpsDisponibili =
        { 30, 60, 120, 144, 165, 240, 0 };

    private static readonly ComandoRimappabile[] Comandi =
    {
        ComandoRimappabile.MovimentoSu,
        ComandoRimappabile.MovimentoGiu,
        ComandoRimappabile.MovimentoSinistra,
        ComandoRimappabile.MovimentoDestra,
        ComandoRimappabile.Fuoco,
        ComandoRimappabile.Schivata,
        ComandoRimappabile.Pausa
    };

    private static readonly string[] NomiComandi =
    {
        "MOVIMENTO SU",
        "MOVIMENTO GIÙ",
        "MOVIMENTO SINISTRA",
        "MOVIMENTO DESTRA",
        "FUOCO",
        "SCHIVATA",
        "PAUSA"
    };

    private GameObject pannelloOverlay;
    private TMP_Text testoRisoluzione;
    private TMP_Text testoModalita;
    private TMP_Text testoVSync;
    private TMP_Text testoFps;
    private TMP_Text testoMessaggio;
    private readonly List<TMP_Text> testiBinding = new List<TMP_Text>();
    private readonly List<Button> pulsantiBinding = new List<Button>();
    private Button pulsanteChiudi;
    private TMP_FontAsset fontInterfaccia;
    private GameOptionsController opzioni;
    private FarmerInputController input;
    private IReadOnlyList<Vector2Int> risoluzioni;
    private bool costruito;

    public static PcSettingsMenu Instance { get; private set; }
    public static bool ApertoGlobale => Instance != null && Instance.Aperto;
    public bool Aperto => pannelloOverlay != null && pannelloOverlay.activeSelf;
    public int NumeroComandiRimappabili => Comandi.Length;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void AzzeraStatoStatico()
    {
        Instance = null;
    }

    public static PcSettingsMenu CreaOTrova()
    {
        if (Instance != null) return Instance;

        PcSettingsMenu esistente = FindFirstObjectByType<PcSettingsMenu>();
        if (esistente != null)
        {
            Instance = esistente;
            return esistente;
        }

        GameObject radice = new GameObject(
            "OpzioniVideoEComandi",
            typeof(RectTransform)
        );
        return radice.AddComponent<PcSettingsMenu>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        if (Application.isPlaying) DontDestroyOnLoad(gameObject);
        opzioni = GameOptionsController.CreaOTrova();
        input = FarmerInputController.CreaOTrova();
        risoluzioni = GameOptionsController.OttieniRisoluzioniSupportate();
        CostruisciInterfaccia();
        opzioni.ImpostazioniCambiate += AggiornaValori;
        SceneManager.sceneLoaded += ScenaCaricata;
    }

    private void OnDestroy()
    {
        if (opzioni != null)
        {
            opzioni.ImpostazioniCambiate -= AggiornaValori;
        }
        SceneManager.sceneLoaded -= ScenaCaricata;
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (!Aperto || input == null || input.RimappaturaInCorso) return;
        if (input.AnnullaPremutaQuestoFrame ||
            input.PausaPremutaQuestoFrame)
        {
            Nascondi();
        }
    }

    public void Mostra()
    {
        if (!costruito || Aperto) return;

        pannelloOverlay.SetActive(true);
        testoMessaggio.text =
            "I COMANDI DEL GAMEPAD RESTANO SEMPRE DISPONIBILI";
        AggiornaValori();
        input?.SelezionaPerInterfaccia(pulsanteChiudi);
        FarmAudioController.RiproduciInterfaccia();
    }

    public void Nascondi()
    {
        if (!costruito || !Aperto ||
            (input != null && input.RimappaturaInCorso))
        {
            return;
        }

        pannelloOverlay.SetActive(false);
        opzioni?.Salva();
        PauseSettingsMenu pausa = PauseSettingsMenu.Instance;
        input?.SelezionaPerInterfaccia(
            pausa != null ? pausa.SelezioneVideoComandi : null
        );
        FarmAudioController.RiproduciInterfaccia();
    }

    private void ScenaCaricata(Scene scena, LoadSceneMode modalita)
    {
        if (pannelloOverlay != null) pannelloOverlay.SetActive(false);
    }

    private void CambiaRisoluzione(int direzione)
    {
        int indice = 0;
        for (int i = 0; i < risoluzioni.Count; i++)
        {
            Vector2Int candidata = risoluzioni[i];
            if (candidata.x == opzioni.LarghezzaRisoluzione &&
                candidata.y == opzioni.AltezzaRisoluzione)
            {
                indice = i + 1;
                break;
            }
        }

        int totale = risoluzioni.Count + 1;
        indice = Modulo(indice + direzione, totale);
        if (indice == 0) opzioni.ImpostaRisoluzione(0, 0);
        else
        {
            Vector2Int scelta = risoluzioni[indice - 1];
            opzioni.ImpostaRisoluzione(scelta.x, scelta.y);
        }
    }

    private void CambiaModalita(int direzione)
    {
        int indice = Array.IndexOf(
            ModalitaDisponibili,
            opzioni.ModalitaSchermo
        );
        if (indice < 0) indice = 1;
        indice = Modulo(indice + direzione, ModalitaDisponibili.Length);
        opzioni.ImpostaModalitaSchermo(ModalitaDisponibili[indice]);
    }

    private void CambiaFps(int direzione)
    {
        int indice = Array.IndexOf(LimitiFpsDisponibili, opzioni.LimiteFps);
        if (indice < 0) indice = 1;
        indice = Modulo(indice + direzione, LimitiFpsDisponibili.Length);
        opzioni.ImpostaLimiteFps(LimitiFpsDisponibili[indice]);
    }

    private void AvviaRimappatura(int indice)
    {
        if (indice < 0 || indice >= Comandi.Length || input == null) return;

        for (int i = 0; i < pulsantiBinding.Count; i++)
        {
            pulsantiBinding[i].interactable = false;
        }
        testiBinding[indice].text = "PREMI UN TASTO...";
        testoMessaggio.text = "ESC ANNULLA LA RIMAPPATURA";
        input.AvviaRimappatura(
            Comandi[indice],
            (_, __) =>
            {
                for (int i = 0; i < pulsantiBinding.Count; i++)
                {
                    pulsantiBinding[i].interactable = true;
                }
                testoMessaggio.text =
                    "I COMANDI DEL GAMEPAD RESTANO SEMPRE DISPONIBILI";
                AggiornaValori();
                input.SelezionaPerInterfaccia(pulsantiBinding[indice]);
            }
        );
    }

    private void RipristinaComandi()
    {
        input?.RipristinaRimappature();
        testoMessaggio.text = "COMANDI RIPRISTINATI";
        AggiornaValori();
    }

    private void RivediTutorial()
    {
        opzioni.ImpostaTutorialCompletato(false);
        opzioni.Salva();
        PcOnboardingController.CreaOTrova()?.RichiediNuovaVisualizzazione();
        testoMessaggio.text = "IL TUTORIAL APPARIRÀ ALLA RIPRESA";
    }

    private void AggiornaValori()
    {
        if (!costruito || opzioni == null) return;

        testoRisoluzione.text = opzioni.LarghezzaRisoluzione <= 0
            ? "AUTOMATICA"
            : opzioni.LarghezzaRisoluzione + " × " +
              opzioni.AltezzaRisoluzione;
        testoModalita.text = NomeModalita(opzioni.ModalitaSchermo);
        testoVSync.text = opzioni.VSyncAttivo ? "ATTIVO" : "DISATTIVO";
        testoVSync.color = opzioni.VSyncAttivo
            ? FarmPixelUI.TestoConfrontoFlat
            : FarmPixelUI.TestoErroreFlat;
        testoFps.text = opzioni.LimiteFps <= 0
            ? "ILLIMITATO"
            : opzioni.LimiteFps.ToString();

        for (int i = 0; i < testiBinding.Count && i < Comandi.Length; i++)
        {
            testiBinding[i].text = input != null
                ? input.OttieniNomeBinding(Comandi[i])
                : "--";
        }
    }

    private void CostruisciInterfaccia()
    {
        if (costruito) return;
        costruito = true;

        TMP_Text riferimento = GameManager.TrovaTestoInterfaccia("OndataText");
        if (riferimento != null) fontInterfaccia = riferimento.font;

        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 2500;
        canvas.pixelPerfect = true;
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 1f;
        gameObject.AddComponent<GraphicRaycaster>();

        pannelloOverlay = CreaImmagine(
            "OverlayVideoComandi",
            transform,
            Vector2.zero,
            Vector2.zero,
            FarmPixelUI.ColoreVeloFlat,
            Vector2.zero,
            Vector2.one
        ).gameObject;
        RectTransform overlay = pannelloOverlay.GetComponent<RectTransform>();
        overlay.offsetMin = Vector2.zero;
        overlay.offsetMax = Vector2.zero;

        Image pannello = CreaImmagine(
            "PannelloVideoComandi",
            pannelloOverlay.transform,
            Vector2.zero,
            new Vector2(1120f, 940f),
            Color.white
        );
        FarmPixelUI.ApplicaPannello(pannello, false, true);

        CreaTesto(
            "Titolo",
            pannello.transform,
            "VIDEO E COMANDI",
            new Vector2(0f, 408f),
            new Vector2(900f, 52f),
            32f,
            FarmPixelUI.TestoTitoloFlat,
            FontStyles.Bold
        );
        CreaTesto(
            "TitoloVideo",
            pannello.transform,
            "SCHERMO",
            new Vector2(-275f, 344f),
            new Vector2(470f, 38f),
            23f,
            FarmPixelUI.TestoChiaroFlat,
            FontStyles.Bold
        );
        CreaTesto(
            "TitoloComandi",
            pannello.transform,
            "TASTIERA E MOUSE",
            new Vector2(275f, 344f),
            new Vector2(470f, 38f),
            23f,
            FarmPixelUI.TestoChiaroFlat,
            FontStyles.Bold
        );

        testoRisoluzione = CreaRigaSelettore(
            pannello.transform,
            "Risoluzione",
            "RISOLUZIONE",
            new Vector2(-275f, 248f),
            () => CambiaRisoluzione(-1),
            () => CambiaRisoluzione(1)
        );
        testoModalita = CreaRigaSelettore(
            pannello.transform,
            "Modalita",
            "MODALITÀ SCHERMO",
            new Vector2(-275f, 126f),
            () => CambiaModalita(-1),
            () => CambiaModalita(1)
        );
        testoVSync = CreaRigaSelettore(
            pannello.transform,
            "VSync",
            "VSYNC",
            new Vector2(-275f, 4f),
            () => opzioni.ImpostaVSyncAttivo(!opzioni.VSyncAttivo),
            () => opzioni.ImpostaVSyncAttivo(!opzioni.VSyncAttivo)
        );
        testoFps = CreaRigaSelettore(
            pannello.transform,
            "LimiteFps",
            "LIMITE FPS (CON VSYNC OFF)",
            new Vector2(-275f, -118f),
            () => CambiaFps(-1),
            () => CambiaFps(1)
        );

        for (int i = 0; i < Comandi.Length; i++)
        {
            int indice = i;
            CreaRigaBinding(
                pannello.transform,
                NomiComandi[i],
                new Vector2(275f, 278f - i * 72f),
                () => AvviaRimappatura(indice)
            );
        }

        testoMessaggio = CreaTesto(
            "Messaggio",
            pannello.transform,
            "I COMANDI DEL GAMEPAD RESTANO SEMPRE DISPONIBILI",
            new Vector2(0f, -292f),
            new Vector2(1000f, 34f),
            18f,
            FarmPixelUI.TestoMetaFlat,
            FontStyles.Bold
        );

        CreaPulsante(
            "RipristinaComandi",
            pannello.transform,
            "RIPRISTINA COMANDI",
            new Vector2(-320f, -382f),
            new Vector2(250f, 60f),
            FarmPixelUI.ColorePulsanteNeutroFlat,
            RipristinaComandi
        );
        CreaPulsante(
            "RivediTutorial",
            pannello.transform,
            "RIVEDI TUTORIAL",
            new Vector2(0f, -382f),
            new Vector2(250f, 60f),
            FarmPixelUI.ColorePulsanteOroFlat,
            RivediTutorial
        );
        pulsanteChiudi = CreaPulsante(
            "Chiudi",
            pannello.transform,
            "INDIETRO",
            new Vector2(320f, -382f),
            new Vector2(250f, 60f),
            FarmPixelUI.ColorePulsanteVerdeFlat,
            Nascondi
        );

        pannelloOverlay.SetActive(false);
    }

    private TMP_Text CreaRigaSelettore(
        Transform parent,
        string nome,
        string etichetta,
        Vector2 posizione,
        UnityEngine.Events.UnityAction precedente,
        UnityEngine.Events.UnityAction successivo
    )
    {
        Transform riga = CreaContenitore(nome, parent, posizione,
            new Vector2(470f, 102f));
        CreaTesto("Etichetta", riga, etichetta, new Vector2(0f, 31f),
            new Vector2(450f, 30f), 18f, FarmPixelUI.TestoChiaroFlat,
            FontStyles.Bold);
        CreaPulsante("Precedente", riga, "‹", new Vector2(-194f, -13f),
            new Vector2(62f, 50f), FarmPixelUI.ColorePulsanteNeutroFlat,
            precedente);
        CreaPulsante("Successivo", riga, "›", new Vector2(194f, -13f),
            new Vector2(62f, 50f), FarmPixelUI.ColorePulsanteNeutroFlat,
            successivo);
        return CreaTesto("Valore", riga, "--", new Vector2(0f, -13f),
            new Vector2(310f, 44f), 19f, FarmPixelUI.TestoTitoloFlat,
            FontStyles.Bold);
    }

    private void CreaRigaBinding(
        Transform parent,
        string etichetta,
        Vector2 posizione,
        UnityEngine.Events.UnityAction azione
    )
    {
        Transform riga = CreaContenitore(etichetta, parent, posizione,
            new Vector2(490f, 58f));
        CreaTesto("Etichetta", riga, etichetta, new Vector2(-120f, 0f),
            new Vector2(230f, 40f), 17f, FarmPixelUI.TestoChiaroFlat,
            FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
        Button pulsante = CreaPulsante("Binding", riga, "--",
            new Vector2(125f, 0f), new Vector2(220f, 48f),
            FarmPixelUI.ColorePulsanteNeutroFlat, azione);
        pulsantiBinding.Add(pulsante);
        testiBinding.Add(pulsante.GetComponentInChildren<TMP_Text>());
    }

    private Transform CreaContenitore(
        string nome,
        Transform parent,
        Vector2 posizione,
        Vector2 dimensioni
    )
    {
        GameObject oggetto = new GameObject(nome, typeof(RectTransform));
        oggetto.transform.SetParent(parent, false);
        ImpostaRect(oggetto.GetComponent<RectTransform>(), posizione, dimensioni);
        return oggetto.transform;
    }

    private Button CreaPulsante(
        string nome,
        Transform parent,
        string etichetta,
        Vector2 posizione,
        Vector2 dimensioni,
        Color tinta,
        UnityEngine.Events.UnityAction azione
    )
    {
        GameObject oggetto = new GameObject(nome, typeof(RectTransform),
            typeof(CanvasRenderer), typeof(Image), typeof(Button));
        oggetto.transform.SetParent(parent, false);
        ImpostaRect(oggetto.GetComponent<RectTransform>(), posizione, dimensioni);
        Button pulsante = oggetto.GetComponent<Button>();
        pulsante.targetGraphic = oggetto.GetComponent<Image>();
        FarmPixelUI.ApplicaPulsante(pulsante, tinta);
        pulsante.onClick.AddListener(azione);
        CreaTesto("Testo", oggetto.transform, etichetta, Vector2.zero,
            dimensioni - new Vector2(14f, 8f), 17f,
            FarmPixelUI.TestoPulsanteFlat, FontStyles.Bold);
        return pulsante;
    }

    private TMP_Text CreaTesto(
        string nome,
        Transform parent,
        string contenuto,
        Vector2 posizione,
        Vector2 dimensioni,
        float dimensione,
        Color colore,
        FontStyles stile,
        TextAlignmentOptions allineamento = TextAlignmentOptions.Center
    )
    {
        GameObject oggetto = new GameObject(nome, typeof(RectTransform),
            typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        oggetto.transform.SetParent(parent, false);
        ImpostaRect(oggetto.GetComponent<RectTransform>(), posizione, dimensioni);
        TextMeshProUGUI testo = oggetto.GetComponent<TextMeshProUGUI>();
        if (fontInterfaccia != null) testo.font = fontInterfaccia;
        testo.text = contenuto;
        testo.fontSize = dimensione;
        testo.fontStyle = stile;
        testo.alignment = allineamento;
        testo.color = colore;
        testo.textWrappingMode = TextWrappingModes.NoWrap;
        testo.overflowMode = TextOverflowModes.Ellipsis;
        testo.raycastTarget = false;
        FarmPixelUI.ApplicaTesto(testo, colore);
        return testo;
    }

    private Image CreaImmagine(
        string nome,
        Transform parent,
        Vector2 posizione,
        Vector2 dimensioni,
        Color colore,
        Vector2? ancoraMinima = null,
        Vector2? ancoraMassima = null
    )
    {
        GameObject oggetto = new GameObject(nome, typeof(RectTransform),
            typeof(CanvasRenderer), typeof(Image));
        oggetto.transform.SetParent(parent, false);
        RectTransform rect = oggetto.GetComponent<RectTransform>();
        rect.anchorMin = ancoraMinima ?? new Vector2(0.5f, 0.5f);
        rect.anchorMax = ancoraMassima ?? new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posizione;
        rect.sizeDelta = dimensioni;
        Image immagine = oggetto.GetComponent<Image>();
        immagine.color = colore;
        return immagine;
    }

    private static void ImpostaRect(
        RectTransform rect,
        Vector2 posizione,
        Vector2 dimensioni
    )
    {
        rect.anchorMin = rect.anchorMax = rect.pivot =
            new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posizione;
        rect.sizeDelta = dimensioni;
    }

    private static int Modulo(int valore, int modulo)
    {
        return (valore % modulo + modulo) % modulo;
    }

    private static string NomeModalita(FullScreenMode modalita)
    {
        switch (modalita)
        {
            case FullScreenMode.Windowed:
                return "FINESTRA";
            case FullScreenMode.ExclusiveFullScreen:
                return "SCHERMO INTERO ESCLUSIVO";
            default:
                return "SCHERMO INTERO SENZA BORDI";
        }
    }
}
