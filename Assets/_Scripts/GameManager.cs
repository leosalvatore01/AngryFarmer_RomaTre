using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public enum StatoPartita
{
    Onda,
    Intervallo,
    FinePartita
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public int gallineRimaste = 0;
    public bool isGameOver = false;

    [SerializeField] private GameObject gameOverPanel;

    private TMP_Text testoUova;
    private TMP_Text testoUovaSalvate;
    private TMP_Text testoMonete;
    private TMP_Text titoloFinePartita;
    private TMP_Text riepilogoFinePartita;
    private Button pulsanteRiprova;
    private ShopInterOndata shopInterOndata;
    private GameObject schedaUovaHud;
    private GameObject schedaUovaSalvateHud;
    private int gallineTotali;
    private int uovaInizioOnda;

    public int monete = 0;
    public int MoneteRaccolte { get; private set; }
    public int MoneteSpese { get; private set; }
    public int UltimoBonusCompletamento { get; private set; }
    public int GallineTotali => gallineTotali;
    public int GallineAlSicuro =>
        Mathf.Min(gallineTotali, Gallina.ContaAlSicuro());
    public int UovaSalvate { get; private set; }
    public int SerieSalvataggi { get; private set; }
    public int MiglioreSerieSalvataggi { get; private set; }
    public int UltimoBonusSerie { get; private set; }
    public int UovaUltimaOnda { get; private set; }
    public int ObiettiviCompletati { get; private set; }
    public int ObiettiviFalliti { get; private set; }
    public string UltimoObiettivo { get; private set; } = string.Empty;
    public bool UltimoObiettivoValutato { get; private set; }
    public bool UltimoObiettivoCompletato { get; private set; }
    public int UovaUltimoObiettivo { get; private set; }
    public StatoPartita StatoCorrente { get; private set; } = StatoPartita.Onda;
    public bool PausaManualeAttiva { get; private set; }
    public bool GameplayAttivo =>
        !isGameOver &&
        StatoCorrente == StatoPartita.Onda &&
        !PausaManualeAttiva;
    public bool PausaInterOndataAttiva =>
        !isGameOver && StatoCorrente == StatoPartita.Intervallo;

    public event Action<int> MoneteCambiate;
    public event Action<int> UovaCambiate;
    public event Action<int, int> GallineCambiate;
    public event Action<StatoPartita> StatoPartitaCambiato;
    public event Action<bool> PausaManualeCambiata;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            monete = Mathf.Max(
                0,
                GameBalanceConfig.Corrente.Shop.moneteIniziali
            );
            MoneteRaccolte = monete;
            MoneteSpese = 0;
            UltimoBonusCompletamento = 0;
            UovaSalvate = 0;
            SerieSalvataggi = 0;
            MiglioreSerieSalvataggi = 0;
            UltimoBonusSerie = 0;
            UovaUltimaOnda = 0;
            ObiettiviCompletati = 0;
            ObiettiviFalliti = 0;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        ImpostaStatoPartita(StatoPartita.Onda);

        testoUova = TrovaTestoInterfaccia("UovaText");
        testoUovaSalvate = TrovaTestoInterfaccia("UovaSalvateText");
        testoMonete = TrovaTestoInterfaccia("MoneteText");

        ConfiguraHUD();
        AggiornaContatoreUova();
        AggiornaContatoreUovaSalvate();
        AggiornaContatoreMonete();

        ConfiguraPannelloFinale();
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        shopInterOndata = ShopInterOndata.CreaOTrova();
        FarmInteractiveArena.CreaOTrova();
        FarmObjectivesController.CreaOTrova();
    }

    public static TMP_Text TrovaTestoInterfaccia(string nome)
    {
        GameObject interfaccia = GameObject.Find("Interfaccia");
        if (interfaccia == null) return null;

        Transform elemento = interfaccia.transform.Find(nome);
        return elemento != null ? elemento.GetComponent<TMP_Text>() : null;
    }

    void ConfiguraHUD()
    {
        GameObject interfaccia = GameObject.Find("Interfaccia");
        if (interfaccia == null) return;

        Canvas canvas = interfaccia.GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.pixelPerfect = true;
        }
        CanvasScaler scaler = interfaccia.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.matchWidthOrHeight = 1f;
        }

        Transform pannelloEsistente = interfaccia.transform.Find("PannelloHUD");
        GameObject pannello;
        if (pannelloEsistente == null)
        {
            pannello = new GameObject(
                "PannelloHUD",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );
            pannello.transform.SetParent(interfaccia.transform, false);
            pannello.transform.SetAsFirstSibling();
        }
        else
        {
            pannello = pannelloEsistente.gameObject;
            pannello.transform.SetAsFirstSibling();
        }

        RectTransform rect = pannello.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(18f, -18f);
        rect.sizeDelta = new Vector2(376f, 278f);

        Image immagine = pannello.GetComponent<Image>();
        FarmPixelUI.ApplicaPannello(immagine, false, false);

        CreaTitoloHUD(pannello.transform);
        CreaSchedaHUD(
            pannello.transform,
            "SchedaOndata",
            FarmPixelIcon.Ondata,
            -68f
        );
        CreaSchedaHUD(
            pannello.transform,
            "SchedaVita",
            FarmPixelIcon.Cuore,
            -110f
        );
        CreaSchedaHUD(
            pannello.transform,
            "SchedaMonete",
            FarmPixelIcon.Moneta,
            -152f
        );
        schedaUovaHud = CreaSchedaHUD(
            pannello.transform,
            "SchedaUova",
            FarmPixelIcon.Gallina,
            -194f
        );
        schedaUovaSalvateHud = CreaSchedaHUD(
            pannello.transform,
            "SchedaUovaSalvate",
            FarmPixelIcon.Uovo,
            -236f
        );

        if (testoUovaSalvate == null)
        {
            testoUovaSalvate = CreaTestoHUDRuntime(
                interfaccia.transform,
                "UovaSalvateText"
            );
        }

        if (schedaUovaHud != null)
        {
            schedaUovaHud.SetActive(gallineRimaste > 0);
        }

        ConfiguraTestoHUD(
            TrovaTestoInterfaccia("OndataText"),
            new Vector2(82f, -86f),
            new Color(1f, 0.77f, 0.32f, 1f)
        );
        ConfiguraTestoHUD(
            TrovaTestoInterfaccia("VitaText"),
            new Vector2(82f, -128f),
            new Color(0.5f, 0.95f, 0.47f, 1f)
        );
        ConfiguraTestoHUD(
            testoMonete,
            new Vector2(82f, -170f),
            new Color(1f, 0.9f, 0.24f, 1f)
        );
        ConfiguraTestoHUD(
            testoUova,
            new Vector2(82f, -212f),
            new Color(1f, 0.94f, 0.76f, 1f)
        );
        ConfiguraTestoHUD(
            testoUovaSalvate,
            new Vector2(82f, -254f),
            new Color(1f, 0.82f, 0.28f, 1f)
        );

        if (testoUova != null)
        {
            testoUova.gameObject.SetActive(gallineRimaste > 0);
        }
        if (schedaUovaSalvateHud != null)
        {
            schedaUovaSalvateHud.SetActive(true);
        }
    }

    void CreaTitoloHUD(Transform parent)
    {
        if (parent.Find("TitoloFattoria") != null) return;

        GameObject oggetto = new GameObject(
            "TitoloFattoria",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );
        oggetto.transform.SetParent(parent, false);

        RectTransform rect = oggetto.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, -26f);
        rect.sizeDelta = new Vector2(330f, 34f);

        TextMeshProUGUI titolo = oggetto.GetComponent<TextMeshProUGUI>();
        TMP_Text riferimento = TrovaTestoInterfaccia("OndataText");
        if (riferimento != null) titolo.font = riferimento.font;
        titolo.text = "FATTORIA";
        titolo.fontSize = 24f;
        titolo.fontStyle = FontStyles.Bold;
        titolo.alignment = TextAlignmentOptions.Center;
        titolo.textWrappingMode = TextWrappingModes.NoWrap;
        titolo.raycastTarget = false;
        FarmPixelUI.ApplicaTesto(
            titolo,
            new Color(1f, 0.88f, 0.55f, 1f)
        );
    }

    static GameObject CreaSchedaHUD(
        Transform parent,
        string nome,
        FarmPixelIcon icona,
        float posizioneY
    )
    {
        Transform esistente = parent.Find(nome);
        if (esistente != null) return esistente.gameObject;

        GameObject scheda = new GameObject(
            nome,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        scheda.transform.SetParent(parent, false);

        RectTransform rect = scheda.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 0.5f);
        rect.anchoredPosition = new Vector2(14f, posizioneY);
        rect.sizeDelta = new Vector2(346f, 40f);

        Image sfondo = scheda.GetComponent<Image>();
        FarmPixelUI.ApplicaPannello(sfondo, true, false);

        FarmPixelUI.AggiungiIcona(
            scheda.transform,
            "Icona",
            icona,
            new Vector2(-145f, 0f),
            new Vector2(32f, 32f)
        );
        return scheda;
    }

    static void ConfiguraTestoHUD(
        TMP_Text testo,
        Vector2 posizione,
        Color colore
    )
    {
        if (testo == null) return;

        RectTransform rect = testo.rectTransform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 0.5f);
        rect.anchoredPosition = posizione;
        rect.sizeDelta = new Vector2(268f, 36f);

        testo.fontSize = 25f;
        testo.fontStyle = FontStyles.Bold;
        testo.alignment = TextAlignmentOptions.MidlineLeft;
        testo.textWrappingMode = TextWrappingModes.NoWrap;
        testo.overflowMode = TextOverflowModes.Overflow;
        testo.raycastTarget = false;
        FarmPixelUI.ApplicaTesto(testo, colore);
    }

    static TMP_Text CreaTestoHUDRuntime(Transform parent, string nome)
    {
        GameObject oggetto = new GameObject(
            nome,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );
        oggetto.transform.SetParent(parent, false);

        TextMeshProUGUI testo = oggetto.GetComponent<TextMeshProUGUI>();
        TMP_Text riferimento = TrovaTestoInterfaccia("OndataText");
        if (riferimento != null) testo.font = riferimento.font;
        testo.raycastTarget = false;
        return testo;
    }

    void ConfiguraPannelloFinale()
    {
        if (gameOverPanel == null) return;

        RectTransform pannelloRect = gameOverPanel.GetComponent<RectTransform>();
        if (pannelloRect != null)
        {
            pannelloRect.anchorMin = Vector2.zero;
            pannelloRect.anchorMax = Vector2.one;
            pannelloRect.offsetMin = Vector2.zero;
            pannelloRect.offsetMax = Vector2.zero;
        }

        Image sfondo = gameOverPanel.GetComponent<Image>();
        if (sfondo != null)
        {
            sfondo.color = FarmPixelUI.ColoreVeloFlat;
        }

        Transform pannelloRiepilogo =
            gameOverPanel.transform.Find("PannelloRiepilogoFinale");
        GameObject pannelloRiepilogoOggetto;
        if (pannelloRiepilogo == null)
        {
            pannelloRiepilogoOggetto = new GameObject(
                "PannelloRiepilogoFinale",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );
            pannelloRiepilogoOggetto.transform.SetParent(
                gameOverPanel.transform,
                false
            );
        }
        else
        {
            pannelloRiepilogoOggetto = pannelloRiepilogo.gameObject;
        }

        RectTransform pannelloRiepilogoRect =
            pannelloRiepilogoOggetto.GetComponent<RectTransform>();
        pannelloRiepilogoRect.anchorMin = new Vector2(0.5f, 0.5f);
        pannelloRiepilogoRect.anchorMax = new Vector2(0.5f, 0.5f);
        pannelloRiepilogoRect.pivot = new Vector2(0.5f, 0.5f);
        pannelloRiepilogoRect.anchoredPosition = Vector2.zero;
        pannelloRiepilogoRect.sizeDelta = new Vector2(920f, 560f);

        FarmPixelUI.ApplicaPannello(
            pannelloRiepilogoOggetto.GetComponent<Image>(),
            false,
            false
        );
        pannelloRiepilogoOggetto.transform.SetAsFirstSibling();

        Transform titoloTransform = gameOverPanel.transform.Find("Text");
        titoloFinePartita = titoloTransform != null
            ? titoloTransform.GetComponent<TMP_Text>()
            : gameOverPanel.GetComponentInChildren<TMP_Text>(true);

        if (titoloFinePartita != null)
        {
            RectTransform rect = titoloFinePartita.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, 210f);
            rect.sizeDelta = new Vector2(620f, 90f);

            titoloFinePartita.fontSize = 50f;
            titoloFinePartita.fontStyle = FontStyles.Bold;
            titoloFinePartita.alignment = TextAlignmentOptions.Center;
            titoloFinePartita.color = FarmPixelUI.TestoTitoloFlat;
            titoloFinePartita.raycastTarget = false;
            FarmPixelUI.ApplicaTesto(
                titoloFinePartita,
                titoloFinePartita.color
            );
        }

        Transform riepilogoEsistente =
            gameOverPanel.transform.Find("RiepilogoPartita");
        if (riepilogoEsistente == null)
        {
            GameObject oggetto = new GameObject(
                "RiepilogoPartita",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI)
            );
            oggetto.transform.SetParent(gameOverPanel.transform, false);
            riepilogoFinePartita =
                oggetto.GetComponent<TextMeshProUGUI>();
        }
        else
        {
            riepilogoFinePartita =
                riepilogoEsistente.GetComponent<TMP_Text>();
        }

        if (riepilogoFinePartita != null)
        {
            RectTransform rect = riepilogoFinePartita.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(840f, 320f);

            if (titoloFinePartita != null)
            {
                riepilogoFinePartita.font = titoloFinePartita.font;
            }
            riepilogoFinePartita.fontSize = 22f;
            riepilogoFinePartita.fontStyle = FontStyles.Bold;
            riepilogoFinePartita.alignment =
                TextAlignmentOptions.Center;
            riepilogoFinePartita.color =
                FarmPixelUI.TestoChiaroFlat;
            riepilogoFinePartita.textWrappingMode =
                TextWrappingModes.Normal;
            riepilogoFinePartita.raycastTarget = false;
            FarmPixelUI.ApplicaTesto(
                riepilogoFinePartita,
                riepilogoFinePartita.color
            );
        }

        pulsanteRiprova = gameOverPanel.GetComponentInChildren<Button>(true);
        if (pulsanteRiprova != null)
        {
            RectTransform rect = pulsanteRiprova.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, -230f);
            rect.sizeDelta = new Vector2(240f, 58f);

            TMP_Text testoPulsante =
                pulsanteRiprova.GetComponentInChildren<TMP_Text>(true);
            if (testoPulsante != null)
            {
                testoPulsante.text = "RIPROVA";
                testoPulsante.fontSize = 25f;
                testoPulsante.fontStyle = FontStyles.Bold;
                FarmPixelUI.ApplicaTesto(
                    testoPulsante,
                    FarmPixelUI.TestoPulsanteFlat
                );
            }

            FarmPixelUI.ApplicaPulsante(
                pulsanteRiprova,
                FarmPixelUI.ColorePulsanteOroFlat
            );
            pulsanteRiprova.onClick.AddListener(Riprova);
        }
    }

    public void RegistraGallina()
    {
        gallineRimaste++;
        gallineTotali++;

        if (testoUova == null)
        {
            testoUova = TrovaTestoInterfaccia("UovaText");
        }
        if (testoUova != null)
        {
            testoUova.gameObject.SetActive(true);
        }
        if (schedaUovaHud != null)
        {
            schedaUovaHud.SetActive(true);
        }

        AggiornaContatoreUova();
        GallineCambiate?.Invoke(GallineAlSicuro, gallineTotali);
    }

    public void GallinaMorta()
    {
        if (isGameOver) return;

        gallineRimaste = Mathf.Max(0, gallineRimaste - 1);
        SerieSalvataggi = 0;
        UltimoBonusSerie = 0;
        AggiornaContatoreUova();
        AggiornaContatoreUovaSalvate();
        GallineCambiate?.Invoke(GallineAlSicuro, gallineTotali);

        if (gallineRimaste <= 0)
        {
            GameOver();
        }
    }

    void AggiornaContatoreUova()
    {
        if (testoUova != null)
        {
            testoUova.text =
                "Galline   " + GallineAlSicuro + " / " + gallineTotali;
        }
    }

    public void NotificaStatoGallineCambiato()
    {
        AggiornaContatoreUova();
        GallineCambiate?.Invoke(GallineAlSicuro, gallineTotali);
    }

    public void AggiungiUova(int quantita)
    {
        int quantitaValida = Mathf.Max(0, quantita);
        if (quantitaValida == 0) return;

        UovaSalvate += quantitaValida;
        AggiornaContatoreUovaSalvate();
        UovaCambiate?.Invoke(UovaSalvate);
    }

    public int RegistraUovoRecuperato()
    {
        FarmObjectivesBalanceSettings config =
            GameBalanceConfig.Corrente.ObiettiviFattoria;
        SerieSalvataggi++;
        MiglioreSerieSalvataggi = Mathf.Max(
            MiglioreSerieSalvataggi,
            SerieSalvataggi
        );

        int livelloBonus = Mathf.Min(
            config.bonusMassimoSerie,
            SerieSalvataggi / Mathf.Max(1, config.salvataggiPerBonusSerie)
        );
        UltimoBonusSerie =
            livelloBonus * config.uovaBonusSeriePerLivello;
        int premio = Mathf.Max(0, config.uovaPerRecupero) +
                     UltimoBonusSerie;
        AggiungiUova(premio);
        AggiornaContatoreUovaSalvate();
        return premio;
    }

    public void PreparaNuovaOnda()
    {
        uovaInizioOnda = UovaSalvate;
        UovaUltimaOnda = 0;
        UltimoBonusSerie = 0;
        UltimoObiettivo = string.Empty;
        UltimoObiettivoValutato = false;
        UltimoObiettivoCompletato = false;
        UovaUltimoObiettivo = 0;
    }

    public void RegistraEsitoObiettivo(
        string nome,
        bool completato,
        int premioUova
    )
    {
        UltimoObiettivo = nome ?? string.Empty;
        UltimoObiettivoValutato = true;
        UltimoObiettivoCompletato = completato;
        UovaUltimoObiettivo = completato
            ? Mathf.Max(0, premioUova)
            : 0;

        if (completato)
        {
            ObiettiviCompletati++;
            AggiungiUova(UovaUltimoObiettivo);
        }
        else
        {
            ObiettiviFalliti++;
        }
    }

    public void ConcludiRegistroOnda()
    {
        UovaUltimaOnda = Mathf.Max(0, UovaSalvate - uovaInizioOnda);
    }

    void AggiornaContatoreUovaSalvate()
    {
        if (testoUovaSalvate != null)
        {
            testoUovaSalvate.text =
                "Uova   " + UovaSalvate +
                "   Serie x" + SerieSalvataggi;
        }
    }

    void GameOver()
    {
        FarmAudioController.RiproduciPericolo();
        MostraFinePartita("FATTORIA PERDUTA");
    }

    public void GameOverGiocatore()
    {
        if (isGameOver) return;

        Debug.Log("Game over: il contadino e stato sconfitto.");
        FarmAudioController.RiproduciPericolo();
        MostraFinePartita("CONTADINO SCONFITTO");
    }

    void MostraFinePartita(string titolo)
    {
        if (isGameOver) return;

        isGameOver = true;
        if (shopInterOndata != null)
        {
            shopInterOndata.Nascondi();
        }
        if (titoloFinePartita != null)
        {
            titoloFinePartita.text = titolo;
        }
        AggiornaRiepilogoFinale();
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            gameOverPanel.transform.SetAsLastSibling();
        }
        ImpostaStatoPartita(StatoPartita.FinePartita);
    }

    public void Riprova()
    {
        ImpostaPausaManuale(false);
        ImpostaStatoPartita(StatoPartita.Onda);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void AggiungiMonete(int quantita)
    {
        int quantitaValida = Mathf.Max(0, quantita);
        if (quantitaValida == 0) return;

        monete += quantitaValida;
        MoneteRaccolte += quantitaValida;
        AggiornaContatoreMonete();
        MoneteCambiate?.Invoke(monete);
        FarmAudioController.RiproduciMoneta();
    }

    public bool ProvaSpendiMonete(int costo)
    {
        if (costo < 0 || monete < costo) return false;

        monete -= costo;
        MoneteSpese += costo;
        AggiornaContatoreMonete();
        MoneteCambiate?.Invoke(monete);
        return true;
    }

    void AggiornaContatoreMonete()
    {
        if (testoMonete != null)
        {
            testoMonete.text = "Monete   " + monete;
        }
    }

    public void Vittoria()
    {
        if (isGameOver) return;
        FarmAudioController.RiproduciSuccesso();
        MostraFinePartita("FATTORIA SALVA!");
    }

    public int RegistraCompletamentoOnda(int indiceOnda)
    {
        if (isGameOver)
        {
            UltimoBonusCompletamento = 0;
            return 0;
        }

        int bonus = Mathf.Max(
            0,
            GameBalanceConfig.Corrente.Shop.bonusCompletamentoOnda
        );
        UltimoBonusCompletamento = bonus;
        if (bonus > 0)
        {
            AggiungiMonete(bonus);
        }
        return bonus;
    }

    private void AggiornaRiepilogoFinale()
    {
        if (riepilogoFinePartita == null) return;

        PlayerUpgrades potenziamenti =
            FindFirstObjectByType<PlayerUpgrades>();
        string build = potenziamenti != null
            ? potenziamenti.DescriviBuildCompatta()
            : "NESSUNA BUILD";
        string buildLeggibile = build
            .Replace("  |  ", "\n")
            .Replace("  •  ", "\n");
        riepilogoFinePartita.text =
            "MONETE RACCOLTE  " + MoneteRaccolte +
            "  |  SPESE  " + MoneteSpese +
            "\nMONETE RIMASTE  " + monete +
            "  |  GALLINE SALVE  " + Mathf.Max(0, gallineRimaste) +
            " / " + gallineTotali +
            "\nUOVA SALVATE  " + UovaSalvate +
            "  |  SERIE MIGLIORE  x" + MiglioreSerieSalvataggi +
            "\nOBIETTIVI  " + ObiettiviCompletati +
            " COMPLETATI  |  " + ObiettiviFalliti + " FALLITI" +
            "\n------------------------------" +
            "\nBUILD FINALE" +
            "\n" + buildLeggibile;
    }

    public void IniziaIntervallo(int ondaCompletata, int totaleOndate)
    {
        IniziaIntervallo(
            ondaCompletata,
            totaleOndate,
            default
        );
    }

    public void IniziaIntervallo(
        int ondaCompletata,
        int totaleOndate,
        AnteprimaOndata prossimaOnda
    )
    {
        if (isGameOver) return;

        if (shopInterOndata == null)
        {
            shopInterOndata = ShopInterOndata.CreaOTrova();
        }

        if (shopInterOndata == null)
        {
            Debug.LogError(
                "Impossibile creare la schermata tra le ondate."
            );
            ImpostaStatoPartita(StatoPartita.Onda);
            return;
        }

        ImpostaStatoPartita(StatoPartita.Intervallo);
        shopInterOndata.Mostra(
            ondaCompletata,
            totaleOndate,
            prossimaOnda
        );
    }

    public void ContinuaConOndataSuccessiva()
    {
        if (isGameOver ||
            PausaManualeAttiva ||
            StatoCorrente != StatoPartita.Intervallo)
        {
            return;
        }

        EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
        if (spawner != null)
        {
            spawner.RichiediAvvioRapido();
        }

        if (shopInterOndata != null)
        {
            shopInterOndata.Nascondi();
        }
        ImpostaStatoPartita(StatoPartita.Onda);
    }

    public void ImpostaPausaManuale(bool attiva)
    {
        if (PausaManualeAttiva == attiva)
        {
            AggiornaScalaTemporale();
            return;
        }

        PausaManualeAttiva = attiva;
        AggiornaScalaTemporale();
        PausaManualeCambiata?.Invoke(attiva);
    }

    void ImpostaStatoPartita(StatoPartita nuovoStato)
    {
        bool cambiato = StatoCorrente != nuovoStato;
        StatoCorrente = nuovoStato;
        AggiornaScalaTemporale();
        if (cambiato)
        {
            StatoPartitaCambiato?.Invoke(nuovoStato);
        }
    }

    void AggiornaScalaTemporale()
    {
        Time.timeScale =
            StatoCorrente == StatoPartita.Onda &&
            !PausaManualeAttiva
                ? 1f
                : 0f;
    }
}
