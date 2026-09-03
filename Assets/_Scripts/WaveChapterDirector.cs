using System;
using UnityEngine;

public enum TipoIncontroOndata
{
    Normale = 0,
    Elite = 1,
    Boss = 2
}

[Serializable]
public sealed class WaveChapterSettings
{
    [Header("Struttura capitoli")]
    [Min(1)] public int ondePerCapitolo = 5;
    [Tooltip("Posizioni nel capitolo dopo cui si apre lo shop completo.")]
    public int[] shopDopoOndeNelCapitolo = { 2, 4 };
    [Min(0.1f)] public float durataTransizioneBreve = 1.15f;
    [Min(1)] public int bossOgniOnde = 10;

    public void Normalizza()
    {
        ondePerCapitolo = Mathf.Max(1, ondePerCapitolo);
        durataTransizioneBreve = Mathf.Clamp(
            durataTransizioneBreve,
            0.1f,
            5f
        );
        bossOgniOnde = Mathf.Max(1, bossOgniOnde);
        if (shopDopoOndeNelCapitolo == null)
        {
            shopDopoOndeNelCapitolo = Array.Empty<int>();
            return;
        }

        int ultimaPosizioneUtile = Mathf.Max(1, ondePerCapitolo - 1);
        for (int i = 0; i < shopDopoOndeNelCapitolo.Length; i++)
        {
            shopDopoOndeNelCapitolo[i] = Mathf.Clamp(
                shopDopoOndeNelCapitolo[i],
                1,
                ultimaPosizioneUtile
            );
        }
    }
}

public readonly struct RitmoOndata
{
    public int NumeroOndata { get; }
    public int NumeroCapitolo { get; }
    public int PosizioneNelCapitolo { get; }
    public int OndePerCapitolo { get; }
    public TipoIncontroOndata TipoIncontro { get; }
    public bool ApreShopDopo { get; }

    public bool IniziaCapitolo => PosizioneNelCapitolo == 1;
    public bool ConcludeCapitolo =>
        PosizioneNelCapitolo == OndePerCapitolo;
    public bool Elite => TipoIncontro == TipoIncontroOndata.Elite;
    public bool Boss => TipoIncontro == TipoIncontroOndata.Boss;

    public RitmoOndata(
        int numeroOndata,
        int numeroCapitolo,
        int posizioneNelCapitolo,
        int ondePerCapitolo,
        TipoIncontroOndata tipoIncontro,
        bool apreShopDopo
    )
    {
        NumeroOndata = numeroOndata;
        NumeroCapitolo = numeroCapitolo;
        PosizioneNelCapitolo = posizioneNelCapitolo;
        OndePerCapitolo = ondePerCapitolo;
        TipoIncontro = tipoIncontro;
        ApreShopDopo = apreShopDopo;
    }
}

/// <summary>
/// Calcola il ritmo della run senza dipendere dalla scena. Il piano resta
/// infinito: capitoli, shop e climax vengono derivati dal numero dell'ondata.
/// </summary>
public static class WaveChapterDirector
{
    private const int OndePerCapitoloPredefinite = 5;
    private const int BossOgniOndePredefinito = 10;

    public static RitmoOndata Calcola(
        int numeroOndata,
        WaveChapterSettings impostazioni
    )
    {
        int onda = Mathf.Max(1, numeroOndata);
        int ondePerCapitolo = impostazioni != null
            ? Mathf.Max(1, impostazioni.ondePerCapitolo)
            : OndePerCapitoloPredefinite;
        int bossOgniOnde = impostazioni != null
            ? Mathf.Max(1, impostazioni.bossOgniOnde)
            : BossOgniOndePredefinito;

        long indiceZeroBased = (long)onda - 1L;
        int capitolo = (int)Math.Min(
            int.MaxValue,
            indiceZeroBased / ondePerCapitolo + 1L
        );
        int posizione = (int)(indiceZeroBased % ondePerCapitolo) + 1;

        bool boss = onda % bossOgniOnde == 0;
        TipoIncontroOndata tipo = boss
            ? TipoIncontroOndata.Boss
            : posizione == ondePerCapitolo
                ? TipoIncontroOndata.Elite
                : TipoIncontroOndata.Normale;

        return new RitmoOndata(
            onda,
            capitolo,
            posizione,
            ondePerCapitolo,
            tipo,
            ContienePosizioneShop(impostazioni, posizione)
        );
    }

    private static bool ContienePosizioneShop(
        WaveChapterSettings impostazioni,
        int posizione
    )
    {
        int[] posizioni = impostazioni != null
            ? impostazioni.shopDopoOndeNelCapitolo
            : null;
        if (posizioni == null)
        {
            return posizione == 2 || posizione == 4;
        }

        for (int i = 0; i < posizioni.Length; i++)
        {
            if (posizioni[i] == posizione) return true;
        }
        return false;
    }
}
