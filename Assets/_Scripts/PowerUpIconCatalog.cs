using UnityEngine;

/// <summary>
/// Unica sorgente visiva per i potenziamenti mostrati in bottega e in partita.
/// </summary>
public static class PowerUpIconCatalog
{
    public static FarmPixelIcon OttieniIcona(TipoPotenziamento tipo)
    {
        switch (tipo)
        {
            case TipoPotenziamento.Movimento:
                return FarmPixelIcon.Movimento;
            case TipoPotenziamento.Resistenza:
                return FarmPixelIcon.Resistenza;
            case TipoPotenziamento.SaluteMassima:
                return FarmPixelIcon.SaluteMassima;
            case TipoPotenziamento.Cura:
                return FarmPixelIcon.Cura;
            case TipoPotenziamento.Danno:
                return FarmPixelIcon.Danno;
            case TipoPotenziamento.Cadenza:
                return FarmPixelIcon.Cadenza;
            case TipoPotenziamento.Penetrazione:
                return FarmPixelIcon.Penetrazione;
            case TipoPotenziamento.ColpoAggiuntivo:
                return FarmPixelIcon.ColpoAggiuntivo;
            case TipoPotenziamento.RafficaRaccolto:
                return FarmPixelIcon.RafficaRaccolto;
            case TipoPotenziamento.PatataGigante:
                return FarmPixelIcon.PatataGigante;
            case TipoPotenziamento.PatataEsplosiva:
                return FarmPixelIcon.PatataEsplosiva;
            case TipoPotenziamento.Critico:
                return FarmPixelIcon.Critico;
            case TipoPotenziamento.Rimbalzo:
                return FarmPixelIcon.Rimbalzo;
            case TipoPotenziamento.Rallentamento:
                return FarmPixelIcon.Rallentamento;
            case TipoPotenziamento.Spinta:
                return FarmPixelIcon.Spinta;
            default:
                return FarmPixelIcon.Obiettivo;
        }
    }

    public static Sprite OttieniSprite(TipoPotenziamento tipo)
    {
        return FarmPixelUI.OttieniIcona(OttieniIcona(tipo));
    }

    public static string OttieniEtichettaCompatta(TipoPotenziamento tipo)
    {
        switch (tipo)
        {
            case TipoPotenziamento.Movimento: return "VELOCITÀ";
            case TipoPotenziamento.Resistenza: return "BLOCCO";
            case TipoPotenziamento.SaluteMassima: return "VITA";
            case TipoPotenziamento.Cura: return "CURA";
            case TipoPotenziamento.Danno: return "DANNO";
            case TipoPotenziamento.Cadenza: return "RAPIDITÀ";
            case TipoPotenziamento.Penetrazione: return "PERFORA";
            case TipoPotenziamento.ColpoAggiuntivo: return "COLPO +";
            case TipoPotenziamento.RafficaRaccolto: return "RAFFICA";
            case TipoPotenziamento.PatataGigante: return "GIGANTE";
            case TipoPotenziamento.PatataEsplosiva: return "ESPLOSIVA";
            case TipoPotenziamento.Critico: return "CRITICO";
            case TipoPotenziamento.Rimbalzo: return "RIMBALZO";
            case TipoPotenziamento.Rallentamento: return "LENTEZZA";
            case TipoPotenziamento.Spinta: return "SPINTA";
            default: return "POWER-UP";
        }
    }

    public static Color OttieniColore(TipoPotenziamento tipo)
    {
        DefinizionePotenziamentoBuild definizione =
            CatalogoPotenziamentiBuild.Ottieni(tipo);
        return definizione != null
            ? CatalogoPotenziamentiBuild.ColorePercorso(definizione.Percorso)
            : new Color32(214, 161, 59, 255);
    }
}
