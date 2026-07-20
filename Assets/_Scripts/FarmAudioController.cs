using System;
using UnityEngine;

/// <summary>
/// Regia audio globale della fattoria. Genera musica ed effetti a runtime,
/// senza dipendere da clip importate o da oggetti presenti nella scena.
/// </summary>
public sealed class FarmAudioController : MonoBehaviour
{
    private const int FrequenzaCampionamento = 22050;
    private const int NumeroSorgentiEffetti = 8;
    private const int BattitiLoop = 16;
    private const float BattitiPerMinuto = 92f;
    private const float VolumeBaseMusicaCalma = 0.135f;
    private const float VolumeBaseMusicaIntensa = 0.095f;
    private const float IntensitaFuoriOnda = 0.1f;
    private const float RapiditaDissolvenza = 4.6f;
    private const float CooldownMoneta = 0.075f;

    private enum TipoEffetto
    {
        Moneta,
        Acquisto,
        Pericolo,
        Successo,
        Interfaccia
    }

    private AudioSource musicaCalma;
    private AudioSource musicaIntensa;
    private AudioSource[] sorgentiEffetti;
    private float[] volumiBaseSorgenti;

    private AudioClip clipMusicaCalma;
    private AudioClip clipMusicaIntensa;
    private AudioClip clipMoneta;
    private AudioClip clipAcquisto;
    private AudioClip clipPericolo;
    private AudioClip clipSuccesso;
    private AudioClip clipInterfaccia;

    private GameOptionsController opzioniCollegate;
    private float volumeMusica = 1f;
    private float volumeEffetti = 1f;
    private float volumeCalmaAttuale;
    private float volumeIntensoAttuale;
    private float ultimoSuonoMoneta = float.NegativeInfinity;
    private float ultimoSuonoPericolo = float.NegativeInfinity;
    private float ultimoSuonoInterfaccia = float.NegativeInfinity;
    private int prossimaSorgente;
    private int varianteMoneta;
    private bool musicaAvviata;

    public static FarmAudioController Instance { get; private set; }

    public float VolumeMusicaApplicato => volumeMusica;
    public float VolumeEffettiApplicato => volumeEffetti;
    public float LivelloMusicaCalma => volumeCalmaAttuale;
    public float LivelloMusicaIntensa => volumeIntensoAttuale;
    public bool MusicaAvviata => musicaAvviata;
    public bool OndaMusicalmenteAttiva => OndaAttiva();
    public int DimensionePoolEffetti => sorgentiEffetti != null
        ? sorgentiEffetti.Length
        : 0;

    public int SuoniMonetaRiprodotti { get; private set; }
    public int SuoniAcquistoRiprodotti { get; private set; }
    public int SuoniPericoloRiprodotti { get; private set; }
    public int SuoniSuccessoRiprodotti { get; private set; }
    public int SuoniInterfacciaRiprodotti { get; private set; }

    public AudioClip ClipMusicaCalmaDiagnostica => clipMusicaCalma;
    public AudioClip ClipMusicaIntensaDiagnostica => clipMusicaIntensa;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void AzzeraStatoStatico()
    {
        Instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreaDopoCaricamentoScena()
    {
        CreaOTrova();
    }

    public static FarmAudioController CreaOTrova()
    {
        if (Instance != null) return Instance;

        FarmAudioController esistente =
            FindFirstObjectByType<FarmAudioController>();
        if (esistente != null)
        {
            Instance = esistente;
            return esistente;
        }

        GameObject oggetto = new GameObject("AudioFattoria");
        return oggetto.AddComponent<FarmAudioController>();
    }

    public static bool RiproduciMoneta(float intensita = 1f)
    {
        FarmAudioController controller = CreaOTrova();
        return controller != null && controller.SuonaMoneta(intensita);
    }

    public static bool RiproduciAcquisto(float intensita = 1f)
    {
        FarmAudioController controller = CreaOTrova();
        return controller != null && controller.Suona(
            TipoEffetto.Acquisto,
            intensita,
            1f
        );
    }

    public static bool RiproduciPericolo(float intensita = 1f)
    {
        FarmAudioController controller = CreaOTrova();
        return controller != null && controller.SuonaPericolo(intensita);
    }

    public static bool RiproduciSuccesso(float intensita = 1f)
    {
        FarmAudioController controller = CreaOTrova();
        return controller != null && controller.Suona(
            TipoEffetto.Successo,
            intensita,
            1f
        );
    }

    public static bool RiproduciInterfaccia(float intensita = 1f)
    {
        FarmAudioController controller = CreaOTrova();
        return controller != null && controller.SuonaInterfaccia(intensita);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        CreaMusica();
        CreaPoolEffetti();
        CreaClipEffetti();
        CollegaOpzioni();
        AvviaMusicaSincronizzata();
    }

    private void Update()
    {
        CollegaOpzioni();
        AggiornaMusica();
    }

    private void CollegaOpzioni()
    {
        GameOptionsController correnti = GameOptionsController.Instance;
        if (correnti == opzioniCollegate) return;

        if (opzioniCollegate != null)
        {
            opzioniCollegate.ImpostazioniCambiate -= AggiornaVolumiDaOpzioni;
        }

        opzioniCollegate = correnti;
        if (opzioniCollegate != null)
        {
            opzioniCollegate.ImpostazioniCambiate += AggiornaVolumiDaOpzioni;
            AggiornaVolumiDaOpzioni();
        }
    }

    private void AggiornaVolumiDaOpzioni()
    {
        if (opzioniCollegate == null) return;

        volumeMusica = Mathf.Clamp01(opzioniCollegate.VolumeMusica);
        volumeEffetti = Mathf.Clamp01(opzioniCollegate.VolumeEffetti);
        AggiornaVolumiEffettiInRiproduzione();
    }

    private void AggiornaMusica()
    {
        float intensita = OndaAttiva() ? 1f : IntensitaFuoriOnda;
        float obiettivoCalma = VolumeBaseMusicaCalma * volumeMusica;
        float obiettivoIntensa =
            VolumeBaseMusicaIntensa * volumeMusica * intensita;
        float delta = Mathf.Max(0f, Time.unscaledDeltaTime);
        float interpolazione = 1f - Mathf.Exp(-RapiditaDissolvenza * delta);

        volumeCalmaAttuale = Mathf.Lerp(
            volumeCalmaAttuale,
            obiettivoCalma,
            interpolazione
        );
        volumeIntensoAttuale = Mathf.Lerp(
            volumeIntensoAttuale,
            obiettivoIntensa,
            interpolazione
        );

        if (musicaCalma != null) musicaCalma.volume = volumeCalmaAttuale;
        if (musicaIntensa != null) musicaIntensa.volume = volumeIntensoAttuale;
    }

    private static bool OndaAttiva()
    {
        GameManager gestore = GameManager.instance;
        return gestore != null && gestore.GameplayAttivo;
    }

    private bool SuonaMoneta(float intensita)
    {
        float adesso = Time.unscaledTime;
        if (adesso - ultimoSuonoMoneta < CooldownMoneta) return false;

        ultimoSuonoMoneta = adesso;
        float[] intonazioni = { 0.96f, 1.04f, 1.11f };
        float intonazione = intonazioni[varianteMoneta % intonazioni.Length];
        varianteMoneta++;
        return Suona(TipoEffetto.Moneta, intensita, intonazione);
    }

    private bool SuonaPericolo(float intensita)
    {
        float adesso = Time.unscaledTime;
        if (adesso - ultimoSuonoPericolo < 0.18f) return false;

        ultimoSuonoPericolo = adesso;
        return Suona(TipoEffetto.Pericolo, intensita, 1f);
    }

    private bool SuonaInterfaccia(float intensita)
    {
        float adesso = Time.unscaledTime;
        if (adesso - ultimoSuonoInterfaccia < 0.035f) return false;

        ultimoSuonoInterfaccia = adesso;
        return Suona(TipoEffetto.Interfaccia, intensita, 1f);
    }

    private bool Suona(
        TipoEffetto tipo,
        float intensita,
        float intonazione
    )
    {
        if (volumeEffetti <= 0.0001f || intensita <= 0f) return false;

        AudioClip clip = ClipPer(tipo);
        AudioSource sorgente = OttieniSorgenteEffetti();
        if (clip == null || sorgente == null) return false;

        float volumeBase = VolumeBasePer(tipo) * Mathf.Clamp01(intensita);
        int indice = Array.IndexOf(sorgentiEffetti, sorgente);
        if (indice >= 0 && indice < volumiBaseSorgenti.Length)
        {
            volumiBaseSorgenti[indice] = volumeBase;
        }

        sorgente.Stop();
        sorgente.clip = clip;
        sorgente.pitch = Mathf.Clamp(intonazione, 0.75f, 1.3f);
        sorgente.volume = volumeBase * volumeEffetti;
        sorgente.Play();
        RegistraEffetto(tipo);
        return true;
    }

    private AudioClip ClipPer(TipoEffetto tipo)
    {
        switch (tipo)
        {
            case TipoEffetto.Moneta:
                return clipMoneta;
            case TipoEffetto.Acquisto:
                return clipAcquisto;
            case TipoEffetto.Pericolo:
                return clipPericolo;
            case TipoEffetto.Successo:
                return clipSuccesso;
            default:
                return clipInterfaccia;
        }
    }

    private static float VolumeBasePer(TipoEffetto tipo)
    {
        switch (tipo)
        {
            case TipoEffetto.Moneta:
                return 0.31f;
            case TipoEffetto.Acquisto:
                return 0.34f;
            case TipoEffetto.Pericolo:
                return 0.38f;
            case TipoEffetto.Successo:
                return 0.36f;
            default:
                return 0.22f;
        }
    }

    private void RegistraEffetto(TipoEffetto tipo)
    {
        switch (tipo)
        {
            case TipoEffetto.Moneta:
                SuoniMonetaRiprodotti++;
                break;
            case TipoEffetto.Acquisto:
                SuoniAcquistoRiprodotti++;
                break;
            case TipoEffetto.Pericolo:
                SuoniPericoloRiprodotti++;
                break;
            case TipoEffetto.Successo:
                SuoniSuccessoRiprodotti++;
                break;
            default:
                SuoniInterfacciaRiprodotti++;
                break;
        }
    }

    private AudioSource OttieniSorgenteEffetti()
    {
        if (sorgentiEffetti == null || sorgentiEffetti.Length == 0)
        {
            return null;
        }

        for (int i = 0; i < sorgentiEffetti.Length; i++)
        {
            int indice = (prossimaSorgente + i) % sorgentiEffetti.Length;
            if (!sorgentiEffetti[indice].isPlaying)
            {
                prossimaSorgente = (indice + 1) % sorgentiEffetti.Length;
                return sorgentiEffetti[indice];
            }
        }

        AudioSource riutilizzata = sorgentiEffetti[prossimaSorgente];
        prossimaSorgente = (prossimaSorgente + 1) % sorgentiEffetti.Length;
        return riutilizzata;
    }

    private void AggiornaVolumiEffettiInRiproduzione()
    {
        if (sorgentiEffetti == null || volumiBaseSorgenti == null) return;

        for (int i = 0; i < sorgentiEffetti.Length; i++)
        {
            if (sorgentiEffetti[i] != null)
            {
                sorgentiEffetti[i].volume =
                    volumiBaseSorgenti[i] * volumeEffetti;
            }
        }
    }

    private void CreaMusica()
    {
        clipMusicaCalma = CreaClipMusicaCalma();
        clipMusicaIntensa = CreaClipMusicaIntensa();
        musicaCalma = CreaSorgente("MusicaCalma", true);
        musicaIntensa = CreaSorgente("MusicaIntensa", true);
        musicaCalma.clip = clipMusicaCalma;
        musicaIntensa.clip = clipMusicaIntensa;
        musicaCalma.volume = 0f;
        musicaIntensa.volume = 0f;
    }

    private void CreaPoolEffetti()
    {
        sorgentiEffetti = new AudioSource[NumeroSorgentiEffetti];
        volumiBaseSorgenti = new float[NumeroSorgentiEffetti];
        for (int i = 0; i < sorgentiEffetti.Length; i++)
        {
            sorgentiEffetti[i] = CreaSorgente("Effetto_" + i, false);
        }
    }

    private AudioSource CreaSorgente(string nome, bool loop)
    {
        GameObject oggetto = new GameObject(nome);
        oggetto.transform.SetParent(transform, false);
        AudioSource sorgente = oggetto.AddComponent<AudioSource>();
        sorgente.playOnAwake = false;
        sorgente.loop = loop;
        sorgente.spatialBlend = 0f;
        sorgente.dopplerLevel = 0f;
        sorgente.ignoreListenerPause = true;
        return sorgente;
    }

    private void AvviaMusicaSincronizzata()
    {
        if (musicaCalma == null || musicaIntensa == null ||
            clipMusicaCalma == null || clipMusicaIntensa == null)
        {
            return;
        }

        double inizio = AudioSettings.dspTime + 0.05d;
        musicaCalma.PlayScheduled(inizio);
        musicaIntensa.PlayScheduled(inizio);
        musicaAvviata = true;
    }

    private void CreaClipEffetti()
    {
        clipMoneta = CreaClipMoneta();
        clipAcquisto = CreaClipAcquisto();
        clipPericolo = CreaClipPericolo();
        clipSuccesso = CreaClipSuccesso();
        clipInterfaccia = CreaClipInterfaccia();
    }

    private static AudioClip CreaClipMusicaCalma()
    {
        float durataBattito = 60f / BattitiPerMinuto;
        float durata = durataBattito * BattitiLoop;
        int campioni = Mathf.RoundToInt(durata * FrequenzaCampionamento);
        float[] dati = new float[campioni];
        float[] melodia =
        {
            261.63f, 329.63f, 392f, 440f,
            392f, 329.63f, 293.66f, 329.63f,
            261.63f, 329.63f, 392f, 523.25f,
            440f, 392f, 329.63f, 293.66f
        };
        float[] bassi =
        {
            130.81f, 130.81f, 146.83f, 164.81f,
            130.81f, 174.61f, 164.81f, 146.83f
        };

        for (int i = 0; i < campioni; i++)
        {
            float tempo = (float)i / FrequenzaCampionamento;
            float battitoContinuo = tempo / durataBattito;
            int battito = Mathf.FloorToInt(battitoContinuo) % BattitiLoop;
            float faseBattito = battitoContinuo - Mathf.Floor(battitoContinuo);
            float inviluppoMelodia = InviluppoPizzicato(faseBattito, 0.055f, 2.15f);
            float tono = OndaPixel(melodia[battito] * tempo);

            int battitoDoppio = battito / 2;
            float faseBasso = (battitoContinuo * 0.5f) -
                              Mathf.Floor(battitoContinuo * 0.5f);
            float inviluppoBasso = InviluppoPizzicato(faseBasso, 0.035f, 1.7f);
            float basso = Mathf.Sin(
                bassi[battitoDoppio % bassi.Length] * tempo * Mathf.PI * 2f
            );

            float campione = tono * inviluppoMelodia * 0.13f +
                              basso * inviluppoBasso * 0.075f;
            dati[i] = Quantizza(campione, 96f);
        }

        return CreaClip("MusicaFattoria_Calma", dati);
    }

    private static AudioClip CreaClipMusicaIntensa()
    {
        float durataBattito = 60f / BattitiPerMinuto;
        float durata = durataBattito * BattitiLoop;
        int campioni = Mathf.RoundToInt(durata * FrequenzaCampionamento);
        float[] dati = new float[campioni];
        float[] arpeggio =
        {
            523.25f, 659.25f, 783.99f, 659.25f,
            587.33f, 698.46f, 880f, 698.46f
        };
        uint rumore = 0xA341316Cu;

        for (int i = 0; i < campioni; i++)
        {
            float tempo = (float)i / FrequenzaCampionamento;
            float battitoContinuo = tempo / durataBattito;
            float mezzoBattito = battitoContinuo * 2f;
            int passo = Mathf.FloorToInt(mezzoBattito);
            float fasePasso = mezzoBattito - Mathf.Floor(mezzoBattito);
            float inviluppoArpeggio = InviluppoPizzicato(fasePasso, 0.035f, 3.2f);
            float tono = OndaPixel(
                arpeggio[passo % arpeggio.Length] * tempo
            );

            float faseBattito = battitoContinuo - Mathf.Floor(battitoContinuo);
            float tempoColpo = faseBattito * durataBattito;
            float frequenzaGrancassa = Mathf.Lerp(94f, 48f, faseBattito);
            float grancassa = Mathf.Sin(
                tempoColpo * frequenzaGrancassa * Mathf.PI * 2f
            ) * Mathf.Pow(1f - faseBattito, 4.2f);

            rumore = rumore * 1664525u + 1013904223u;
            float valoreRumore = ((rumore >> 9) & 0x7FFFFF) /
                                 4194303.5f - 1f;
            float charleston = valoreRumore *
                               Mathf.Pow(1f - fasePasso, 7f);

            float campione = tono * inviluppoArpeggio * 0.105f +
                              grancassa * 0.13f +
                              charleston * 0.035f;
            dati[i] = Quantizza(campione, 80f);
        }

        return CreaClip("MusicaFattoria_Ondata", dati);
    }

    private static AudioClip CreaClipMoneta()
    {
        const float durata = 0.145f;
        int campioni = Mathf.CeilToInt(durata * FrequenzaCampionamento);
        float[] dati = new float[campioni];
        for (int i = 0; i < campioni; i++)
        {
            float t = (float)i / FrequenzaCampionamento;
            float primo = NotaPixel(t, 0f, 0.09f, 987.77f);
            float secondo = NotaPixel(t, 0.062f, 0.083f, 1318.51f);
            dati[i] = Quantizza(primo * 0.34f + secondo * 0.29f, 64f);
        }
        return CreaClip("Sfx_Moneta", dati);
    }

    private static AudioClip CreaClipAcquisto()
    {
        const float durata = 0.255f;
        int campioni = Mathf.CeilToInt(durata * FrequenzaCampionamento);
        float[] dati = new float[campioni];
        for (int i = 0; i < campioni; i++)
        {
            float t = (float)i / FrequenzaCampionamento;
            float valore =
                NotaPixel(t, 0f, 0.11f, 523.25f) * 0.25f +
                NotaPixel(t, 0.068f, 0.115f, 659.25f) * 0.27f +
                NotaPixel(t, 0.142f, 0.113f, 783.99f) * 0.3f;
            dati[i] = Quantizza(valore, 72f);
        }
        return CreaClip("Sfx_Acquisto", dati);
    }

    private static AudioClip CreaClipPericolo()
    {
        const float durata = 0.31f;
        int campioni = Mathf.CeilToInt(durata * FrequenzaCampionamento);
        float[] dati = new float[campioni];
        for (int i = 0; i < campioni; i++)
        {
            float t = (float)i / FrequenzaCampionamento;
            float prima = NotaAllarme(t, 0f, 0.145f, 220f);
            float seconda = NotaAllarme(t, 0.155f, 0.155f, 164.81f);
            dati[i] = Quantizza(prima * 0.31f + seconda * 0.34f, 56f);
        }
        return CreaClip("Sfx_Pericolo", dati);
    }

    private static AudioClip CreaClipSuccesso()
    {
        const float durata = 0.42f;
        int campioni = Mathf.CeilToInt(durata * FrequenzaCampionamento);
        float[] dati = new float[campioni];
        float[] inizi = { 0f, 0.075f, 0.15f, 0.235f };
        float[] frequenze = { 523.25f, 659.25f, 783.99f, 1046.5f };
        for (int i = 0; i < campioni; i++)
        {
            float t = (float)i / FrequenzaCampionamento;
            float valore = 0f;
            for (int nota = 0; nota < frequenze.Length; nota++)
            {
                float lunghezza = nota == frequenze.Length - 1
                    ? 0.185f
                    : 0.12f;
                valore += NotaPixel(
                    t,
                    inizi[nota],
                    lunghezza,
                    frequenze[nota]
                ) * 0.24f;
            }
            dati[i] = Quantizza(valore, 72f);
        }
        return CreaClip("Sfx_Successo", dati);
    }

    private static AudioClip CreaClipInterfaccia()
    {
        const float durata = 0.055f;
        int campioni = Mathf.CeilToInt(durata * FrequenzaCampionamento);
        float[] dati = new float[campioni];
        uint rumore = 0x9E3779B9u;
        float filtro = 0f;
        for (int i = 0; i < campioni; i++)
        {
            float t = (float)i / FrequenzaCampionamento;
            float progresso = t / durata;
            rumore = rumore * 1103515245u + 12345u;
            float valoreRumore = ((rumore >> 9) & 0x7FFFFF) /
                                 4194303.5f - 1f;
            filtro = Mathf.Lerp(filtro, valoreRumore, 0.18f);
            float tocco = Mathf.Sin(t * 860f * Mathf.PI * 2f) * 0.16f +
                           filtro * 0.12f;
            dati[i] = Quantizza(
                tocco * Mathf.Pow(Mathf.Clamp01(1f - progresso), 2.4f),
                64f
            );
        }
        return CreaClip("Sfx_Interfaccia", dati);
    }

    private static float NotaPixel(
        float tempo,
        float inizio,
        float durata,
        float frequenza
    )
    {
        float locale = tempo - inizio;
        if (locale < 0f || locale >= durata) return 0f;
        float progresso = locale / durata;
        float attacco = Mathf.Clamp01(progresso / 0.07f);
        float rilascio = Mathf.Pow(1f - progresso, 1.85f);
        return OndaPixel(frequenza * locale) * attacco * rilascio;
    }

    private static float NotaAllarme(
        float tempo,
        float inizio,
        float durata,
        float frequenza
    )
    {
        float locale = tempo - inizio;
        if (locale < 0f || locale >= durata) return 0f;
        float progresso = locale / durata;
        float attacco = Mathf.Clamp01(progresso / 0.04f);
        float rilascio = Mathf.Clamp01((1f - progresso) / 0.18f);
        float fase = locale * frequenza;
        float fondamentale = Mathf.Sin(fase * Mathf.PI * 2f);
        float armonica = Mathf.Sin(fase * Mathf.PI * 6f);
        return (fondamentale * 0.74f + armonica * 0.26f) *
               attacco * rilascio;
    }

    private static float InviluppoPizzicato(
        float fase,
        float attacco,
        float decadimento
    )
    {
        float ingresso = Mathf.Clamp01(fase / Mathf.Max(0.001f, attacco));
        return ingresso * Mathf.Pow(Mathf.Clamp01(1f - fase), decadimento);
    }

    private static float OndaPixel(float cicli)
    {
        float fase = cicli * Mathf.PI * 2f;
        return Mathf.Sin(fase) * 0.72f +
               Mathf.Sin(fase * 3f) * 0.2f +
               Mathf.Sin(fase * 5f) * 0.08f;
    }

    private static float Quantizza(float valore, float livelli)
    {
        float limitato = Mathf.Clamp(valore, -0.95f, 0.95f);
        return Mathf.Round(limitato * livelli) / livelli;
    }

    private static AudioClip CreaClip(string nome, float[] dati)
    {
        AudioClip clip = AudioClip.Create(
            nome,
            dati.Length,
            1,
            FrequenzaCampionamento,
            false
        );
        clip.SetData(dati, 0);
        clip.hideFlags = HideFlags.HideAndDontSave;
        return clip;
    }

    private void OnDestroy()
    {
        if (opzioniCollegate != null)
        {
            opzioniCollegate.ImpostazioniCambiate -= AggiornaVolumiDaOpzioni;
        }
        if (Instance == this) Instance = null;

        DistruggiClip(ref clipMusicaCalma);
        DistruggiClip(ref clipMusicaIntensa);
        DistruggiClip(ref clipMoneta);
        DistruggiClip(ref clipAcquisto);
        DistruggiClip(ref clipPericolo);
        DistruggiClip(ref clipSuccesso);
        DistruggiClip(ref clipInterfaccia);
    }

    private static void DistruggiClip(ref AudioClip clip)
    {
        if (clip == null) return;
        Destroy(clip);
        clip = null;
    }
}
