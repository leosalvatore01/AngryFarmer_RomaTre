using System;
using System.Text;
using UnityEngine;

public enum TipoVolpe
{
    Comune = 0,
    Agile = 1,
    Robusta = 2,
    Ladra = 3,
    Alfa = 4
}

[Serializable]
public sealed class FoxVariantStats
{
    [Min(0.1f)] public float moltiplicatoreVelocita = 1f;
    [Min(0.1f)] public float moltiplicatoreAccelerazione = 1f;
    [Min(0.1f)] public float moltiplicatoreDecelerazione = 1f;
    [Min(0.1f)] public float moltiplicatoreVita = 1f;
    [Min(0.1f)] public float moltiplicatoreIntervalloAttacco = 1f;
    [Min(0.1f)] public float scala = 1f;
    [Range(0f, 2f)] public float moltiplicatoreRinculo = 1f;
    [Min(0)] public int monetePerEliminazione = 1;
    [Range(0f, 0.8f)] public float ampiezzaSerpentina;
    [Min(0f)] public float frequenzaSerpentina;
}

[Serializable]
public sealed class FoxVariantsBalanceSettings
{
    public FoxVariantStats comune = CreaComune();
    public FoxVariantStats agile = CreaAgile();
    public FoxVariantStats robusta = CreaRobusta();
    public FoxVariantStats ladra = CreaLadra();
    public FoxVariantStats alfa = CreaAlfa();

    [Header("Volpe ladra")]
    [Min(0.05f)] public float intervalloRicercaGallina = 0.3f;
    [Min(0.1f)] public float distanzaPrelievoGallina = 0.58f;
    [Min(2f)] public float distanzaFugaLadra = 11.6f;
    [Min(1f)] public float moltiplicatoreFugaLadra = 1.34f;

    [Header("Attacco alfa")]
    [Min(0.5f)] public float distanzaPreparazioneAlfa = 2.65f;
    [Min(0.1f)] public float durataPreparazioneAlfa = 0.72f;
    [Min(0.1f)] public float durataScattoAlfa = 0.32f;
    [Min(1f)] public float moltiplicatoreScattoAlfa = 3.15f;
    [Min(0.1f)] public float recuperoScattoAlfa = 2.25f;

    [Header("Audio")]
    [Range(0f, 1f)] public float volumeVersi = 0.16f;

    public FoxVariantStats Ottieni(TipoVolpe tipo)
    {
        switch (FoxVariantStyle.Normalizza(tipo))
        {
            case TipoVolpe.Agile:
                return agile ?? (agile = CreaAgile());
            case TipoVolpe.Robusta:
                return robusta ?? (robusta = CreaRobusta());
            case TipoVolpe.Ladra:
                return ladra ?? (ladra = CreaLadra());
            case TipoVolpe.Alfa:
                return alfa ?? (alfa = CreaAlfa());
            default:
                return comune ?? (comune = CreaComune());
        }
    }

    public static FoxVariantStats CreaComune()
    {
        return new FoxVariantStats
        {
            moltiplicatoreVelocita = 1f,
            moltiplicatoreAccelerazione = 1f,
            moltiplicatoreDecelerazione = 1f,
            moltiplicatoreVita = 1f,
            moltiplicatoreIntervalloAttacco = 1f,
            scala = 1f,
            moltiplicatoreRinculo = 1f,
            monetePerEliminazione = 1
        };
    }

    public static FoxVariantStats CreaAgile()
    {
        return new FoxVariantStats
        {
            moltiplicatoreVelocita = 1.45f,
            moltiplicatoreAccelerazione = 1.55f,
            moltiplicatoreDecelerazione = 1.3f,
            moltiplicatoreVita = 0.7f,
            moltiplicatoreIntervalloAttacco = 0.82f,
            scala = 0.88f,
            moltiplicatoreRinculo = 1.2f,
            monetePerEliminazione = 1,
            ampiezzaSerpentina = 0.34f,
            frequenzaSerpentina = 2.15f
        };
    }

    public static FoxVariantStats CreaRobusta()
    {
        return new FoxVariantStats
        {
            moltiplicatoreVelocita = 0.7f,
            moltiplicatoreAccelerazione = 0.74f,
            moltiplicatoreDecelerazione = 0.82f,
            moltiplicatoreVita = 1.6f,
            moltiplicatoreIntervalloAttacco = 1.2f,
            scala = 1.2f,
            moltiplicatoreRinculo = 0.2f,
            monetePerEliminazione = 2
        };
    }

    public static FoxVariantStats CreaLadra()
    {
        return new FoxVariantStats
        {
            moltiplicatoreVelocita = 1.12f,
            moltiplicatoreAccelerazione = 1.2f,
            moltiplicatoreDecelerazione = 1.15f,
            moltiplicatoreVita = 0.9f,
            moltiplicatoreIntervalloAttacco = 1f,
            scala = 0.96f,
            moltiplicatoreRinculo = 0.9f,
            monetePerEliminazione = 2
        };
    }

    public static FoxVariantStats CreaAlfa()
    {
        return new FoxVariantStats
        {
            moltiplicatoreVelocita = 0.86f,
            moltiplicatoreAccelerazione = 1.05f,
            moltiplicatoreDecelerazione = 1.1f,
            moltiplicatoreVita = 2f,
            moltiplicatoreIntervalloAttacco = 1.15f,
            scala = 1.4f,
            moltiplicatoreRinculo = 0.12f,
            monetePerEliminazione = 5
        };
    }
}

public readonly struct ComposizioneVolpi
{
    public int Comuni { get; }
    public int Agili { get; }
    public int Robuste { get; }
    public int Ladre { get; }
    public int Alfa { get; }
    public int Totale => Comuni + Agili + Robuste + Ladre + Alfa;

    public ComposizioneVolpi(
        int comuni,
        int agili,
        int robuste,
        int ladre,
        int alfa
    )
    {
        Comuni = Mathf.Max(0, comuni);
        Agili = Mathf.Max(0, agili);
        Robuste = Mathf.Max(0, robuste);
        Ladre = Mathf.Max(0, ladre);
        Alfa = Mathf.Max(0, alfa);
    }

    public int Ottieni(TipoVolpe tipo)
    {
        switch (FoxVariantStyle.Normalizza(tipo))
        {
            case TipoVolpe.Agile: return Agili;
            case TipoVolpe.Robusta: return Robuste;
            case TipoVolpe.Ladra: return Ladre;
            case TipoVolpe.Alfa: return Alfa;
            default: return Comuni;
        }
    }

    public ComposizioneVolpi Aggiungi(TipoVolpe tipo, int quantita = 1)
    {
        int valore = Mathf.Max(0, quantita);
        switch (FoxVariantStyle.Normalizza(tipo))
        {
            case TipoVolpe.Agile:
                return new ComposizioneVolpi(
                    Comuni, Agili + valore, Robuste, Ladre, Alfa
                );
            case TipoVolpe.Robusta:
                return new ComposizioneVolpi(
                    Comuni, Agili, Robuste + valore, Ladre, Alfa
                );
            case TipoVolpe.Ladra:
                return new ComposizioneVolpi(
                    Comuni, Agili, Robuste, Ladre + valore, Alfa
                );
            case TipoVolpe.Alfa:
                return new ComposizioneVolpi(
                    Comuni, Agili, Robuste, Ladre, Alfa + valore
                );
            default:
                return new ComposizioneVolpi(
                    Comuni + valore, Agili, Robuste, Ladre, Alfa
                );
        }
    }

    public ComposizioneVolpi Rimuovi(TipoVolpe tipo, int quantita = 1)
    {
        int valore = Mathf.Max(0, quantita);
        switch (FoxVariantStyle.Normalizza(tipo))
        {
            case TipoVolpe.Agile:
                return new ComposizioneVolpi(
                    Comuni, Agili - valore, Robuste, Ladre, Alfa
                );
            case TipoVolpe.Robusta:
                return new ComposizioneVolpi(
                    Comuni, Agili, Robuste - valore, Ladre, Alfa
                );
            case TipoVolpe.Ladra:
                return new ComposizioneVolpi(
                    Comuni, Agili, Robuste, Ladre - valore, Alfa
                );
            case TipoVolpe.Alfa:
                return new ComposizioneVolpi(
                    Comuni, Agili, Robuste, Ladre, Alfa - valore
                );
            default:
                return new ComposizioneVolpi(
                    Comuni - valore, Agili, Robuste, Ladre, Alfa
                );
        }
    }

    public static ComposizioneVolpi operator +(
        ComposizioneVolpi sinistra,
        ComposizioneVolpi destra
    )
    {
        return new ComposizioneVolpi(
            sinistra.Comuni + destra.Comuni,
            sinistra.Agili + destra.Agili,
            sinistra.Robuste + destra.Robuste,
            sinistra.Ladre + destra.Ladre,
            sinistra.Alfa + destra.Alfa
        );
    }

    public string FormattaEstesa(string separatore = " + ")
    {
        StringBuilder testo = new StringBuilder(96);
        AggiungiVoce(testo, TipoVolpe.Comune, Comuni, separatore, false);
        AggiungiVoce(testo, TipoVolpe.Agile, Agili, separatore, false);
        AggiungiVoce(testo, TipoVolpe.Robusta, Robuste, separatore, false);
        AggiungiVoce(testo, TipoVolpe.Ladra, Ladre, separatore, false);
        AggiungiVoce(testo, TipoVolpe.Alfa, Alfa, separatore, false);
        return testo.Length > 0 ? testo.ToString() : "NESSUNA VOLPE";
    }

    public string FormattaCompatta(string separatore = "  ")
    {
        StringBuilder testo = new StringBuilder(48);
        AggiungiVoce(testo, TipoVolpe.Comune, Comuni, separatore, true);
        AggiungiVoce(testo, TipoVolpe.Agile, Agili, separatore, true);
        AggiungiVoce(testo, TipoVolpe.Robusta, Robuste, separatore, true);
        AggiungiVoce(testo, TipoVolpe.Ladra, Ladre, separatore, true);
        AggiungiVoce(testo, TipoVolpe.Alfa, Alfa, separatore, true);
        return testo.Length > 0 ? testo.ToString() : "-";
    }

    private static void AggiungiVoce(
        StringBuilder testo,
        TipoVolpe tipo,
        int quantita,
        string separatore,
        bool compatta
    )
    {
        if (quantita <= 0) return;
        if (testo.Length > 0) testo.Append(separatore);
        if (compatta)
        {
            testo.Append(FoxVariantStyle.Abbreviazione(tipo));
            testo.Append(" x");
            testo.Append(quantita);
            return;
        }
        testo.Append(quantita);
        testo.Append(' ');
        testo.Append(FoxVariantStyle.Nome(tipo, quantita));
    }
}

public static class FoxVariantStyle
{
    public static TipoVolpe Normalizza(TipoVolpe tipo)
    {
        int valore = (int)tipo;
        return valore >= (int)TipoVolpe.Comune &&
               valore <= (int)TipoVolpe.Alfa
            ? tipo
            : TipoVolpe.Comune;
    }

    public static string Nome(TipoVolpe tipo, int quantita = 1)
    {
        bool plurale = quantita != 1;
        switch (Normalizza(tipo))
        {
            case TipoVolpe.Agile: return plurale ? "AGILI" : "AGILE";
            case TipoVolpe.Robusta: return plurale ? "ROBUSTE" : "ROBUSTA";
            case TipoVolpe.Ladra: return plurale ? "LADRE" : "LADRA";
            case TipoVolpe.Alfa: return "ALFA";
            default: return plurale ? "COMUNI" : "COMUNE";
        }
    }

    public static string Abbreviazione(TipoVolpe tipo)
    {
        switch (Normalizza(tipo))
        {
            case TipoVolpe.Agile: return "AG";
            case TipoVolpe.Robusta: return "R";
            case TipoVolpe.Ladra: return "L";
            case TipoVolpe.Alfa: return "AL";
            default: return "C";
        }
    }

    public static int Priorita(TipoVolpe tipo)
    {
        switch (Normalizza(tipo))
        {
            case TipoVolpe.Alfa: return 5;
            case TipoVolpe.Ladra: return 4;
            case TipoVolpe.Robusta: return 3;
            case TipoVolpe.Agile: return 2;
            default: return 1;
        }
    }

    public static Color32 ColoreUi(TipoVolpe tipo)
    {
        switch (Normalizza(tipo))
        {
            case TipoVolpe.Agile: return new Color32(63, 214, 190, 255);
            case TipoVolpe.Robusta: return new Color32(151, 106, 77, 255);
            case TipoVolpe.Ladra: return new Color32(238, 190, 55, 255);
            case TipoVolpe.Alfa: return new Color32(224, 62, 58, 255);
            default: return new Color32(237, 132, 48, 255);
        }
    }

    public static Color ColoreCorpo(TipoVolpe tipo)
    {
        switch (Normalizza(tipo))
        {
            case TipoVolpe.Agile: return new Color(0.7f, 1f, 0.9f, 1f);
            case TipoVolpe.Robusta: return new Color(0.72f, 0.6f, 0.52f, 1f);
            case TipoVolpe.Ladra: return new Color(0.82f, 0.72f, 0.92f, 1f);
            case TipoVolpe.Alfa: return new Color(1f, 0.47f, 0.43f, 1f);
            default: return Color.white;
        }
    }
}

public sealed class FoxVariantPresentation : MonoBehaviour
{
    private static readonly Sprite[] accessori = new Sprite[5];
    private static readonly Sprite[] badge = new Sprite[5];
    private static Sprite auraAlfa;
    private static Sprite uovoRubato;

    private TipoVolpe tipo;
    private SpriteRenderer corpo;
    private Transform grafica;
    private SpriteRenderer accessorioRenderer;
    private SpriteRenderer badgeRenderer;
    private SpriteRenderer auraRenderer;
    private SpriteRenderer uovoRenderer;
    private Vector2 posizioneAccessorio;
    private bool telegraphAlfa;

    public TipoVolpe Tipo => tipo;
    public bool AccessorioVisibile =>
        accessorioRenderer != null && accessorioRenderer.enabled;
    public bool BadgeVisibile =>
        badgeRenderer != null && badgeRenderer.enabled;
    public bool TelegraphAlfaVisibile =>
        auraRenderer != null && auraRenderer.enabled;
    public bool TrasportoGallinaVisibile =>
        uovoRenderer != null && uovoRenderer.enabled;
    public Color ColoreCorpo => corpo != null ? corpo.color : Color.white;

    public void Configura(
        TipoVolpe nuovoTipo,
        SpriteRenderer rendererCorpo,
        Transform trasformazioneGrafica,
        bool riproduciVerso
    )
    {
        tipo = FoxVariantStyle.Normalizza(nuovoTipo);
        corpo = rendererCorpo;
        grafica = trasformazioneGrafica;

        if (corpo != null)
        {
            corpo.color = FoxVariantStyle.ColoreCorpo(tipo);
        }

        CreaAccessorio();
        CreaBadge();
        CreaAuraAlfa();
        SincronizzaOrientamento();

        if (riproduciVerso)
        {
            FoxVariantAudioController audio =
                FoxVariantAudioController.CreaOTrova();
            if (audio != null)
            {
                audio.RiproduciSpawn(tipo, transform.position);
            }
        }
    }

    public void ImpostaTelegraphAlfa(bool attivo, float progresso)
    {
        telegraphAlfa = attivo && tipo == TipoVolpe.Alfa;
        if (auraRenderer == null) return;

        auraRenderer.enabled = telegraphAlfa;
        if (!telegraphAlfa) return;

        float t = Mathf.Clamp01(progresso);
        float impulso = 0.5f + 0.5f * Mathf.Sin(t * Mathf.PI * 6f);
        auraRenderer.transform.localScale = new Vector3(
            Mathf.Lerp(0.72f, 1.18f, t),
            Mathf.Lerp(0.56f, 0.92f, t),
            1f
        );
        Color colore = FoxVariantStyle.ColoreUi(TipoVolpe.Alfa);
        colore.a = Mathf.Lerp(0.34f, 0.82f, impulso);
        auraRenderer.color = colore;
    }

    public void RiproduciCaricaAlfa()
    {
        FoxVariantAudioController audio =
            FoxVariantAudioController.CreaOTrova();
        if (audio != null) audio.RiproduciCaricaAlfa(transform.position);
    }

    public void RiproduciScattoAlfa()
    {
        FoxVariantAudioController audio =
            FoxVariantAudioController.CreaOTrova();
        if (audio != null) audio.RiproduciScattoAlfa(transform.position);
    }

    public void RiproduciPredazione()
    {
        FoxVariantAudioController audio =
            FoxVariantAudioController.CreaOTrova();
        if (audio != null)
        {
            audio.RiproduciPredazione(tipo, transform.position);
        }
    }

    public void ImpostaTrasportoGallina(bool attivo)
    {
        if (tipo != TipoVolpe.Ladra || grafica == null) return;
        if (uovoRenderer == null)
        {
            GameObject oggetto = new GameObject("UovoRubato");
            oggetto.layer = gameObject.layer;
            oggetto.transform.SetParent(grafica, false);
            uovoRenderer = oggetto.AddComponent<SpriteRenderer>();
            uovoRenderer.sprite = OttieniUovoRubato();
            uovoRenderer.sortingLayerID = corpo != null
                ? corpo.sortingLayerID
                : 0;
            uovoRenderer.sortingOrder = corpo != null
                ? corpo.sortingOrder + 4
                : 4;
            uovoRenderer.transform.localScale = Vector3.one * 0.32f;
        }
        uovoRenderer.enabled = attivo;
        SincronizzaOrientamento();
    }

    public void NascondiPerMorte()
    {
        telegraphAlfa = false;
        if (accessorioRenderer != null) accessorioRenderer.enabled = false;
        if (badgeRenderer != null) badgeRenderer.enabled = false;
        if (auraRenderer != null) auraRenderer.enabled = false;
        if (uovoRenderer != null) uovoRenderer.enabled = false;
    }

    void LateUpdate()
    {
        SincronizzaOrientamento();
    }

    private void CreaAccessorio()
    {
        if (tipo == TipoVolpe.Comune || grafica == null)
        {
            if (accessorioRenderer != null) accessorioRenderer.enabled = false;
            return;
        }

        if (accessorioRenderer == null)
        {
            GameObject oggetto = new GameObject("Accessorio_" + tipo);
            oggetto.layer = gameObject.layer;
            oggetto.transform.SetParent(grafica, false);
            accessorioRenderer = oggetto.AddComponent<SpriteRenderer>();
        }

        accessorioRenderer.sprite = OttieniAccessorio(tipo);
        accessorioRenderer.sortingLayerID = corpo != null
            ? corpo.sortingLayerID
            : 0;
        accessorioRenderer.sortingOrder = corpo != null
            ? corpo.sortingOrder + 2
            : 2;
        accessorioRenderer.enabled = true;

        float scala;
        switch (tipo)
        {
            case TipoVolpe.Agile:
                posizioneAccessorio = new Vector2(-0.36f, 0.11f);
                scala = 0.38f;
                break;
            case TipoVolpe.Robusta:
                posizioneAccessorio = new Vector2(0.2f, 0.29f);
                scala = 0.4f;
                break;
            case TipoVolpe.Ladra:
                posizioneAccessorio = new Vector2(-0.3f, 0.12f);
                scala = 0.38f;
                break;
            default:
                posizioneAccessorio = new Vector2(0.12f, 0.44f);
                scala = 0.3f;
                break;
        }
        accessorioRenderer.transform.localScale = Vector3.one * scala;
    }

    private void CreaBadge()
    {
        if (tipo == TipoVolpe.Comune || grafica == null)
        {
            if (badgeRenderer != null) badgeRenderer.enabled = false;
            return;
        }

        if (badgeRenderer == null)
        {
            GameObject oggetto = new GameObject("BadgeTipoVolpe");
            oggetto.layer = gameObject.layer;
            oggetto.transform.SetParent(grafica, false);
            badgeRenderer = oggetto.AddComponent<SpriteRenderer>();
        }

        badgeRenderer.sprite = OttieniBadge(tipo);
        badgeRenderer.sortingLayerID = corpo != null ? corpo.sortingLayerID : 0;
        badgeRenderer.sortingOrder = corpo != null ? corpo.sortingOrder + 12 : 12;
        badgeRenderer.transform.localPosition = new Vector3(0f, 0.79f, 0f);
        badgeRenderer.transform.localScale = Vector3.one * 0.24f;
        badgeRenderer.enabled = true;
    }

    private void CreaAuraAlfa()
    {
        if (tipo != TipoVolpe.Alfa)
        {
            if (auraRenderer != null) auraRenderer.enabled = false;
            return;
        }

        if (auraRenderer == null)
        {
            GameObject oggetto = new GameObject("TelegraphScattoAlfa");
            oggetto.layer = gameObject.layer;
            oggetto.transform.SetParent(transform, false);
            auraRenderer = oggetto.AddComponent<SpriteRenderer>();
        }

        auraRenderer.sprite = OttieniAuraAlfa();
        auraRenderer.sortingLayerID = corpo != null ? corpo.sortingLayerID : 0;
        auraRenderer.sortingOrder = corpo != null ? corpo.sortingOrder - 2 : -2;
        auraRenderer.transform.localPosition = new Vector3(0f, -0.24f, 0f);
        auraRenderer.enabled = telegraphAlfa;
    }

    private void SincronizzaOrientamento()
    {
        if (corpo == null) return;
        bool invertita = corpo.flipX;

        if (accessorioRenderer != null && accessorioRenderer.enabled)
        {
            accessorioRenderer.flipX = invertita;
            accessorioRenderer.transform.localPosition = new Vector3(
                invertita ? -posizioneAccessorio.x : posizioneAccessorio.x,
                posizioneAccessorio.y,
                0f
            );
        }
        if (badgeRenderer != null) badgeRenderer.flipX = false;
        if (uovoRenderer != null && uovoRenderer.enabled)
        {
            uovoRenderer.flipX = false;
            uovoRenderer.transform.localPosition = new Vector3(
                invertita ? 0.23f : -0.23f,
                0.35f,
                0f
            );
        }
    }

    private static Sprite OttieniAccessorio(TipoVolpe tipo)
    {
        int indice = (int)FoxVariantStyle.Normalizza(tipo);
        if (accessori[indice] != null) return accessori[indice];

        switch (tipo)
        {
            case TipoVolpe.Agile:
                accessori[indice] = CreaAccessorioAgile();
                break;
            case TipoVolpe.Robusta:
                accessori[indice] = CreaAccessorioRobusta();
                break;
            case TipoVolpe.Ladra:
                accessori[indice] = CreaAccessorioLadra();
                break;
            case TipoVolpe.Alfa:
                accessori[indice] = CreaAccessorioAlfa();
                break;
        }
        return accessori[indice];
    }

    private static Sprite OttieniBadge(TipoVolpe tipo)
    {
        int indice = (int)FoxVariantStyle.Normalizza(tipo);
        if (badge[indice] != null) return badge[indice];

        Color32 colore = FoxVariantStyle.ColoreUi(tipo);
        Color32 scuro = new Color32(47, 27, 24, 255);
        Color32 chiaro = new Color32(255, 244, 199, 255);
        Color32[] pixel = NuovaTela(11, 11);

        Rettangolo(pixel, 11, 11, 2, 1, 8, 9, scuro);
        Rettangolo(pixel, 11, 11, 1, 2, 9, 8, scuro);
        Rettangolo(pixel, 11, 11, 2, 2, 8, 8, colore);

        switch (FoxVariantStyle.Normalizza(tipo))
        {
            case TipoVolpe.Agile:
                Linea(pixel, 11, 11, 6, 7, 4, 5, chiaro);
                Linea(pixel, 11, 11, 4, 5, 6, 4, chiaro);
                Linea(pixel, 11, 11, 6, 4, 4, 2, chiaro);
                break;
            case TipoVolpe.Robusta:
                Rettangolo(pixel, 11, 11, 3, 6, 7, 7, chiaro);
                Rettangolo(pixel, 11, 11, 4, 3, 6, 6, chiaro);
                Imposta(pixel, 11, 11, 5, 2, chiaro);
                break;
            case TipoVolpe.Ladra:
                Rettangolo(pixel, 11, 11, 4, 3, 6, 7, chiaro);
                Rettangolo(pixel, 11, 11, 3, 4, 7, 6, chiaro);
                Imposta(pixel, 11, 11, 5, 8, chiaro);
                break;
            case TipoVolpe.Alfa:
                Rettangolo(pixel, 11, 11, 4, 4, 6, 7, chiaro);
                Rettangolo(pixel, 11, 11, 4, 2, 6, 2, chiaro);
                break;
        }
        badge[indice] = CreaSprite("BadgeVolpe_" + tipo, pixel, 11, 11, 11f);
        return badge[indice];
    }

    private static Sprite OttieniAuraAlfa()
    {
        if (auraAlfa != null) return auraAlfa;
        const int larghezza = 24;
        const int altezza = 12;
        Color32[] pixel = NuovaTela(larghezza, altezza);
        Color32 chiaro = new Color32(255, 255, 255, 255);
        for (int x = 4; x <= 19; x++)
        {
            Imposta(pixel, larghezza, altezza, x, x < 7 || x > 16 ? 3 : 2, chiaro);
            Imposta(pixel, larghezza, altezza, x, x < 7 || x > 16 ? 8 : 9, chiaro);
        }
        for (int y = 4; y <= 7; y++)
        {
            Imposta(pixel, larghezza, altezza, 2, y, chiaro);
            Imposta(pixel, larghezza, altezza, 21, y, chiaro);
        }
        auraAlfa = CreaSprite("AuraVolpeAlfa", pixel, larghezza, altezza, 16f);
        return auraAlfa;
    }

    private static Sprite OttieniUovoRubato()
    {
        if (uovoRubato != null) return uovoRubato;
        const int larghezza = 9;
        const int altezza = 11;
        Color32[] pixel = NuovaTela(larghezza, altezza);
        Color32 scuro = new Color32(75, 47, 29, 255);
        Color32 guscio = new Color32(255, 237, 181, 255);
        Color32 luce = new Color32(255, 252, 226, 255);
        Rettangolo(pixel, larghezza, altezza, 2, 2, 6, 7, scuro);
        Rettangolo(pixel, larghezza, altezza, 1, 3, 7, 6, scuro);
        Rettangolo(pixel, larghezza, altezza, 2, 3, 6, 7, guscio);
        Rettangolo(pixel, larghezza, altezza, 3, 7, 5, 9, scuro);
        Rettangolo(pixel, larghezza, altezza, 3, 7, 5, 8, guscio);
        Imposta(pixel, larghezza, altezza, 3, 6, luce);
        Imposta(pixel, larghezza, altezza, 4, 7, luce);
        uovoRubato = CreaSprite(
            "UovoRubatoVolpeLadra",
            pixel,
            larghezza,
            altezza,
            11f
        );
        return uovoRubato;
    }

    private static Sprite CreaAccessorioAgile()
    {
        Color32[] pixel = NuovaTela(15, 9);
        Color32 scuro = new Color32(25, 71, 75, 255);
        Color32 chiaro = new Color32(52, 220, 203, 255);
        Rettangolo(pixel, 15, 9, 7, 3, 13, 5, scuro);
        Rettangolo(pixel, 15, 9, 8, 4, 13, 5, chiaro);
        Linea(pixel, 15, 9, 7, 4, 3, 7, scuro);
        Linea(pixel, 15, 9, 6, 4, 1, 2, scuro);
        Linea(pixel, 15, 9, 6, 5, 2, 6, chiaro);
        return CreaSprite("AccessorioVolpeAgile", pixel, 15, 9, 15f);
    }

    private static Sprite CreaAccessorioRobusta()
    {
        Color32[] pixel = NuovaTela(16, 11);
        Color32 scuro = new Color32(55, 39, 34, 255);
        Color32 metallo = new Color32(176, 170, 145, 255);
        Color32 luce = new Color32(219, 207, 169, 255);
        Rettangolo(pixel, 16, 11, 3, 2, 13, 4, scuro);
        Rettangolo(pixel, 16, 11, 4, 3, 12, 4, metallo);
        Rettangolo(pixel, 16, 11, 5, 5, 11, 8, scuro);
        Rettangolo(pixel, 16, 11, 6, 5, 10, 8, metallo);
        Rettangolo(pixel, 16, 11, 7, 7, 9, 9, luce);
        Imposta(pixel, 16, 11, 5, 6, luce);
        return CreaSprite("AccessorioVolpeRobusta", pixel, 16, 11, 16f);
    }

    private static Sprite CreaAccessorioLadra()
    {
        Color32[] pixel = NuovaTela(14, 14);
        Color32 scuro = new Color32(49, 33, 48, 255);
        Color32 sacco = new Color32(205, 169, 91, 255);
        Color32 corda = new Color32(245, 218, 132, 255);
        Rettangolo(pixel, 14, 14, 3, 2, 11, 9, scuro);
        Rettangolo(pixel, 14, 14, 2, 4, 12, 8, scuro);
        Rettangolo(pixel, 14, 14, 3, 3, 11, 8, sacco);
        Rettangolo(pixel, 14, 14, 5, 9, 9, 11, scuro);
        Linea(pixel, 14, 14, 5, 10, 9, 10, corda);
        Imposta(pixel, 14, 14, 7, 6, corda);
        return CreaSprite("AccessorioVolpeLadra", pixel, 14, 14, 14f);
    }

    private static Sprite CreaAccessorioAlfa()
    {
        Color32[] pixel = NuovaTela(17, 12);
        Color32 scuro = new Color32(74, 30, 23, 255);
        Color32 oro = new Color32(255, 198, 53, 255);
        Color32 luce = new Color32(255, 239, 150, 255);
        Rettangolo(pixel, 17, 12, 3, 2, 13, 4, scuro);
        Rettangolo(pixel, 17, 12, 4, 3, 12, 4, oro);
        Rettangolo(pixel, 17, 12, 4, 5, 6, 7, scuro);
        Rettangolo(pixel, 17, 12, 5, 5, 6, 7, oro);
        Rettangolo(pixel, 17, 12, 7, 5, 9, 10, scuro);
        Rettangolo(pixel, 17, 12, 8, 5, 9, 9, oro);
        Rettangolo(pixel, 17, 12, 10, 5, 12, 7, scuro);
        Rettangolo(pixel, 17, 12, 10, 5, 11, 7, oro);
        Imposta(pixel, 17, 12, 8, 9, luce);
        return CreaSprite("AccessorioVolpeAlfa", pixel, 17, 12, 17f);
    }

    private static Color32[] NuovaTela(int larghezza, int altezza)
    {
        return new Color32[larghezza * altezza];
    }

    private static void Rettangolo(
        Color32[] pixel,
        int larghezza,
        int altezza,
        int xMin,
        int yMin,
        int xMax,
        int yMax,
        Color32 colore
    )
    {
        for (int y = yMin; y <= yMax; y++)
        {
            for (int x = xMin; x <= xMax; x++)
            {
                Imposta(pixel, larghezza, altezza, x, y, colore);
            }
        }
    }

    private static void Linea(
        Color32[] pixel,
        int larghezza,
        int altezza,
        int x0,
        int y0,
        int x1,
        int y1,
        Color32 colore
    )
    {
        int dx = Mathf.Abs(x1 - x0);
        int sx = x0 < x1 ? 1 : -1;
        int dy = -Mathf.Abs(y1 - y0);
        int sy = y0 < y1 ? 1 : -1;
        int errore = dx + dy;
        while (true)
        {
            Imposta(pixel, larghezza, altezza, x0, y0, colore);
            if (x0 == x1 && y0 == y1) break;
            int doppio = 2 * errore;
            if (doppio >= dy)
            {
                errore += dy;
                x0 += sx;
            }
            if (doppio <= dx)
            {
                errore += dx;
                y0 += sy;
            }
        }
    }

    private static void Imposta(
        Color32[] pixel,
        int larghezza,
        int altezza,
        int x,
        int y,
        Color32 colore
    )
    {
        if (x < 0 || x >= larghezza || y < 0 || y >= altezza) return;
        pixel[y * larghezza + x] = colore;
    }

    private static Sprite CreaSprite(
        string nome,
        Color32[] pixel,
        int larghezza,
        int altezza,
        float pixelPerUnita
    )
    {
        Texture2D texture = new Texture2D(
            larghezza,
            altezza,
            TextureFormat.RGBA32,
            false
        );
        texture.name = nome + "_Texture";
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.hideFlags = HideFlags.HideAndDontSave;
        texture.SetPixels32(pixel);
        texture.Apply(false, true);

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, larghezza, altezza),
            new Vector2(0.5f, 0.5f),
            pixelPerUnita,
            0,
            SpriteMeshType.FullRect
        );
        sprite.name = nome;
        sprite.hideFlags = HideFlags.HideAndDontSave;
        return sprite;
    }
}

internal sealed class FoxVariantAudioController : MonoBehaviour
{
    private const int FrequenzaCampionamento = 22050;
    private const int NumeroSorgenti = 5;
    private static FoxVariantAudioController instance;

    private AudioSource[] sorgenti;
    private AudioClip[] versiSpawn;
    private AudioClip clipCaricaAlfa;
    private AudioClip clipScattoAlfa;
    private AudioClip clipPredazione;
    private int prossimaSorgente;

    public static FoxVariantAudioController CreaOTrova()
    {
        if (!AudioConsentito()) return null;
        if (instance != null) return instance;
        FoxVariantAudioController esistente = FindFirstObjectByType<
            FoxVariantAudioController
        >();
        if (esistente != null)
        {
            instance = esistente;
            return instance;
        }
        GameObject oggetto = new GameObject("AudioTipiVolpe");
        instance = oggetto.AddComponent<FoxVariantAudioController>();
        return instance;
    }

    private static bool AudioConsentito()
    {
        CombatFeedbackSettings feedback =
            GameBalanceConfig.Corrente.FeedbackCombattimento;
        CombatFeedbackController controller = CombatFeedbackController.Instance;
        return feedback.audioAttivo &&
               (controller == null || controller.AudioAbilitato);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void AzzeraStatici()
    {
        instance = null;
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        CreaSorgenti();
        CreaClip();
    }

    public void RiproduciSpawn(TipoVolpe tipo, Vector2 posizione)
    {
        int indice = (int)FoxVariantStyle.Normalizza(tipo);
        float volumeTipo = tipo == TipoVolpe.Alfa ? 1f : 0.72f;
        Riproduci(versiSpawn[indice], posizione, volumeTipo);
    }

    public void RiproduciCaricaAlfa(Vector2 posizione)
    {
        Riproduci(clipCaricaAlfa, posizione, 1f);
    }

    public void RiproduciScattoAlfa(Vector2 posizione)
    {
        Riproduci(clipScattoAlfa, posizione, 0.92f);
    }

    public void RiproduciPredazione(TipoVolpe tipo, Vector2 posizione)
    {
        Riproduci(
            tipo == TipoVolpe.Ladra ? clipPredazione : versiSpawn[(int)tipo],
            posizione,
            0.9f
        );
    }

    private void Riproduci(AudioClip clip, Vector2 posizione, float intensita)
    {
        if (clip == null || sorgenti == null || sorgenti.Length == 0) return;
        CombatFeedbackSettings feedback =
            GameBalanceConfig.Corrente.FeedbackCombattimento;
        CombatFeedbackController controller = CombatFeedbackController.Instance;
        if (!feedback.audioAttivo ||
            (controller != null && !controller.AudioAbilitato))
        {
            return;
        }

        FoxVariantsBalanceSettings varianti = GameBalanceConfig.Corrente.VariantiVolpe;
        AudioSource sorgente = OttieniSorgente();
        sorgente.panStereo = CalcolaPan(posizione);
        sorgente.volume = Mathf.Clamp01(varianti.volumeVersi * intensita);
        sorgente.pitch = 1f;
        // Ogni sorgente riproduce una sola voce: se tutte sono occupate,
        // la piu vecchia viene sostituita e la polifonia resta limitata.
        sorgente.Stop();
        sorgente.clip = clip;
        sorgente.Play();
    }

    private AudioSource OttieniSorgente()
    {
        for (int i = 0; i < sorgenti.Length; i++)
        {
            int indice = (prossimaSorgente + i) % sorgenti.Length;
            if (!sorgenti[indice].isPlaying)
            {
                prossimaSorgente = (indice + 1) % sorgenti.Length;
                return sorgenti[indice];
            }
        }
        AudioSource scelta = sorgenti[prossimaSorgente];
        prossimaSorgente = (prossimaSorgente + 1) % sorgenti.Length;
        return scelta;
    }

    private static float CalcolaPan(Vector2 posizione)
    {
        Camera cameraPrincipale = Camera.main;
        if (cameraPrincipale == null) return 0f;
        float x = cameraPrincipale.WorldToViewportPoint(posizione).x;
        return Mathf.Clamp((x - 0.5f) * 1.5f, -0.7f, 0.7f);
    }

    private void CreaSorgenti()
    {
        sorgenti = new AudioSource[NumeroSorgenti];
        for (int i = 0; i < sorgenti.Length; i++)
        {
            AudioSource sorgente = gameObject.AddComponent<AudioSource>();
            sorgente.playOnAwake = false;
            sorgente.loop = false;
            sorgente.spatialBlend = 0f;
            sorgente.dopplerLevel = 0f;
            sorgenti[i] = sorgente;
        }
    }

    private void CreaClip()
    {
        versiSpawn = new AudioClip[5];
        versiSpawn[0] = GeneraVerso("VolpeComune_Yip", 0.14f, 410f, 535f, 0);
        versiSpawn[1] = GeneraVerso("VolpeAgile_DoppioFischio", 0.2f, 720f, 1040f, 1);
        versiSpawn[2] = GeneraVerso("VolpeRobusta_Bronto", 0.27f, 205f, 132f, 2);
        versiSpawn[3] = GeneraVerso("VolpeLadra_Fruscio", 0.19f, 580f, 330f, 3);
        versiSpawn[4] = GeneraVerso("VolpeAlfa_Ululo", 0.42f, 155f, 96f, 4);
        clipCaricaAlfa = GeneraVerso("VolpeAlfa_Carica", 0.48f, 118f, 174f, 5);
        clipScattoAlfa = GeneraVerso("VolpeAlfa_Scatto", 0.18f, 260f, 610f, 6);
        clipPredazione = GeneraVerso("VolpeLadra_Furto", 0.28f, 690f, 245f, 7);
    }

    private static AudioClip GeneraVerso(
        string nome,
        float durata,
        float frequenzaInizio,
        float frequenzaFine,
        int stile
    )
    {
        int campioni = Mathf.CeilToInt(durata * FrequenzaCampionamento);
        float[] dati = new float[campioni];
        float fase = 0f;
        for (int i = 0; i < campioni; i++)
        {
            float t = i / (float)Mathf.Max(1, campioni - 1);
            float frequenza = Mathf.Lerp(frequenzaInizio, frequenzaFine, t);
            fase += frequenza * Mathf.PI * 2f / FrequenzaCampionamento;
            float inviluppo = Mathf.Sin(Mathf.PI * t);
            inviluppo *= inviluppo;
            if (stile == 1)
            {
                float finestra = t < 0.44f
                    ? Mathf.Sin(Mathf.Clamp01(t / 0.44f) * Mathf.PI)
                    : Mathf.Sin(Mathf.Clamp01((t - 0.5f) / 0.5f) * Mathf.PI);
                inviluppo *= Mathf.Max(0f, finestra);
            }
            float armonica = Mathf.Sin(fase) + 0.24f * Mathf.Sin(fase * 2.03f);
            float grana = Mathf.Sin(i * (0.71f + stile * 0.053f)) *
                          Mathf.Sin(i * 0.037f);
            float miscelaGrana = stile == 2 || stile == 4 || stile == 5
                ? 0.18f
                : stile == 3 || stile == 7 ? 0.1f : 0.035f;
            dati[i] = (armonica * 0.36f + grana * miscelaGrana) * inviluppo;
        }

        AudioClip clip = AudioClip.Create(
            nome,
            campioni,
            1,
            FrequenzaCampionamento,
            false
        );
        clip.SetData(dati, 0);
        return clip;
    }

    void OnDestroy()
    {
        if (instance == this) instance = null;
        if (versiSpawn != null)
        {
            for (int i = 0; i < versiSpawn.Length; i++)
            {
                if (versiSpawn[i] != null) Destroy(versiSpawn[i]);
            }
        }
        if (clipCaricaAlfa != null) Destroy(clipCaricaAlfa);
        if (clipScattoAlfa != null) Destroy(clipScattoAlfa);
        if (clipPredazione != null) Destroy(clipPredazione);
    }
}
