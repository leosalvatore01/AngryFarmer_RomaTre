using System;
using System.Collections.Generic;
using UnityEngine;

public enum TipoDropTemporaneo
{
    TriploSparo = 0,
    Velocita = 1,
    Scudo = 2,
    Cura = 3,
    Calamita = 4,
    Furia = 5,
    Esplosione = 6
}

public readonly struct DefinizioneDropTemporaneo
{
    public TipoDropTemporaneo Tipo { get; }
    public string Nome { get; }
    public string Effetto { get; }
    public string EtichettaCompatta { get; }
    public FarmPixelIcon Icona { get; }
    public Color32 Colore { get; }
    public bool Istantaneo { get; }

    public DefinizioneDropTemporaneo(
        TipoDropTemporaneo tipo,
        string nome,
        string effetto,
        string etichettaCompatta,
        FarmPixelIcon icona,
        Color32 colore,
        bool istantaneo = false
    )
    {
        Tipo = tipo;
        Nome = nome;
        Effetto = effetto;
        EtichettaCompatta = etichettaCompatta;
        Icona = icona;
        Colore = colore;
        Istantaneo = istantaneo;
    }
}

/// <summary>
/// Catalogo unico dei drop da combattimento. I primi due valori mantengono la
/// compatibilita con i vecchi prefab Dente e Coda.
/// </summary>
public static class CatalogoDropTemporanei
{
    private static readonly DefinizioneDropTemporaneo[] definizioni =
    {
        new DefinizioneDropTemporaneo(
            TipoDropTemporaneo.TriploSparo,
            "Dente della raffica",
            "Triplo colpo",
            "TRIPLO",
            FarmPixelIcon.TriploSparo,
            new Color32(181, 115, 217, 255)
        ),
        new DefinizioneDropTemporaneo(
            TipoDropTemporaneo.Velocita,
            "Coda scattante",
            "Velocita temporanea",
            "SCATTO",
            FarmPixelIcon.BoostVelocita,
            new Color32(244, 196, 61, 255)
        ),
        new DefinizioneDropTemporaneo(
            TipoDropTemporaneo.Scudo,
            "Scudo di paglia",
            "Scudo temporaneo",
            "SCUDO",
            FarmPixelIcon.ScudoTemporaneo,
            new Color32(86, 205, 232, 255)
        ),
        new DefinizioneDropTemporaneo(
            TipoDropTemporaneo.Cura,
            "Razione fresca",
            "Cura immediata",
            "CURA",
            FarmPixelIcon.CuraTemporanea,
            new Color32(105, 218, 112, 255),
            true
        ),
        new DefinizioneDropTemporaneo(
            TipoDropTemporaneo.Calamita,
            "Calamita da raccolto",
            "Attrae i drop vicini",
            "MAGNETE",
            FarmPixelIcon.Calamita,
            new Color32(93, 207, 215, 255)
        ),
        new DefinizioneDropTemporaneo(
            TipoDropTemporaneo.Furia,
            "Furia contadina",
            "Danno e cadenza aumentati",
            "FURIA",
            FarmPixelIcon.Furia,
            new Color32(242, 92, 54, 255)
        ),
        new DefinizioneDropTemporaneo(
            TipoDropTemporaneo.Esplosione,
            "Raccolto esplosivo",
            "Esplosione immediata",
            "BOOM",
            FarmPixelIcon.EsplosioneIstantanea,
            new Color32(255, 153, 42, 255),
            true
        )
    };

    public static IReadOnlyList<DefinizioneDropTemporaneo> Tutte =>
        definizioni;

    public static TipoDropTemporaneo Normalizza(int valore)
    {
        return valore >= 0 && valore < definizioni.Length
            ? (TipoDropTemporaneo)valore
            : TipoDropTemporaneo.TriploSparo;
    }

    public static DefinizioneDropTemporaneo Ottieni(
        TipoDropTemporaneo tipo
    )
    {
        return definizioni[(int)Normalizza((int)tipo)];
    }
}

/// <summary>
/// Gestisce casualita, varieta e protezione dalla sfortuna senza dipendere da
/// MonoBehaviour, cosi il comportamento e testabile in modo deterministico.
/// </summary>
public sealed class TemporaryDropDirector
{
    private readonly FoxBalanceSettings impostazioni;
    private readonly System.Random casualita;
    private TipoDropTemporaneo ultimoTipo;
    private int ripetizioniUltimoTipo;

    public int EliminazioniSenzaDrop { get; private set; }
    public TipoDropTemporaneo UltimoTipo => ultimoTipo;

    public TemporaryDropDirector(
        FoxBalanceSettings impostazioni,
        int? seed = null
    )
    {
        this.impostazioni = impostazioni ?? new FoxBalanceSettings();
        casualita = seed.HasValue
            ? new System.Random(seed.Value)
            : new System.Random(Environment.TickCount);
        ultimoTipo = TipoDropTemporaneo.TriploSparo;
    }

    public bool DeveCreareDrop(float estrazione01, bool garantito = false)
    {
        int limite = Mathf.Max(1, impostazioni.uccisioniSenzaDropMassime);
        bool protezioneSfortuna = EliminazioniSenzaDrop >= limite - 1;
        bool successo = garantito || protezioneSfortuna ||
            Mathf.Clamp01(estrazione01) <
            Mathf.Clamp(impostazioni.probabilitaDrop / 100f, 0f, 1f);

        if (successo) EliminazioniSenzaDrop = 0;
        else EliminazioniSenzaDrop++;
        return successo;
    }

    public TipoDropTemporaneo EstraiTipo(bool vitaPiena)
    {
        float[] pesi =
        {
            impostazioni.pesoDropTriploSparo,
            impostazioni.pesoDropVelocita,
            impostazioni.pesoDropScudo,
            vitaPiena ? 0f : impostazioni.pesoDropCura,
            impostazioni.pesoDropCalamita,
            impostazioni.pesoDropFuria,
            impostazioni.pesoDropEsplosione
        };

        int ripetizioniMassime = Mathf.Max(
            1,
            impostazioni.ripetizioniMassimeStessoDrop
        );
        if (ripetizioniUltimoTipo >= ripetizioniMassime)
        {
            pesi[(int)ultimoTipo] = 0f;
        }

        float totale = 0f;
        for (int i = 0; i < pesi.Length; i++)
        {
            pesi[i] = Mathf.Max(0f, pesi[i]);
            totale += pesi[i];
        }

        if (totale <= 0f)
        {
            ultimoTipo = TipoDropTemporaneo.TriploSparo;
            ripetizioniUltimoTipo = 1;
            return ultimoTipo;
        }

        float estrazione = (float)casualita.NextDouble() * totale;
        TipoDropTemporaneo scelto = TipoDropTemporaneo.TriploSparo;
        for (int i = 0; i < pesi.Length; i++)
        {
            estrazione -= pesi[i];
            if (estrazione <= 0f)
            {
                scelto = (TipoDropTemporaneo)i;
                break;
            }
        }

        if (scelto == ultimoTipo) ripetizioniUltimoTipo++;
        else
        {
            ultimoTipo = scelto;
            ripetizioniUltimoTipo = 1;
        }
        return scelto;
    }
}
