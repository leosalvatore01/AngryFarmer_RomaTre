using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Barra compatta che riunisce potenziamenti permanenti e drop temporanei.
/// Viene costruita a runtime per non richiedere configurazione nella scena.
/// </summary>
[DisallowMultipleComponent]
public sealed class PowerUpHudController : MonoBehaviour
{
    private const int ColonneMassime = 8;
    private static readonly Vector2 DimensioneSlot = new Vector2(48f, 54f);
    private static readonly Vector2 SpaziaturaSlot = new Vector2(4f, 4f);

    private sealed class SlotHud
    {
        public GameObject radice;
        public TMP_Text testoLivello;
        public RectTransform barraTempo;
        public float larghezzaBarra;
    }

    private readonly Dictionary<TipoPotenziamento, SlotHud> slotPermanenti =
        new Dictionary<TipoPotenziamento, SlotHud>();

    private RectTransform rectRadice;
    private RectTransform rectGriglia;
    private GridLayoutGroup griglia;
    private CanvasGroup gruppo;
    private TMP_Text intestazione;
    private SlotHud slotTriploSparo;
    private SlotHud slotBoostVelocita;
    private PlayerUpgrades potenziamenti;
    private PlayerShooting sparo;
    private PlayerMovement movimento;
    private float prossimoAggiornamento;
    private int numeroSlotVisibili;

    public int NumeroPotenziamentiPermanentiAttivi { get; private set; }
    public int NumeroEffettiTemporaneiAttivi { get; private set; }
    public int NumeroSlotVisibili => numeroSlotVisibili;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InizializzaDopoCaricamentoScena()
    {
        CreaOTrova();
    }

    public static PowerUpHudController CreaOTrova()
    {
        GameObject interfaccia = GameObject.Find("Interfaccia");
        if (interfaccia == null || interfaccia.GetComponent<Canvas>() == null)
            return null;

        PowerUpHudController esistente =
            interfaccia.GetComponentInChildren<PowerUpHudController>(true);
        if (esistente != null) return esistente;

        GameObject radice = new GameObject(
            "PotenziamentiAttiviHUD",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(CanvasGroup)
        );
        radice.transform.SetParent(interfaccia.transform, false);
        radice.transform.SetAsLastSibling();
        return radice.AddComponent<PowerUpHudController>();
    }

    void Awake()
    {
        CostruisciInterfaccia();
        CollegaGiocatore();
        AggiornaStatoCompleto();
    }

    void Update()
    {
        if (Time.unscaledTime >= prossimoAggiornamento)
        {
            prossimoAggiornamento = Time.unscaledTime + 0.1f;
            if (potenziamenti == null || sparo == null || movimento == null)
                CollegaGiocatore();
            AggiornaStatoCompleto();
        }
        else
        {
            AggiornaTimerTemporanei();
        }

        bool partitaVisibile = GameManager.instance == null ||
            GameManager.instance.GameplayAttivo;
        gruppo.alpha = partitaVisibile && numeroSlotVisibili > 0 ? 1f : 0f;
    }

    public void AggiornaSubitoPerTest()
    {
        CollegaGiocatore();
        AggiornaStatoCompleto();
    }

    private void CostruisciInterfaccia()
    {
        rectRadice = GetComponent<RectTransform>();
        rectRadice.anchorMin = new Vector2(0.5f, 1f);
        rectRadice.anchorMax = new Vector2(0.5f, 1f);
        rectRadice.pivot = new Vector2(0.5f, 1f);
        rectRadice.anchoredPosition = new Vector2(0f, -144f);

        Image sfondo = GetComponent<Image>();
        FarmPixelUI.ApplicaPannello(sfondo, true, false);
        sfondo.color = new Color32(47, 31, 22, 132);
        Outline bordo = sfondo.GetComponent<Outline>();
        if (bordo != null)
        {
            bordo.effectColor = new Color32(73, 43, 26, 145);
            bordo.effectDistance = new Vector2(1f, 1f);
        }

        gruppo = GetComponent<CanvasGroup>();
        gruppo.interactable = false;
        gruppo.blocksRaycasts = false;
        gruppo.alpha = 0f;

        intestazione = CreaTesto(
            transform,
            "Intestazione",
            "POWER-UP ATTIVI",
            11f,
            new Color32(248, 221, 166, 235),
            TextAlignmentOptions.MidlineLeft
        );
        RectTransform rectIntestazione = intestazione.rectTransform;
        rectIntestazione.anchorMin = new Vector2(0f, 1f);
        rectIntestazione.anchorMax = new Vector2(1f, 1f);
        rectIntestazione.pivot = new Vector2(0.5f, 1f);
        rectIntestazione.anchoredPosition = new Vector2(0f, -4f);
        rectIntestazione.sizeDelta = new Vector2(-14f, 17f);

        GameObject oggettoGriglia = new GameObject(
            "GrigliaPowerUp",
            typeof(RectTransform),
            typeof(GridLayoutGroup)
        );
        oggettoGriglia.transform.SetParent(transform, false);
        rectGriglia = oggettoGriglia.GetComponent<RectTransform>();
        rectGriglia.anchorMin = new Vector2(0f, 1f);
        rectGriglia.anchorMax = new Vector2(0f, 1f);
        rectGriglia.pivot = new Vector2(0f, 1f);
        rectGriglia.anchoredPosition = new Vector2(7f, -23f);

        griglia = oggettoGriglia.GetComponent<GridLayoutGroup>();
        griglia.cellSize = DimensioneSlot;
        griglia.spacing = SpaziaturaSlot;
        griglia.startCorner = GridLayoutGroup.Corner.UpperLeft;
        griglia.startAxis = GridLayoutGroup.Axis.Horizontal;
        griglia.childAlignment = TextAnchor.UpperLeft;
        griglia.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        griglia.constraintCount = ColonneMassime;

        IReadOnlyList<DefinizionePotenziamentoBuild> definizioni =
            CatalogoPotenziamentiBuild.Tutte;
        for (int i = 0; i < definizioni.Count; i++)
        {
            TipoPotenziamento tipo = definizioni[i].Tipo;
            SlotHud slot = CreaSlot(
                tipo.ToString(),
                PowerUpIconCatalog.OttieniSprite(tipo),
                PowerUpIconCatalog.OttieniEtichettaCompatta(tipo),
                PowerUpIconCatalog.OttieniColore(tipo),
                false
            );
            slotPermanenti[tipo] = slot;
        }

        slotTriploSparo = CreaSlot(
            "TriploSparo",
            FarmPixelUI.OttieniIcona(FarmPixelIcon.TriploSparo),
            "TRIPLO",
            new Color32(181, 115, 217, 255),
            true
        );
        slotBoostVelocita = CreaSlot(
            "BoostVelocita",
            FarmPixelUI.OttieniIcona(FarmPixelIcon.BoostVelocita),
            "SCATTO",
            new Color32(244, 196, 61, 255),
            true
        );
    }

    private SlotHud CreaSlot(
        string nome,
        Sprite sprite,
        string etichetta,
        Color colore,
        bool temporaneo
    )
    {
        GameObject radice = new GameObject(
            "PowerUp_" + nome,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Outline)
        );
        radice.transform.SetParent(rectGriglia, false);
        Image sfondo = radice.GetComponent<Image>();
        sfondo.color = new Color32(28, 22, 18, 188);
        sfondo.raycastTarget = false;
        Outline bordo = radice.GetComponent<Outline>();
        bordo.effectColor = new Color(colore.r, colore.g, colore.b, 0.7f);
        bordo.effectDistance = new Vector2(1f, 1f);
        bordo.useGraphicAlpha = true;

        GameObject accento = new GameObject(
            "Accento",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        accento.transform.SetParent(radice.transform, false);
        RectTransform rectAccento = accento.GetComponent<RectTransform>();
        rectAccento.anchorMin = new Vector2(0f, 1f);
        rectAccento.anchorMax = new Vector2(1f, 1f);
        rectAccento.pivot = new Vector2(0.5f, 1f);
        rectAccento.anchoredPosition = Vector2.zero;
        rectAccento.sizeDelta = new Vector2(0f, 3f);
        Image immagineAccento = accento.GetComponent<Image>();
        immagineAccento.color = colore;
        immagineAccento.raycastTarget = false;

        GameObject oggettoIcona = new GameObject(
            "Icona",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        oggettoIcona.transform.SetParent(radice.transform, false);
        RectTransform rectIcona = oggettoIcona.GetComponent<RectTransform>();
        ImpostaRectCentrato(rectIcona, new Vector2(0f, 7f), new Vector2(30f, 30f));
        Image icona = oggettoIcona.GetComponent<Image>();
        icona.sprite = sprite;
        icona.preserveAspect = true;
        icona.raycastTarget = false;

        TMP_Text testoEtichetta = CreaTesto(
            radice.transform,
            "Etichetta",
            etichetta,
            9f,
            new Color32(243, 225, 190, 240),
            TextAlignmentOptions.Center
        );
        ImpostaRectCentrato(
            testoEtichetta.rectTransform,
            new Vector2(0f, -19f),
            new Vector2(44f, 12f)
        );
        testoEtichetta.enableAutoSizing = true;
        testoEtichetta.fontSizeMin = 6f;
        testoEtichetta.fontSizeMax = 9f;

        TMP_Text testoLivello = CreaTesto(
            radice.transform,
            "Livello",
            temporaneo ? "0,0s" : "L1",
            10f,
            temporaneo
                ? new Color32(255, 227, 111, 255)
                : new Color32(255, 244, 213, 255),
            TextAlignmentOptions.Center
        );
        RectTransform rectLivello = testoLivello.rectTransform;
        rectLivello.anchorMin = new Vector2(1f, 1f);
        rectLivello.anchorMax = new Vector2(1f, 1f);
        rectLivello.pivot = new Vector2(1f, 1f);
        rectLivello.anchoredPosition = new Vector2(-2f, -3f);
        rectLivello.sizeDelta = new Vector2(30f, 14f);
        testoLivello.enableAutoSizing = true;
        testoLivello.fontSizeMin = 6f;
        testoLivello.fontSizeMax = 10f;

        RectTransform barraTempo = null;
        const float larghezzaBarra = 42f;
        if (temporaneo)
        {
            GameObject fondoBarra = CreaBarra(
                radice.transform,
                "FondoTimer",
                new Color32(66, 48, 35, 220)
            );
            ImpostaRectCentrato(
                fondoBarra.GetComponent<RectTransform>(),
                new Vector2(0f, -25f),
                new Vector2(larghezzaBarra, 3f)
            );
            GameObject riempimento = CreaBarra(
                fondoBarra.transform,
                "Timer",
                colore
            );
            barraTempo = riempimento.GetComponent<RectTransform>();
            barraTempo.anchorMin = new Vector2(0f, 0.5f);
            barraTempo.anchorMax = new Vector2(0f, 0.5f);
            barraTempo.pivot = new Vector2(0f, 0.5f);
            barraTempo.anchoredPosition = Vector2.zero;
            barraTempo.sizeDelta = new Vector2(larghezzaBarra, 3f);
        }

        radice.SetActive(false);
        return new SlotHud
        {
            radice = radice,
            testoLivello = testoLivello,
            barraTempo = barraTempo,
            larghezzaBarra = larghezzaBarra
        };
    }

    private void CollegaGiocatore()
    {
        GameObject giocatore = GameObject.FindGameObjectWithTag("Player");
        if (giocatore == null)
        {
            potenziamenti = null;
            sparo = null;
            movimento = null;
            return;
        }

        potenziamenti = giocatore.GetComponent<PlayerUpgrades>();
        sparo = giocatore.GetComponent<PlayerShooting>();
        movimento = giocatore.GetComponent<PlayerMovement>();
    }

    private void AggiornaStatoCompleto()
    {
        NumeroPotenziamentiPermanentiAttivi = 0;
        foreach (KeyValuePair<TipoPotenziamento, SlotHud> coppia in
                 slotPermanenti)
        {
            int livello = potenziamenti != null
                ? potenziamenti.OttieniLivello(coppia.Key)
                : 0;
            bool attivo = livello > 0;
            coppia.Value.radice.SetActive(attivo);
            if (!attivo) continue;

            NumeroPotenziamentiPermanentiAttivi++;
            coppia.Value.testoLivello.text = "L" + livello;
        }

        bool triploAttivo = sparo != null && sparo.TriploSparoAttivo;
        bool boostAttivo = movimento != null && movimento.BoostVelocitaAttivo;
        slotTriploSparo.radice.SetActive(triploAttivo);
        slotBoostVelocita.radice.SetActive(boostAttivo);
        NumeroEffettiTemporaneiAttivi =
            (triploAttivo ? 1 : 0) + (boostAttivo ? 1 : 0);

        numeroSlotVisibili = NumeroPotenziamentiPermanentiAttivi +
                            NumeroEffettiTemporaneiAttivi;
        intestazione.text = numeroSlotVisibili == 1
            ? "1 POWER-UP ATTIVO"
            : numeroSlotVisibili + " POWER-UP ATTIVI";
        AggiornaTimerTemporanei();
        AggiornaDimensioni();
    }

    private void AggiornaTimerTemporanei()
    {
        if (sparo != null && slotTriploSparo.radice.activeSelf)
        {
            AggiornaTimer(
                slotTriploSparo,
                sparo.TempoTriploSparoRimasto,
                sparo.DurataTriploSparoTotale
            );
        }
        if (movimento != null && slotBoostVelocita.radice.activeSelf)
        {
            AggiornaTimer(
                slotBoostVelocita,
                movimento.TempoBoostVelocitaRimasto,
                movimento.DurataBoostVelocitaTotale
            );
        }
    }

    private static void AggiornaTimer(
        SlotHud slot,
        float rimanente,
        float durata
    )
    {
        slot.testoLivello.text = Mathf.Max(0f, rimanente)
            .ToString("0.0")
            .Replace('.', ',') + "s";
        if (slot.barraTempo == null) return;

        float rapporto = durata > 0f
            ? Mathf.Clamp01(rimanente / durata)
            : 0f;
        slot.barraTempo.sizeDelta = new Vector2(
            slot.larghezzaBarra * rapporto,
            slot.barraTempo.sizeDelta.y
        );
    }

    private void AggiornaDimensioni()
    {
        if (numeroSlotVisibili <= 0)
        {
            rectRadice.sizeDelta = Vector2.zero;
            return;
        }

        int colonne = Mathf.Min(ColonneMassime, numeroSlotVisibili);
        int righe = Mathf.CeilToInt(
            numeroSlotVisibili / (float)ColonneMassime
        );
        float larghezza = 14f + colonne * DimensioneSlot.x +
            Mathf.Max(0, colonne - 1) * SpaziaturaSlot.x;
        float altezzaGriglia = righe * DimensioneSlot.y +
            Mathf.Max(0, righe - 1) * SpaziaturaSlot.y;
        rectRadice.sizeDelta = new Vector2(larghezza, 29f + altezzaGriglia);
        rectGriglia.sizeDelta = new Vector2(larghezza - 14f, altezzaGriglia);
        griglia.constraintCount = ColonneMassime;
    }

    private static GameObject CreaBarra(
        Transform parent,
        string nome,
        Color colore
    )
    {
        GameObject oggetto = new GameObject(
            nome,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        oggetto.transform.SetParent(parent, false);
        Image immagine = oggetto.GetComponent<Image>();
        immagine.color = colore;
        immagine.raycastTarget = false;
        return oggetto;
    }

    private static TMP_Text CreaTesto(
        Transform parent,
        string nome,
        string contenuto,
        float dimensione,
        Color colore,
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
        TMP_Text testo = oggetto.GetComponent<TMP_Text>();
        testo.text = contenuto;
        testo.fontSize = dimensione;
        testo.fontStyle = FontStyles.Bold;
        testo.alignment = allineamento;
        testo.textWrappingMode = TextWrappingModes.NoWrap;
        testo.overflowMode = TextOverflowModes.Overflow;
        testo.raycastTarget = false;
        FarmPixelUI.ApplicaTesto(testo, colore);
        return testo;
    }

    private static void ImpostaRectCentrato(
        RectTransform rect,
        Vector2 posizione,
        Vector2 dimensioni
    )
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posizione;
        rect.sizeDelta = dimensioni;
    }
}
