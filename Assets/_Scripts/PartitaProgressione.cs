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
    public int MassimaOndata { get; }
    public bool NuovoPunteggio { get; }
    public bool NuovoRecordVolpi { get; }
    public bool NuovoRecordGalline { get; }
    public bool NuovoRecordTempo { get; }
    public bool NuovoRecordOndata { get; }
    public bool HaNuovoRecord =>
        NuovoPunteggio ||
        NuovoRecordVolpi ||
        NuovoRecordGalline ||
        NuovoRecordTempo ||
        NuovoRecordOndata;

    public EsitoRecordPartita(
        int migliorPunteggio,
        int massimoVolpi,
        int migliorePercentualeGalline,
        float migliorTempoVittoria,
        int massimaOndata,
        bool nuovoPunteggio,
        bool nuovoRecordVolpi,
        bool nuovoRecordGalline,
        bool nuovoRecordTempo,
        bool nuovoRecordOndata
    )
    {
        MigliorPunteggio = migliorPunteggio;
        MassimoVolpi = massimoVolpi;
        MigliorePercentualeGalline = migliorePercentualeGalline;
        MigliorTempoVittoria = migliorTempoVittoria;
        MassimaOndata = massimaOndata;
        NuovoPunteggio = nuovoPunteggio;
        NuovoRecordVolpi = nuovoRecordVolpi;
        NuovoRecordGalline = nuovoRecordGalline;
        NuovoRecordTempo = nuovoRecordTempo;
        NuovoRecordOndata = nuovoRecordOndata;
    }
}

public static class ProgressionePartita
{
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

    public static int MiglioreOndataAssoluta =>
        Mathf.Max(0, SaveService.Profilo.miglioreOndataAssoluta);

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
        SaveService.ModificaProfilo(dati =>
        {
            dati.difficoltaPreferita = (int)difficoltaCorrente;
        });
    }

    public static void PreparaRiavvioImmediato()
    {
        InizializzaSeNecessario();
        saltaSelezioneAlProssimoCaricamento = true;
    }

    public static void PreparaAvvioDaMenu()
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
        int gallineTotali,
        int ondateCompletate = 0
    )
    {
        difficolta = Normalizza(difficolta);
        DatiRecordDifficolta recordPrecedente =
            OttieniDatiRecord(difficolta);
        int percentualeGalline = gallineTotali > 0
            ? Mathf.RoundToInt(
                Mathf.Clamp01(gallineSalve / (float)gallineTotali) * 100f
            )
            : 0;

        int vecchioPunteggio =
            Mathf.Max(0, recordPrecedente.migliorPunteggio);
        int vecchieVolpi =
            Mathf.Max(0, recordPrecedente.massimoVolpi);
        int vecchieGalline = Mathf.Clamp(
            recordPrecedente.migliorePercentualeGalline,
            0,
            100
        );
        float vecchioTempo =
            Mathf.Max(0f, recordPrecedente.migliorTempoVittoria);
        int vecchiaOndata =
            Mathf.Max(0, recordPrecedente.massimaOndata);
        int ondateValide = Mathf.Max(0, ondateCompletate);

        bool nuovoPunteggio = punteggio > vecchioPunteggio;
        bool nuovoRecordVolpi = volpiEliminate > vecchieVolpi;
        bool nuovoRecordGalline = percentualeGalline > vecchieGalline;
        bool nuovoRecordOndata = ondateValide > vecchiaOndata;
        bool nuovoRecordTempo =
            vittoria &&
            durata > 0f &&
            (vecchioTempo <= 0f || durata < vecchioTempo);

        int migliorPunteggio = Mathf.Max(vecchioPunteggio, punteggio);
        int massimoVolpi = Mathf.Max(vecchieVolpi, volpiEliminate);
        int miglioriGalline = Mathf.Max(vecchieGalline, percentualeGalline);
        float migliorTempo = nuovoRecordTempo ? durata : vecchioTempo;
        int massimaOndata = Mathf.Max(vecchiaOndata, ondateValide);

        if (nuovoPunteggio || nuovoRecordVolpi ||
            nuovoRecordGalline || nuovoRecordTempo ||
            nuovoRecordOndata)
        {
            int indice = (int)difficolta;
            SaveService.ModificaProfilo(dati =>
            {
                DatiRecordDifficolta record =
                    dati.recordDifficolta[indice];
                record.migliorPunteggio = migliorPunteggio;
                record.massimoVolpi = massimoVolpi;
                record.migliorePercentualeGalline =
                    miglioriGalline;
                record.migliorTempoVittoria = migliorTempo;
                record.massimaOndata = massimaOndata;
                dati.miglioreOndataAssoluta = Mathf.Max(
                    dati.miglioreOndataAssoluta,
                    massimaOndata
                );
            });
        }

        return new EsitoRecordPartita(
            migliorPunteggio,
            massimoVolpi,
            miglioriGalline,
            migliorTempo,
            massimaOndata,
            nuovoPunteggio,
            nuovoRecordVolpi,
            nuovoRecordGalline,
            nuovoRecordTempo,
            nuovoRecordOndata
        );
    }

    public static EsitoRecordPartita OttieniRecord(
        DifficoltaPartita difficolta
    )
    {
        DatiRecordDifficolta record =
            OttieniDatiRecord(Normalizza(difficolta));
        return new EsitoRecordPartita(
            Mathf.Max(0, record.migliorPunteggio),
            Mathf.Max(0, record.massimoVolpi),
            Mathf.Clamp(record.migliorePercentualeGalline, 0, 100),
            Mathf.Max(0f, record.migliorTempoVittoria),
            Mathf.Max(0, record.massimaOndata),
            false,
            false,
            false,
            false,
            false
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
            (DifficoltaPartita)
                SaveService.Profilo.difficoltaPreferita
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

    private static DatiRecordDifficolta OttieniDatiRecord(
        DifficoltaPartita difficolta
    )
    {
        SaveData dati = SaveService.Profilo;
        int indice = (int)Normalizza(difficolta);
        return dati.recordDifficolta[indice];
    }
}
