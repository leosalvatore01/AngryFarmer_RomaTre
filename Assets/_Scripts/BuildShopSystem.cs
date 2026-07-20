using System;
using System.Collections.Generic;
using UnityEngine;

public enum PercorsoBuild
{
    Utilita,
    Raffica,
    Artiglieria,
    Perforazione,
    Controllo
}

public enum RaritaBuild
{
    Comune,
    Rara,
    Epica
}

public enum CategoriaBuild
{
    Statistica,
    Modificatore
}

public sealed class DefinizionePotenziamentoBuild
{
    public TipoPotenziamento Tipo { get; }
    public PercorsoBuild Percorso { get; }
    public RaritaBuild Rarita { get; }
    public CategoriaBuild Categoria { get; }
    public int OndaMinima { get; }

    public DefinizionePotenziamentoBuild(
        TipoPotenziamento tipo,
        PercorsoBuild percorso,
        RaritaBuild rarita,
        CategoriaBuild categoria,
        int ondaMinima
    )
    {
        Tipo = tipo;
        Percorso = percorso;
        Rarita = rarita;
        Categoria = categoria;
        OndaMinima = Mathf.Max(1, ondaMinima);
    }
}

public static class CatalogoPotenziamentiBuild
{
    private static readonly DefinizionePotenziamentoBuild[] definizioni =
    {
        new DefinizionePotenziamentoBuild(
            TipoPotenziamento.Movimento,
            PercorsoBuild.Utilita,
            RaritaBuild.Comune,
            CategoriaBuild.Statistica,
            1
        ),
        new DefinizionePotenziamentoBuild(
            TipoPotenziamento.Resistenza,
            PercorsoBuild.Utilita,
            RaritaBuild.Rara,
            CategoriaBuild.Statistica,
            2
        ),
        new DefinizionePotenziamentoBuild(
            TipoPotenziamento.SaluteMassima,
            PercorsoBuild.Utilita,
            RaritaBuild.Comune,
            CategoriaBuild.Statistica,
            1
        ),
        new DefinizionePotenziamentoBuild(
            TipoPotenziamento.Danno,
            PercorsoBuild.Artiglieria,
            RaritaBuild.Comune,
            CategoriaBuild.Statistica,
            1
        ),
        new DefinizionePotenziamentoBuild(
            TipoPotenziamento.Cadenza,
            PercorsoBuild.Raffica,
            RaritaBuild.Comune,
            CategoriaBuild.Statistica,
            1
        ),
        new DefinizionePotenziamentoBuild(
            TipoPotenziamento.Penetrazione,
            PercorsoBuild.Perforazione,
            RaritaBuild.Rara,
            CategoriaBuild.Statistica,
            2
        ),
        new DefinizionePotenziamentoBuild(
            TipoPotenziamento.ColpoAggiuntivo,
            PercorsoBuild.Raffica,
            RaritaBuild.Rara,
            CategoriaBuild.Modificatore,
            2
        ),
        new DefinizionePotenziamentoBuild(
            TipoPotenziamento.RafficaRaccolto,
            PercorsoBuild.Raffica,
            RaritaBuild.Epica,
            CategoriaBuild.Modificatore,
            3
        ),
        new DefinizionePotenziamentoBuild(
            TipoPotenziamento.PatataGigante,
            PercorsoBuild.Artiglieria,
            RaritaBuild.Comune,
            CategoriaBuild.Modificatore,
            1
        ),
        new DefinizionePotenziamentoBuild(
            TipoPotenziamento.PatataEsplosiva,
            PercorsoBuild.Artiglieria,
            RaritaBuild.Epica,
            CategoriaBuild.Modificatore,
            3
        ),
        new DefinizionePotenziamentoBuild(
            TipoPotenziamento.Critico,
            PercorsoBuild.Perforazione,
            RaritaBuild.Comune,
            CategoriaBuild.Modificatore,
            1
        ),
        new DefinizionePotenziamentoBuild(
            TipoPotenziamento.Rimbalzo,
            PercorsoBuild.Perforazione,
            RaritaBuild.Rara,
            CategoriaBuild.Modificatore,
            2
        ),
        new DefinizionePotenziamentoBuild(
            TipoPotenziamento.Rallentamento,
            PercorsoBuild.Controllo,
            RaritaBuild.Comune,
            CategoriaBuild.Modificatore,
            1
        ),
        new DefinizionePotenziamentoBuild(
            TipoPotenziamento.Spinta,
            PercorsoBuild.Controllo,
            RaritaBuild.Comune,
            CategoriaBuild.Modificatore,
            1
        )
    };

    public static IReadOnlyList<DefinizionePotenziamentoBuild> Tutte =>
        definizioni;

    public static DefinizionePotenziamentoBuild Ottieni(
        TipoPotenziamento tipo
    )
    {
        for (int i = 0; i < definizioni.Length; i++)
        {
            if (definizioni[i].Tipo == tipo) return definizioni[i];
        }
        return null;
    }

    public static string NomePercorso(PercorsoBuild percorso)
    {
        switch (percorso)
        {
            case PercorsoBuild.Raffica: return "RAFFICA";
            case PercorsoBuild.Artiglieria: return "ARTIGLIERIA";
            case PercorsoBuild.Perforazione: return "PERFORAZIONE";
            case PercorsoBuild.Controllo: return "CONTROLLO";
            default: return "UTILITA";
        }
    }

    public static string NomeRarita(RaritaBuild rarita)
    {
        switch (rarita)
        {
            case RaritaBuild.Rara: return "RARA";
            case RaritaBuild.Epica: return "EPICA";
            default: return "COMUNE";
        }
    }

    public static string NomeCategoria(CategoriaBuild categoria)
    {
        return categoria == CategoriaBuild.Modificatore
            ? "MODIFICATORE"
            : "STATISTICA";
    }

    public static Color ColorePercorso(PercorsoBuild percorso)
    {
        switch (percorso)
        {
            case PercorsoBuild.Raffica:
                return new Color32(237, 171, 54, 255);
            case PercorsoBuild.Artiglieria:
                return new Color32(218, 91, 55, 255);
            case PercorsoBuild.Perforazione:
                return new Color32(93, 184, 207, 255);
            case PercorsoBuild.Controllo:
                return new Color32(93, 190, 118, 255);
            default:
                return new Color32(199, 174, 128, 255);
        }
    }

    public static Color ColoreRarita(RaritaBuild rarita)
    {
        switch (rarita)
        {
            case RaritaBuild.Rara:
                return new Color32(78, 171, 224, 255);
            case RaritaBuild.Epica:
                return new Color32(194, 91, 223, 255);
            default:
                return new Color32(190, 179, 143, 255);
        }
    }
}

public sealed class GeneratoreOfferteBuild
{
    private readonly List<DefinizionePotenziamentoBuild> candidati =
        new List<DefinizionePotenziamentoBuild>();
    private readonly List<DefinizionePotenziamentoBuild> tuttiCandidati =
        new List<DefinizionePotenziamentoBuild>();
    private readonly List<DefinizionePotenziamentoBuild> selezionabili =
        new List<DefinizionePotenziamentoBuild>();
    private System.Random casualita;

    public GeneratoreOfferteBuild(int seed)
    {
        ImpostaSeed(seed);
    }

    public void ImpostaSeed(int seed)
    {
        casualita = new System.Random(seed);
    }

    public List<TipoPotenziamento> Genera(
        PlayerUpgrades potenziamenti,
        int ondaCompletata,
        int monete,
        int numeroOfferte,
        ICollection<TipoPotenziamento> offertePrecedenti = null,
        PercorsoBuild? percorsoPreferito = null,
        bool usaStrutturaPercorsi = true
    )
    {
        List<TipoPotenziamento> risultato =
            new List<TipoPotenziamento>(Mathf.Max(0, numeroOfferte));
        if (potenziamenti == null || numeroOfferte <= 0) return risultato;

        PreparaCandidati(
            candidati,
            potenziamenti,
            ondaCompletata,
            offertePrecedenti,
            true
        );
        PreparaCandidati(
            tuttiCandidati,
            potenziamenti,
            ondaCompletata,
            offertePrecedenti,
            false
        );

        PercorsoBuild? preferenzaCombattimento =
            percorsoPreferito.HasValue &&
            percorsoPreferito.Value != PercorsoBuild.Utilita
                ? percorsoPreferito
                : null;
        bool generaShopStrutturato =
            usaStrutturaPercorsi &&
            preferenzaCombattimento.HasValue &&
            numeroOfferte >= 3;

        if (generaShopStrutturato)
        {
            AggiungiGarantitaPercorso(
                risultato,
                potenziamenti,
                Mathf.Max(0, monete),
                preferenzaCombattimento.Value,
                candidati,
                tuttiCandidati,
                preferenzaCombattimento
            );
            AggiungiGarantitaPercorso(
                risultato,
                potenziamenti,
                Mathf.Max(0, monete),
                PercorsoBuild.Utilita,
                candidati,
                tuttiCandidati,
                preferenzaCombattimento
            );
        }
        else
        {
            AggiungiGarantitaAccessibile(
                risultato,
                potenziamenti,
                Mathf.Max(0, monete),
                candidati,
                tuttiCandidati,
                preferenzaCombattimento
            );
            AggiungiModificatoriGarantiti(
                risultato,
                2,
                candidati,
                tuttiCandidati,
                potenziamenti,
                preferenzaCombattimento
            );
        }

        while (risultato.Count < numeroOfferte)
        {
            DefinizionePotenziamentoBuild scelta =
                EstraiPesata(
                    risultato,
                    potenziamenti,
                    preferenzaCombattimento
                );
            if (scelta == null)
            {
                scelta = EstraiPesataDaLista(
                    tuttiCandidati,
                    risultato,
                    potenziamenti,
                    preferenzaCombattimento
                );
            }
            if (scelta == null) break;
            risultato.Add(scelta.Tipo);
        }
        return risultato;
    }

    private void PreparaCandidati(
        List<DefinizionePotenziamentoBuild> destinazione,
        PlayerUpgrades potenziamenti,
        int ondaCompletata,
        ICollection<TipoPotenziamento> offertePrecedenti,
        bool escludiPrecedenti
    )
    {
        destinazione.Clear();
        IReadOnlyList<DefinizionePotenziamentoBuild> tutte =
            CatalogoPotenziamentiBuild.Tutte;
        int onda = Mathf.Max(1, ondaCompletata);
        for (int i = 0; i < tutte.Count; i++)
        {
            DefinizionePotenziamentoBuild definizione = tutte[i];
            if (definizione.OndaMinima > onda) continue;
            if (!potenziamenti.PuoComparire(definizione.Tipo)) continue;
            if (escludiPrecedenti &&
                offertePrecedenti != null &&
                offertePrecedenti.Contains(definizione.Tipo))
            {
                continue;
            }
            destinazione.Add(definizione);
        }
    }

    private void AggiungiGarantitaAccessibile(
        List<TipoPotenziamento> risultato,
        PlayerUpgrades potenziamenti,
        int monete,
        List<DefinizionePotenziamentoBuild> preferiti,
        List<DefinizionePotenziamentoBuild> fallback,
        PercorsoBuild? percorsoPreferito
    )
    {
        DefinizionePotenziamentoBuild scelta = EstraiAccessibile(
            preferiti,
            risultato,
            potenziamenti,
            monete,
            percorsoPreferito
        );
        if (scelta == null)
        {
            scelta = EstraiAccessibile(
                fallback,
                risultato,
                potenziamenti,
                monete,
                percorsoPreferito
            );
        }
        if (scelta != null) risultato.Add(scelta.Tipo);
    }

    private void AggiungiGarantitaPercorso(
        List<TipoPotenziamento> risultato,
        PlayerUpgrades potenziamenti,
        int monete,
        PercorsoBuild percorso,
        List<DefinizionePotenziamentoBuild> preferiti,
        List<DefinizionePotenziamentoBuild> fallback,
        PercorsoBuild? percorsoPreferito
    )
    {
        DefinizionePotenziamentoBuild scelta =
            EstraiDaPercorso(
                preferiti,
                risultato,
                potenziamenti,
                percorso,
                monete,
                true,
                percorsoPreferito
            );
        if (scelta == null)
        {
            scelta = EstraiDaPercorso(
                fallback,
                risultato,
                potenziamenti,
                percorso,
                monete,
                true,
                percorsoPreferito
            );
        }
        if (scelta == null)
        {
            scelta = EstraiDaPercorso(
                preferiti,
                risultato,
                potenziamenti,
                percorso,
                monete,
                false,
                percorsoPreferito
            );
        }
        if (scelta == null)
        {
            scelta = EstraiDaPercorso(
                fallback,
                risultato,
                potenziamenti,
                percorso,
                monete,
                false,
                percorsoPreferito
            );
        }

        // Se il percorso non ha carte eleggibili, non lasciare uno slot vuoto.
        if (scelta == null)
        {
            scelta = EstraiAccessibile(
                preferiti,
                risultato,
                potenziamenti,
                monete,
                percorsoPreferito
            );
        }
        if (scelta == null)
        {
            scelta = EstraiAccessibile(
                fallback,
                risultato,
                potenziamenti,
                monete,
                percorsoPreferito
            );
        }
        if (scelta == null)
        {
            scelta = EstraiPesataDaLista(
                preferiti,
                risultato,
                potenziamenti,
                percorsoPreferito
            );
        }
        if (scelta == null)
        {
            scelta = EstraiPesataDaLista(
                fallback,
                risultato,
                potenziamenti,
                percorsoPreferito
            );
        }
        if (scelta != null) risultato.Add(scelta.Tipo);
    }

    private void AggiungiModificatoriGarantiti(
        List<TipoPotenziamento> risultato,
        int quantitaDesiderata,
        List<DefinizionePotenziamentoBuild> preferiti,
        List<DefinizionePotenziamentoBuild> fallback,
        PlayerUpgrades potenziamenti,
        PercorsoBuild? percorsoPreferito
    )
    {
        while (ContaModificatori(risultato) < quantitaDesiderata)
        {
            DefinizionePotenziamentoBuild scelta =
                EstraiModificatore(
                    preferiti,
                    risultato,
                    potenziamenti,
                    percorsoPreferito
                );
            if (scelta == null)
            {
                scelta = EstraiModificatore(
                    fallback,
                    risultato,
                    potenziamenti,
                    percorsoPreferito
                );
            }
            if (scelta == null) return;
            risultato.Add(scelta.Tipo);
        }
    }

    private DefinizionePotenziamentoBuild EstraiAccessibile(
        List<DefinizionePotenziamentoBuild> sorgente,
        List<TipoPotenziamento> giaScelte,
        PlayerUpgrades potenziamenti,
        int monete,
        PercorsoBuild? percorsoPreferito
    )
    {
        selezionabili.Clear();
        for (int i = 0; i < sorgente.Count; i++)
        {
            if (potenziamenti.OttieniCosto(sorgente[i].Tipo) <= monete)
            {
                selezionabili.Add(sorgente[i]);
            }
        }
        return EstraiPesataDaLista(
            selezionabili,
            giaScelte,
            potenziamenti,
            percorsoPreferito
        );
    }

    private DefinizionePotenziamentoBuild EstraiDaPercorso(
        List<DefinizionePotenziamentoBuild> sorgente,
        List<TipoPotenziamento> giaScelte,
        PlayerUpgrades potenziamenti,
        PercorsoBuild percorso,
        int monete,
        bool soloAccessibili,
        PercorsoBuild? percorsoPreferito
    )
    {
        selezionabili.Clear();
        for (int i = 0; i < sorgente.Count; i++)
        {
            DefinizionePotenziamentoBuild definizione = sorgente[i];
            if (definizione.Percorso != percorso) continue;
            if (soloAccessibili &&
                potenziamenti.OttieniCosto(definizione.Tipo) > monete)
            {
                continue;
            }
            selezionabili.Add(definizione);
        }
        return EstraiPesataDaLista(
            selezionabili,
            giaScelte,
            potenziamenti,
            percorsoPreferito
        );
    }

    private DefinizionePotenziamentoBuild EstraiModificatore(
        List<DefinizionePotenziamentoBuild> sorgente,
        List<TipoPotenziamento> giaScelte,
        PlayerUpgrades potenziamenti,
        PercorsoBuild? percorsoPreferito
    )
    {
        selezionabili.Clear();
        for (int i = 0; i < sorgente.Count; i++)
        {
            if (sorgente[i].Categoria == CategoriaBuild.Modificatore)
            {
                selezionabili.Add(sorgente[i]);
            }
        }
        return EstraiPesataDaLista(
            selezionabili,
            giaScelte,
            potenziamenti,
            percorsoPreferito
        );
    }

    private static int ContaModificatori(
        List<TipoPotenziamento> risultato
    )
    {
        int totale = 0;
        for (int i = 0; i < risultato.Count; i++)
        {
            DefinizionePotenziamentoBuild definizione =
                CatalogoPotenziamentiBuild.Ottieni(risultato[i]);
            if (definizione != null &&
                definizione.Categoria == CategoriaBuild.Modificatore)
            {
                totale++;
            }
        }
        return totale;
    }

    private DefinizionePotenziamentoBuild EstraiPesata(
        List<TipoPotenziamento> giaScelte,
        PlayerUpgrades potenziamenti,
        PercorsoBuild? percorsoPreferito
    )
    {
        return EstraiPesataDaLista(
            candidati,
            giaScelte,
            potenziamenti,
            percorsoPreferito
        );
    }

    private DefinizionePotenziamentoBuild EstraiPesataDaLista(
        List<DefinizionePotenziamentoBuild> sorgente,
        List<TipoPotenziamento> giaScelte,
        PlayerUpgrades potenziamenti,
        PercorsoBuild? percorsoPreferito
    )
    {
        int pesoTotale = 0;
        for (int i = 0; i < sorgente.Count; i++)
        {
            if (giaScelte.Contains(sorgente[i].Tipo)) continue;
            pesoTotale += CalcolaPeso(
                sorgente[i],
                potenziamenti,
                percorsoPreferito
            );
        }
        if (pesoTotale <= 0) return null;

        int estrazione = casualita.Next(pesoTotale);
        for (int i = 0; i < sorgente.Count; i++)
        {
            DefinizionePotenziamentoBuild definizione = sorgente[i];
            if (giaScelte.Contains(definizione.Tipo)) continue;
            estrazione -= CalcolaPeso(
                definizione,
                potenziamenti,
                percorsoPreferito
            );
            if (estrazione < 0) return definizione;
        }
        return null;
    }

    private static int CalcolaPeso(
        DefinizionePotenziamentoBuild definizione,
        PlayerUpgrades potenziamenti,
        PercorsoBuild? percorsoPreferito
    )
    {
        int peso;
        switch (definizione.Rarita)
        {
            case RaritaBuild.Rara:
                peso = 62;
                break;
            case RaritaBuild.Epica:
                peso = 25;
                break;
            default:
                peso = 100;
                break;
        }

        if (percorsoPreferito.HasValue &&
            definizione.Percorso == percorsoPreferito.Value)
        {
            peso = Mathf.RoundToInt(peso * 1.7f);
        }
        else if (potenziamenti != null &&
            definizione.Percorso != PercorsoBuild.Utilita &&
            potenziamenti.OttieniPuntiPercorso(definizione.Percorso) > 0)
        {
            peso = Mathf.RoundToInt(peso * 1.35f);
        }
        return Mathf.Max(1, peso);
    }
}

public struct ProfiloProiettileBuild
{
    public int Danno;
    public int Penetrazioni;
    public float Scala;
    public float MoltiplicatoreVelocita;
    public bool Critico;
    public int Rimbalzi;
    public float RaggioRimbalzo;
    public float MoltiplicatoreDannoRimbalzo;
    public float RaggioEsplosione;
    public int DannoEsplosione;
    public float MoltiplicatoreRallentamento;
    public float DurataRallentamento;
    public float ForzaSpinta;

    public bool Esplosivo => RaggioEsplosione > 0f && DannoEsplosione > 0;
    public bool Rallentante =>
        MoltiplicatoreRallentamento > 0f &&
        MoltiplicatoreRallentamento < 1f &&
        DurataRallentamento > 0f;
    public bool Spinge => ForzaSpinta > 0f;
}
