using System.Collections.Generic;
using UnityEngine;

public enum ArchetipoBuild
{
    Tiratore = 0,
    Artigliere = 1,
    SpecialistaCritico = 2,
    Controllore = 3
}

public sealed class DefinizioneArchetipoBuild
{
    private readonly TipoPotenziamento[] sinergie;

    public ArchetipoBuild Tipo { get; }
    public string Nome { get; }
    public string StileGioco { get; }
    public string EtichettaSinergie { get; }
    public string BonusIniziale { get; }
    public PercorsoBuild PercorsoPrincipale { get; }
    public PercorsoBuild PercorsoSecondario { get; }
    public TipoPotenziamento PotenziamentoIniziale { get; }
    public FarmPixelIcon Icona { get; }
    public IReadOnlyList<TipoPotenziamento> Sinergie => sinergie;

    public DefinizioneArchetipoBuild(
        ArchetipoBuild tipo,
        string nome,
        string stileGioco,
        string etichettaSinergie,
        string bonusIniziale,
        PercorsoBuild percorsoPrincipale,
        PercorsoBuild percorsoSecondario,
        TipoPotenziamento potenziamentoIniziale,
        FarmPixelIcon icona,
        params TipoPotenziamento[] nuoveSinergie
    )
    {
        Tipo = tipo;
        Nome = nome ?? string.Empty;
        StileGioco = stileGioco ?? string.Empty;
        EtichettaSinergie = etichettaSinergie ?? string.Empty;
        BonusIniziale = bonusIniziale ?? string.Empty;
        PercorsoPrincipale = percorsoPrincipale;
        PercorsoSecondario = percorsoSecondario;
        PotenziamentoIniziale = potenziamentoIniziale;
        Icona = icona;
        sinergie = nuoveSinergie ?? new TipoPotenziamento[0];
    }

    public bool Consiglia(TipoPotenziamento tipo)
    {
        for (int i = 0; i < sinergie.Length; i++)
        {
            if (sinergie[i] == tipo) return true;
        }
        return false;
    }
}

/// <summary>
/// Identità iniziali morbide: assegnano un bonus gratuito e modificano i
/// pesi dello shop, senza rimuovere alcuna carta dal catalogo.
/// </summary>
public static class CatalogoArchetipiBuild
{
    private static readonly DefinizioneArchetipoBuild[] definizioni =
    {
        new DefinizioneArchetipoBuild(
            ArchetipoBuild.Tiratore,
            "TIRATORE",
            "Molti colpi e pressione costante contro gli sciami.",
            "CADENZA • COLPI EXTRA • RAFFICA",
            "RAPIDITÀ +1",
            PercorsoBuild.Raffica,
            PercorsoBuild.Perforazione,
            TipoPotenziamento.Cadenza,
            FarmPixelIcon.ArchetipoTiratore,
            TipoPotenziamento.ColpoAggiuntivo,
            TipoPotenziamento.RafficaRaccolto,
            TipoPotenziamento.Critico
        ),
        new DefinizioneArchetipoBuild(
            ArchetipoBuild.Artigliere,
            "ARTIGLIERE",
            "Colpi enormi, danno ad area e forte impatto.",
            "DANNO • GIGANTE • ESPLOSIVA • SPINTA",
            "PATATA GIGANTE +1",
            PercorsoBuild.Artiglieria,
            PercorsoBuild.Controllo,
            TipoPotenziamento.PatataGigante,
            FarmPixelIcon.ArchetipoArtigliere,
            TipoPotenziamento.Danno,
            TipoPotenziamento.PatataEsplosiva,
            TipoPotenziamento.Spinta
        ),
        new DefinizioneArchetipoBuild(
            ArchetipoBuild.SpecialistaCritico,
            "SPECIALISTA CRITICO",
            "Colpi precisi che perforano e cercano nuovi bersagli.",
            "CRITICO • PERFORAZIONE • RIMBALZO",
            "CRITICO +1",
            PercorsoBuild.Perforazione,
            PercorsoBuild.Raffica,
            TipoPotenziamento.Critico,
            FarmPixelIcon.ArchetipoCritico,
            TipoPotenziamento.Penetrazione,
            TipoPotenziamento.Rimbalzo,
            TipoPotenziamento.Cadenza
        ),
        new DefinizioneArchetipoBuild(
            ArchetipoBuild.Controllore,
            "CONTROLLORE",
            "Rallenta e respinge le volpi per dominare lo spazio.",
            "RALLENTAMENTO • SPINTA • COLPI EXTRA",
            "RALLENTAMENTO +1",
            PercorsoBuild.Controllo,
            PercorsoBuild.Raffica,
            TipoPotenziamento.Rallentamento,
            FarmPixelIcon.ArchetipoControllore,
            TipoPotenziamento.Spinta,
            TipoPotenziamento.ColpoAggiuntivo,
            TipoPotenziamento.PatataGigante
        )
    };

    private static readonly TipoPotenziamento[] potenziamentiIniziali =
    {
        TipoPotenziamento.Cadenza,
        TipoPotenziamento.PatataGigante,
        TipoPotenziamento.Critico,
        TipoPotenziamento.Rallentamento
    };

    public static IReadOnlyList<DefinizioneArchetipoBuild> Tutte =>
        definizioni;
    public static IReadOnlyList<TipoPotenziamento> PotenziamentiIniziali =>
        potenziamentiIniziali;

    public static DefinizioneArchetipoBuild Ottieni(ArchetipoBuild tipo)
    {
        int indice = Mathf.Clamp((int)tipo, 0, definizioni.Length - 1);
        return definizioni[indice];
    }

    public static DefinizioneArchetipoBuild TrovaDaPotenziamento(
        TipoPotenziamento tipo
    )
    {
        for (int i = 0; i < definizioni.Length; i++)
        {
            if (definizioni[i].PotenziamentoIniziale == tipo)
            {
                return definizioni[i];
            }
        }
        return null;
    }

    public static float MoltiplicatoreOfferta(
        ArchetipoBuild archetipo,
        DefinizionePotenziamentoBuild offerta,
        ShopBalanceSettings impostazioni
    )
    {
        if (offerta == null) return 1f;

        DefinizioneArchetipoBuild definizione = Ottieni(archetipo);
        float primario = impostazioni != null
            ? impostazioni.pesoArchetipoPercorsoPrincipale
            : 1.9f;
        float secondario = impostazioni != null
            ? impostazioni.pesoArchetipoPercorsoSecondario
            : 1.3f;
        float sinergia = impostazioni != null
            ? impostazioni.pesoArchetipoSinergia
            : 1.5f;

        float risultato = 1f;
        if (offerta.Percorso == definizione.PercorsoPrincipale)
        {
            risultato = Mathf.Max(risultato, primario);
        }
        else if (offerta.Percorso == definizione.PercorsoSecondario)
        {
            risultato = Mathf.Max(risultato, secondario);
        }
        if (definizione.Consiglia(offerta.Tipo))
        {
            risultato = Mathf.Max(risultato, sinergia);
        }
        return Mathf.Max(0.01f, risultato);
    }
}
