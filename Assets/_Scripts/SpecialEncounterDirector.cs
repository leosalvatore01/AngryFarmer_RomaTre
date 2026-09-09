using System;
using UnityEngine;

public enum RuoloIncontroSpeciale
{
    Nessuno = 0,
    Elite = 1,
    Boss = 2
}

/// <summary>
/// Regole pure e verificabili per trasformare una volpe Alfa nello scontro
/// speciale previsto dal ritmo infinito delle ondate.
/// </summary>
public static class SpecialEncounterDirector
{
    public static RuoloIncontroSpeciale ScegliRuolo(
        TipoIncontroOndata incontro,
        TipoVolpe tipo,
        bool ruoloGiaAssegnato
    )
    {
        if (ruoloGiaAssegnato || tipo != TipoVolpe.Alfa)
        {
            return RuoloIncontroSpeciale.Nessuno;
        }

        switch (incontro)
        {
            case TipoIncontroOndata.Boss:
                return RuoloIncontroSpeciale.Boss;
            case TipoIncontroOndata.Elite:
                return RuoloIncontroSpeciale.Elite;
            default:
                return RuoloIncontroSpeciale.Nessuno;
        }
    }

    public static int CalcolaApparizioneBoss(
        int numeroOndata,
        int bossOgniOnde
    )
    {
        int cadenza = Mathf.Max(1, bossOgniOnde);
        return Mathf.Max(1, Mathf.Max(1, numeroOndata) / cadenza);
    }

    public static int CalcolaNumeroCapitolo(
        int numeroOndata,
        int ondePerCapitolo
    )
    {
        int dimensione = Mathf.Max(1, ondePerCapitolo);
        return (Mathf.Max(1, numeroOndata) - 1) / dimensione + 1;
    }

    public static int CalcolaVitaElite(
        int vitaBase,
        SpecialEncounterBalanceSettings impostazioni
    )
    {
        float moltiplicatore = impostazioni != null
            ? Mathf.Max(1f, impostazioni.moltiplicatoreVitaElite)
            : 1.85f;
        return CalcolaValoreCrescente(vitaBase, moltiplicatore);
    }

    public static int CalcolaVitaBoss(
        int vitaBase,
        int apparizione,
        SpecialEncounterBalanceSettings impostazioni
    )
    {
        float baseBoss = impostazioni != null
            ? Mathf.Max(1f, impostazioni.moltiplicatoreVitaBoss)
            : 5.5f;
        float crescita = impostazioni != null
            ? Mathf.Max(0f, impostazioni.crescitaVitaPerApparizione)
            : 0.35f;
        int indice = Mathf.Max(1, apparizione) - 1;
        return CalcolaValoreCrescente(
            vitaBase,
            baseBoss * (1f + crescita * indice)
        );
    }

    public static int CalcolaMoneteElite(
        int capitolo,
        SpecialEncounterBalanceSettings impostazioni
    )
    {
        int baseMonete = impostazioni != null
            ? Mathf.Max(0, impostazioni.moneteEliteBase)
            : 8;
        int incremento = impostazioni != null
            ? Mathf.Max(0, impostazioni.moneteElitePerCapitolo)
            : 2;
        return SommaSaturata(baseMonete, incremento, capitolo - 1);
    }

    public static int CalcolaMoneteBoss(
        int apparizione,
        SpecialEncounterBalanceSettings impostazioni
    )
    {
        int baseMonete = impostazioni != null
            ? Mathf.Max(0, impostazioni.moneteBossBase)
            : 28;
        int incremento = impostazioni != null
            ? Mathf.Max(0, impostazioni.moneteBossPerApparizione)
            : 12;
        return SommaSaturata(baseMonete, incremento, apparizione - 1);
    }

    private static int CalcolaValoreCrescente(
        int valoreBase,
        float moltiplicatore
    )
    {
        double risultato = Math.Ceiling(
            Math.Max(1, valoreBase) * Math.Max(1d, moltiplicatore)
        );
        return (int)Math.Min(int.MaxValue, risultato);
    }

    private static int SommaSaturata(int baseValore, int passo, int indice)
    {
        long risultato = Math.Max(0, baseValore) +
            (long)Math.Max(0, passo) * Math.Max(0, indice);
        return (int)Math.Min(int.MaxValue, risultato);
    }
}
