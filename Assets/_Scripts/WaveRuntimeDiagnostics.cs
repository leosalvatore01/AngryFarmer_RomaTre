using UnityEngine;

public enum EsitoDiagnosticaOndata
{
    Nessuno,
    Completata,
    Sconfitta,
    Interrotta
}

public struct RiepilogoDiagnosticaOndata
{
    public int IndiceOndata { get; }
    public int TotaleOndate { get; }
    public string NomeOndata { get; }
    public EsitoDiagnosticaOndata Esito { get; }
    public int NemiciPrevisti { get; }
    public int TentativiSpawnNemici { get; }
    public int NemiciSpawnati { get; }
    public int NemiciViviFinali { get; }
    public int PiccoNemiciVivi { get; }
    public int MaialiniPrevisti { get; }
    public int TentativiSpawnMaialini { get; }
    public int MaialiniSpawnati { get; }
    public float DurataBanner { get; }
    public float DurataSpawn { get; }
    public float DurataCombattimento { get; }
    public float DurataTotale { get; }

    public bool Valido => Esito != EsitoDiagnosticaOndata.Nessuno;

    public RiepilogoDiagnosticaOndata(
        int indiceOndata,
        int totaleOndate,
        string nomeOndata,
        EsitoDiagnosticaOndata esito,
        int nemiciPrevisti,
        int tentativiSpawnNemici,
        int nemiciSpawnati,
        int nemiciViviFinali,
        int piccoNemiciVivi,
        int maialiniPrevisti,
        int tentativiSpawnMaialini,
        int maialiniSpawnati,
        float durataBanner,
        float durataSpawn,
        float durataCombattimento,
        float durataTotale
    )
    {
        IndiceOndata = indiceOndata;
        TotaleOndate = totaleOndate;
        NomeOndata = nomeOndata;
        Esito = esito;
        NemiciPrevisti = nemiciPrevisti;
        TentativiSpawnNemici = tentativiSpawnNemici;
        NemiciSpawnati = nemiciSpawnati;
        NemiciViviFinali = nemiciViviFinali;
        PiccoNemiciVivi = piccoNemiciVivi;
        MaialiniPrevisti = maialiniPrevisti;
        TentativiSpawnMaialini = tentativiSpawnMaialini;
        MaialiniSpawnati = maialiniSpawnati;
        DurataBanner = durataBanner;
        DurataSpawn = durataSpawn;
        DurataCombattimento = durataCombattimento;
        DurataTotale = durataTotale;
    }
}

[DisallowMultipleComponent]
public sealed class WaveRuntimeDiagnostics : MonoBehaviour
{
    [Header("Attivazione sviluppo")]
    [SerializeField] private bool attivaAllAvvio;
    [SerializeField] private KeyCode tastoToggle = KeyCode.F3;

    [Header("Output")]
    [SerializeField] private bool mostraOverlay = true;
    [SerializeField] private bool scriviLog = true;

    private bool attiva;
    private bool ondaInCorso;
    private bool combattimentoIniziato;
    private bool faseSpawnTerminata;

    private int indiceOndata;
    private int totaleOndate;
    private string nomeOndata;
    private int nemiciPrevisti;
    private int tentativiSpawnNemici;
    private int nemiciSpawnati;
    private int nemiciVivi;
    private int piccoNemiciVivi;
    private int maialiniPrevisti;
    private int tentativiSpawnMaialini;
    private int maialiniSpawnati;

    private double tempoInizioOndata;
    private double tempoInizioCombattimento;
    private double tempoFineSpawn;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private GUIStyle stileOverlay;
#endif

    public bool Attiva => attiva;
    public bool OndaInCorso => ondaInCorso;
    public int NemiciVivi => nemiciVivi;
    public int PiccoNemiciVivi => piccoNemiciVivi;
    public int NemiciSpawnati => nemiciSpawnati;
    public int MaialiniSpawnati => maialiniSpawnati;
    public RiepilogoDiagnosticaOndata UltimoRiepilogo { get; private set; }

    void Awake()
    {
        attiva = attivaAllAvvio;
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    void Update()
    {
        if (Input.GetKeyDown(tastoToggle))
        {
            ImpostaAttiva(!attiva);
        }
    }

    void OnGUI()
    {
        if (!attiva || !mostraOverlay) return;

        if (stileOverlay == null)
        {
            stileOverlay = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 14,
                wordWrap = false,
                padding = new RectOffset(12, 12, 9, 9)
            };
            stileOverlay.normal.textColor = new Color(1f, 0.92f, 0.68f, 1f);
        }

        const float larghezza = 410f;
        const float altezza = 126f;
        Rect area = new Rect(
            Mathf.Max(12f, Screen.width - larghezza - 16f),
            16f,
            larghezza,
            altezza
        );
        GUI.Box(area, CreaTestoOverlay(), stileOverlay);
    }
#endif

    public void ConfiguraOutput(
        bool attivaDiagnostica,
        bool overlayVisibile,
        bool logAbilitato,
        KeyCode tasto = KeyCode.F3
    )
    {
        mostraOverlay = overlayVisibile;
        scriviLog = logAbilitato;
        tastoToggle = tasto;
        ImpostaAttiva(attivaDiagnostica);
    }

    public void ImpostaAttiva(bool valore)
    {
        if (attiva == valore) return;

        attiva = valore;
        if (!attiva)
        {
            ondaInCorso = false;
            combattimentoIniziato = false;
            faseSpawnTerminata = false;
        }

        if (scriviLog)
        {
            Debug.Log(
                attiva
                    ? "[WaveDiag] Diagnostica attiva; le metriche iniziano dalla prossima ondata."
                    : "[WaveDiag] Diagnostica disattivata.",
                this
            );
        }
    }

    public void IniziaOndata(
        int nuovoIndiceOndata,
        int nuovoTotaleOndate,
        string nuovoNomeOndata,
        int nuoviNemiciPrevisti,
        int nuoviMaialiniPrevisti
    )
    {
        if (!attiva) return;

        indiceOndata = Mathf.Max(1, nuovoIndiceOndata);
        totaleOndate = Mathf.Max(indiceOndata, nuovoTotaleOndate);
        nomeOndata = string.IsNullOrWhiteSpace(nuovoNomeOndata)
            ? "Senza nome"
            : nuovoNomeOndata;
        nemiciPrevisti = Mathf.Max(0, nuoviNemiciPrevisti);
        maialiniPrevisti = Mathf.Max(0, nuoviMaialiniPrevisti);

        tentativiSpawnNemici = 0;
        nemiciSpawnati = 0;
        nemiciVivi = 0;
        piccoNemiciVivi = 0;
        tentativiSpawnMaialini = 0;
        maialiniSpawnati = 0;

        tempoInizioOndata = Time.timeAsDouble;
        tempoInizioCombattimento = tempoInizioOndata;
        tempoFineSpawn = tempoInizioOndata;
        ondaInCorso = true;
        combattimentoIniziato = false;
        faseSpawnTerminata = false;
    }

    public void AvviaCombattimento()
    {
        if (!attiva || !ondaInCorso || combattimentoIniziato) return;

        tempoInizioCombattimento = Time.timeAsDouble;
        tempoFineSpawn = tempoInizioCombattimento;
        combattimentoIniziato = true;
    }

    public void RegistraSpawnNemico(bool riuscito)
    {
        if (!attiva || !ondaInCorso) return;

        tentativiSpawnNemici++;
        if (riuscito)
        {
            nemiciSpawnati++;
        }
        CampionaNemiciVivi();
    }

    public void RegistraSpawnMaialino(bool riuscito)
    {
        if (!attiva || !ondaInCorso) return;

        tentativiSpawnMaialini++;
        if (riuscito)
        {
            maialiniSpawnati++;
        }
    }

    public void SegnaFineSpawn()
    {
        if (!attiva || !ondaInCorso || faseSpawnTerminata) return;

        tempoFineSpawn = Time.timeAsDouble;
        faseSpawnTerminata = true;
    }

    public int CampionaNemiciVivi()
    {
        if (!attiva || !ondaInCorso) return nemiciVivi;

        EnemyAI[] nemici = FindObjectsByType<EnemyAI>(
            FindObjectsSortMode.None
        );
        int conteggio = 0;
        foreach (EnemyAI nemico in nemici)
        {
            if (nemico != null && !nemico.IsDead)
            {
                conteggio++;
            }
        }

        nemiciVivi = conteggio;
        piccoNemiciVivi = Mathf.Max(piccoNemiciVivi, nemiciVivi);
        return nemiciVivi;
    }

    public RiepilogoDiagnosticaOndata TerminaOndata(
        EsitoDiagnosticaOndata esito
    )
    {
        if (!attiva || !ondaInCorso)
        {
            return UltimoRiepilogo;
        }

        if (esito == EsitoDiagnosticaOndata.Nessuno)
        {
            esito = EsitoDiagnosticaOndata.Interrotta;
        }

        CampionaNemiciVivi();
        if (!faseSpawnTerminata)
        {
            SegnaFineSpawn();
        }

        double adesso = Time.timeAsDouble;
        double inizioCombattimento = combattimentoIniziato
            ? tempoInizioCombattimento
            : tempoInizioOndata;

        UltimoRiepilogo = new RiepilogoDiagnosticaOndata(
            indiceOndata,
            totaleOndate,
            nomeOndata,
            esito,
            nemiciPrevisti,
            tentativiSpawnNemici,
            nemiciSpawnati,
            nemiciVivi,
            piccoNemiciVivi,
            maialiniPrevisti,
            tentativiSpawnMaialini,
            maialiniSpawnati,
            Mathf.Max(0f, (float)(inizioCombattimento - tempoInizioOndata)),
            Mathf.Max(0f, (float)(tempoFineSpawn - inizioCombattimento)),
            Mathf.Max(0f, (float)(adesso - inizioCombattimento)),
            Mathf.Max(0f, (float)(adesso - tempoInizioOndata))
        );

        ondaInCorso = false;
        combattimentoIniziato = false;
        faseSpawnTerminata = false;

        if (scriviLog)
        {
            Debug.Log(CreaTestoRiepilogo(UltimoRiepilogo), this);
        }
        return UltimoRiepilogo;
    }

    private string CreaTestoOverlay()
    {
        if (!ondaInCorso)
        {
            return UltimoRiepilogo.Valido
                ? "WAVE DIAGNOSTICS  [F3]\n" +
                  "In attesa - ultimo esito: " + UltimoRiepilogo.Esito + "\n" +
                  "Ultima ondata: " + UltimoRiepilogo.IndiceOndata + "/" +
                  UltimoRiepilogo.TotaleOndate +
                  "  durata " + UltimoRiepilogo.DurataCombattimento.ToString("F2") + " s"
                : "WAVE DIAGNOSTICS  [F3]\nIn attesa della prossima ondata.";
        }

        double inizio = combattimentoIniziato
            ? tempoInizioCombattimento
            : tempoInizioOndata;
        float durata = Mathf.Max(0f, (float)(Time.timeAsDouble - inizio));
        return
            "WAVE DIAGNOSTICS  [F3]\n" +
            "Ondata " + indiceOndata + "/" + totaleOndate + " - " + nomeOndata + "\n" +
            "Tempo " + durata.ToString("F2") + " s  |  Spawn " +
            nemiciSpawnati + "/" + nemiciPrevisti + "\n" +
            "Vivi " + nemiciVivi + "  |  Picco " + piccoNemiciVivi +
            "  |  Maialini " + maialiniSpawnati + "/" + maialiniPrevisti;
    }

    private static string CreaTestoRiepilogo(
        RiepilogoDiagnosticaOndata riepilogo
    )
    {
        return
            "[WaveDiag] #" + riepilogo.IndiceOndata + "/" +
            riepilogo.TotaleOndate + " " + riepilogo.NomeOndata +
            " | esito=" + riepilogo.Esito +
            " | durata=" + riepilogo.DurataCombattimento.ToString("F2") + "s" +
            " (banner=" + riepilogo.DurataBanner.ToString("F2") +
            "s, spawn=" + riepilogo.DurataSpawn.ToString("F2") + "s)" +
            " | volpi=" + riepilogo.NemiciSpawnati + "/" +
            riepilogo.NemiciPrevisti + " tentativi=" +
            riepilogo.TentativiSpawnNemici + " vivi=" +
            riepilogo.NemiciViviFinali + " picco=" +
            riepilogo.PiccoNemiciVivi + " | maialini=" +
            riepilogo.MaialiniSpawnati + "/" + riepilogo.MaialiniPrevisti;
    }
}
