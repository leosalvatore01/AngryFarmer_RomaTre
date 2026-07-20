using System;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

public enum TipoVolpe
{
    Comune = 0,
    Agile = 1,
    Robusta = 2,
    Schivatrice = 3,
    Alfa = 4,
    Ululatrice = 5,
    Sputafango = 6,
    Scavatrice = 7
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

    [FormerlySerializedAs("ladra")]
    public FoxVariantStats schivatrice = CreaSchivatrice();

    public FoxVariantStats alfa = CreaAlfa();
    public FoxVariantStats ululatrice = CreaUlulatrice();
    public FoxVariantStats sputafango = CreaSputafango();
    public FoxVariantStats scavatrice = CreaScavatrice();

    [Header("Schivata laterale")]
    [Min(0.25f)] public float distanzaAttivazioneSchivata = 2.1f;
    [Min(0.05f)] public float durataSchivata = 0.24f;
    [Min(0.1f)] public float recuperoSchivata = 1.65f;
    [Min(1f)] public float moltiplicatoreVelocitaSchivata = 2.8f;

    [Header("Attacco alfa")]
    [Min(0.5f)] public float distanzaPreparazioneAlfa = 2.65f;
    [Min(0.1f)] public float durataPreparazioneAlfa = 0.72f;
    [Min(0.1f)] public float durataScattoAlfa = 0.32f;
    [Min(1f)] public float moltiplicatoreScattoAlfa = 3.15f;
    [Min(0.1f)] public float recuperoScattoAlfa = 2.25f;

    [Header("Ululato di controllo")]
    [Min(0.5f)] public float raggioUlulato = 4.6f;
    [Min(0.1f)] public float recuperoUlulato = 6.5f;
    [Min(0.1f)] public float durataPreparazioneUlulato = 0.85f;
    [Min(0.1f)] public float durataRallentamentoUlulato = 2.5f;
    [Range(0.1f, 1f)] public float moltiplicatoreRallentamentoUlulato = 0.72f;

    [Header("Tiro e pozza di fango")]
    [Min(1f)] public float distanzaTiroFango = 5.5f;
    [Min(0.1f)] public float recuperoTiroFango = 3.4f;
    [Min(0.5f)] public float velocitaProiettileFango = 6.2f;
    [Min(0.25f)] public float durataPozzaFango = 4.5f;
    [Min(0.25f)] public float raggioPozzaFango = 1.35f;
    [Range(0.1f, 1f)] public float moltiplicatoreRallentamentoFango = 0.58f;
    [Min(1)] public int dannoFango = 1;

    [Header("Scavo ed emersione")]
    [Min(1f)] public float distanzaInizioScavo = 5f;
    [Min(0.1f)] public float durataScavo = 0.8f;
    [Min(0.1f)] public float durataEmersione = 0.35f;
    [Min(0.1f)] public float recuperoScavo = 5.2f;
    [Min(1f)] public float moltiplicatoreVelocitaScavo = 1.65f;

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
            case TipoVolpe.Schivatrice:
                return schivatrice ?? (schivatrice = CreaSchivatrice());
            case TipoVolpe.Alfa:
                return alfa ?? (alfa = CreaAlfa());
            case TipoVolpe.Ululatrice:
                return ululatrice ?? (ululatrice = CreaUlulatrice());
            case TipoVolpe.Sputafango:
                return sputafango ?? (sputafango = CreaSputafango());
            case TipoVolpe.Scavatrice:
                return scavatrice ?? (scavatrice = CreaScavatrice());
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
            moltiplicatoreVelocita = 0.68f,
            moltiplicatoreAccelerazione = 0.74f,
            moltiplicatoreDecelerazione = 0.82f,
            moltiplicatoreVita = 2.2f,
            moltiplicatoreIntervalloAttacco = 1.15f,
            scala = 1.18f,
            moltiplicatoreRinculo = 0.18f,
            monetePerEliminazione = 2
        };
    }

    public static FoxVariantStats CreaSchivatrice()
    {
        return new FoxVariantStats
        {
            moltiplicatoreVelocita = 1.12f,
            moltiplicatoreAccelerazione = 1.35f,
            moltiplicatoreDecelerazione = 1.42f,
            moltiplicatoreVita = 0.85f,
            moltiplicatoreIntervalloAttacco = 0.95f,
            scala = 0.96f,
            moltiplicatoreRinculo = 1.1f,
            monetePerEliminazione = 2,
            ampiezzaSerpentina = 0.48f,
            frequenzaSerpentina = 2.8f
        };
    }

    public static FoxVariantStats CreaAlfa()
    {
        return new FoxVariantStats
        {
            moltiplicatoreVelocita = 0.86f,
            moltiplicatoreAccelerazione = 1.05f,
            moltiplicatoreDecelerazione = 1.1f,
            moltiplicatoreVita = 2.2f,
            moltiplicatoreIntervalloAttacco = 1.15f,
            scala = 1.35f,
            moltiplicatoreRinculo = 0.12f,
            monetePerEliminazione = 5
        };
    }

    public static FoxVariantStats CreaUlulatrice()
    {
        return new FoxVariantStats
        {
            moltiplicatoreVelocita = 0.82f,
            moltiplicatoreAccelerazione = 0.92f,
            moltiplicatoreDecelerazione = 1f,
            moltiplicatoreVita = 1.3f,
            moltiplicatoreIntervalloAttacco = 1.25f,
            scala = 1.08f,
            moltiplicatoreRinculo = 0.72f,
            monetePerEliminazione = 3
        };
    }

    public static FoxVariantStats CreaSputafango()
    {
        return new FoxVariantStats
        {
            moltiplicatoreVelocita = 0.78f,
            moltiplicatoreAccelerazione = 0.9f,
            moltiplicatoreDecelerazione = 0.95f,
            moltiplicatoreVita = 1.35f,
            moltiplicatoreIntervalloAttacco = 1.35f,
            scala = 1.05f,
            moltiplicatoreRinculo = 0.68f,
            monetePerEliminazione = 3
        };
    }

    public static FoxVariantStats CreaScavatrice()
    {
        return new FoxVariantStats
        {
            moltiplicatoreVelocita = 0.88f,
            moltiplicatoreAccelerazione = 1.05f,
            moltiplicatoreDecelerazione = 1.05f,
            moltiplicatoreVita = 1.55f,
            moltiplicatoreIntervalloAttacco = 1.1f,
            scala = 1.1f,
            moltiplicatoreRinculo = 0.5f,
            monetePerEliminazione = 4
        };
    }
}

public readonly struct ComposizioneVolpi
{
    public int Comuni { get; }
    public int Agili { get; }
    public int Robuste { get; }
    public int Schivatrici { get; }
    public int Alfa { get; }
    public int Ululatrici { get; }
    public int Sputafango { get; }
    public int Scavatrici { get; }

    public int Totale =>
        Comuni + Agili + Robuste + Schivatrici + Alfa +
        Ululatrici + Sputafango + Scavatrici;

    public ComposizioneVolpi(
        int comuni,
        int agili,
        int robuste,
        int schivatrici,
        int alfa,
        int ululatrici,
        int sputafango,
        int scavatrici
    )
    {
        Comuni = Mathf.Max(0, comuni);
        Agili = Mathf.Max(0, agili);
        Robuste = Mathf.Max(0, robuste);
        Schivatrici = Mathf.Max(0, schivatrici);
        Alfa = Mathf.Max(0, alfa);
        Ululatrici = Mathf.Max(0, ululatrici);
        Sputafango = Mathf.Max(0, sputafango);
        Scavatrici = Mathf.Max(0, scavatrici);
    }

    public int Ottieni(TipoVolpe tipo)
    {
        switch (FoxVariantStyle.Normalizza(tipo))
        {
            case TipoVolpe.Agile: return Agili;
            case TipoVolpe.Robusta: return Robuste;
            case TipoVolpe.Schivatrice: return Schivatrici;
            case TipoVolpe.Alfa: return Alfa;
            case TipoVolpe.Ululatrice: return Ululatrici;
            case TipoVolpe.Sputafango: return Sputafango;
            case TipoVolpe.Scavatrice: return Scavatrici;
            default: return Comuni;
        }
    }

    public ComposizioneVolpi Aggiungi(TipoVolpe tipo, int quantita = 1)
    {
        return ConDelta(tipo, Mathf.Max(0, quantita));
    }

    public ComposizioneVolpi Rimuovi(TipoVolpe tipo, int quantita = 1)
    {
        return ConDelta(tipo, -Mathf.Max(0, quantita));
    }

    private ComposizioneVolpi ConDelta(TipoVolpe tipo, int delta)
    {
        int comuni = Comuni;
        int agili = Agili;
        int robuste = Robuste;
        int schivatrici = Schivatrici;
        int alfa = Alfa;
        int ululatrici = Ululatrici;
        int sputafango = Sputafango;
        int scavatrici = Scavatrici;

        switch (FoxVariantStyle.Normalizza(tipo))
        {
            case TipoVolpe.Agile: agili += delta; break;
            case TipoVolpe.Robusta: robuste += delta; break;
            case TipoVolpe.Schivatrice: schivatrici += delta; break;
            case TipoVolpe.Alfa: alfa += delta; break;
            case TipoVolpe.Ululatrice: ululatrici += delta; break;
            case TipoVolpe.Sputafango: sputafango += delta; break;
            case TipoVolpe.Scavatrice: scavatrici += delta; break;
            default: comuni += delta; break;
        }

        return new ComposizioneVolpi(
            comuni,
            agili,
            robuste,
            schivatrici,
            alfa,
            ululatrici,
            sputafango,
            scavatrici
        );
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
            sinistra.Schivatrici + destra.Schivatrici,
            sinistra.Alfa + destra.Alfa,
            sinistra.Ululatrici + destra.Ululatrici,
            sinistra.Sputafango + destra.Sputafango,
            sinistra.Scavatrici + destra.Scavatrici
        );
    }

    public string FormattaEstesa(string separatore = " + ")
    {
        StringBuilder testo = new StringBuilder(160);
        AggiungiVoci(testo, separatore, false);
        return testo.Length > 0 ? testo.ToString() : "NESSUNA VOLPE";
    }

    public string FormattaCompatta(string separatore = "  ")
    {
        StringBuilder testo = new StringBuilder(96);
        AggiungiVoci(testo, separatore, true);
        return testo.Length > 0 ? testo.ToString() : "-";
    }

    private void AggiungiVoci(
        StringBuilder testo,
        string separatore,
        bool compatta
    )
    {
        AggiungiVoce(testo, TipoVolpe.Comune, Comuni, separatore, compatta);
        AggiungiVoce(testo, TipoVolpe.Agile, Agili, separatore, compatta);
        AggiungiVoce(testo, TipoVolpe.Robusta, Robuste, separatore, compatta);
        AggiungiVoce(
            testo,
            TipoVolpe.Schivatrice,
            Schivatrici,
            separatore,
            compatta
        );
        AggiungiVoce(testo, TipoVolpe.Alfa, Alfa, separatore, compatta);
        AggiungiVoce(
            testo,
            TipoVolpe.Ululatrice,
            Ululatrici,
            separatore,
            compatta
        );
        AggiungiVoce(
            testo,
            TipoVolpe.Sputafango,
            Sputafango,
            separatore,
            compatta
        );
        AggiungiVoce(
            testo,
            TipoVolpe.Scavatrice,
            Scavatrici,
            separatore,
            compatta
        );
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
    public const int NumeroTipi = 8;

    public static TipoVolpe Normalizza(TipoVolpe tipo)
    {
        int valore = (int)tipo;
        return valore >= (int)TipoVolpe.Comune &&
               valore <= (int)TipoVolpe.Scavatrice
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
            case TipoVolpe.Schivatrice:
                return plurale ? "SCHIVATRICI" : "SCHIVATRICE";
            case TipoVolpe.Alfa: return "ALFA";
            case TipoVolpe.Ululatrice:
                return plurale ? "ULULATRICI" : "ULULATRICE";
            case TipoVolpe.Sputafango: return "SPUTAFANGO";
            case TipoVolpe.Scavatrice:
                return plurale ? "SCAVATRICI" : "SCAVATRICE";
            default: return plurale ? "COMUNI" : "COMUNE";
        }
    }

    public static string Abbreviazione(TipoVolpe tipo)
    {
        switch (Normalizza(tipo))
        {
            case TipoVolpe.Agile: return "AG";
            case TipoVolpe.Robusta: return "R";
            case TipoVolpe.Schivatrice: return "SV";
            case TipoVolpe.Alfa: return "AL";
            case TipoVolpe.Ululatrice: return "U";
            case TipoVolpe.Sputafango: return "SF";
            case TipoVolpe.Scavatrice: return "SC";
            default: return "C";
        }
    }

    public static int Priorita(TipoVolpe tipo)
    {
        switch (Normalizza(tipo))
        {
            case TipoVolpe.Alfa: return 8;
            case TipoVolpe.Sputafango: return 7;
            case TipoVolpe.Ululatrice: return 6;
            case TipoVolpe.Scavatrice: return 5;
            case TipoVolpe.Schivatrice: return 4;
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
            case TipoVolpe.Schivatrice: return new Color32(180, 105, 222, 255);
            case TipoVolpe.Alfa: return new Color32(224, 62, 58, 255);
            case TipoVolpe.Ululatrice: return new Color32(91, 120, 230, 255);
            case TipoVolpe.Sputafango: return new Color32(118, 155, 63, 255);
            case TipoVolpe.Scavatrice: return new Color32(210, 154, 69, 255);
            default: return new Color32(237, 132, 48, 255);
        }
    }

    public static Color ColoreCorpo(TipoVolpe tipo)
    {
        return Color.white;
    }
}

public sealed class FoxVariantPresentation : MonoBehaviour
{
    private static Sprite telegraphAlfaSprite;
    private static Sprite telegraphAbilitaSprite;

    private TipoVolpe tipo;
    private SpriteRenderer corpo;
    private Transform grafica;
    private SpriteRenderer telegraphAlfaRenderer;
    private SpriteRenderer telegraphAbilitaRenderer;
    private bool telegraphAlfa;
    private bool telegraphAbilita;

    public TipoVolpe Tipo => tipo;
    public bool TelegraphAlfaVisibile =>
        telegraphAlfaRenderer != null && telegraphAlfaRenderer.enabled;
    public bool TelegraphAbilitaVisibile =>
        telegraphAbilitaRenderer != null && telegraphAbilitaRenderer.enabled;
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
        RipristinaCorpo();
        CreaTelegraphAlfaSeNecessario();

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
        CreaTelegraphAlfaSeNecessario();
        if (telegraphAlfaRenderer == null) return;

        telegraphAlfaRenderer.enabled = telegraphAlfa;
        if (!telegraphAlfa) return;

        float t = Mathf.Clamp01(progresso);
        float impulso = 0.5f + 0.5f * Mathf.Sin(t * Mathf.PI * 6f);
        telegraphAlfaRenderer.transform.localScale = new Vector3(
            Mathf.Lerp(0.72f, 1.18f, t),
            Mathf.Lerp(0.56f, 0.92f, t),
            1f
        );
        Color colore = FoxVariantStyle.ColoreUi(TipoVolpe.Alfa);
        colore.a = Mathf.Lerp(0.34f, 0.82f, impulso);
        telegraphAlfaRenderer.color = colore;
    }

    public void ImpostaTelegraphAbilita(bool attivo, float progresso)
    {
        telegraphAbilita = attivo && tipo != TipoVolpe.Comune;
        CreaTelegraphAbilitaSeNecessario();
        if (telegraphAbilitaRenderer == null) return;

        telegraphAbilitaRenderer.enabled = telegraphAbilita;
        if (!telegraphAbilita) return;

        float t = Mathf.Clamp01(progresso);
        float impulso = 0.5f + 0.5f * Mathf.Sin(t * Mathf.PI * 8f);
        float scala = Mathf.Lerp(0.72f, 1.12f, t);
        telegraphAbilitaRenderer.transform.localScale =
            new Vector3(scala, scala * 0.58f, 1f);
        Color colore = FoxVariantStyle.ColoreUi(tipo);
        colore.a = Mathf.Lerp(0.3f, 0.76f, impulso);
        telegraphAbilitaRenderer.color = colore;
    }

    public void ImpostaTrasparenzaCorpo(float opacita)
    {
        if (corpo == null) return;
        corpo.color = new Color(1f, 1f, 1f, Mathf.Clamp01(opacita));
    }

    public void RipristinaCorpo()
    {
        if (corpo != null) corpo.color = Color.white;
        if (grafica != null)
        {
            grafica.localRotation = Quaternion.identity;
        }
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

    public void RiproduciAbilita()
    {
        FoxVariantAudioController audio =
            FoxVariantAudioController.CreaOTrova();
        if (audio != null) audio.RiproduciAbilita(tipo, transform.position);
    }

    public void NascondiPerMorte()
    {
        telegraphAlfa = false;
        telegraphAbilita = false;
        if (telegraphAlfaRenderer != null)
        {
            telegraphAlfaRenderer.enabled = false;
        }
        if (telegraphAbilitaRenderer != null)
        {
            telegraphAbilitaRenderer.enabled = false;
        }
        RipristinaCorpo();
    }

    private void CreaTelegraphAlfaSeNecessario()
    {
        if (tipo != TipoVolpe.Alfa)
        {
            if (telegraphAlfaRenderer != null)
            {
                telegraphAlfaRenderer.enabled = false;
            }
            return;
        }
        if (telegraphAlfaRenderer != null) return;

        GameObject oggetto = new GameObject("TelegraphScattoAlfa");
        oggetto.layer = gameObject.layer;
        oggetto.transform.SetParent(transform, false);
        telegraphAlfaRenderer = oggetto.AddComponent<SpriteRenderer>();
        telegraphAlfaRenderer.sprite = OttieniTelegraphAlfa();
        ConfiguraOrdineTelegraph(telegraphAlfaRenderer);
        telegraphAlfaRenderer.transform.localPosition =
            new Vector3(0f, -0.24f, 0f);
        telegraphAlfaRenderer.enabled = telegraphAlfa;
    }

    private void CreaTelegraphAbilitaSeNecessario()
    {
        if (tipo == TipoVolpe.Comune || telegraphAbilitaRenderer != null)
        {
            return;
        }

        GameObject oggetto = new GameObject("TelegraphAbilita_" + tipo);
        oggetto.layer = gameObject.layer;
        oggetto.transform.SetParent(transform, false);
        telegraphAbilitaRenderer = oggetto.AddComponent<SpriteRenderer>();
        telegraphAbilitaRenderer.sprite = OttieniTelegraphAbilita();
        ConfiguraOrdineTelegraph(telegraphAbilitaRenderer);
        telegraphAbilitaRenderer.transform.localPosition =
            new Vector3(0f, -0.2f, 0f);
        telegraphAbilitaRenderer.enabled = telegraphAbilita;
    }

    private void ConfiguraOrdineTelegraph(SpriteRenderer renderer)
    {
        renderer.sortingLayerID = corpo != null ? corpo.sortingLayerID : 0;
        renderer.sortingOrder = corpo != null ? corpo.sortingOrder - 2 : -2;
    }

    private static Sprite OttieniTelegraphAlfa()
    {
        if (telegraphAlfaSprite != null) return telegraphAlfaSprite;
        const int larghezza = 24;
        const int altezza = 12;
        Color32[] pixel = NuovaTela(larghezza, altezza);
        Color32 bianco = new Color32(255, 255, 255, 255);
        for (int x = 4; x <= 19; x++)
        {
            int y = x < 7 || x > 16 ? 3 : 2;
            Imposta(pixel, larghezza, altezza, x, y, bianco);
            Imposta(pixel, larghezza, altezza, x, altezza - 1 - y, bianco);
        }
        for (int y = 4; y <= 7; y++)
        {
            Imposta(pixel, larghezza, altezza, 2, y, bianco);
            Imposta(pixel, larghezza, altezza, 21, y, bianco);
        }
        telegraphAlfaSprite = CreaSprite(
            "TelegraphVolpeAlfa",
            pixel,
            larghezza,
            altezza,
            16f
        );
        return telegraphAlfaSprite;
    }

    private static Sprite OttieniTelegraphAbilita()
    {
        if (telegraphAbilitaSprite != null) return telegraphAbilitaSprite;
        const int larghezza = 24;
        const int altezza = 14;
        Color32[] pixel = NuovaTela(larghezza, altezza);
        Color32 bianco = new Color32(255, 255, 255, 255);
        for (int x = 3; x <= 20; x++)
        {
            if ((x & 1) != 0) continue;
            int y = x < 7 || x > 16 ? 4 : 2;
            Imposta(pixel, larghezza, altezza, x, y, bianco);
            Imposta(pixel, larghezza, altezza, x, altezza - 1 - y, bianco);
        }
        for (int y = 5; y <= 8; y += 2)
        {
            Imposta(pixel, larghezza, altezza, 2, y, bianco);
            Imposta(pixel, larghezza, altezza, 21, y, bianco);
        }
        telegraphAbilitaSprite = CreaSprite(
            "TelegraphAbilitaVolpe",
            pixel,
            larghezza,
            altezza,
            16f
        );
        return telegraphAbilitaSprite;
    }

    private static Color32[] NuovaTela(int larghezza, int altezza)
    {
        return new Color32[larghezza * altezza];
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
    private const int NumeroSorgenti = 6;
    private static FoxVariantAudioController instance;

    private AudioSource[] sorgenti;
    private AudioClip[] versiSpawn;
    private AudioClip[] versiAbilita;
    private AudioClip clipCaricaAlfa;
    private AudioClip clipScattoAlfa;
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
        TipoVolpe normalizzato = FoxVariantStyle.Normalizza(tipo);
        int indice = (int)normalizzato;
        float volumeTipo = normalizzato == TipoVolpe.Alfa ? 1f : 0.72f;
        Riproduci(versiSpawn[indice], posizione, volumeTipo);
    }

    public void RiproduciCaricaAlfa(Vector2 posizione)
    {
        FarmAudioController.RiproduciPericolo(0.9f);
        Riproduci(clipCaricaAlfa, posizione, 1f);
    }

    public void RiproduciScattoAlfa(Vector2 posizione)
    {
        Riproduci(clipScattoAlfa, posizione, 0.92f);
    }

    public void RiproduciAbilita(TipoVolpe tipo, Vector2 posizione)
    {
        TipoVolpe normalizzato = FoxVariantStyle.Normalizza(tipo);
        if (normalizzato == TipoVolpe.Alfa ||
            normalizzato == TipoVolpe.Ululatrice ||
            normalizzato == TipoVolpe.Sputafango)
        {
            FarmAudioController.RiproduciPericolo(0.68f);
        }
        Riproduci(versiAbilita[(int)normalizzato], posizione, 0.88f);
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

        FoxVariantsBalanceSettings varianti =
            GameBalanceConfig.Corrente.VariantiVolpe;
        AudioSource sorgente = OttieniSorgente();
        sorgente.panStereo = CalcolaPan(posizione);
        float volumeEffetti = GameOptionsController.Instance != null
            ? GameOptionsController.Instance.VolumeEffetti
            : 1f;
        sorgente.volume = Mathf.Clamp01(
            varianti.volumeVersi * intensita * volumeEffetti
        );
        sorgente.pitch = 1f;
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
        versiSpawn = new AudioClip[FoxVariantStyle.NumeroTipi];
        versiSpawn[0] = GeneraVerso("VolpeComune_Yip", 0.14f, 410f, 535f, 0);
        versiSpawn[1] = GeneraVerso("VolpeAgile_DoppioFischio", 0.2f, 720f, 1040f, 1);
        versiSpawn[2] = GeneraVerso("VolpeRobusta_Bronto", 0.27f, 205f, 132f, 2);
        versiSpawn[3] = GeneraVerso("VolpeSchivatrice_Sibilo", 0.17f, 620f, 900f, 3);
        versiSpawn[4] = GeneraVerso("VolpeAlfa_Ululo", 0.42f, 155f, 96f, 4);
        versiSpawn[5] = GeneraVerso("VolpeUlulatrice_Richiamo", 0.48f, 210f, 112f, 5);
        versiSpawn[6] = GeneraVerso("VolpeSputafango_Gorgoglio", 0.25f, 285f, 160f, 6);
        versiSpawn[7] = GeneraVerso("VolpeScavatrice_Rombo", 0.28f, 120f, 205f, 7);

        versiAbilita = new AudioClip[FoxVariantStyle.NumeroTipi];
        versiAbilita[0] = GeneraVerso("VolpeComune_Attacco", 0.12f, 480f, 390f, 0);
        versiAbilita[1] = GeneraVerso("VolpeAgile_Scatto", 0.13f, 850f, 1240f, 1);
        versiAbilita[2] = GeneraVerso("VolpeRobusta_Impatto", 0.2f, 170f, 105f, 2);
        versiAbilita[3] = GeneraVerso("VolpeSchivatrice_Schivata", 0.14f, 980f, 510f, 3);
        versiAbilita[4] = GeneraVerso("VolpeAlfa_Minaccia", 0.32f, 145f, 88f, 4);
        versiAbilita[5] = GeneraVerso("VolpeUlulatrice_Ululato", 0.62f, 245f, 82f, 5);
        versiAbilita[6] = GeneraVerso("VolpeSputafango_Sputo", 0.22f, 330f, 118f, 6);
        versiAbilita[7] = GeneraVerso("VolpeScavatrice_Scavo", 0.3f, 95f, 230f, 7);

        clipCaricaAlfa = GeneraVerso(
            "VolpeAlfa_Carica",
            0.48f,
            118f,
            174f,
            5
        );
        clipScattoAlfa = GeneraVerso(
            "VolpeAlfa_Scatto",
            0.18f,
            260f,
            610f,
            6
        );
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
            if (stile == 1 || stile == 3)
            {
                float finestra = t < 0.44f
                    ? Mathf.Sin(Mathf.Clamp01(t / 0.44f) * Mathf.PI)
                    : Mathf.Sin(
                        Mathf.Clamp01((t - 0.5f) / 0.5f) * Mathf.PI
                    );
                inviluppo *= Mathf.Max(0f, finestra);
            }
            float armonica =
                Mathf.Sin(fase) + 0.24f * Mathf.Sin(fase * 2.03f);
            float grana = Mathf.Sin(i * (0.71f + stile * 0.053f)) *
                          Mathf.Sin(i * 0.037f);
            float miscelaGrana = stile == 2 || stile >= 4
                ? 0.18f
                : stile == 3 ? 0.1f : 0.035f;
            dati[i] =
                (armonica * 0.36f + grana * miscelaGrana) * inviluppo;
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

    private static void DistruggiClip(AudioClip[] clip)
    {
        if (clip == null) return;
        for (int i = 0; i < clip.Length; i++)
        {
            if (clip[i] != null) Destroy(clip[i]);
        }
    }

    void OnDestroy()
    {
        if (instance == this) instance = null;
        DistruggiClip(versiSpawn);
        DistruggiClip(versiAbilita);
        if (clipCaricaAlfa != null) Destroy(clipCaricaAlfa);
        if (clipScattoAlfa != null) Destroy(clipScattoAlfa);
    }
}
