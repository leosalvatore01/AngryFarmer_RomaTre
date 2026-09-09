using System;
using System.Collections.Generic;
using UnityEngine;

public enum ArchetipoOndata
{
    Addestramento = 0,
    Bilanciata = 1,
    Sciame = 2,
    Assedio = 3,
    Distanza = 4,
    Elite = 5,
    Boss = 6
}

[Serializable]
public sealed class FoxThreatCosts
{
    [Min(1)] public int comune = 1;
    [Min(1)] public int agile = 2;
    [Min(1)] public int robusta = 3;
    [Min(1)] public int schivatrice = 3;
    [Min(1)] public int alfa = 6;
    [Min(1)] public int ululatrice = 5;
    [Min(1)] public int sputafango = 5;
    [Min(1)] public int scavatrice = 5;

    public int Ottieni(TipoVolpe tipo)
    {
        switch (FoxVariantStyle.Normalizza(tipo))
        {
            case TipoVolpe.Agile: return Mathf.Max(1, agile);
            case TipoVolpe.Robusta: return Mathf.Max(1, robusta);
            case TipoVolpe.Schivatrice:
                return Mathf.Max(1, schivatrice);
            case TipoVolpe.Alfa: return Mathf.Max(1, alfa);
            case TipoVolpe.Ululatrice:
                return Mathf.Max(1, ululatrice);
            case TipoVolpe.Sputafango:
                return Mathf.Max(1, sputafango);
            case TipoVolpe.Scavatrice:
                return Mathf.Max(1, scavatrice);
            default: return Mathf.Max(1, comune);
        }
    }

    public void Normalizza()
    {
        comune = Mathf.Max(1, comune);
        agile = Mathf.Max(1, agile);
        robusta = Mathf.Max(1, robusta);
        schivatrice = Mathf.Max(1, schivatrice);
        alfa = Mathf.Max(1, alfa);
        ululatrice = Mathf.Max(1, ululatrice);
        sputafango = Mathf.Max(1, sputafango);
        scavatrice = Mathf.Max(1, scavatrice);
    }
}

[Serializable]
public sealed class ThreatBudgetSettings
{
    [Header("Budget per ondata")]
    public bool attivo = true;
    [Min(1)] public int budgetPrimaOndata = 3;
    [Min(0)] public int bonusBudgetDopoPrimaOndata = 1;
    [Min(0)] public int incrementoBudgetPerOnda = 2;
    [Min(1)] public int ondeCrescitaLineare = 20;
    [Min(0f)] public float incrementoBudgetAvanzato = 2f;
    [Min(0)] public int bonusBudgetElite = 4;
    [Min(0)] public int bonusBudgetBoss = 10;
    [Range(8, 250)] public int limiteVolpiPerOndata = 120;

    [Header("Limite contemporaneo")]
    [Range(1, 30)] public int limiteContemporaneoBase = 6;
    [Min(1)] public int capitoliPerIncrementoLimite = 2;
    [Range(1, 40)] public int limiteContemporaneoMassimo = 18;

    [Header("Combinazioni")]
    [Range(1, 4)] public int limiteAlfaPerOndata = 2;
    [Range(1, 8)] public int limiteSupportiDistanza = 4;
    [Range(1, 6)] public int limiteScavatrici = 3;
    public FoxThreatCosts costi = new FoxThreatCosts();

    [Header("Anello di spawn")]
    [Min(0f)] public float distanzaMinimaDalContadino = 8.5f;
    [Min(0f)] public float distanzaMassimaDalContadino = 11.5f;

    [Header("Vita alle ondate avanzate")]
    [Min(1)] public int ondeCrescitaVitaLineare = 12;
    [Min(0.05f)] public float crescitaVitaAvanzata = 0.65f;

    public void Normalizza()
    {
        budgetPrimaOndata = Mathf.Max(1, budgetPrimaOndata);
        bonusBudgetDopoPrimaOndata = Mathf.Max(
            0,
            bonusBudgetDopoPrimaOndata
        );
        incrementoBudgetPerOnda = Mathf.Max(0, incrementoBudgetPerOnda);
        ondeCrescitaLineare = Mathf.Max(1, ondeCrescitaLineare);
        incrementoBudgetAvanzato = Mathf.Max(0f, incrementoBudgetAvanzato);
        bonusBudgetElite = Mathf.Max(0, bonusBudgetElite);
        bonusBudgetBoss = Mathf.Max(0, bonusBudgetBoss);
        limiteVolpiPerOndata = Mathf.Clamp(
            limiteVolpiPerOndata,
            8,
            250
        );

        limiteContemporaneoBase = Mathf.Clamp(
            limiteContemporaneoBase,
            1,
            30
        );
        capitoliPerIncrementoLimite = Mathf.Max(
            1,
            capitoliPerIncrementoLimite
        );
        limiteContemporaneoMassimo = Mathf.Clamp(
            limiteContemporaneoMassimo,
            limiteContemporaneoBase,
            40
        );

        limiteAlfaPerOndata = Mathf.Clamp(limiteAlfaPerOndata, 1, 4);
        limiteSupportiDistanza = Mathf.Clamp(
            limiteSupportiDistanza,
            1,
            8
        );
        limiteScavatrici = Mathf.Clamp(limiteScavatrici, 1, 6);
        if (costi == null) costi = new FoxThreatCosts();
        costi.Normalizza();

        distanzaMinimaDalContadino = Mathf.Max(
            0f,
            distanzaMinimaDalContadino
        );
        distanzaMassimaDalContadino = Mathf.Max(
            distanzaMinimaDalContadino,
            distanzaMassimaDalContadino
        );
        ondeCrescitaVitaLineare = Mathf.Max(1, ondeCrescitaVitaLineare);
        crescitaVitaAvanzata = Mathf.Max(0.05f, crescitaVitaAvanzata);
    }
}

public readonly struct PianoMinacciaOndata
{
    public int NumeroOndata { get; }
    public ArchetipoOndata Archetipo { get; }
    public int BudgetTotale { get; }
    public int BudgetUsato { get; }
    public int BudgetResiduo => Mathf.Max(0, BudgetTotale - BudgetUsato);
    public int LimiteContemporaneo { get; }
    public float DistanzaSpawnMinima { get; }
    public float DistanzaSpawnMassima { get; }
    public TipoVolpe[] Sequenza { get; }
    public ComposizioneVolpi Composizione { get; }
    public int TotaleVolpi => Sequenza != null ? Sequenza.Length : 0;

    public PianoMinacciaOndata(
        int numeroOndata,
        ArchetipoOndata archetipo,
        int budgetTotale,
        int budgetUsato,
        int limiteContemporaneo,
        float distanzaSpawnMinima,
        float distanzaSpawnMassima,
        TipoVolpe[] sequenza,
        ComposizioneVolpi composizione
    )
    {
        NumeroOndata = numeroOndata;
        Archetipo = archetipo;
        BudgetTotale = Mathf.Max(0, budgetTotale);
        BudgetUsato = Mathf.Max(0, budgetUsato);
        LimiteContemporaneo = Mathf.Max(1, limiteContemporaneo);
        DistanzaSpawnMinima = Mathf.Max(0f, distanzaSpawnMinima);
        DistanzaSpawnMassima = Mathf.Max(
            DistanzaSpawnMinima,
            distanzaSpawnMassima
        );
        Sequenza = sequenza ?? Array.Empty<TipoVolpe>();
        Composizione = composizione;
    }
}

/// <summary>
/// Compone ondate deterministiche spendendo un budget. Il numero di volpi,
/// le varianti e il limite contemporaneo crescono senza dipendere dalla scena.
/// </summary>
public static class WaveThreatDirector
{
    private static readonly TipoVolpe[] SchemaBilanciato =
    {
        TipoVolpe.Comune,
        TipoVolpe.Agile,
        TipoVolpe.Robusta,
        TipoVolpe.Comune,
        TipoVolpe.Schivatrice,
        TipoVolpe.Ululatrice,
        TipoVolpe.Comune,
        TipoVolpe.Sputafango,
        TipoVolpe.Scavatrice,
        TipoVolpe.Comune,
        TipoVolpe.Alfa
    };

    private static readonly TipoVolpe[] SchemaSciame =
    {
        TipoVolpe.Agile,
        TipoVolpe.Comune,
        TipoVolpe.Agile,
        TipoVolpe.Schivatrice,
        TipoVolpe.Comune,
        TipoVolpe.Agile,
        TipoVolpe.Comune
    };

    private static readonly TipoVolpe[] SchemaAssedio =
    {
        TipoVolpe.Robusta,
        TipoVolpe.Comune,
        TipoVolpe.Scavatrice,
        TipoVolpe.Robusta,
        TipoVolpe.Ululatrice,
        TipoVolpe.Comune,
        TipoVolpe.Alfa,
        TipoVolpe.Comune
    };

    private static readonly TipoVolpe[] SchemaDistanza =
    {
        TipoVolpe.Sputafango,
        TipoVolpe.Comune,
        TipoVolpe.Agile,
        TipoVolpe.Ululatrice,
        TipoVolpe.Comune,
        TipoVolpe.Schivatrice,
        TipoVolpe.Comune
    };

    private static readonly TipoVolpe[] SchemaElite =
    {
        TipoVolpe.Alfa,
        TipoVolpe.Robusta,
        TipoVolpe.Schivatrice,
        TipoVolpe.Agile,
        TipoVolpe.Comune,
        TipoVolpe.Ululatrice,
        TipoVolpe.Comune,
        TipoVolpe.Sputafango,
        TipoVolpe.Scavatrice
    };

    private static readonly TipoVolpe[] SchemaBoss =
    {
        TipoVolpe.Alfa,
        TipoVolpe.Robusta,
        TipoVolpe.Scavatrice,
        TipoVolpe.Ululatrice,
        TipoVolpe.Sputafango,
        TipoVolpe.Comune,
        TipoVolpe.Robusta,
        TipoVolpe.Comune
    };

    public static PianoMinacciaOndata CreaPiano(
        int numeroOndata,
        ThreatBudgetSettings impostazioni,
        RitmoOndata ritmo,
        float moltiplicatoreBudget = 1f
    )
    {
        ThreatBudgetSettings regole = impostazioni ??
            new ThreatBudgetSettings();
        FoxThreatCosts costi = regole.costi ?? new FoxThreatCosts();
        int onda = Mathf.Max(1, numeroOndata);
        ArchetipoOndata archetipo = DeterminaArchetipo(onda, ritmo);
        int budget = CalcolaBudget(
            onda,
            regole,
            ritmo,
            moltiplicatoreBudget
        );
        int limiteVolpi = Mathf.Clamp(
            regole.limiteVolpiPerOndata,
            8,
            250
        );

        List<TipoVolpe> sequenza = new List<TipoVolpe>(
            Mathf.Min(budget, limiteVolpi)
        );
        int[] conteggi = new int[FoxVariantStyle.NumeroTipi];
        int usato = 0;

        TipoVolpe? introduzione = TipoIntrodottoNellOnda(onda);
        if (introduzione.HasValue)
        {
            AggiungiSePossibile(
                introduzione.Value,
                onda,
                archetipo,
                budget,
                limiteVolpi,
                regole,
                costi,
                sequenza,
                conteggi,
                ref usato,
                true
            );
        }

        TipoVolpe identita = TipoIdentita(archetipo, onda);
        if (conteggi[(int)identita] == 0)
        {
            AggiungiSePossibile(
                identita,
                onda,
                archetipo,
                budget,
                limiteVolpi,
                regole,
                costi,
                sequenza,
                conteggi,
                ref usato,
                true
            );
        }

        TipoVolpe[] schema = OttieniSchema(archetipo);
        int seme = unchecked(onda * 1103515245 + 12345);
        int passo = 0;
        while (usato < budget && sequenza.Count < limiteVolpi)
        {
            bool aggiunta = false;
            int partenza = Mod(seme + passo * 3, schema.Length);
            for (int tentativo = 0; tentativo < schema.Length; tentativo++)
            {
                TipoVolpe candidato = schema[
                    (partenza + tentativo) % schema.Length
                ];
                if (!AggiungiSePossibile(
                        candidato,
                        onda,
                        archetipo,
                        budget,
                        limiteVolpi,
                        regole,
                        costi,
                        sequenza,
                        conteggi,
                        ref usato,
                        false
                    ))
                {
                    continue;
                }

                aggiunta = true;
                break;
            }

            if (!aggiunta && !AggiungiSePossibile(
                    TipoVolpe.Comune,
                    onda,
                    archetipo,
                    budget,
                    limiteVolpi,
                    regole,
                    costi,
                    sequenza,
                    conteggi,
                    ref usato,
                    true
                ))
            {
                break;
            }
            passo++;
        }

        SeparaMinacceCostose(sequenza, costi);
        ComposizioneVolpi composizione = CalcolaComposizione(sequenza);
        CalcolaDistanzeSpawn(
            archetipo,
            regole,
            out float distanzaMinima,
            out float distanzaMassima
        );

        return new PianoMinacciaOndata(
            onda,
            archetipo,
            budget,
            usato,
            CalcolaLimiteContemporaneo(regole, ritmo),
            distanzaMinima,
            distanzaMassima,
            sequenza.ToArray(),
            composizione
        );
    }

    public static int CalcolaBudget(
        int numeroOndata,
        ThreatBudgetSettings impostazioni,
        RitmoOndata ritmo,
        float moltiplicatoreBudget = 1f
    )
    {
        ThreatBudgetSettings regole = impostazioni ??
            new ThreatBudgetSettings();
        int onda = Mathf.Max(1, numeroOndata);
        long progresso = (long)onda - 1L;
        long trattoLineare = Math.Min(
            progresso,
            Math.Max(1, regole.ondeCrescitaLineare)
        );
        long trattoAvanzato = Math.Max(0L, progresso - trattoLineare);
        double crescitaAvanzata = Math.Sqrt(trattoAvanzato) *
            Math.Max(0f, regole.incrementoBudgetAvanzato);
        long budget = Math.Max(1, regole.budgetPrimaOndata) +
            (onda > 1
                ? Math.Max(0, regole.bonusBudgetDopoPrimaOndata)
                : 0) +
            trattoLineare * Math.Max(0, regole.incrementoBudgetPerOnda) +
            (long)Math.Floor(crescitaAvanzata);

        if (ritmo.Boss)
        {
            budget += Math.Max(0, regole.bonusBudgetBoss);
        }
        else if (ritmo.Elite)
        {
            budget += Math.Max(0, regole.bonusBudgetElite);
        }

        double moltiplicato = budget * Math.Max(0.1f, moltiplicatoreBudget);
        return (int)Math.Min(int.MaxValue, Math.Max(1d, Math.Round(
            moltiplicato,
            MidpointRounding.AwayFromZero
        )));
    }

    public static int CalcolaVitaBase(
        int numeroOndata,
        int vitaPrimaOndata,
        int incrementoPerOnda,
        ThreatBudgetSettings impostazioni
    )
    {
        ThreatBudgetSettings regole = impostazioni ??
            new ThreatBudgetSettings();
        long progresso = Math.Max(0L, (long)Mathf.Max(1, numeroOndata) - 1L);
        int limiteLineare = Math.Max(1, regole.ondeCrescitaVitaLineare);
        long lineare = Math.Min(progresso, limiteLineare);
        long avanzato = Math.Max(0L, progresso - lineare);
        long scattiAvanzati = (long)Math.Floor(
            Math.Sqrt(avanzato) * Math.Max(0.05f, regole.crescitaVitaAvanzata)
        );
        long vita = Math.Max(1, vitaPrimaOndata) +
            (lineare + scattiAvanzati) * Math.Max(0, incrementoPerOnda);
        return (int)Math.Min(int.MaxValue, Math.Max(1L, vita));
    }

    public static ArchetipoOndata DeterminaArchetipo(
        int numeroOndata,
        RitmoOndata ritmo
    )
    {
        int onda = Mathf.Max(1, numeroOndata);
        if (onda == 1) return ArchetipoOndata.Addestramento;
        if (ritmo.Boss) return ArchetipoOndata.Boss;
        if (ritmo.Elite) return ArchetipoOndata.Elite;

        switch (onda)
        {
            case 2:
            case 4:
            case 7:
                return ArchetipoOndata.Sciame;
            case 3:
            case 9:
                return ArchetipoOndata.Assedio;
            case 6:
            case 8:
                return ArchetipoOndata.Distanza;
        }

        switch ((onda - 11) & 3)
        {
            case 1: return ArchetipoOndata.Sciame;
            case 2: return ArchetipoOndata.Assedio;
            case 3: return ArchetipoOndata.Distanza;
            default: return ArchetipoOndata.Bilanciata;
        }
    }

    public static bool TipoConsentito(
        ArchetipoOndata archetipo,
        TipoVolpe tipo
    )
    {
        TipoVolpe normale = FoxVariantStyle.Normalizza(tipo);
        switch (archetipo)
        {
            case ArchetipoOndata.Addestramento:
                return normale == TipoVolpe.Comune;
            case ArchetipoOndata.Sciame:
                return normale == TipoVolpe.Comune ||
                       normale == TipoVolpe.Agile ||
                       normale == TipoVolpe.Schivatrice;
            case ArchetipoOndata.Assedio:
                return normale == TipoVolpe.Comune ||
                       normale == TipoVolpe.Robusta ||
                       normale == TipoVolpe.Alfa ||
                       normale == TipoVolpe.Ululatrice ||
                       normale == TipoVolpe.Scavatrice;
            case ArchetipoOndata.Distanza:
                return normale == TipoVolpe.Comune ||
                       normale == TipoVolpe.Agile ||
                       normale == TipoVolpe.Schivatrice ||
                       normale == TipoVolpe.Ululatrice ||
                       normale == TipoVolpe.Sputafango;
            default:
                return true;
        }
    }

    public static int OndaSblocco(TipoVolpe tipo)
    {
        switch (FoxVariantStyle.Normalizza(tipo))
        {
            case TipoVolpe.Agile: return 2;
            case TipoVolpe.Robusta: return 3;
            case TipoVolpe.Schivatrice: return 4;
            case TipoVolpe.Alfa: return 5;
            case TipoVolpe.Ululatrice: return 6;
            case TipoVolpe.Sputafango: return 8;
            case TipoVolpe.Scavatrice: return 10;
            default: return 1;
        }
    }

    public static string Nome(ArchetipoOndata archetipo)
    {
        switch (archetipo)
        {
            case ArchetipoOndata.Addestramento: return "Riscaldamento";
            case ArchetipoOndata.Sciame: return "Sciame rapido";
            case ArchetipoOndata.Assedio: return "Assedio pesante";
            case ArchetipoOndata.Distanza: return "Attacco a distanza";
            case ArchetipoOndata.Elite: return "Branco elite";
            case ArchetipoOndata.Boss: return "Sfida boss";
            default: return "Branco misto";
        }
    }

    private static int CalcolaLimiteContemporaneo(
        ThreatBudgetSettings impostazioni,
        RitmoOndata ritmo
    )
    {
        int baseLimite = Mathf.Clamp(
            impostazioni.limiteContemporaneoBase,
            1,
            30
        );
        int massimo = Mathf.Clamp(
            impostazioni.limiteContemporaneoMassimo,
            baseLimite,
            40
        );
        int capitoliPerIncremento = Mathf.Max(
            1,
            impostazioni.capitoliPerIncrementoLimite
        );
        int capitolo = Mathf.Max(1, ritmo.NumeroCapitolo);
        int incremento = (capitolo - 1) / capitoliPerIncremento;
        return Mathf.Clamp(baseLimite + incremento, baseLimite, massimo);
    }

    private static bool AggiungiSePossibile(
        TipoVolpe tipo,
        int onda,
        ArchetipoOndata archetipo,
        int budget,
        int limiteVolpi,
        ThreatBudgetSettings impostazioni,
        FoxThreatCosts costi,
        List<TipoVolpe> sequenza,
        int[] conteggi,
        ref int usato,
        bool ignoraLimitiCombinazione
    )
    {
        TipoVolpe candidato = FoxVariantStyle.Normalizza(tipo);
        if (onda < OndaSblocco(candidato) ||
            !TipoConsentito(archetipo, candidato) ||
            sequenza.Count >= limiteVolpi)
        {
            return false;
        }

        int costo = costi.Ottieni(candidato);
        if ((long)usato + costo > budget) return false;

        if (!ignoraLimitiCombinazione)
        {
            if (candidato == TipoVolpe.Alfa &&
                conteggi[(int)TipoVolpe.Alfa] >= Mathf.Clamp(
                    impostazioni.limiteAlfaPerOndata,
                    1,
                    4
                ))
            {
                return false;
            }

            int supporti = conteggi[(int)TipoVolpe.Ululatrice] +
                            conteggi[(int)TipoVolpe.Sputafango];
            if ((candidato == TipoVolpe.Ululatrice ||
                 candidato == TipoVolpe.Sputafango) &&
                supporti >= Mathf.Clamp(
                    impostazioni.limiteSupportiDistanza,
                    1,
                    8
                ))
            {
                return false;
            }

            if (candidato == TipoVolpe.Scavatrice &&
                conteggi[(int)TipoVolpe.Scavatrice] >= Mathf.Clamp(
                    impostazioni.limiteScavatrici,
                    1,
                    6
                ))
            {
                return false;
            }
        }

        sequenza.Add(candidato);
        conteggi[(int)candidato]++;
        usato += costo;
        return true;
    }

    private static TipoVolpe? TipoIntrodottoNellOnda(int onda)
    {
        switch (onda)
        {
            case 1: return TipoVolpe.Comune;
            case 2: return TipoVolpe.Agile;
            case 3: return TipoVolpe.Robusta;
            case 4: return TipoVolpe.Schivatrice;
            case 5: return TipoVolpe.Alfa;
            case 6: return TipoVolpe.Ululatrice;
            case 8: return TipoVolpe.Sputafango;
            case 10: return TipoVolpe.Scavatrice;
            default: return null;
        }
    }

    private static TipoVolpe TipoIdentita(
        ArchetipoOndata archetipo,
        int onda
    )
    {
        switch (archetipo)
        {
            case ArchetipoOndata.Sciame:
                return onda >= 2 ? TipoVolpe.Agile : TipoVolpe.Comune;
            case ArchetipoOndata.Assedio:
                return onda >= 3 ? TipoVolpe.Robusta : TipoVolpe.Comune;
            case ArchetipoOndata.Distanza:
                return onda >= 8
                    ? TipoVolpe.Sputafango
                    : TipoVolpe.Ululatrice;
            case ArchetipoOndata.Elite:
            case ArchetipoOndata.Boss:
                return TipoVolpe.Alfa;
            default:
                return TipoVolpe.Comune;
        }
    }

    private static TipoVolpe[] OttieniSchema(ArchetipoOndata archetipo)
    {
        switch (archetipo)
        {
            case ArchetipoOndata.Sciame: return SchemaSciame;
            case ArchetipoOndata.Assedio: return SchemaAssedio;
            case ArchetipoOndata.Distanza: return SchemaDistanza;
            case ArchetipoOndata.Elite: return SchemaElite;
            case ArchetipoOndata.Boss: return SchemaBoss;
            case ArchetipoOndata.Addestramento:
                return new[] { TipoVolpe.Comune };
            default: return SchemaBilanciato;
        }
    }

    private static void CalcolaDistanzeSpawn(
        ArchetipoOndata archetipo,
        ThreatBudgetSettings impostazioni,
        out float minima,
        out float massima
    )
    {
        float baseMinima = Mathf.Max(
            0f,
            impostazioni.distanzaMinimaDalContadino
        );
        float baseMassima = Mathf.Max(
            baseMinima,
            impostazioni.distanzaMassimaDalContadino
        );

        switch (archetipo)
        {
            case ArchetipoOndata.Sciame:
                minima = baseMinima;
                massima = Mathf.Lerp(baseMinima, baseMassima, 0.58f);
                return;
            case ArchetipoOndata.Assedio:
                minima = Mathf.Lerp(baseMinima, baseMassima, 0.24f);
                massima = baseMassima;
                return;
            case ArchetipoOndata.Distanza:
                minima = Mathf.Lerp(baseMinima, baseMassima, 0.62f);
                massima = baseMassima;
                return;
            default:
                minima = baseMinima;
                massima = baseMassima;
                return;
        }
    }

    private static void SeparaMinacceCostose(
        List<TipoVolpe> sequenza,
        FoxThreatCosts costi
    )
    {
        for (int i = 1; i < sequenza.Count; i++)
        {
            if (costi.Ottieni(sequenza[i - 1]) < 5 ||
                costi.Ottieni(sequenza[i]) < 5)
            {
                continue;
            }

            for (int candidato = i + 1; candidato < sequenza.Count; candidato++)
            {
                if (costi.Ottieni(sequenza[candidato]) >= 5) continue;
                TipoVolpe temporanea = sequenza[i];
                sequenza[i] = sequenza[candidato];
                sequenza[candidato] = temporanea;
                break;
            }
        }
    }

    private static ComposizioneVolpi CalcolaComposizione(
        List<TipoVolpe> sequenza
    )
    {
        ComposizioneVolpi composizione = default;
        for (int i = 0; i < sequenza.Count; i++)
        {
            composizione = composizione.Aggiungi(sequenza[i]);
        }
        return composizione;
    }

    private static int Mod(int valore, int divisore)
    {
        int resto = valore % Mathf.Max(1, divisore);
        return resto < 0 ? resto + divisore : resto;
    }
}
