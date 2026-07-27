using System;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// Punto unico per la persistenza locale. Mantiene separati il profilo
/// sincronizzabile e le preferenze del dispositivo, usa file JSON versionati
/// e conserva una copia di sicurezza dell'ultimo salvataggio valido.
/// </summary>
public static class SaveService
{
    public const int VersioneSchemaCorrente = 1;
    public const int VersioneMigrazioneLegacy = 1;
    public const string IdProfiloOspite = "guest";

    private const string CartellaSalvataggi = "Saves";
    private const string CartellaProfili = "profiles";
    private const string NomeFileDispositivo = "device.json";
    private const string NomeFileProfiloOspite = "guest.json";

    private const string MetaPrefisso = "AngryFarmer.Meta.v1";
    private const string MetaSaldo =
        MetaPrefisso + ".Gettoni.Saldo";
    private const string MetaGuadagnati =
        MetaPrefisso + ".Gettoni.Guadagnati";
    private const string MetaSpesi =
        MetaPrefisso + ".Gettoni.Spesi";
    private const string MetaLivello =
        MetaPrefisso + ".Livello.";

    private const string PartitaPrefisso = "AngryFarmer.Blocco8";
    private const string PartitaDifficolta =
        PartitaPrefisso + ".Difficolta";

    private const string OpzioniPrefisso = "AngryFarmer.Opzioni.";
    private const string OpzioniVolumeMusica =
        OpzioniPrefisso + "VolumeMusica";
    private const string OpzioniVolumeEffetti =
        OpzioniPrefisso + "VolumeEffetti";
    private const string OpzioniVibrazione =
        OpzioniPrefisso + "Vibrazione";
    private const string OpzioniFlash =
        OpzioniPrefisso + "Flash";
    private const string OpzioniNumeriDanno =
        OpzioniPrefisso + "NumeriDanno";
    private const string OpzioniDimensioneMirino =
        OpzioniPrefisso + "DimensioneMirino";

    private static readonly Encoding CodificaUtf8 =
        new UTF8Encoding(false);

    private static bool inizializzato;
    private static bool profiloSolaLettura;
    private static bool dispositivoSolaLettura;
    private static SaveData profilo;
    private static DeviceSettingsData dispositivo;
    private static string radiceDatiPersistenti;

#if UNITY_EDITOR
    private const string ChiaveRadicePlayModeTest =
        "AngryFarmer.Tests.PlayModeSaveRoot";
#endif

    public static event Action ProfiloCambiato;
    public static event Action ImpostazioniDispositivoCambiate;

    public static SaveData Profilo
    {
        get
        {
            AssicuraInizializzato();
            return profilo;
        }
    }

    public static DeviceSettingsData Dispositivo
    {
        get
        {
            AssicuraInizializzato();
            return dispositivo;
        }
    }

    public static string PercorsoProfiloOspite =>
        Path.Combine(
            RadiceDatiPersistenti,
            CartellaSalvataggi,
            CartellaProfili,
            NomeFileProfiloOspite
        );

    public static string PercorsoImpostazioniDispositivo =>
        Path.Combine(
            RadiceDatiPersistenti,
            CartellaSalvataggi,
            NomeFileDispositivo
        );

    private static string RadiceDatiPersistenti
    {
        get
        {
            if (!string.IsNullOrEmpty(radiceDatiPersistenti))
            {
                return radiceDatiPersistenti;
            }

#if UNITY_EDITOR
            string radicePlayModeTest = UnityEditor.SessionState.GetString(
                ChiaveRadicePlayModeTest,
                string.Empty
            );
            if (!string.IsNullOrWhiteSpace(radicePlayModeTest))
            {
                radiceDatiPersistenti =
                    Path.GetFullPath(radicePlayModeTest);
                return radiceDatiPersistenti;
            }
#endif

            string[] argomenti = Environment.GetCommandLineArgs();
            for (int i = 0; i < argomenti.Length - 1; i++)
            {
                if (!string.Equals(
                        argomenti[i],
                        "-angryFarmerSaveDirectory",
                        StringComparison.OrdinalIgnoreCase
                    ))
                {
                    continue;
                }

                try
                {
                    radiceDatiPersistenti =
                        Path.GetFullPath(argomenti[i + 1]);
                    return radiceDatiPersistenti;
                }
                catch (Exception eccezione)
                {
                    Debug.LogWarning(
                        "Percorso salvataggi di test non valido: " +
                        eccezione.Message
                    );
                    break;
                }
            }

            radiceDatiPersistenti = Application.persistentDataPath;
            return radiceDatiPersistenti;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void AzzeraStatoSessione()
    {
        inizializzato = false;
        profiloSolaLettura = false;
        dispositivoSolaLettura = false;
        profilo = null;
        dispositivo = null;
        radiceDatiPersistenti = null;
        ProfiloCambiato = null;
        ImpostazioniDispositivoCambiate = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InizializzaPrimaDellaScena()
    {
        InizializzaProfiloOspite();
    }

    public static void InizializzaProfiloOspite()
    {
        AssicuraInizializzato();
    }

    public static bool ModificaProfilo(
        Action<SaveData> modifica,
        bool salvaSubito = true
    )
    {
        if (modifica == null)
        {
            return false;
        }

        AssicuraInizializzato();
        if (profiloSolaLettura)
        {
            Debug.LogError(
                "Il salvataggio del profilo usa una versione piu recente " +
                "e non puo essere modificato da questa build."
            );
            return false;
        }

        string copia = JsonUtility.ToJson(profilo);
        try
        {
            modifica(profilo);
            NormalizzaProfilo(profilo);
        }
        catch (Exception eccezione)
        {
            RipristinaProfiloDaJson(copia);
            Debug.LogException(eccezione);
            return false;
        }

        if (salvaSubito && !SalvaProfiloInterno())
        {
            RipristinaProfiloDaJson(copia);
            return false;
        }

        ProfiloCambiato?.Invoke();
        return true;
    }

    public static bool ModificaDispositivo(
        Action<DeviceSettingsData> modifica,
        bool salvaSubito = false
    )
    {
        if (modifica == null)
        {
            return false;
        }

        AssicuraInizializzato();
        if (dispositivoSolaLettura)
        {
            Debug.LogError(
                "Le impostazioni locali usano una versione piu recente " +
                "e non possono essere modificate da questa build."
            );
            return false;
        }

        string copia = JsonUtility.ToJson(dispositivo);
        try
        {
            modifica(dispositivo);
            NormalizzaDispositivo(dispositivo);
        }
        catch (Exception eccezione)
        {
            RipristinaDispositivoDaJson(copia);
            Debug.LogException(eccezione);
            return false;
        }

        if (salvaSubito && !SalvaDispositivoInterno())
        {
            RipristinaDispositivoDaJson(copia);
            return false;
        }

        ImpostazioniDispositivoCambiate?.Invoke();
        return true;
    }

    public static bool SalvaProfiloOra()
    {
        AssicuraInizializzato();
        return !profiloSolaLettura && SalvaProfiloInterno();
    }

    public static bool SalvaDispositivoOra()
    {
        AssicuraInizializzato();
        return !dispositivoSolaLettura && SalvaDispositivoInterno();
    }

    public static bool Flush()
    {
        AssicuraInizializzato();
        bool profiloSalvato =
            profiloSolaLettura || SalvaProfiloInterno();
        bool dispositivoSalvato =
            dispositivoSolaLettura || SalvaDispositivoInterno();
        return profiloSalvato && dispositivoSalvato;
    }

#if UNITY_EDITOR
    public static string RadicePlayModePerTestAttiva =>
        UnityEditor.SessionState.GetString(
            ChiaveRadicePlayModeTest,
            string.Empty
        );

    /// <summary>
    /// Mantiene una radice temporanea attraverso il domain reload necessario
    /// ai test che entrano in Play Mode dall'Editor.
    /// </summary>
    public static void PreparaRadicePlayModePerTest(string percorso)
    {
        if (string.IsNullOrWhiteSpace(percorso))
        {
            throw new ArgumentException(
                "La radice Play Mode dei test non puo essere vuota.",
                nameof(percorso)
            );
        }

        UnityEditor.SessionState.SetString(
            ChiaveRadicePlayModeTest,
            Path.GetFullPath(percorso)
        );
    }

    /// <summary>
    /// Rimuove l'override persistito per il test Play Mode corrente.
    /// </summary>
    public static void RimuoviRadicePlayModePerTest()
    {
        UnityEditor.SessionState.EraseString(ChiaveRadicePlayModeTest);
    }

    /// <summary>
    /// Isola i file di persistenza usati da un test dell'Editor e restituisce
    /// uno scope che ripristina integralmente lo stato precedente. Il metodo
    /// non viene incluso nelle build.
    /// </summary>
    public static IDisposable IsolaDatiPerTest(string percorso)
    {
        if (string.IsNullOrWhiteSpace(percorso))
        {
            throw new ArgumentException(
                "La radice dati dei test non puo essere vuota.",
                nameof(percorso)
            );
        }

        return new AmbitoDatiTest(Path.GetFullPath(percorso));
    }

    /// <summary>
    /// Forza un nuovo caricamento dalla radice temporanea dello scope corrente.
    /// </summary>
    public static void RicaricaDatiPerTest()
    {
        if (string.IsNullOrWhiteSpace(radiceDatiPersistenti))
        {
            throw new InvalidOperationException(
                "Nessun ambiente dati di test attivo."
            );
        }

        string radiceTest = radiceDatiPersistenti;
        AzzeraStatoSessione();
        radiceDatiPersistenti = radiceTest;
    }

    private sealed class AmbitoDatiTest : IDisposable
    {
        private readonly bool inizializzatoPrecedente;
        private readonly bool profiloSolaLetturaPrecedente;
        private readonly bool dispositivoSolaLetturaPrecedente;
        private readonly SaveData profiloPrecedente;
        private readonly DeviceSettingsData dispositivoPrecedente;
        private readonly string radicePrecedente;
        private readonly Action profiloCambiatoPrecedente;
        private readonly Action dispositivoCambiatoPrecedente;
        private bool chiuso;

        public AmbitoDatiTest(string radiceTest)
        {
            inizializzatoPrecedente = inizializzato;
            profiloSolaLetturaPrecedente = profiloSolaLettura;
            dispositivoSolaLetturaPrecedente = dispositivoSolaLettura;
            profiloPrecedente = profilo;
            dispositivoPrecedente = dispositivo;
            radicePrecedente = radiceDatiPersistenti;
            profiloCambiatoPrecedente = ProfiloCambiato;
            dispositivoCambiatoPrecedente =
                ImpostazioniDispositivoCambiate;

            AzzeraStatoSessione();
            radiceDatiPersistenti = radiceTest;
        }

        public void Dispose()
        {
            if (chiuso)
            {
                return;
            }

            chiuso = true;
            AzzeraStatoSessione();
            inizializzato = inizializzatoPrecedente;
            profiloSolaLettura = profiloSolaLetturaPrecedente;
            dispositivoSolaLettura =
                dispositivoSolaLetturaPrecedente;
            profilo = profiloPrecedente;
            dispositivo = dispositivoPrecedente;
            radiceDatiPersistenti = radicePrecedente;
            ProfiloCambiato = profiloCambiatoPrecedente;
            ImpostazioniDispositivoCambiate =
                dispositivoCambiatoPrecedente;
        }
    }
#endif

    private static void AssicuraInizializzato()
    {
        if (inizializzato)
        {
            return;
        }

        string percorsoProfilo = PercorsoProfiloOspite;
        string percorsoDispositivo = PercorsoImpostazioniDispositivo;

        profilo = CaricaConBackup<SaveData>(
            percorsoProfilo,
            out bool profiloCaricato
        ) ?? new SaveData();
        dispositivo = CaricaConBackup<DeviceSettingsData>(
            percorsoDispositivo,
            out bool dispositivoCaricato
        ) ?? CreaDispositivoPredefinito();

        profiloSolaLettura =
            profilo.versioneSchema > VersioneSchemaCorrente;
        dispositivoSolaLettura =
            dispositivo.versioneSchema > VersioneSchemaCorrente;

        if (profiloSolaLettura)
        {
            Debug.LogError(
                "Save profilo v" + profilo.versioneSchema +
                " non supportato dalla build corrente (v" +
                VersioneSchemaCorrente + "). Modalita sola lettura."
            );
        }
        else
        {
            NormalizzaProfilo(profilo);
        }

        if (dispositivoSolaLettura)
        {
            Debug.LogError(
                "Save dispositivo v" + dispositivo.versioneSchema +
                " non supportato dalla build corrente (v" +
                VersioneSchemaCorrente + "). Modalita sola lettura."
            );
        }
        else
        {
            NormalizzaDispositivo(dispositivo);
        }

        inizializzato = true;

        if (!profiloSolaLettura &&
            profilo.migrazioneLegacyVersione <
            VersioneMigrazioneLegacy)
        {
            ImportaProfiloLegacy();
            profilo.migrazioneLegacyVersione =
                VersioneMigrazioneLegacy;
            SalvaProfiloInterno();
        }
        else if (!profiloCaricato && !profiloSolaLettura)
        {
            SalvaProfiloInterno();
        }

        if (!dispositivoSolaLettura &&
            dispositivo.migrazioneLegacyVersione <
            VersioneMigrazioneLegacy)
        {
            ImportaDispositivoLegacy();
            dispositivo.migrazioneLegacyVersione =
                VersioneMigrazioneLegacy;
            SalvaDispositivoInterno();
        }
        else if (!dispositivoCaricato && !dispositivoSolaLettura)
        {
            SalvaDispositivoInterno();
        }
    }

    private static void ImportaProfiloLegacy()
    {
        DatiProgressionePermanente meta =
            profilo.progressionePermanente;

        meta.saldoGettoni = MassimoConPlayerPrefs(
            meta.saldoGettoni,
            MetaSaldo
        );
        meta.totaleGettoniGuadagnati = MassimoConPlayerPrefs(
            meta.totaleGettoniGuadagnati,
            MetaGuadagnati
        );
        meta.totaleGettoniSpesi = MassimoConPlayerPrefs(
            meta.totaleGettoniSpesi,
            MetaSpesi
        );

        for (int i = 0; i < meta.livelli.Length; i++)
        {
            meta.livelli[i] = MassimoConPlayerPrefs(
                meta.livelli[i],
                MetaLivello + i
            );
        }

        if (PlayerPrefs.HasKey(PartitaDifficolta))
        {
            profilo.difficoltaPreferita = PlayerPrefs.GetInt(
                PartitaDifficolta,
                (int)DifficoltaPartita.Normale
            );
        }

        for (int i = 0; i < profilo.recordDifficolta.Length; i++)
        {
            DatiRecordDifficolta record = profilo.recordDifficolta[i];
            string prefisso =
                PartitaPrefisso + ".Record." + i;

            record.migliorPunteggio = MassimoConPlayerPrefs(
                record.migliorPunteggio,
                prefisso + ".Punti"
            );
            record.massimoVolpi = MassimoConPlayerPrefs(
                record.massimoVolpi,
                prefisso + ".Volpi"
            );
            record.migliorePercentualeGalline = MassimoConPlayerPrefs(
                record.migliorePercentualeGalline,
                prefisso + ".Galline"
            );

            string chiaveTempo = prefisso + ".Tempo";
            if (PlayerPrefs.HasKey(chiaveTempo))
            {
                float tempoLegacy = Mathf.Max(
                    0f,
                    PlayerPrefs.GetFloat(chiaveTempo, 0f)
                );
                if (tempoLegacy > 0f &&
                    (record.migliorTempoVittoria <= 0f ||
                     tempoLegacy < record.migliorTempoVittoria))
                {
                    record.migliorTempoVittoria = tempoLegacy;
                }
            }
        }

        NormalizzaProfilo(profilo);
    }

    private static DeviceSettingsData CreaDispositivoPredefinito()
    {
        DeviceSettingsData risultato = new DeviceSettingsData();
        CombatFeedbackSettings feedback =
            GameBalanceConfig.Corrente.FeedbackCombattimento;
        if (feedback == null)
        {
            return risultato;
        }

        risultato.volumeMusica =
            feedback.audioAttivo ? 0.55f : 0f;
        risultato.volumeEffetti =
            feedback.audioAttivo ? 1f : 0f;
        risultato.vibrazioneAttiva =
            feedback.vibrazioneCameraAttiva;
        risultato.flashAttivi = feedback.effettiVisiviAttivi;
        risultato.numeriDannoAttivi = true;
        risultato.dimensioneMirino = feedback.dimensioneMirino;
        return risultato;
    }

    private static void ImportaDispositivoLegacy()
    {
        if (PlayerPrefs.HasKey(OpzioniVolumeMusica))
        {
            dispositivo.volumeMusica = PlayerPrefs.GetFloat(
                OpzioniVolumeMusica,
                dispositivo.volumeMusica
            );
        }
        if (PlayerPrefs.HasKey(OpzioniVolumeEffetti))
        {
            dispositivo.volumeEffetti = PlayerPrefs.GetFloat(
                OpzioniVolumeEffetti,
                dispositivo.volumeEffetti
            );
        }
        if (PlayerPrefs.HasKey(OpzioniVibrazione))
        {
            dispositivo.vibrazioneAttiva =
                PlayerPrefs.GetInt(OpzioniVibrazione, 1) != 0;
        }
        if (PlayerPrefs.HasKey(OpzioniFlash))
        {
            dispositivo.flashAttivi =
                PlayerPrefs.GetInt(OpzioniFlash, 1) != 0;
        }
        if (PlayerPrefs.HasKey(OpzioniNumeriDanno))
        {
            dispositivo.numeriDannoAttivi =
                PlayerPrefs.GetInt(OpzioniNumeriDanno, 1) != 0;
        }
        if (PlayerPrefs.HasKey(OpzioniDimensioneMirino))
        {
            dispositivo.dimensioneMirino = PlayerPrefs.GetFloat(
                OpzioniDimensioneMirino,
                dispositivo.dimensioneMirino
            );
        }

        NormalizzaDispositivo(dispositivo);
    }

    private static int MassimoConPlayerPrefs(
        int valoreCorrente,
        string chiave
    )
    {
        if (!PlayerPrefs.HasKey(chiave))
        {
            return Mathf.Max(0, valoreCorrente);
        }

        return Mathf.Max(
            Mathf.Max(0, valoreCorrente),
            Mathf.Max(0, PlayerPrefs.GetInt(chiave, 0))
        );
    }

    private static bool SalvaProfiloInterno()
    {
        if (profilo == null || profiloSolaLettura)
        {
            return false;
        }

        long revisionePrecedente = profilo.revisione;
        long istantePrecedente = profilo.ultimoSalvataggioUtcTicks;
        profilo.revisione = IncrementaSaturato(profilo.revisione);
        profilo.ultimoSalvataggioUtcTicks = DateTime.UtcNow.Ticks;
        profilo.versioneSchema = VersioneSchemaCorrente;

        if (ScriviAtomico(PercorsoProfiloOspite, profilo))
        {
            return true;
        }

        profilo.revisione = revisionePrecedente;
        profilo.ultimoSalvataggioUtcTicks = istantePrecedente;
        return false;
    }

    private static bool SalvaDispositivoInterno()
    {
        if (dispositivo == null || dispositivoSolaLettura)
        {
            return false;
        }

        long revisionePrecedente = dispositivo.revisione;
        long istantePrecedente =
            dispositivo.ultimoSalvataggioUtcTicks;
        dispositivo.revisione =
            IncrementaSaturato(dispositivo.revisione);
        dispositivo.ultimoSalvataggioUtcTicks = DateTime.UtcNow.Ticks;
        dispositivo.versioneSchema = VersioneSchemaCorrente;

        if (ScriviAtomico(
                PercorsoImpostazioniDispositivo,
                dispositivo
            ))
        {
            return true;
        }

        dispositivo.revisione = revisionePrecedente;
        dispositivo.ultimoSalvataggioUtcTicks = istantePrecedente;
        return false;
    }

    private static T CaricaConBackup<T>(
        string percorso,
        out bool caricato
    ) where T : class
    {
        if (ProvaCaricare(percorso, out T risultato))
        {
            caricato = true;
            return risultato;
        }

        string backup = percorso + ".bak";
        if (ProvaCaricare(backup, out risultato))
        {
            caricato = true;
            Debug.LogWarning(
                "Recuperato salvataggio dalla copia di sicurezza: " +
                backup
            );
            return risultato;
        }

        caricato = false;
        return null;
    }

    private static bool ProvaCaricare<T>(
        string percorso,
        out T risultato
    ) where T : class
    {
        risultato = null;
        if (!File.Exists(percorso))
        {
            return false;
        }

        try
        {
            string json = File.ReadAllText(percorso, CodificaUtf8);
            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            risultato = JsonUtility.FromJson<T>(json);
            return risultato != null;
        }
        catch (Exception eccezione)
        {
            Debug.LogWarning(
                "Salvataggio non leggibile: " + percorso + "\n" +
                eccezione.Message
            );
            return false;
        }
    }

    private static bool ScriviAtomico<T>(string percorso, T dati)
    {
        string temporaneo = percorso + ".tmp";
        string backup = percorso + ".bak";

        try
        {
            string cartella = Path.GetDirectoryName(percorso);
            if (!string.IsNullOrEmpty(cartella))
            {
                Directory.CreateDirectory(cartella);
            }

            string json = JsonUtility.ToJson(dati, true);
            File.WriteAllText(temporaneo, json, CodificaUtf8);

            if (File.Exists(percorso))
            {
                try
                {
                    File.Replace(
                        temporaneo,
                        percorso,
                        backup,
                        true
                    );
                }
                catch (PlatformNotSupportedException)
                {
                    SostituisciConFallback(
                        temporaneo,
                        percorso,
                        backup
                    );
                }
                catch (IOException)
                {
                    SostituisciConFallback(
                        temporaneo,
                        percorso,
                        backup
                    );
                }
            }
            else
            {
                File.Move(temporaneo, percorso);
            }

            return true;
        }
        catch (Exception eccezione)
        {
            Debug.LogError(
                "Impossibile salvare " + percorso + "\n" +
                eccezione.Message
            );
            return false;
        }
        finally
        {
            try
            {
                if (File.Exists(temporaneo))
                {
                    File.Delete(temporaneo);
                }
            }
            catch (Exception)
            {
                // Un residuo .tmp verra sovrascritto al prossimo salvataggio.
            }
        }
    }

    private static void SostituisciConFallback(
        string temporaneo,
        string percorso,
        string backup
    )
    {
        File.Copy(percorso, backup, true);
        File.Copy(temporaneo, percorso, true);
        File.Delete(temporaneo);
    }

    private static void NormalizzaProfilo(SaveData dati)
    {
        if (dati == null)
        {
            return;
        }

        if (dati.versioneSchema <= 0)
        {
            dati.versioneSchema = VersioneSchemaCorrente;
        }
        if (string.IsNullOrWhiteSpace(dati.idProfilo))
        {
            dati.idProfilo = IdProfiloOspite;
        }
        if (string.IsNullOrWhiteSpace(dati.nomeProfilo))
        {
            dati.nomeProfilo = "Contadino";
        }
        else
        {
            dati.nomeProfilo = dati.nomeProfilo.Trim();
            if (dati.nomeProfilo.Length > 20)
            {
                dati.nomeProfilo = dati.nomeProfilo.Substring(0, 20);
            }
        }

        dati.revisione = Math.Max(0L, dati.revisione);
        dati.ultimoSalvataggioUtcTicks =
            Math.Max(0L, dati.ultimoSalvataggioUtcTicks);
        dati.migrazioneLegacyVersione =
            Mathf.Max(0, dati.migrazioneLegacyVersione);
        dati.difficoltaPreferita = Mathf.Clamp(
            dati.difficoltaPreferita,
            (int)DifficoltaPartita.Tranquilla,
            (int)DifficoltaPartita.Difficile
        );
        dati.miglioreOndataAssoluta =
            Mathf.Max(0, dati.miglioreOndataAssoluta);

        if (dati.progressionePermanente == null)
        {
            dati.progressionePermanente =
                new DatiProgressionePermanente();
        }

        DatiProgressionePermanente meta =
            dati.progressionePermanente;
        meta.saldoGettoni = Mathf.Max(0, meta.saldoGettoni);
        meta.totaleGettoniGuadagnati =
            Mathf.Max(0, meta.totaleGettoniGuadagnati);
        meta.totaleGettoniSpesi =
            Mathf.Max(0, meta.totaleGettoniSpesi);
        meta.livelli = RidimensionaInteriNonNegativi(
            meta.livelli,
            6
        );

        DatiRecordDifficolta[] precedenti =
            dati.recordDifficolta;
        if (precedenti == null || precedenti.Length != 3)
        {
            DatiRecordDifficolta[] nuovi =
                new DatiRecordDifficolta[3];
            if (precedenti != null)
            {
                Array.Copy(
                    precedenti,
                    nuovi,
                    Math.Min(precedenti.Length, nuovi.Length)
                );
            }
            dati.recordDifficolta = nuovi;
        }

        for (int i = 0; i < dati.recordDifficolta.Length; i++)
        {
            DatiRecordDifficolta record =
                dati.recordDifficolta[i] ??
                (dati.recordDifficolta[i] =
                    new DatiRecordDifficolta());
            record.difficolta = i;
            record.migliorPunteggio =
                Mathf.Max(0, record.migliorPunteggio);
            record.massimoVolpi =
                Mathf.Max(0, record.massimoVolpi);
            record.migliorePercentualeGalline = Mathf.Clamp(
                record.migliorePercentualeGalline,
                0,
                100
            );
            if (float.IsNaN(record.migliorTempoVittoria) ||
                float.IsInfinity(record.migliorTempoVittoria))
            {
                record.migliorTempoVittoria = 0f;
            }
            record.migliorTempoVittoria =
                Mathf.Max(0f, record.migliorTempoVittoria);
            record.massimaOndata =
                Mathf.Max(0, record.massimaOndata);
            dati.miglioreOndataAssoluta = Mathf.Max(
                dati.miglioreOndataAssoluta,
                record.massimaOndata
            );
        }
    }

    private static void NormalizzaDispositivo(
        DeviceSettingsData dati
    )
    {
        if (dati == null)
        {
            return;
        }

        if (dati.versioneSchema <= 0)
        {
            dati.versioneSchema = VersioneSchemaCorrente;
        }
        dati.revisione = Math.Max(0L, dati.revisione);
        dati.ultimoSalvataggioUtcTicks =
            Math.Max(0L, dati.ultimoSalvataggioUtcTicks);
        dati.migrazioneLegacyVersione =
            Mathf.Max(0, dati.migrazioneLegacyVersione);
        dati.volumeMusica = NormalizzaFloat(
            dati.volumeMusica,
            0.55f,
            0f,
            1f
        );
        dati.volumeEffetti = NormalizzaFloat(
            dati.volumeEffetti,
            1f,
            0f,
            1f
        );
        dati.dimensioneMirino = Mathf.Round(
            NormalizzaFloat(
                dati.dimensioneMirino,
                24f,
                14f,
                48f
            )
        );
    }

    private static int[] RidimensionaInteriNonNegativi(
        int[] sorgente,
        int lunghezza
    )
    {
        int[] risultato = new int[Mathf.Max(0, lunghezza)];
        if (sorgente != null)
        {
            int conteggio = Math.Min(sorgente.Length, risultato.Length);
            for (int i = 0; i < conteggio; i++)
            {
                risultato[i] = Mathf.Max(0, sorgente[i]);
            }
        }
        return risultato;
    }

    private static float NormalizzaFloat(
        float valore,
        float predefinito,
        float minimo,
        float massimo
    )
    {
        if (float.IsNaN(valore) || float.IsInfinity(valore))
        {
            valore = predefinito;
        }
        return Mathf.Clamp(valore, minimo, massimo);
    }

    private static long IncrementaSaturato(long valore)
    {
        return valore >= long.MaxValue
            ? long.MaxValue
            : Math.Max(0L, valore) + 1L;
    }

    private static void RipristinaProfiloDaJson(string json)
    {
        SaveData ripristinato = JsonUtility.FromJson<SaveData>(json);
        if (ripristinato != null)
        {
            profilo = ripristinato;
            NormalizzaProfilo(profilo);
        }
    }

    private static void RipristinaDispositivoDaJson(string json)
    {
        DeviceSettingsData ripristinato =
            JsonUtility.FromJson<DeviceSettingsData>(json);
        if (ripristinato != null)
        {
            dispositivo = ripristinato;
            NormalizzaDispositivo(dispositivo);
        }
    }
}
