public readonly struct EsitoDanno
{
    public bool Applicato { get; }
    public bool Ucciso { get; }
    public bool ConsentePenetrazioneAllaMorte { get; }
    public int DannoApplicato { get; }

    public static EsitoDanno NessunDanno =>
        new EsitoDanno(false, false, false);

    public EsitoDanno(
        bool applicato,
        bool ucciso,
        bool consentePenetrazioneAllaMorte,
        int dannoApplicato = 0
    )
    {
        Applicato = applicato;
        Ucciso = ucciso;
        ConsentePenetrazioneAllaMorte =
            consentePenetrazioneAllaMorte;
        DannoApplicato = dannoApplicato;
    }
}

public interface IDanneggiabile
{
    EsitoDanno ProvaSubireDanno(int quantita);
}
