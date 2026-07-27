using System;

[Serializable]
public sealed class DatiProgressionePermanente
{
    public int saldoGettoni;
    public int totaleGettoniGuadagnati;
    public int totaleGettoniSpesi;
    public int[] livelli = new int[6];
}

[Serializable]
public sealed class DatiRecordDifficolta
{
    public int difficolta;
    public int migliorPunteggio;
    public int massimoVolpi;
    public int migliorePercentualeGalline;
    public float migliorTempoVittoria;
    public int massimaOndata;
}

/// <summary>
/// Dati legati al profilo del giocatore. In futuro questo stesso payload
/// potra essere sincronizzato con il cloud senza includere le opzioni del
/// dispositivo.
/// </summary>
[Serializable]
public sealed class SaveData
{
    public int versioneSchema = SaveService.VersioneSchemaCorrente;
    public string idProfilo = SaveService.IdProfiloOspite;
    public string nomeProfilo = "Contadino";
    public long revisione;
    public long ultimoSalvataggioUtcTicks;
    public int migrazioneLegacyVersione;
    public int difficoltaPreferita = (int)DifficoltaPartita.Normale;
    public int miglioreOndataAssoluta;
    public DatiProgressionePermanente progressionePermanente =
        new DatiProgressionePermanente();
    public DatiRecordDifficolta[] recordDifficolta =
        new DatiRecordDifficolta[3];
}

/// <summary>
/// Preferenze locali al dispositivo: non fanno parte del profilo e non
/// dovranno essere sovrascritte da una futura sincronizzazione cloud.
/// </summary>
[Serializable]
public sealed class DeviceSettingsData
{
    public int versioneSchema = SaveService.VersioneSchemaCorrente;
    public long revisione;
    public long ultimoSalvataggioUtcTicks;
    public int migrazioneLegacyVersione;
    public float volumeMusica = 0.55f;
    public float volumeEffetti = 1f;
    public bool vibrazioneAttiva = true;
    public bool flashAttivi = true;
    public bool numeriDannoAttivi = true;
    public float dimensioneMirino = 24f;
}
