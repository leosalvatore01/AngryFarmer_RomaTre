public readonly struct EsitoDanno
{
    public bool Applicato { get; }
    public bool Ucciso { get; }
    public bool ConsentePenetrazioneAllaMorte { get; }

    public static EsitoDanno NessunDanno =>
        new EsitoDanno(false, false, false);

    public EsitoDanno(
        bool applicato,
        bool ucciso,
        bool consentePenetrazioneAllaMorte
    )
    {
        Applicato = applicato;
        Ucciso = ucciso;
        ConsentePenetrazioneAllaMorte =
            consentePenetrazioneAllaMorte;
    }
}

public interface IDanneggiabile
{
    EsitoDanno ProvaSubireDanno(int quantita);
}
