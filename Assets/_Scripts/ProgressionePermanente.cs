using System;
using System.Collections.Generic;
using UnityEngine;

public enum TipoPotenziamentoPermanente
{
    VitaMassima = 0,
    Danno = 1,
    Cadenza = 2,
    Movimento = 3,
    Resistenza = 4,
    Provviste = 5
}

public sealed class DefinizionePotenziamentoPermanente
{
    public TipoPotenziamentoPermanente Tipo { get; }
    public string Titolo { get; }
    public string Descrizione { get; }
    public FarmPixelIcon Icona { get; }
    public int CostoBase { get; }
    public float EsponenteCosto { get; }

    public DefinizionePotenziamentoPermanente(
        TipoPotenziamentoPermanente tipo,
        string titolo,
        string descrizione,
        FarmPixelIcon icona,
        int costoBase,
        float esponenteCosto
    )
    {
        Tipo = tipo;
        Titolo = titolo ?? string.Empty;
        Descrizione = descrizione ?? string.Empty;
        Icona = icona;
        CostoBase = Mathf.Max(1, costoBase);
        EsponenteCosto = Mathf.Max(1f, esponenteCosto);
    }

    public int CalcolaCosto(int livelloAttuale)
    {
        int livelloValido = Mathf.Max(0, livelloAttuale);
        double fattore = Math.Pow(
            (double)livelloValido + 1d,
            EsponenteCosto
        );
        double costo = Math.Ceiling(CostoBase * fattore);

        if (double.IsNaN(costo) || costo <= 1d)
        {
            return 1;
        }

        return costo >= ProgressionePermanente.CostoMassimo
            ? ProgressionePermanente.CostoMassimo
            : (int)costo;
    }
}

/// <summary>
/// Economia persistente separata dalle monete della singola partita.
/// Ogni mutazione viene salvata subito per non perdere i progressi.
/// </summary>
public static class ProgressionePermanente
{
    public const int CostoMassimo = 1000000000;

    private const string Prefisso = "AngryFarmer.Meta.v1";
    private const string ChiaveSaldo = Prefisso + ".Gettoni.Saldo";
    private const string ChiaveTotaleGuadagnato =
        Prefisso + ".Gettoni.Guadagnati";
    private const string ChiaveTotaleSpeso =
        Prefisso + ".Gettoni.Spesi";
    private const string PrefissoLivello = Prefisso + ".Livello.";

    private const float LimiteRiduzioneCadenza = 0.18f;
    private const float CurvaCadenza = 7f;
    private const float LimiteVelocitaMovimento = 2.25f;
    private const float CurvaMovimento = 8f;
    private const float LimiteProbabilitaBlocco = 0.30f;
    private const float CurvaResistenza = 9f;

    private static readonly DefinizionePotenziamentoPermanente[] catalogo =
    {
        new DefinizionePotenziamentoPermanente(
            TipoPotenziamentoPermanente.VitaMassima,
            "Cuore temprato",
            "Aumenta la vita massima in ogni partita.",
            FarmPixelIcon.SaluteMassima,
            6,
            1.32f
        ),
        new DefinizionePotenziamentoPermanente(
            TipoPotenziamentoPermanente.Danno,
            "Patate rinforzate",
            "Aumenta il danno di ogni colpo.",
            FarmPixelIcon.Danno,
            10,
            1.40f
        ),
        new DefinizionePotenziamentoPermanente(
            TipoPotenziamentoPermanente.Cadenza,
            "Mani veloci",
            "Riduce in modo permanente il tempo tra i colpi.",
            FarmPixelIcon.Cadenza,
            7,
            1.36f
        ),
        new DefinizionePotenziamentoPermanente(
            TipoPotenziamentoPermanente.Movimento,
            "Passo del raccolto",
            "Aumenta la velocita base del contadino.",
            FarmPixelIcon.Movimento,
            4,
            1.30f
        ),
        new DefinizionePotenziamentoPermanente(
            TipoPotenziamentoPermanente.Resistenza,
            "Corazza da lavoro",
            "Aumenta la probabilita di bloccare un colpo.",
            FarmPixelIcon.Resistenza,
            8,
            1.38f
        ),
        new DefinizionePotenziamentoPermanente(
            TipoPotenziamentoPermanente.Provviste,
            "Scorte di partenza",
            "Aggiunge monete alla partenza di ogni partita.",
            FarmPixelIcon.Moneta,
            5,
            1.34f
        )
    };

    private static bool inizializzata;
    private static int saldoGettoni;
    private static int totaleGettoniGuadagnati;
    private static int totaleGettoniSpesi;
    private static int[] livelli;

    public static event Action StatoCambiato;
    public static event Action<int, int> GettoniAggiunti;
    public static event Action<TipoPotenziamentoPermanente, int>
        PotenziamentoAcquistato;

    public static IReadOnlyList<DefinizionePotenziamentoPermanente>
        Catalogo => catalogo;

    public static int SaldoGettoni
    {
        get
        {
            InizializzaSeNecessario();
            return saldoGettoni;
        }
    }

    public static int TotaleGettoniGuadagnati
    {
        get
        {
            InizializzaSeNecessario();
            return totaleGettoniGuadagnati;
        }
    }

    public static int TotaleGettoniSpesi
    {
        get
        {
            InizializzaSeNecessario();
            return totaleGettoniSpesi;
        }
    }

    public static int TotaleLivelliAcquistati
    {
        get
        {
            InizializzaSeNecessario();
            long totale = 0L;
            for (int i = 0; i < livelli.Length; i++)
            {
                totale += livelli[i];
                if (totale >= int.MaxValue)
                {
                    return int.MaxValue;
                }
            }

            return (int)totale;
        }
    }

    public static int BonusVitaMassima =>
        CalcolaBonusVitaMassima(
            OttieniLivello(TipoPotenziamentoPermanente.VitaMassima)
        );

    public static int BonusDanno =>
        CalcolaBonusDanno(
            OttieniLivello(TipoPotenziamentoPermanente.Danno)
        );

    public static float BonusRiduzioneIntervalloSparo =>
        CalcolaBonusRiduzioneIntervalloSparo(
            OttieniLivello(TipoPotenziamentoPermanente.Cadenza)
        );

    public static float BonusVelocitaMovimento =>
        CalcolaBonusVelocitaMovimento(
            OttieniLivello(TipoPotenziamentoPermanente.Movimento)
        );

    public static float BonusProbabilitaBlocco =>
        CalcolaBonusProbabilitaBlocco(
            OttieniLivello(TipoPotenziamentoPermanente.Resistenza)
        );

    public static int BonusMoneteIniziali =>
        CalcolaBonusMoneteIniziali(
            OttieniLivello(TipoPotenziamentoPermanente.Provviste)
        );

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void AzzeraCacheSessione()
    {
        inizializzata = false;
        saldoGettoni = 0;
        totaleGettoniGuadagnati = 0;
        totaleGettoniSpesi = 0;
        livelli = null;
        StatoCambiato = null;
        GettoniAggiunti = null;
        PotenziamentoAcquistato = null;
    }

    public static DefinizionePotenziamentoPermanente OttieniDefinizione(
        TipoPotenziamentoPermanente tipo
    )
    {
        int indice = (int)tipo;
        if (indice < 0 || indice >= catalogo.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(tipo));
        }

        return catalogo[indice];
    }

    public static int OttieniLivello(TipoPotenziamentoPermanente tipo)
    {
        InizializzaSeNecessario();
        int indice = (int)tipo;
        return indice >= 0 && indice < livelli.Length
            ? livelli[indice]
            : 0;
    }

    public static int OttieniCosto(TipoPotenziamentoPermanente tipo)
    {
        DefinizionePotenziamentoPermanente definizione =
            OttieniDefinizione(tipo);
        return definizione.CalcolaCosto(OttieniLivello(tipo));
    }

    public static bool PuoAcquistare(TipoPotenziamentoPermanente tipo)
    {
        if (!TipoValido(tipo))
        {
            return false;
        }

        InizializzaSeNecessario();
        int livello = livelli[(int)tipo];
        return livello < int.MaxValue &&
               saldoGettoni >= catalogo[(int)tipo].CalcolaCosto(livello);
    }

    public static bool ProvaAcquistare(
        TipoPotenziamentoPermanente tipo,
        out string messaggio
    )
    {
        if (!TipoValido(tipo))
        {
            messaggio = "Potenziamento non valido.";
            return false;
        }

        InizializzaSeNecessario();
        int indice = (int)tipo;
        int livelloAttuale = livelli[indice];
        if (livelloAttuale >= int.MaxValue)
        {
            messaggio = "Il livello non puo essere rappresentato.";
            return false;
        }

        DefinizionePotenziamentoPermanente definizione = catalogo[indice];
        int costo = definizione.CalcolaCosto(livelloAttuale);
        if (saldoGettoni < costo)
        {
            int mancanti = costo - saldoGettoni;
            messaggio = "Servono ancora " + mancanti +
                        " gettoni permanenti.";
            return false;
        }

        int nuovoSaldo = saldoGettoni - costo;
        int nuovoLivello = livelloAttuale + 1;
        int nuovoTotaleSpeso = SommaSaturata(
            totaleGettoniSpesi,
            costo
        );

        PlayerPrefs.SetInt(ChiaveSaldo, nuovoSaldo);
        PlayerPrefs.SetInt(ChiaveTotaleSpeso, nuovoTotaleSpeso);
        PlayerPrefs.SetInt(ChiaveLivello(tipo), nuovoLivello);
        PlayerPrefs.Save();

        saldoGettoni = nuovoSaldo;
        totaleGettoniSpesi = nuovoTotaleSpeso;
        livelli[indice] = nuovoLivello;

        messaggio = definizione.Titolo + " raggiunge il livello " +
                    nuovoLivello + ".";
        PotenziamentoAcquistato?.Invoke(tipo, nuovoLivello);
        StatoCambiato?.Invoke();
        return true;
    }

    public static int AggiungiGettoni(int quantita)
    {
        if (quantita <= 0)
        {
            return 0;
        }

        InizializzaSeNecessario();
        int nuovoSaldo = SommaSaturata(saldoGettoni, quantita);
        int quantitaAccreditata = nuovoSaldo - saldoGettoni;
        if (quantitaAccreditata <= 0)
        {
            return 0;
        }

        int nuovoTotaleGuadagnato = SommaSaturata(
            totaleGettoniGuadagnati,
            quantitaAccreditata
        );

        PlayerPrefs.SetInt(ChiaveSaldo, nuovoSaldo);
        PlayerPrefs.SetInt(
            ChiaveTotaleGuadagnato,
            nuovoTotaleGuadagnato
        );
        PlayerPrefs.Save();

        saldoGettoni = nuovoSaldo;
        totaleGettoniGuadagnati = nuovoTotaleGuadagnato;

        GettoniAggiunti?.Invoke(quantitaAccreditata, nuovoSaldo);
        StatoCambiato?.Invoke();
        return quantitaAccreditata;
    }

    public static int CalcolaRicompensaOndata(int indiceOnda)
    {
        return indiceOnda <= 0 ? 0 : 1 + (indiceOnda - 1) / 5;
    }

    public static int AssegnaRicompensaOndata(int indiceOnda)
    {
        return AggiungiGettoni(CalcolaRicompensaOndata(indiceOnda));
    }

    public static int CalcolaBonusVitaMassima(int livello)
    {
        return Mathf.Max(0, livello);
    }

    public static int CalcolaBonusDanno(int livello)
    {
        return Mathf.Max(0, livello);
    }

    public static float CalcolaBonusRiduzioneIntervalloSparo(int livello)
    {
        return CalcolaBonusAsintotico(
            livello,
            LimiteRiduzioneCadenza,
            CurvaCadenza
        );
    }

    public static float CalcolaBonusVelocitaMovimento(int livello)
    {
        return CalcolaBonusAsintotico(
            livello,
            LimiteVelocitaMovimento,
            CurvaMovimento
        );
    }

    public static float CalcolaBonusProbabilitaBlocco(int livello)
    {
        return CalcolaBonusAsintotico(
            livello,
            LimiteProbabilitaBlocco,
            CurvaResistenza
        );
    }

    public static int CalcolaBonusMoneteIniziali(int livello)
    {
        return Mathf.Max(0, livello);
    }

    public static string DescriviBonus(
        TipoPotenziamentoPermanente tipo,
        int livello
    )
    {
        int livelloValido = Mathf.Max(0, livello);
        switch (tipo)
        {
            case TipoPotenziamentoPermanente.VitaMassima:
                return "+" + CalcolaBonusVitaMassima(livelloValido) +
                       " vita massima";
            case TipoPotenziamentoPermanente.Danno:
                return "+" + CalcolaBonusDanno(livelloValido) +
                       " danno";
            case TipoPotenziamentoPermanente.Cadenza:
                return "-" +
                       CalcolaRiduzioneIntervalloEffettiva(
                           livelloValido
                       )
                           .ToString("0.000") +
                       " s tra i colpi";
            case TipoPotenziamentoPermanente.Movimento:
                return "+" +
                       CalcolaBonusVelocitaMovimento(livelloValido)
                           .ToString("0.00") +
                       " velocita";
            case TipoPotenziamentoPermanente.Resistenza:
                return (
                    CalcolaBonusProbabilitaBlocco(livelloValido) * 100f
                ).ToString("0.0") + "% blocco";
            case TipoPotenziamentoPermanente.Provviste:
                return "+" +
                       CalcolaBonusMoneteIniziali(livelloValido) +
                       " monete iniziali";
            default:
                return string.Empty;
        }
    }

    public static string DescriviIncrementoProssimo(
        TipoPotenziamentoPermanente tipo
    )
    {
        int livello = OttieniLivello(tipo);
        int prossimoLivello = livello == int.MaxValue
            ? int.MaxValue
            : livello + 1;

        switch (tipo)
        {
            case TipoPotenziamentoPermanente.VitaMassima:
                return "+1 vita massima";
            case TipoPotenziamentoPermanente.Danno:
                return "+1 danno";
            case TipoPotenziamentoPermanente.Cadenza:
                return "-" +
                       (
                           CalcolaRiduzioneIntervalloEffettiva(
                               prossimoLivello
                           ) -
                           CalcolaRiduzioneIntervalloEffettiva(livello)
                       ).ToString("0.000") +
                       " s tra i colpi";
            case TipoPotenziamentoPermanente.Movimento:
                return "+" +
                       (
                           CalcolaBonusVelocitaMovimento(prossimoLivello) -
                           CalcolaBonusVelocitaMovimento(livello)
                       ).ToString("0.00") +
                       " velocita";
            case TipoPotenziamentoPermanente.Resistenza:
                return "+" +
                       (
                           (
                               CalcolaBonusProbabilitaBlocco(
                                   prossimoLivello
                               ) -
                               CalcolaBonusProbabilitaBlocco(livello)
                           ) * 100f
                       ).ToString("0.0") +
                       "% blocco";
            case TipoPotenziamentoPermanente.Provviste:
                return "+1 moneta iniziale";
            default:
                return string.Empty;
        }
    }

    private static void InizializzaSeNecessario()
    {
        if (inizializzata)
        {
            return;
        }

        saldoGettoni = LeggiInteroNonNegativo(ChiaveSaldo);
        totaleGettoniGuadagnati =
            LeggiInteroNonNegativo(ChiaveTotaleGuadagnato);
        totaleGettoniSpesi =
            LeggiInteroNonNegativo(ChiaveTotaleSpeso);
        livelli = new int[catalogo.Length];

        for (int i = 0; i < livelli.Length; i++)
        {
            livelli[i] = LeggiInteroNonNegativo(
                ChiaveLivello((TipoPotenziamentoPermanente)i)
            );
        }

        inizializzata = true;
    }

    private static int LeggiInteroNonNegativo(string chiave)
    {
        return Mathf.Max(0, PlayerPrefs.GetInt(chiave, 0));
    }

    private static string ChiaveLivello(
        TipoPotenziamentoPermanente tipo
    )
    {
        return PrefissoLivello + (int)tipo;
    }

    private static bool TipoValido(TipoPotenziamentoPermanente tipo)
    {
        int indice = (int)tipo;
        return indice >= 0 && indice < catalogo.Length;
    }

    private static int SommaSaturata(int valore, int incremento)
    {
        if (incremento <= 0)
        {
            return Mathf.Max(0, valore);
        }

        long totale = (long)Mathf.Max(0, valore) + incremento;
        return totale >= int.MaxValue ? int.MaxValue : (int)totale;
    }

    private static float CalcolaBonusAsintotico(
        int livello,
        float limite,
        float curva
    )
    {
        int livelloValido = Mathf.Max(0, livello);
        if (livelloValido == 0)
        {
            return 0f;
        }

        double rapporto = livelloValido /
                          ((double)livelloValido + Math.Max(0.01d, curva));
        return (float)(Math.Max(0f, limite) * rapporto);
    }

    private static float CalcolaRiduzioneIntervalloEffettiva(int livello)
    {
        PlayerBalanceSettings configurazione =
            GameBalanceConfig.Corrente.Giocatore;
        float minimo = Mathf.Max(
            0.01f,
            configurazione.intervalloSparoMinimo
        );
        float baseSparo = Mathf.Max(
            minimo,
            configurazione.intervalloSparo
        );
        float spazioBase = Mathf.Max(0.0001f, baseSparo - minimo);
        float intensita =
            CalcolaBonusRiduzioneIntervalloSparo(livello) / spazioBase;
        float intervalloFinale =
            minimo + spazioBase / (1f + Mathf.Max(0f, intensita));
        return Mathf.Max(0f, baseSparo - intervalloFinale);
    }
}
