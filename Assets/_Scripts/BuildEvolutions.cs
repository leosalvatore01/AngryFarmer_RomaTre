using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class DefinizioneEvoluzioneBuild
{
    public PercorsoBuild Percorso { get; }
    public string NomePrimoStadio { get; }
    public string NomeSecondoStadio { get; }
    public string EtichettaCompatta { get; }
    public string DescrizionePrimoStadio { get; }
    public string DescrizioneSecondoStadio { get; }
    public FarmPixelIcon Icona { get; }

    public DefinizioneEvoluzioneBuild(
        PercorsoBuild percorso,
        string nomePrimoStadio,
        string nomeSecondoStadio,
        string etichettaCompatta,
        string descrizionePrimoStadio,
        string descrizioneSecondoStadio,
        FarmPixelIcon icona
    )
    {
        Percorso = percorso;
        NomePrimoStadio = nomePrimoStadio ?? string.Empty;
        NomeSecondoStadio = nomeSecondoStadio ?? string.Empty;
        EtichettaCompatta = etichettaCompatta ?? string.Empty;
        DescrizionePrimoStadio = descrizionePrimoStadio ?? string.Empty;
        DescrizioneSecondoStadio = descrizioneSecondoStadio ?? string.Empty;
        Icona = icona;
    }

    public string OttieniNome(int livello)
    {
        return livello >= 2 ? NomeSecondoStadio : NomePrimoStadio;
    }

    public string OttieniDescrizione(int livello)
    {
        return livello >= 2
            ? DescrizioneSecondoStadio
            : DescrizionePrimoStadio;
    }
}

/// <summary>
/// Soglie qualitative delle quattro build. I livelli oltre la seconda soglia
/// restano infiniti, ma il contributo numerico cresce in modo sublineare.
/// </summary>
public static class CatalogoEvoluzioniBuild
{
    private static readonly DefinizioneEvoluzioneBuild[] definizioni =
    {
        new DefinizioneEvoluzioneBuild(
            PercorsoBuild.Raffica,
            "SEMI BALISTICI",
            "RACCOLTO RAMIFICATO",
            "DIVISIONE",
            "Ogni proiettile genera due frammenti laterali al primo impatto.",
            "La divisione genera tre frammenti piu potenti.",
            FarmPixelIcon.EvoluzioneRaffica
        ),
        new DefinizioneEvoluzioneBuild(
            PercorsoBuild.Artiglieria,
            "ECO ESPLOSIVA",
            "SCOSSA DEL RACCOLTO",
            "SCOPPIO +",
            "Gli impatti esplodono e producono una seconda detonazione.",
            "Ogni esplosione lascia due detonazioni secondarie.",
            FarmPixelIcon.EvoluzioneArtiglieria
        ),
        new DefinizioneEvoluzioneBuild(
            PercorsoBuild.Perforazione,
            "TRAPASSO",
            "TRAIETTORIA IMPOSSIBILE",
            "TRAPASSO",
            "I colpi attraversano anche le volpi che sopravvivono.",
            "Dopo l'attraversamento il colpo cerca un nuovo bersaglio.",
            FarmPixelIcon.EvoluzionePerforazione
        ),
        new DefinizioneEvoluzioneBuild(
            PercorsoBuild.Controllo,
            "GELO CONTAGIOSO",
            "CAMPO DI BONIFICA",
            "CONTAGIO",
            "Il rallentamento si propaga alle volpi vicine.",
            "Il contagio copre un'area maggiore ed e quasi a piena forza.",
            FarmPixelIcon.EvoluzioneControllo
        )
    };

    public static IReadOnlyList<DefinizioneEvoluzioneBuild> Tutte =>
        definizioni;

    public static int SogliaPrimoStadio(ShopBalanceSettings impostazioni)
    {
        return Mathf.Max(
            1,
            impostazioni != null
                ? impostazioni.sogliaPrimaEvoluzione
                : 3
        );
    }

    public static int SogliaSecondoStadio(ShopBalanceSettings impostazioni)
    {
        return Mathf.Max(
            SogliaPrimoStadio(impostazioni) + 1,
            impostazioni != null
                ? impostazioni.sogliaSecondaEvoluzione
                : 6
        );
    }

    public static int OttieniLivello(
        int puntiPercorso,
        ShopBalanceSettings impostazioni
    )
    {
        int punti = Mathf.Max(0, puntiPercorso);
        if (punti >= SogliaSecondoStadio(impostazioni)) return 2;
        return punti >= SogliaPrimoStadio(impostazioni) ? 1 : 0;
    }

    public static DefinizioneEvoluzioneBuild Ottieni(PercorsoBuild percorso)
    {
        for (int i = 0; i < definizioni.Length; i++)
        {
            if (definizioni[i].Percorso == percorso) return definizioni[i];
        }
        return null;
    }

    public static double CalcolaProgressoDecrescente(
        int livello,
        ShopBalanceSettings impostazioni
    )
    {
        int valore = Mathf.Max(0, livello);
        int soglia = SogliaSecondoStadio(impostazioni);
        if (valore <= soglia) return valore;

        double esponente = Mathf.Clamp(
            impostazioni != null
                ? impostazioni.esponenteRendimentoDecrescente
                : 0.65f,
            0.45f,
            0.9f
        );
        return soglia + Math.Pow(valore - soglia, esponente);
    }
}
