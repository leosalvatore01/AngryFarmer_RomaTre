using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum TipoObiettivoFattoria
{
    ProteggiGalline,
    CatturaMaialino,
    SenzaDanni,
    SalvaUova,
    EliminaAlfa
}

public enum StatoObiettivoFattoria
{
    Inattivo,
    Attivo,
    Completato,
    Fallito
}

public sealed class FarmObjectivesController : MonoBehaviour
{
    public static FarmObjectivesController Instance { get; private set; }

    private GameObject pannello;
    private TMP_Text testoTitolo;
    private TMP_Text testoDescrizione;
    private TMP_Text testoProgresso;
    private Image barraProgresso;
    private PlayerHealth saluteGiocatore;
    private AnteprimaOndata onda;
    private TipoObiettivoFattoria tipo;
    private StatoObiettivoFattoria stato;
    private int gallineInizio;
    private int danniSubiti;
    private int maialiniCatturati;
    private int uovaRubate;
    private int uovaRecuperate;
    private int uovaPerse;
    private int furtiSventati;
    private int alfaEliminate;
    private bool alfaHaColpito;
    private bool ondaAttiva;

    public TipoObiettivoFattoria Tipo => tipo;
    public StatoObiettivoFattoria Stato => stato;
    public bool OndaAttiva => ondaAttiva;
    public bool RichiedeMaialino =>
        ondaAttiva && tipo == TipoObiettivoFattoria.CatturaMaialino;

    public static FarmObjectivesController CreaOTrova()
    {
        if (Instance != null) return Instance;

        FarmObjectivesController esistente =
            FindFirstObjectByType<FarmObjectivesController>();
        if (esistente != null) return esistente;

        GameObject oggetto = new GameObject("ObiettiviFattoria");
        return oggetto.AddComponent<FarmObjectivesController>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        CostruisciHUD();
    }

    public void IniziaOnda(AnteprimaOndata anteprima)
    {
        SganciaSaluteGiocatore();
        CostruisciHUD();

        onda = anteprima;
        tipo = SelezionaTipo(anteprima);
        stato = StatoObiettivoFattoria.Attivo;
        ondaAttiva = true;
        gallineInizio = GameManager.instance != null
            ? GameManager.instance.gallineRimaste
            : 0;
        danniSubiti = 0;
        maialiniCatturati = 0;
        uovaRubate = 0;
        uovaRecuperate = 0;
        uovaPerse = 0;
        furtiSventati = 0;
        alfaEliminate = 0;
        alfaHaColpito = false;

        if (GameManager.instance != null)
        {
            GameManager.instance.PreparaNuovaOnda();
        }

        saluteGiocatore = FindFirstObjectByType<PlayerHealth>();
        if (saluteGiocatore != null)
        {
            saluteGiocatore.DannoSubito += GiocatoreDanneggiato;
        }

        if (pannello != null) pannello.SetActive(true);
        AggiornaHUD();
    }

    public void ConcludiOnda()
    {
        if (!ondaAttiva) return;

        ValutaFinale();
        ondaAttiva = false;
        SganciaSaluteGiocatore();

        bool completato = stato == StatoObiettivoFattoria.Completato;
        int premio = completato
            ? GameBalanceConfig.Corrente.ObiettiviFattoria.uovaPerObiettivo
            : 0;

        if (GameManager.instance != null)
        {
            GameManager.instance.RegistraEsitoObiettivo(
                NomeObiettivo(tipo),
                completato,
                premio
            );
            GameManager.instance.ConcludiRegistroOnda();
        }
        AggiornaHUD();
    }

    public void InterrompiOnda()
    {
        if (!ondaAttiva && stato == StatoObiettivoFattoria.Inattivo) return;

        ondaAttiva = false;
        stato = StatoObiettivoFattoria.Inattivo;
        SganciaSaluteGiocatore();
        if (pannello != null) pannello.SetActive(false);
    }

    public void NotificaUovoRubato()
    {
        if (!ondaAttiva) return;
        uovaRubate++;
        AggiornaHUD();
    }

    public void NotificaUovoDaRecuperare()
    {
        if (!ondaAttiva) return;
        AggiornaHUD();
    }

    public void NotificaUovoRecuperato()
    {
        if (!ondaAttiva) return;
        uovaRecuperate++;
        furtiSventati++;
        ValutaIntermedia();
        AggiornaHUD();
    }

    public void NotificaUovoPerso()
    {
        if (!ondaAttiva) return;
        uovaPerse++;
        ValutaIntermedia();
        AggiornaHUD();
    }

    public void NotificaMaialinoCatturato()
    {
        if (!ondaAttiva) return;
        maialiniCatturati++;
        ValutaIntermedia();
        AggiornaHUD();
    }

    public void NotificaVolpeEliminata(
        TipoVolpe tipoVolpe,
        bool richiedeRecupero
    )
    {
        if (!ondaAttiva) return;

        if (tipoVolpe == TipoVolpe.Alfa)
        {
            alfaEliminate++;
        }
        else if (tipoVolpe == TipoVolpe.Ladra && !richiedeRecupero)
        {
            furtiSventati++;
        }

        ValutaIntermedia();
        AggiornaHUD();
    }

    public void NotificaAlfaHaColpito()
    {
        if (!ondaAttiva) return;
        alfaHaColpito = true;
        ValutaIntermedia();
        AggiornaHUD();
    }

    private void GiocatoreDanneggiato(int danno)
    {
        if (!ondaAttiva) return;
        danniSubiti += Mathf.Max(0, danno);
        ValutaIntermedia();
        AggiornaHUD();
    }

    private void ValutaIntermedia()
    {
        if (stato != StatoObiettivoFattoria.Attivo) return;

        switch (tipo)
        {
            case TipoObiettivoFattoria.ProteggiGalline:
                if (GallineRimaste() < gallineInizio)
                {
                    stato = StatoObiettivoFattoria.Fallito;
                }
                break;
            case TipoObiettivoFattoria.CatturaMaialino:
                if (maialiniCatturati >= BersaglioMaialini())
                {
                    stato = StatoObiettivoFattoria.Completato;
                }
                break;
            case TipoObiettivoFattoria.SenzaDanni:
                if (danniSubiti > 0)
                {
                    stato = StatoObiettivoFattoria.Fallito;
                }
                break;
            case TipoObiettivoFattoria.SalvaUova:
                if (uovaPerse > 0)
                {
                    stato = StatoObiettivoFattoria.Fallito;
                }
                else if (furtiSventati >= BersaglioLadre())
                {
                    stato = StatoObiettivoFattoria.Completato;
                }
                break;
            case TipoObiettivoFattoria.EliminaAlfa:
                if (alfaHaColpito)
                {
                    stato = StatoObiettivoFattoria.Fallito;
                }
                else if (alfaEliminate > 0)
                {
                    stato = StatoObiettivoFattoria.Completato;
                }
                break;
        }
    }

    private void ValutaFinale()
    {
        if (stato == StatoObiettivoFattoria.Fallito) return;

        bool completato;
        switch (tipo)
        {
            case TipoObiettivoFattoria.ProteggiGalline:
                completato = GallineRimaste() >= gallineInizio;
                break;
            case TipoObiettivoFattoria.CatturaMaialino:
                completato = maialiniCatturati >= BersaglioMaialini();
                break;
            case TipoObiettivoFattoria.SenzaDanni:
                completato = danniSubiti == 0;
                break;
            case TipoObiettivoFattoria.SalvaUova:
                completato =
                    uovaPerse == 0 &&
                    furtiSventati >= BersaglioLadre();
                break;
            case TipoObiettivoFattoria.EliminaAlfa:
                completato = alfaEliminate > 0 && !alfaHaColpito;
                break;
            default:
                completato = false;
                break;
        }

        stato = completato
            ? StatoObiettivoFattoria.Completato
            : StatoObiettivoFattoria.Fallito;
    }

    private int GallineRimaste()
    {
        return GameManager.instance != null
            ? GameManager.instance.gallineRimaste
            : 0;
    }

    private int BersaglioMaialini()
    {
        return Mathf.Max(1, onda.NumeroMaialini);
    }

    private int BersaglioLadre()
    {
        return Mathf.Max(1, Mathf.Min(3, onda.Composizione.Ladre));
    }

    public static TipoObiettivoFattoria SelezionaTipo(
        AnteprimaOndata anteprima
    )
    {
        switch (anteprima.Indice)
        {
            case 1:
                return TipoObiettivoFattoria.ProteggiGalline;
            case 2:
                if (anteprima.NumeroMaialini > 0)
                    return TipoObiettivoFattoria.CatturaMaialino;
                break;
            case 3:
                return TipoObiettivoFattoria.SenzaDanni;
            case 4:
                if (anteprima.Composizione.Ladre > 0)
                    return TipoObiettivoFattoria.SalvaUova;
                break;
            case 5:
                if (anteprima.Composizione.Alfa > 0)
                    return TipoObiettivoFattoria.EliminaAlfa;
                break;
            default:
                if (anteprima.Composizione.Ladre > 0)
                    return TipoObiettivoFattoria.SalvaUova;
                if (anteprima.Composizione.Alfa > 0)
                    return TipoObiettivoFattoria.EliminaAlfa;
                if (anteprima.NumeroMaialini > 0)
                    return TipoObiettivoFattoria.CatturaMaialino;
                break;
        }
        return TipoObiettivoFattoria.ProteggiGalline;
    }

    public static string DescriviAnteprima(AnteprimaOndata anteprima)
    {
        TipoObiettivoFattoria tipoAnteprima = SelezionaTipo(anteprima);
        switch (tipoAnteprima)
        {
            case TipoObiettivoFattoria.CatturaMaialino:
                return "CATTURA IL MAIALINO BONUS";
            case TipoObiettivoFattoria.SenzaDanni:
                return "NON SUBIRE DANNI";
            case TipoObiettivoFattoria.SalvaUova:
                return "FERMA LE LADRE E RECUPERA LE UOVA";
            case TipoObiettivoFattoria.EliminaAlfa:
                return "ELIMINA L'ALFA SENZA FARTI COLPIRE";
            default:
                return "NON PERDERE GALLINE";
        }
    }

    private static string NomeObiettivo(TipoObiettivoFattoria obiettivo)
    {
        switch (obiettivo)
        {
            case TipoObiettivoFattoria.CatturaMaialino:
                return "Cattura il maialino";
            case TipoObiettivoFattoria.SenzaDanni:
                return "Nessun danno";
            case TipoObiettivoFattoria.SalvaUova:
                return "Salva le uova";
            case TipoObiettivoFattoria.EliminaAlfa:
                return "Elimina la volpe alfa";
            default:
                return "Proteggi le galline";
        }
    }

    private void AggiornaHUD()
    {
        if (pannello == null) return;

        testoTitolo.text = stato == StatoObiettivoFattoria.Completato
            ? "OBIETTIVO COMPLETATO"
            : stato == StatoObiettivoFattoria.Fallito
                ? "OBIETTIVO FALLITO"
                : "OBIETTIVO FACOLTATIVO";
        testoDescrizione.text = DescrizioneCorrente();
        testoProgresso.text = ProgressoCorrente();

        Color colore = stato == StatoObiettivoFattoria.Completato
            ? FarmPixelUI.TestoConfrontoFlat
            : stato == StatoObiettivoFattoria.Fallito
                ? FarmPixelUI.TestoErroreFlat
                : FarmPixelUI.TestoTitoloFlat;
        testoTitolo.color = colore;
        barraProgresso.color = colore;

        RectTransform barra = barraProgresso.rectTransform;
        barra.anchorMax = new Vector2(Mathf.Clamp01(RapportoProgresso()), 1f);
        barra.offsetMin = Vector2.zero;
        barra.offsetMax = Vector2.zero;
    }

    private string DescrizioneCorrente()
    {
        switch (tipo)
        {
            case TipoObiettivoFattoria.CatturaMaialino:
                return "Insegui e rompi tutti i salvadanai prima che fuggano.";
            case TipoObiettivoFattoria.SenzaDanni:
                return "Termina l'ondata senza perdere neppure un punto vita.";
            case TipoObiettivoFattoria.SalvaUova:
                return "Ferma le ladre. Se cade un uovo, raccoglilo in tempo.";
            case TipoObiettivoFattoria.EliminaAlfa:
                return "Abbatti la volpe alfa senza farti travolgere.";
            default:
                return "Concludi l'ondata senza perdere nessuna gallina.";
        }
    }

    private string ProgressoCorrente()
    {
        int premio = GameBalanceConfig.Corrente.ObiettiviFattoria
            .uovaPerObiettivo;
        string ricompensa = "  PREMIO +" + premio + " UOVA";

        switch (tipo)
        {
            case TipoObiettivoFattoria.CatturaMaialino:
                return "Maialini " + maialiniCatturati + " / " +
                       BersaglioMaialini() + ricompensa;
            case TipoObiettivoFattoria.SenzaDanni:
                return danniSubiti == 0
                    ? "Danni subiti 0" + ricompensa
                    : "Danni subiti " + danniSubiti + ricompensa;
            case TipoObiettivoFattoria.SalvaUova:
                return "Furti sventati " + furtiSventati + " / " +
                       BersaglioLadre() +
                       "  Recuperi " + uovaRecuperate +
                       "  A terra " + UovoRecuperabile.RecuperiAttivi;
            case TipoObiettivoFattoria.EliminaAlfa:
                return alfaHaColpito
                    ? "L'Alfa ti ha colpito" + ricompensa
                    : "Alfa eliminate " + alfaEliminate + " / 1" +
                      ricompensa;
            default:
                return "Galline vive " + GallineRimaste() + " / " +
                       gallineInizio + ricompensa;
        }
    }

    private float RapportoProgresso()
    {
        if (stato == StatoObiettivoFattoria.Completato) return 1f;
        if (stato == StatoObiettivoFattoria.Fallito) return 0f;

        switch (tipo)
        {
            case TipoObiettivoFattoria.CatturaMaialino:
                return (float)maialiniCatturati / BersaglioMaialini();
            case TipoObiettivoFattoria.SenzaDanni:
                return danniSubiti == 0 ? 1f : 0f;
            case TipoObiettivoFattoria.SalvaUova:
                return (float)furtiSventati / BersaglioLadre();
            case TipoObiettivoFattoria.EliminaAlfa:
                return alfaEliminate > 0 ? 1f : 0f;
            default:
                return gallineInizio > 0
                    ? (float)GallineRimaste() / gallineInizio
                    : 0f;
        }
    }

    private void CostruisciHUD()
    {
        if (pannello != null) return;

        GameObject interfaccia = GameObject.Find("Interfaccia");
        if (interfaccia == null) return;

        pannello = new GameObject(
            "PannelloObiettivoFattoria",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        pannello.transform.SetParent(interfaccia.transform, false);

        RectTransform rect = pannello.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-18f, -206f);
        rect.sizeDelta = new Vector2(480f, 178f);

        FarmPixelUI.ApplicaPannello(
            pannello.GetComponent<Image>(),
            false,
            false
        );
        FarmPixelUI.AggiungiIcona(
            pannello.transform,
            "IconaObiettivo",
            FarmPixelIcon.Obiettivo,
            new Vector2(-207f, 61f),
            new Vector2(42f, 42f)
        );
        testoTitolo = CreaTesto(
            "Titolo",
            new Vector2(20f, 63f),
            new Vector2(402f, 34f),
            22f,
            FontStyles.Bold
        );
        testoDescrizione = CreaTesto(
            "Descrizione",
            new Vector2(0f, 19f),
            new Vector2(444f, 54f),
            18f,
            FontStyles.Normal
        );
        testoDescrizione.textWrappingMode = TextWrappingModes.Normal;
        testoDescrizione.overflowMode = TextOverflowModes.Ellipsis;
        testoDescrizione.maxVisibleLines = 2;
        testoProgresso = CreaTesto(
            "Progresso",
            new Vector2(0f, -34f),
            new Vector2(444f, 30f),
            18f,
            FontStyles.Bold
        );
        testoProgresso.overflowMode = TextOverflowModes.Ellipsis;

        GameObject contenitoreBarra = new GameObject(
            "BarraProgresso",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        contenitoreBarra.transform.SetParent(pannello.transform, false);
        RectTransform barraRect =
            contenitoreBarra.GetComponent<RectTransform>();
        barraRect.anchorMin = new Vector2(0.5f, 0.5f);
        barraRect.anchorMax = new Vector2(0.5f, 0.5f);
        barraRect.pivot = new Vector2(0.5f, 0.5f);
        barraRect.anchoredPosition = new Vector2(0f, -71f);
        barraRect.sizeDelta = new Vector2(444f, 12f);
        Image sfondoBarra = contenitoreBarra.GetComponent<Image>();
        sfondoBarra.color = new Color(0.16f, 0.07f, 0.025f, 0.95f);
        sfondoBarra.raycastTarget = false;

        GameObject riempimento = new GameObject(
            "Riempimento",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        riempimento.transform.SetParent(contenitoreBarra.transform, false);
        RectTransform riempimentoRect =
            riempimento.GetComponent<RectTransform>();
        riempimentoRect.anchorMin = Vector2.zero;
        riempimentoRect.anchorMax = Vector2.one;
        riempimentoRect.pivot = new Vector2(0f, 0.5f);
        riempimentoRect.offsetMin = Vector2.zero;
        riempimentoRect.offsetMax = Vector2.zero;
        barraProgresso = riempimento.GetComponent<Image>();
        barraProgresso.raycastTarget = false;

        pannello.SetActive(false);
    }

    private TMP_Text CreaTesto(
        string nome,
        Vector2 posizione,
        Vector2 dimensione,
        float fontSize,
        FontStyles stile
    )
    {
        GameObject oggetto = new GameObject(
            nome,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );
        oggetto.transform.SetParent(pannello.transform, false);

        RectTransform rect = oggetto.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posizione;
        rect.sizeDelta = dimensione;

        TextMeshProUGUI testo = oggetto.GetComponent<TextMeshProUGUI>();
        TMP_Text riferimento = GameManager.TrovaTestoInterfaccia("OndataText");
        if (riferimento != null) testo.font = riferimento.font;
        testo.fontSize = fontSize;
        testo.fontStyle = stile;
        testo.alignment = TextAlignmentOptions.Center;
        testo.textWrappingMode = TextWrappingModes.NoWrap;
        testo.overflowMode = TextOverflowModes.Ellipsis;
        testo.raycastTarget = false;
        FarmPixelUI.ApplicaTesto(
            testo,
            new Color(1f, 0.9f, 0.65f, 1f)
        );
        return testo;
    }

    private void SganciaSaluteGiocatore()
    {
        if (saluteGiocatore != null)
        {
            saluteGiocatore.DannoSubito -= GiocatoreDanneggiato;
            saluteGiocatore = null;
        }
    }

    private void OnDestroy()
    {
        SganciaSaluteGiocatore();
        if (Instance == this) Instance = null;
    }
}
