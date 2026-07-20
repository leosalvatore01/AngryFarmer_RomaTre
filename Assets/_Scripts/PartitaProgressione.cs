using System;
using UnityEngine;

public enum DifficoltaPartita
{
    Tranquilla = 0,
    Normale = 1,
    Difficile = 2
}

[Serializable]
public sealed class ProfiloDifficolta
{
    public string nomeVisualizzato = "NORMALE";
    [TextArea] public string descrizione = "Esperienza consigliata.";
    [Range(0.5f, 2f)] public float moltiplicatoreVita = 1f;
    [Range(0.5f, 1.5f)] public float moltiplicatoreQuantita = 1f;
    [Range(0.5f, 1.5f)] public float moltiplicatoreIntervalli = 1f;
    [Range(0.5f, 1.5f)] public float moltiplicatoreVelocita = 1f;
    [Range(0.5f, 2f)] public float moltiplicatorePunteggio = 1f;

    public string Nome => string.IsNullOrWhiteSpace(nomeVisualizzato)
        ? "NORMALE"
        : nomeVisualizzato.Trim().ToUpperInvariant();

    public int ApplicaQuantita(int quantitaBase)
    {
        if (quantitaBase <= 0) return 0;
        return Mathf.Max(
            1,
            Mathf.RoundToInt(quantitaBase * moltiplicatoreQuantita)
        );
    }

    public int ApplicaVita(int vitaBase)
    {
        return Mathf.Max(
            1,
            Mathf.RoundToInt(Mathf.Max(1, vitaBase) * moltiplicatoreVita)
        );
    }

    public float ApplicaIntervallo(float intervalloBase)
    {
        return Mathf.Max(
            0.05f,
            intervalloBase * moltiplicatoreIntervalli
        );
    }

    public void Normalizza()
    {
        moltiplicatoreVita = Mathf.Clamp(moltiplicatoreVita, 0.5f, 2f);
        moltiplicatoreQuantita = Mathf.Clamp(
            moltiplicatoreQuantita,
            0.5f,
            1.5f
        );
        moltiplicatoreIntervalli = Mathf.Clamp(
            moltiplicatoreIntervalli,
            0.5f,
            1.5f
        );
        moltiplicatoreVelocita = Mathf.Clamp(
            moltiplicatoreVelocita,
            0.5f,
            1.5f
        );
        moltiplicatorePunteggio = Mathf.Clamp(
            moltiplicatorePunteggio,
            0.5f,
            2f
        );
    }
}

[Serializable]
public sealed class BilanciamentoDifficolta
{
    public ProfiloDifficolta tranquilla = new ProfiloDifficolta
    {
        nomeVisualizzato = "TRANQUILLA",
        descrizione = "Volpi piu lente, meno resistenti e piu distanziate.",
        moltiplicatoreVita = 0.85f,
        moltiplicatoreQuantita = 1f,
        moltiplicatoreIntervalli = 1.15f,
        moltiplicatoreVelocita = 0.9f,
        moltiplicatorePunteggio = 0.85f
    };

    public ProfiloDifficolta normale = new ProfiloDifficolta
    {
        nomeVisualizzato = "NORMALE",
        descrizione = "La difficolta consigliata per la prima partita.",
        moltiplicatoreVita = 1f,
        moltiplicatoreQuantita = 1f,
        moltiplicatoreIntervalli = 1f,
        moltiplicatoreVelocita = 1f,
        moltiplicatorePunteggio = 1f
    };

    public ProfiloDifficolta difficile = new ProfiloDifficolta
    {
        nomeVisualizzato = "DIFFICILE",
        descrizione = "Volpi piu rapide, resistenti e meno distanziate.",
        moltiplicatoreVita = 1.2f,
        moltiplicatoreQuantita = 1f,
        moltiplicatoreIntervalli = 0.88f,
        moltiplicatoreVelocita = 1.08f,
        moltiplicatorePunteggio = 1.2f
    };

    public ProfiloDifficolta Ottieni(DifficoltaPartita difficolta)
    {
        switch (difficolta)
        {
            case DifficoltaPartita.Tranquilla:
                return tranquilla ?? (tranquilla = new ProfiloDifficolta());
            case DifficoltaPartita.Difficile:
                return difficile ?? (difficile = new ProfiloDifficolta());
            default:
                return normale ?? (normale = new ProfiloDifficolta());
        }
    }

    public void Normalizza()
    {
        Ottieni(DifficoltaPartita.Tranquilla).Normalizza();
        Ottieni(DifficoltaPartita.Normale).Normalizza();
        Ottieni(DifficoltaPartita.Difficile).Normalizza();
    }
}

public readonly struct EsitoRecordPartita
{
    public int MigliorPunteggio { get; }
    public int MassimoVolpi { get; }
    public int MigliorePercentualeGalline { get; }
    public float MigliorTempoVittoria { get; }
    public bool NuovoPunteggio { get; }
    public bool NuovoRecordVolpi { get; }
    public bool NuovoRecordGalline { get; }
    public bool NuovoRecordTempo { get; }
    public bool HaNuovoRecord =>
        NuovoPunteggio ||
        NuovoRecordVolpi ||
        NuovoRecordGalline ||
        NuovoRecordTempo;

    public EsitoRecordPartita(
        int migliorPunteggio,
        int massimoVolpi,
        int migliorePercentualeGalline,
        float migliorTempoVittoria,
        bool nuovoPunteggio,
        bool nuovoRecordVolpi,
        bool nuovoRecordGalline,
        bool nuovoRecordTempo
    )
    {
        MigliorPunteggio = migliorPunteggio;
        MassimoVolpi = massimoVolpi;
        MigliorePercentualeGalline = migliorePercentualeGalline;
        MigliorTempoVittoria = migliorTempoVittoria;
        NuovoPunteggio = nuovoPunteggio;
        NuovoRecordVolpi = nuovoRecordVolpi;
        NuovoRecordGalline = nuovoRecordGalline;
        NuovoRecordTempo = nuovoRecordTempo;
    }
}

public static class ProgressionePartita
{
    private const string Prefisso = "AngryFarmer.Blocco8";
    private const string ChiaveDifficolta = Prefisso + ".Difficolta";

    private static bool inizializzata;
    private static DifficoltaPartita difficoltaCorrente;
    private static bool saltaSelezioneAlProssimoCaricamento;

    public static DifficoltaPartita DifficoltaCorrente
    {
        get
        {
            InizializzaSeNecessario();
            return difficoltaCorrente;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void AzzeraSessione()
    {
        inizializzata = false;
        difficoltaCorrente = DifficoltaPartita.Normale;
        saltaSelezioneAlProssimoCaricamento = false;
    }

    public static void ImpostaDifficolta(DifficoltaPartita difficolta)
    {
        InizializzaSeNecessario();
        difficoltaCorrente = Normalizza(difficolta);
        PlayerPrefs.SetInt(ChiaveDifficolta, (int)difficoltaCorrente);
        PlayerPrefs.Save();
    }

    public static void PreparaRiavvioImmediato()
    {
        InizializzaSeNecessario();
        saltaSelezioneAlProssimoCaricamento = true;
    }

    public static void PreparaCambioDifficolta()
    {
        saltaSelezioneAlProssimoCaricamento = false;
    }

    public static bool ConsumaRiavvioImmediato()
    {
        InizializzaSeNecessario();
        bool salta = saltaSelezioneAlProssimoCaricamento;
        saltaSelezioneAlProssimoCaricamento = false;
        return salta;
    }

    public static int CalcolaPunteggio(
        bool vittoria,
        int volpiEliminate,
        int ondateCompletate,
        int moneteRaccolte,
        int gallineSalve,
        int uovaSalvate,
        int obiettiviCompletati,
        float precisione,
        float moltiplicatoreDifficolta
    )
    {
        int basePunteggio =
            Mathf.Max(0, volpiEliminate) * 100 +
            Mathf.Max(0, ondateCompletate) * 250 +
            Mathf.Max(0, moneteRaccolte) * 10 +
            Mathf.Max(0, gallineSalve) * 150 +
            Mathf.Max(0, uovaSalvate) * 80 +
            Mathf.Max(0, obiettiviCompletati) * 200 +
            Mathf.RoundToInt(Mathf.Clamp01(precisione) * 200f) +
            (vittoria ? 1000 : 0);
        return Mathf.Max(
            0,
            Mathf.RoundToInt(
                basePunteggio * Mathf.Max(0.1f, moltiplicatoreDifficolta)
            )
        );
    }

    public static EsitoRecordPartita SalvaRecord(
        DifficoltaPartita difficolta,
        bool vittoria,
        int punteggio,
        float durata,
        int volpiEliminate,
        int gallineSalve,
        int gallineTotali
    )
    {
        string prefisso = PrefissoRecord(difficolta);
        int percentualeGalline = gallineTotali > 0
            ? Mathf.RoundToInt(
                Mathf.Clamp01(gallineSalve / (float)gallineTotali) * 100f
            )
            : 0;

        int vecchioPunteggio = PlayerPrefs.GetInt(prefisso + ".Punti", 0);
        int vecchieVolpi = PlayerPrefs.GetInt(prefisso + ".Volpi", 0);
        int vecchieGalline = PlayerPrefs.GetInt(prefisso + ".Galline", 0);
        float vecchioTempo = PlayerPrefs.GetFloat(prefisso + ".Tempo", 0f);

        bool nuovoPunteggio = punteggio > vecchioPunteggio;
        bool nuovoRecordVolpi = volpiEliminate > vecchieVolpi;
        bool nuovoRecordGalline = percentualeGalline > vecchieGalline;
        bool nuovoRecordTempo =
            vittoria &&
            durata > 0f &&
            (vecchioTempo <= 0f || durata < vecchioTempo);

        int migliorPunteggio = Mathf.Max(vecchioPunteggio, punteggio);
        int massimoVolpi = Mathf.Max(vecchieVolpi, volpiEliminate);
        int miglioriGalline = Mathf.Max(vecchieGalline, percentualeGalline);
        float migliorTempo = nuovoRecordTempo ? durata : vecchioTempo;

        if (nuovoPunteggio)
            PlayerPrefs.SetInt(prefisso + ".Punti", migliorPunteggio);
        if (nuovoRecordVolpi)
            PlayerPrefs.SetInt(prefisso + ".Volpi", massimoVolpi);
        if (nuovoRecordGalline)
            PlayerPrefs.SetInt(prefisso + ".Galline", miglioriGalline);
        if (nuovoRecordTempo)
            PlayerPrefs.SetFloat(prefisso + ".Tempo", migliorTempo);
        if (nuovoPunteggio || nuovoRecordVolpi ||
            nuovoRecordGalline || nuovoRecordTempo)
        {
            PlayerPrefs.Save();
        }

        return new EsitoRecordPartita(
            migliorPunteggio,
            massimoVolpi,
            miglioriGalline,
            migliorTempo,
            nuovoPunteggio,
            nuovoRecordVolpi,
            nuovoRecordGalline,
            nuovoRecordTempo
        );
    }

    public static string FormattaTempo(float secondi)
    {
        int totale = Mathf.Max(0, Mathf.FloorToInt(secondi));
        int minuti = totale / 60;
        int resto = totale % 60;
        return minuti.ToString("00") + ":" + resto.ToString("00");
    }

    private static void InizializzaSeNecessario()
    {
        if (inizializzata) return;
        difficoltaCorrente = Normalizza(
            (DifficoltaPartita)PlayerPrefs.GetInt(
                ChiaveDifficolta,
                (int)DifficoltaPartita.Normale
            )
        );
        inizializzata = true;
    }

    private static DifficoltaPartita Normalizza(
        DifficoltaPartita difficolta
    )
    {
        return difficolta < DifficoltaPartita.Tranquilla ||
               difficolta > DifficoltaPartita.Difficile
            ? DifficoltaPartita.Normale
            : difficolta;
    }

    private static string PrefissoRecord(DifficoltaPartita difficolta)
    {
        return Prefisso + ".Record." + (int)Normalizza(difficolta);
    }
}
