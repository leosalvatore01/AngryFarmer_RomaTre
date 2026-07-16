using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class ShopInterOndata : MonoBehaviour
{
    private sealed class CartaOfferta
    {
        public GameObject radice;
        public Image sfondo;
        public Image fasciaPercorso;
        public Outline bordo;
        public TMP_Text testoPercorso;
        public TMP_Text testoTitolo;
        public TMP_Text testoDescrizione;
        public TMP_Text testoConfronto;
        public TMP_Text testoStato;
        public Button pulsante;
        public TMP_Text testoPulsante;
        public Image iconaCosto;
        public TipoPotenziamento tipo;
        public bool valida;
        public bool acquistata;
    }

    private readonly List<CartaOfferta> carte =
        new List<CartaOfferta>();
    private readonly List<TipoPotenziamento> offerteCorrenti =
        new List<TipoPotenziamento>();

    private GameObject pannelloRiepilogo;
    private GameObject pannelloBottega;
    private TMP_Text testoRiepilogo;
    private TMP_Text testoAnteprimaRiepilogo;
    private TMP_Text testoAnteprimaBottega;
    private TMP_Text testoMoneteRiepilogo;
    private TMP_Text testoMoneteBottega;
    private TMP_Text testoBuildRiepilogo;
    private TMP_Text testoBuildBottega;
    private TMP_Text testoMessaggioBottega;
    private TMP_Text testoReroll;
    private TMP_Text testoCura;
    private Button pulsanteReroll;
    private Button pulsanteCura;
    private Image iconaCostoReroll;
    private Image iconaCostoCura;
    private TMP_FontAsset fontInterfaccia;
    private PlayerUpgrades potenziamenti;
    private GeneratoreOfferteBuild generatore;
    private AnteprimaOndata anteprimaCorrente;
    private int ondaCompletataCorrente;
    private int numeroReroll;
    private int acquistiIntervallo;
    private bool costruito;

    public IReadOnlyList<TipoPotenziamento> OfferteCorrenti =>
        offerteCorrenti;
    public int NumeroReroll => numeroReroll;
    public int AcquistiIntervallo => acquistiIntervallo;
    public int CostoRerollCorrente
    {
        get
        {
            ShopBalanceSettings config = GameBalanceConfig.Corrente.Shop;
            return Mathf.Max(
                0,
                config.costoRerollBase +
                numeroReroll * config.incrementoCostoReroll
            );
        }
    }

    public static ShopInterOndata CreaOTrova()
    {
        GameObject interfaccia = GameObject.Find("Interfaccia");
        if (interfaccia == null)
        {
            Debug.LogError(
                "L'oggetto Interfaccia non è presente: shop non creato."
            );
            return null;
        }

        ShopInterOndata esistente =
            interfaccia.GetComponentInChildren<ShopInterOndata>(true);
        if (esistente != null) return esistente;

        GameObject overlay = new GameObject(
            "IntervalloTraOndate",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        overlay.transform.SetParent(interfaccia.transform, false);
        return overlay.AddComponent<ShopInterOndata>();
    }

    void Awake()
    {
        generatore = new GeneratoreOfferteBuild(
            unchecked(Environment.TickCount ^ GetInstanceID() * 48611)
        );
        CostruisciInterfaccia();
    }

    void OnEnable()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.MoneteCambiate += MoneteAggiornate;
        }
        AggiornaInterfaccia();
    }

    void OnDisable()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.MoneteCambiate -= MoneteAggiornate;
        }
    }

    void Update()
    {
        if (GameManager.instance == null ||
            GameManager.instance.StatoCorrente != StatoPartita.Intervallo)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            AvviaOndataSuccessiva();
        }
    }

    public void Mostra(int ondaCompletata, int totaleOndate)
    {
        Mostra(ondaCompletata, totaleOndate, default);
    }

    public void Mostra(
        int ondaCompletata,
        int totaleOndate,
        AnteprimaOndata prossimaOnda
    )
    {
        if (!costruito) CostruisciInterfaccia();

        PreparaPotenziamentiGiocatore();
        ondaCompletataCorrente = Mathf.Max(1, ondaCompletata);
        anteprimaCorrente = prossimaOnda;
        numeroReroll = 0;
        acquistiIntervallo = 0;
        GeneraOfferte(null);

        int bonus = GameManager.instance != null
            ? GameManager.instance.UltimoBonusCompletamento
            : 0;
        testoRiepilogo.text =
            "Ondata " + ondaCompletata + " di " + totaleOndate +
            " superata\n" +
            (bonus > 0
                ? "Difesa completata: +" + bonus + " moneta."
                : "La fattoria ha un momento per riorganizzarsi.");
        string anteprima = FormattaAnteprima(prossimaOnda);
        testoAnteprimaRiepilogo.text = anteprima;
        testoAnteprimaBottega.text = anteprima;
        testoMessaggioBottega.text =
            "Quattro offerte per costruire la tua strategia.";

        pannelloRiepilogo.SetActive(true);
        pannelloBottega.SetActive(false);
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        AggiornaInterfaccia();
    }

    public void Nascondi()
    {
        if (gameObject.activeSelf) gameObject.SetActive(false);
    }

    public void ImpostaSeedOffertePerTest(int seed)
    {
        if (generatore == null)
        {
            generatore = new GeneratoreOfferteBuild(seed);
        }
        else
        {
            generatore.ImpostaSeed(seed);
        }
    }

    public void RigeneraOffertePerTest(int ondaCompletata, int monete)
    {
        PreparaPotenziamentiGiocatore();
        ondaCompletataCorrente = Mathf.Max(1, ondaCompletata);
        GeneraOfferte(null, monete);
        AggiornaInterfaccia();
    }

    void ApriBottega()
    {
        PreparaPotenziamentiGiocatore();
        pannelloRiepilogo.SetActive(false);
        pannelloBottega.SetActive(true);
        testoMessaggioBottega.text =
            "Le offerte acquistate durano per tutta la partita.";
        AggiornaInterfaccia();
    }

    void TornaAlRiepilogo()
    {
        pannelloBottega.SetActive(false);
        pannelloRiepilogo.SetActive(true);
        AggiornaInterfaccia();
    }

    void AvviaOndataSuccessiva()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.ContinuaConOndataSuccessiva();
        }
    }

    void Reroll()
    {
        if (GameManager.instance == null) return;

        int costo = CostoRerollCorrente;
        if (!GameManager.instance.ProvaSpendiMonete(costo))
        {
            int mancanti = Mathf.Max(0, costo - GameManager.instance.monete);
            testoMessaggioBottega.text =
                "Servono ancora " + mancanti +
                " monete per cambiare le offerte.";
            return;
        }

        List<TipoPotenziamento> precedenti =
            new List<TipoPotenziamento>(offerteCorrenti);
        numeroReroll++;
        GeneraOfferte(precedenti);
        testoMessaggioBottega.text =
            "Nuove offerte arrivate. Il prossimo reroll costerà " +
            CostoRerollCorrente + " monete.";
        AggiornaInterfaccia();
    }

    void AcquistaCarta(int indice)
    {
        if (indice < 0 || indice >= carte.Count) return;
        CartaOfferta carta = carte[indice];
        if (!carta.valida || carta.acquistata || potenziamenti == null)
        {
            return;
        }

        string messaggio;
        bool acquistato = potenziamenti.ProvaAcquistare(
            carta.tipo,
            out messaggio
        );
        if (acquistato)
        {
            carta.acquistata = true;
            acquistiIntervallo++;
            testoMessaggioBottega.text =
                messaggio + "  " +
                potenziamenti.OttieniTitolo(carta.tipo);
        }
        else
        {
            testoMessaggioBottega.text = messaggio;
        }
        AggiornaInterfaccia();
    }

    void AcquistaCura()
    {
        if (potenziamenti == null) return;
        string messaggio;
        bool acquistato = potenziamenti.ProvaAcquistare(
            TipoPotenziamento.Cura,
            out messaggio
        );
        testoMessaggioBottega.text = acquistato
            ? messaggio + "  Rimedio della nonna"
            : messaggio;
        AggiornaInterfaccia();
    }

    void GeneraOfferte(
        ICollection<TipoPotenziamento> precedenti,
        int? moneteForzate = null
    )
    {
        offerteCorrenti.Clear();
        if (potenziamenti == null || generatore == null) return;

        ShopBalanceSettings config = GameBalanceConfig.Corrente.Shop;
        int monete = moneteForzate ??
            (GameManager.instance != null ? GameManager.instance.monete : 0);
        List<TipoPotenziamento> nuove = generatore.Genera(
            potenziamenti,
            ondaCompletataCorrente,
            monete,
            Mathf.Clamp(config.numeroOfferte, 3, 4),
            precedenti
        );
        offerteCorrenti.AddRange(nuove);

        for (int i = 0; i < carte.Count; i++)
        {
            CartaOfferta carta = carte[i];
            carta.acquistata = false;
            carta.valida = i < offerteCorrenti.Count;
            carta.radice.SetActive(carta.valida);
            if (carta.valida) carta.tipo = offerteCorrenti[i];
        }
    }

    static string FormattaAnteprima(AnteprimaOndata anteprima)
    {
        if (!anteprima.Valida)
        {
            return "PROSSIMA ONDATA PRONTA";
        }

        string bonus;
        if (anteprima.NumeroMaialini > 0)
        {
            int premioMassimo =
                anteprima.NumeroMaialini * anteprima.MoneteMaialino;
            bonus =
                anteprima.NumeroMaialini +
                (anteprima.NumeroMaialini == 1
                    ? " MAIALINO"
                    : " MAIALINI") +
                "  (FINO A +" + premioMassimo + ")";
        }
        else
        {
            bonus = "NESSUN MAIALINO BONUS";
        }
        string gruppi = anteprima.NumeroGruppi == 1
            ? "1 GRUPPO"
            : anteprima.NumeroGruppi + " GRUPPI";

        return
            "PROSSIMA: ONDATA " + anteprima.Indice + " / " + anteprima.Totale +
            "  -  " + anteprima.Nome.ToUpperInvariant() + "  -  " +
            anteprima.NumeroVolpi + " VOLPI  -  " + gruppi +
            "  -  " + bonus +
            "\nTIPI: " + anteprima.Composizione.FormattaCompatta();
    }

    void PreparaPotenziamentiGiocatore()
    {
        if (potenziamenti != null) return;

        GameObject giocatore = GameObject.FindGameObjectWithTag("Player");
        if (giocatore == null) return;

        potenziamenti = giocatore.GetComponent<PlayerUpgrades>();
        if (potenziamenti == null)
        {
            potenziamenti = giocatore.AddComponent<PlayerUpgrades>();
        }
    }

    void MoneteAggiornate(int nuovaQuantita)
    {
        AggiornaInterfaccia();
    }

    void AggiornaInterfaccia()
    {
        int monete = GameManager.instance != null
            ? GameManager.instance.monete
            : 0;
        string testoMonete = "MONETE  " + monete;

        if (testoMoneteRiepilogo != null)
        {
            testoMoneteRiepilogo.text = testoMonete;
        }
        if (testoMoneteBottega != null)
        {
            testoMoneteBottega.text = testoMonete;
        }

        string build = potenziamenti != null
            ? "BUILD:  " + potenziamenti.DescriviBuildCompatta()
            : "BUILD:  NON DISPONIBILE";
        if (testoBuildRiepilogo != null) testoBuildRiepilogo.text = build;
        if (testoBuildBottega != null) testoBuildBottega.text = build;

        for (int i = 0; i < carte.Count; i++)
        {
            AggiornaCarta(carte[i], monete);
        }

        int costoReroll = CostoRerollCorrente;
        if (testoReroll != null)
        {
            testoReroll.text = EtichettaMonete(costoReroll);
            testoReroll.color = monete >= costoReroll
                ? new Color(1f, 0.94f, 0.77f, 1f)
                : new Color(1f, 0.52f, 0.42f, 1f);
        }
        if (pulsanteReroll != null) pulsanteReroll.interactable = true;
        if (iconaCostoReroll != null) iconaCostoReroll.enabled = true;

        if (potenziamenti != null && testoCura != null)
        {
            bool disponibile =
                potenziamenti.PuoAcquistare(TipoPotenziamento.Cura);
            int costo = potenziamenti.OttieniCosto(TipoPotenziamento.Cura);
            testoCura.text = disponibile
                ? EtichettaMonete(costo)
                : "SALUTE PIENA";
            testoCura.color = disponibile && monete < costo
                ? new Color(1f, 0.52f, 0.42f, 1f)
                : new Color(1f, 0.94f, 0.77f, 1f);
            pulsanteCura.interactable = disponibile;
            if (iconaCostoCura != null) iconaCostoCura.enabled = disponibile;
        }
    }

    void AggiornaCarta(CartaOfferta carta, int monete)
    {
        if (carta == null || !carta.valida)
        {
            if (carta != null) carta.radice.SetActive(false);
            return;
        }
        carta.radice.SetActive(true);

        DefinizionePotenziamentoBuild definizione =
            CatalogoPotenziamentiBuild.Ottieni(carta.tipo);
        if (definizione == null || potenziamenti == null)
        {
            carta.pulsante.interactable = false;
            return;
        }

        Color colorePercorso =
            CatalogoPotenziamentiBuild.ColorePercorso(
                definizione.Percorso
            );
        Color coloreRarita =
            CatalogoPotenziamentiBuild.ColoreRarita(definizione.Rarita);
        carta.fasciaPercorso.color = colorePercorso;
        carta.bordo.effectColor = new Color(
            coloreRarita.r,
            coloreRarita.g,
            coloreRarita.b,
            0.86f
        );
        carta.testoPercorso.text =
            CatalogoPotenziamentiBuild.NomePercorso(
                definizione.Percorso
            ) +
            "  •  " +
            CatalogoPotenziamentiBuild.NomeCategoria(
                definizione.Categoria
            ) +
            "  •  " +
            CatalogoPotenziamentiBuild.NomeRarita(definizione.Rarita);
        carta.testoPercorso.color = coloreRarita;
        carta.testoTitolo.text = potenziamenti.OttieniTitolo(carta.tipo);
        carta.testoDescrizione.text =
            potenziamenti.OttieniDescrizione(carta.tipo);
        carta.testoConfronto.text =
            potenziamenti.OttieniConfronto(carta.tipo);
        carta.testoStato.text = potenziamenti.OttieniStato(carta.tipo);

        bool disponibile = potenziamenti.PuoAcquistare(carta.tipo);
        int costo = potenziamenti.OttieniCosto(carta.tipo);
        if (carta.acquistata)
        {
            carta.testoPulsante.text = "ACQUISTATO";
            carta.testoPulsante.color =
                new Color(1f, 0.94f, 0.77f, 1f);
            carta.pulsante.interactable = false;
            carta.iconaCosto.enabled = false;
            carta.sfondo.color = Color.Lerp(
                Color.white,
                colorePercorso,
                0.28f
            );
        }
        else if (!disponibile)
        {
            carta.testoPulsante.text = "MAX";
            carta.testoPulsante.color =
                new Color(1f, 0.94f, 0.77f, 1f);
            carta.pulsante.interactable = false;
            carta.iconaCosto.enabled = false;
            carta.sfondo.color = Color.Lerp(
                Color.white,
                colorePercorso,
                0.28f
            );
        }
        else
        {
            carta.testoPulsante.text = EtichettaMonete(costo);
            carta.testoPulsante.color = monete >= costo
                ? new Color(1f, 0.94f, 0.77f, 1f)
                : new Color(1f, 0.52f, 0.42f, 1f);
            carta.pulsante.interactable = true;
            carta.iconaCosto.enabled = true;
            carta.sfondo.color = Color.Lerp(
                Color.white,
                colorePercorso,
                0.13f
            );
        }
    }

    static string EtichettaMonete(int quantita)
    {
        return quantita + (quantita == 1 ? " MONETA" : " MONETE");
    }

    void CostruisciInterfaccia()
    {
        if (costruito) return;
        costruito = true;

        TMP_Text testoHUD = GameManager.TrovaTestoInterfaccia("OndataText");
        if (testoHUD != null) fontInterfaccia = testoHUD.font;

        RectTransform rootRect = GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        Image velo = GetComponent<Image>();
        velo.color = new Color(0.055f, 0.035f, 0.02f, 0.84f);
        velo.raycastTarget = true;

        pannelloRiepilogo = CreaPannello(
            "RiepilogoOndata",
            transform,
            new Vector2(860f, 560f),
            new Color(0.13f, 0.072f, 0.035f, 0.98f)
        );
        CostruisciRiepilogo(pannelloRiepilogo.transform);

        pannelloBottega = CreaPannello(
            "BottegaBuild",
            transform,
            new Vector2(1220f, 920f),
            new Color(0.105f, 0.057f, 0.027f, 0.99f)
        );
        CostruisciBottega(pannelloBottega.transform);

        pannelloRiepilogo.SetActive(true);
        pannelloBottega.SetActive(false);
        gameObject.SetActive(false);
    }

    void CostruisciRiepilogo(Transform parent)
    {
        FarmPixelUI.AggiungiIcona(
            parent,
            "IconaBottega",
            FarmPixelIcon.Bottega,
            new Vector2(-322f, 204f),
            new Vector2(58f, 58f)
        );

        CreaTesto(
            "Titolo",
            parent,
            "ONDATA COMPLETATA",
            new Vector2(0f, 204f),
            new Vector2(760f, 64f),
            42f,
            new Color(1f, 0.76f, 0.25f, 1f),
            FontStyles.Bold,
            TextAlignmentOptions.Center
        );

        testoRiepilogo = CreaTesto(
            "Riepilogo",
            parent,
            string.Empty,
            new Vector2(0f, 128f),
            new Vector2(740f, 76f),
            24f,
            new Color(1f, 0.91f, 0.71f, 1f),
            FontStyles.Normal,
            TextAlignmentOptions.Center
        );

        testoAnteprimaRiepilogo = CreaTesto(
            "AnteprimaProssimaOnda",
            parent,
            string.Empty,
            new Vector2(0f, 50f),
            new Vector2(760f, 72f),
            17f,
            new Color(1f, 0.76f, 0.32f, 1f),
            FontStyles.Bold,
            TextAlignmentOptions.Center
        );
        testoAnteprimaRiepilogo.overflowMode = TextOverflowModes.Overflow;

        testoMoneteRiepilogo = CreaTesto(
            "Monete",
            parent,
            "MONETE  0",
            new Vector2(0f, -28f),
            new Vector2(400f, 46f),
            28f,
            new Color(1f, 0.86f, 0.22f, 1f),
            FontStyles.Bold,
            TextAlignmentOptions.Center
        );
        FarmPixelUI.AggiungiIcona(
            parent,
            "IconaMonete",
            FarmPixelIcon.Moneta,
            new Vector2(-125f, -28f),
            new Vector2(34f, 34f)
        );

        testoBuildRiepilogo = CreaTesto(
            "BuildCorrente",
            parent,
            "BUILD: NESSUNA BUILD",
            new Vector2(0f, -76f),
            new Vector2(760f, 34f),
            17f,
            new Color(0.71f, 0.9f, 0.68f, 1f),
            FontStyles.Bold,
            TextAlignmentOptions.Center
        );

        CreaTesto(
            "Suggerimento",
            parent,
            "Le offerte cambiano a ogni ondata. Spazio: riparti subito.",
            new Vector2(0f, -122f),
            new Vector2(730f, 44f),
            18f,
            new Color(0.86f, 0.78f, 0.65f, 1f),
            FontStyles.Normal,
            TextAlignmentOptions.Center
        );

        CreaPulsante(
            "ApriBottega",
            parent,
            "SCEGLI LA BUILD",
            new Vector2(-195f, -204f),
            new Vector2(350f, 64f),
            new Color(0.7f, 0.35f, 0.08f, 1f),
            ApriBottega
        );
        CreaPulsante(
            "OndataSuccessiva",
            parent,
            "PARTI SUBITO  [SPAZIO]",
            new Vector2(195f, -204f),
            new Vector2(350f, 64f),
            new Color(0.24f, 0.55f, 0.2f, 1f),
            AvviaOndataSuccessiva
        );
    }

    void CostruisciBottega(Transform parent)
    {
        FarmPixelUI.AggiungiIcona(
            parent,
            "IconaBottega",
            FarmPixelIcon.Bottega,
            new Vector2(-425f, 412f),
            new Vector2(52f, 52f)
        );

        CreaTesto(
            "Titolo",
            parent,
            "BOTTEGA DELLE BUILD",
            new Vector2(0f, 412f),
            new Vector2(760f, 54f),
            37f,
            new Color(1f, 0.76f, 0.25f, 1f),
            FontStyles.Bold,
            TextAlignmentOptions.Center
        );

        testoMoneteBottega = CreaTesto(
            "Monete",
            parent,
            "MONETE  0",
            new Vector2(448f, 412f),
            new Vector2(250f, 48f),
            25f,
            new Color(1f, 0.86f, 0.22f, 1f),
            FontStyles.Bold,
            TextAlignmentOptions.Center
        );
        FarmPixelUI.AggiungiIcona(
            parent,
            "IconaMonete",
            FarmPixelIcon.Moneta,
            new Vector2(330f, 412f),
            new Vector2(34f, 34f)
        );

        testoBuildBottega = CreaTesto(
            "BuildCorrente",
            parent,
            "BUILD: NESSUNA BUILD",
            new Vector2(0f, 365f),
            new Vector2(1040f, 32f),
            17f,
            new Color(0.72f, 0.92f, 0.68f, 1f),
            FontStyles.Bold,
            TextAlignmentOptions.Center
        );

        float[] posizioniY = { 286f, 158f, 30f, -98f };
        for (int i = 0; i < posizioniY.Length; i++)
        {
            CreaCartaOfferta(parent, i, posizioniY[i]);
        }

        pulsanteCura = CreaPulsante(
            "Cura",
            parent,
            "2 MONETE",
            new Vector2(-300f, -224f),
            new Vector2(280f, 54f),
            new Color(0.32f, 0.55f, 0.24f, 1f),
            AcquistaCura
        );
        testoCura = pulsanteCura.GetComponentInChildren<TMP_Text>();
        iconaCostoCura = FarmPixelUI.AggiungiIcona(
            pulsanteCura.transform,
            "IconaCosto",
            FarmPixelIcon.Cura,
            new Vector2(-103f, 0f),
            new Vector2(27f, 27f)
        );

        pulsanteReroll = CreaPulsante(
            "Reroll",
            parent,
            "1 MONETA",
            new Vector2(0f, -224f),
            new Vector2(280f, 54f),
            new Color(0.35f, 0.32f, 0.58f, 1f),
            Reroll
        );
        testoReroll = pulsanteReroll.GetComponentInChildren<TMP_Text>();
        iconaCostoReroll = FarmPixelUI.AggiungiIcona(
            pulsanteReroll.transform,
            "IconaCosto",
            FarmPixelIcon.Moneta,
            new Vector2(-103f, 0f),
            new Vector2(27f, 27f)
        );

        CreaTesto(
            "EtichettaServizi",
            parent,
            "CURA RAPIDA                         CAMBIA OFFERTE",
            new Vector2(-150f, -264f),
            new Vector2(610f, 28f),
            14f,
            new Color(0.84f, 0.75f, 0.62f, 1f),
            FontStyles.Bold,
            TextAlignmentOptions.Center
        );

        testoMessaggioBottega = CreaTesto(
            "Messaggio",
            parent,
            string.Empty,
            new Vector2(0f, -302f),
            new Vector2(1020f, 36f),
            17f,
            new Color(1f, 0.84f, 0.51f, 1f),
            FontStyles.Italic,
            TextAlignmentOptions.Center
        );

        testoAnteprimaBottega = CreaTesto(
            "AnteprimaProssimaOnda",
            parent,
            string.Empty,
            new Vector2(0f, -350f),
            new Vector2(1060f, 52f),
            15f,
            new Color(1f, 0.74f, 0.28f, 1f),
            FontStyles.Bold,
            TextAlignmentOptions.Center
        );
        testoAnteprimaBottega.overflowMode = TextOverflowModes.Overflow;

        CreaPulsante(
            "Indietro",
            parent,
            "INDIETRO",
            new Vector2(-205f, -415f),
            new Vector2(330f, 56f),
            new Color(0.39f, 0.24f, 0.13f, 1f),
            TornaAlRiepilogo
        );
        CreaPulsante(
            "OndataSuccessiva",
            parent,
            "PARTI SUBITO  [SPAZIO]",
            new Vector2(205f, -415f),
            new Vector2(330f, 56f),
            new Color(0.24f, 0.55f, 0.2f, 1f),
            AvviaOndataSuccessiva
        );
    }

    void CreaCartaOfferta(Transform parent, int indice, float posizioneY)
    {
        GameObject radice = CreaPannello(
            "Offerta_" + (indice + 1),
            parent,
            new Vector2(1080f, 112f),
            new Color(0.22f, 0.125f, 0.06f, 0.96f),
            new Vector2(0f, posizioneY),
            false
        );
        Image sfondo = radice.GetComponent<Image>();
        Outline bordo = radice.GetComponent<Outline>();
        bordo.enabled = true;
        bordo.effectDistance = new Vector2(2f, -2f);

        GameObject fascia = new GameObject(
            "FasciaPercorso",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        fascia.transform.SetParent(radice.transform, false);
        RectTransform fasciaRect = fascia.GetComponent<RectTransform>();
        fasciaRect.anchorMin = new Vector2(0f, 0f);
        fasciaRect.anchorMax = new Vector2(0f, 1f);
        fasciaRect.pivot = new Vector2(0f, 0.5f);
        fasciaRect.anchoredPosition = Vector2.zero;
        fasciaRect.sizeDelta = new Vector2(12f, 0f);
        Image fasciaImmagine = fascia.GetComponent<Image>();
        fasciaImmagine.color = Color.white;
        fasciaImmagine.raycastTarget = false;

        TMP_Text percorso = CreaTesto(
            "PercorsoRarita",
            radice.transform,
            string.Empty,
            new Vector2(-293f, 36f),
            new Vector2(460f, 25f),
            13f,
            Color.white,
            FontStyles.Bold,
            TextAlignmentOptions.MidlineLeft
        );
        TMP_Text titolo = CreaTesto(
            "Titolo",
            radice.transform,
            string.Empty,
            new Vector2(-330f, 8f),
            new Vector2(385f, 30f),
            21f,
            new Color(1f, 0.82f, 0.38f, 1f),
            FontStyles.Bold,
            TextAlignmentOptions.MidlineLeft
        );
        TMP_Text descrizione = CreaTesto(
            "Descrizione",
            radice.transform,
            string.Empty,
            new Vector2(-275f, -27f),
            new Vector2(500f, 28f),
            15f,
            new Color(0.91f, 0.84f, 0.72f, 1f),
            FontStyles.Normal,
            TextAlignmentOptions.MidlineLeft
        );
        TMP_Text confronto = CreaTesto(
            "Confronto",
            radice.transform,
            string.Empty,
            new Vector2(145f, 19f),
            new Vector2(370f, 30f),
            15f,
            new Color(0.72f, 0.94f, 0.69f, 1f),
            FontStyles.Bold,
            TextAlignmentOptions.Center
        );
        confronto.enableAutoSizing = true;
        confronto.fontSizeMin = 12f;
        confronto.fontSizeMax = 15f;
        TMP_Text stato = CreaTesto(
            "Stato",
            radice.transform,
            string.Empty,
            new Vector2(145f, -22f),
            new Vector2(260f, 28f),
            14f,
            new Color(0.78f, 0.72f, 0.62f, 1f),
            FontStyles.Bold,
            TextAlignmentOptions.Center
        );

        int indiceCatturato = indice;
        Button pulsante = CreaPulsante(
            "Acquista",
            radice.transform,
            "--",
            new Vector2(430f, 0f),
            new Vector2(190f, 54f),
            new Color(0.68f, 0.33f, 0.075f, 1f),
            () => AcquistaCarta(indiceCatturato)
        );
        Image iconaCosto = FarmPixelUI.AggiungiIcona(
            pulsante.transform,
            "IconaCosto",
            FarmPixelIcon.Moneta,
            new Vector2(-67f, 0f),
            new Vector2(25f, 25f)
        );
        TMP_Text testoCosto = pulsante.GetComponentInChildren<TMP_Text>();
        if (testoCosto != null)
        {
            testoCosto.rectTransform.anchoredPosition = new Vector2(15f, 0f);
            testoCosto.rectTransform.sizeDelta = new Vector2(142f, 40f);
        }

        carte.Add(new CartaOfferta
        {
            radice = radice,
            sfondo = sfondo,
            fasciaPercorso = fasciaImmagine,
            bordo = bordo,
            testoPercorso = percorso,
            testoTitolo = titolo,
            testoDescrizione = descrizione,
            testoConfronto = confronto,
            testoStato = stato,
            pulsante = pulsante,
            testoPulsante = testoCosto,
            iconaCosto = iconaCosto
        });
    }

    GameObject CreaPannello(
        string nome,
        Transform parent,
        Vector2 dimensioni,
        Color colore,
        Vector2? posizione = null,
        bool bordoMarcato = true
    )
    {
        GameObject oggetto = new GameObject(
            nome,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Outline)
        );
        oggetto.transform.SetParent(parent, false);

        RectTransform rect = oggetto.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posizione ?? Vector2.zero;
        rect.sizeDelta = dimensioni;

        Image immagine = oggetto.GetComponent<Image>();
        FarmPixelUI.ApplicaPannello(immagine, !bordoMarcato, true);
        immagine.color = Color.Lerp(
            Color.white,
            colore,
            bordoMarcato ? 0.1f : 0.16f
        );

        Outline bordo = oggetto.GetComponent<Outline>();
        bordo.effectColor = new Color(0.12f, 0.055f, 0.025f, 0.72f);
        bordo.effectDistance = bordoMarcato
            ? new Vector2(3f, -3f)
            : new Vector2(2f, -2f);
        bordo.useGraphicAlpha = true;
        return oggetto;
    }

    TMP_Text CreaTesto(
        string nome,
        Transform parent,
        string contenuto,
        Vector2 posizione,
        Vector2 dimensioni,
        float dimensioneFont,
        Color colore,
        FontStyles stile,
        TextAlignmentOptions allineamento
    )
    {
        GameObject oggetto = new GameObject(
            nome,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );
        oggetto.transform.SetParent(parent, false);

        RectTransform rect = oggetto.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posizione;
        rect.sizeDelta = dimensioni;

        TextMeshProUGUI testo = oggetto.GetComponent<TextMeshProUGUI>();
        if (fontInterfaccia != null) testo.font = fontInterfaccia;
        testo.text = contenuto;
        testo.fontSize = dimensioneFont;
        testo.fontStyle = stile;
        testo.alignment = allineamento;
        testo.color = colore;
        testo.textWrappingMode = TextWrappingModes.Normal;
        testo.overflowMode = TextOverflowModes.Ellipsis;
        testo.raycastTarget = false;
        FarmPixelUI.ApplicaTesto(testo, colore);
        return testo;
    }

    Button CreaPulsante(
        string nome,
        Transform parent,
        string etichetta,
        Vector2 posizione,
        Vector2 dimensioni,
        Color colore,
        UnityEngine.Events.UnityAction azione
    )
    {
        GameObject oggetto = new GameObject(
            nome,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button),
            typeof(Outline)
        );
        oggetto.transform.SetParent(parent, false);

        RectTransform rect = oggetto.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posizione;
        rect.sizeDelta = dimensioni;

        Image immagine = oggetto.GetComponent<Image>();
        immagine.color = colore;

        Outline bordo = oggetto.GetComponent<Outline>();
        bordo.enabled = false;

        Button pulsante = oggetto.GetComponent<Button>();
        pulsante.targetGraphic = immagine;
        FarmPixelUI.ApplicaPulsante(pulsante, colore);
        pulsante.onClick.AddListener(azione);

        TMP_Text testo = CreaTesto(
            "Testo",
            oggetto.transform,
            etichetta,
            Vector2.zero,
            dimensioni - new Vector2(16f, 10f),
            18f,
            new Color(1f, 0.94f, 0.77f, 1f),
            FontStyles.Bold,
            TextAlignmentOptions.Center
        );
        testo.textWrappingMode = TextWrappingModes.NoWrap;
        return pulsante;
    }
}
