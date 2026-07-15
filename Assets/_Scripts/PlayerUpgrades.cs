using System.Globalization;
using UnityEngine;

public enum TipoPotenziamento
{
    Movimento,
    Resistenza,
    SaluteMassima,
    Cura,
    Danno,
    Cadenza,
    Penetrazione
}

[DisallowMultipleComponent]
public class PlayerUpgrades : MonoBehaviour
{
    private PlayerMovement movimento;
    private PlayerHealth salute;
    private PlayerShooting sparo;

    private int livelloMovimento;
    private int livelloResistenza;
    private int livelloSalute;
    private int livelloDanno;
    private int livelloCadenza;
    private int livelloPenetrazione;

    private ShopBalanceSettings Configurazione =>
        GameBalanceConfig.Corrente.Shop;

    void Awake()
    {
        movimento = GetComponent<PlayerMovement>();
        salute = GetComponent<PlayerHealth>();
        sparo = GetComponent<PlayerShooting>();
    }

    public int OttieniCosto(TipoPotenziamento tipo)
    {
        ShopBalanceSettings configurazione = Configurazione;
        switch (tipo)
        {
            case TipoPotenziamento.Movimento:
                return CostoLivello(
                    configurazione.costiMovimento,
                    livelloMovimento
                );
            case TipoPotenziamento.Resistenza:
                return CostoLivello(
                    configurazione.costiResistenza,
                    livelloResistenza
                );
            case TipoPotenziamento.SaluteMassima:
                return CostoLivello(
                    configurazione.costiSalute,
                    livelloSalute
                );
            case TipoPotenziamento.Cura:
                return Mathf.Max(0, configurazione.costoCura);
            case TipoPotenziamento.Danno:
                return CostoLivello(
                    configurazione.costiDanno,
                    livelloDanno
                );
            case TipoPotenziamento.Cadenza:
                return CostoLivello(
                    configurazione.costiCadenza,
                    livelloCadenza
                );
            case TipoPotenziamento.Penetrazione:
                return CostoLivello(
                    configurazione.costiPenetrazione,
                    livelloPenetrazione
                );
            default:
                return 0;
        }
    }

    public bool PuoAcquistare(TipoPotenziamento tipo)
    {
        switch (tipo)
        {
            case TipoPotenziamento.Movimento:
                return movimento != null &&
                       livelloMovimento < OttieniLivelloMassimo(tipo);
            case TipoPotenziamento.Resistenza:
                return salute != null &&
                       livelloResistenza < OttieniLivelloMassimo(tipo);
            case TipoPotenziamento.SaluteMassima:
                return salute != null &&
                       livelloSalute < OttieniLivelloMassimo(tipo);
            case TipoPotenziamento.Cura:
                return salute != null && !salute.VitaPiena;
            case TipoPotenziamento.Danno:
                return sparo != null &&
                       livelloDanno < OttieniLivelloMassimo(tipo);
            case TipoPotenziamento.Cadenza:
                return sparo != null &&
                       livelloCadenza < OttieniLivelloMassimo(tipo);
            case TipoPotenziamento.Penetrazione:
                return sparo != null &&
                       livelloPenetrazione < OttieniLivelloMassimo(tipo);
            default:
                return false;
        }
    }

    public bool ProvaAcquistare(
        TipoPotenziamento tipo,
        out string messaggio
    )
    {
        if (!PuoAcquistare(tipo))
        {
            messaggio = tipo == TipoPotenziamento.Cura &&
                        salute != null && salute.VitaPiena
                ? "La salute è già al massimo."
                : "Potenziamento già al massimo.";
            return false;
        }

        int costo = OttieniCosto(tipo);
        if (GameManager.instance == null ||
            !GameManager.instance.ProvaSpendiMonete(costo))
        {
            messaggio = "Non hai abbastanza monete.";
            return false;
        }

        Applica(tipo);
        messaggio = "Potenziamento acquistato!";
        return true;
    }

    public string OttieniTitolo(TipoPotenziamento tipo)
    {
        switch (tipo)
        {
            case TipoPotenziamento.Movimento: return "PASSO PIÙ RAPIDO";
            case TipoPotenziamento.Resistenza: return "GIACCA RINFORZATA";
            case TipoPotenziamento.SaluteMassima: return "SALUTE BONUS";
            case TipoPotenziamento.Cura: return "RIMEDIO DELLA NONNA";
            case TipoPotenziamento.Danno: return "PATATE PIU DURE";
            case TipoPotenziamento.Cadenza: return "CARICATORE RAPIDO";
            case TipoPotenziamento.Penetrazione: return "PATATA PERFORANTE";
            default: return "POTENZIAMENTO";
        }
    }

    public string OttieniDescrizione(TipoPotenziamento tipo)
    {
        ShopBalanceSettings configurazione = Configurazione;
        switch (tipo)
        {
            case TipoPotenziamento.Movimento:
                return "+" + FormattaDecimale(
                    configurazione.incrementoMovimento
                ) + " velocità di movimento";
            case TipoPotenziamento.Resistenza:
                return DescrizioneResistenza(configurazione);
            case TipoPotenziamento.SaluteMassima:
                return "+" + configurazione.incrementoSaluteMassima +
                       " salute massima e cura " +
                       configurazione.curaSuIncrementoSalute;
            case TipoPotenziamento.Cura:
                return "Recupera subito " + configurazione.quantitaCura +
                       " salute";
            case TipoPotenziamento.Danno:
                return "+" + configurazione.incrementoDanno +
                       " danno per ogni patata";
            case TipoPotenziamento.Cadenza:
                return "Riduce di " + FormattaDecimale(
                    configurazione.riduzioneIntervalloSparo
                ) + " s il tempo tra i colpi";
            case TipoPotenziamento.Penetrazione:
                return "+" + configurazione.incrementoPenetrazione +
                       " volpe attraversata sul colpo finale";
            default:
                return string.Empty;
        }
    }

    public string OttieniStato(TipoPotenziamento tipo)
    {
        if (tipo == TipoPotenziamento.Cura)
        {
            return salute != null
                ? "Salute " + salute.VitaCorrente + " / " +
                  salute.VitaMassima
                : "Non disponibile";
        }

        int livello = OttieniLivello(tipo);
        int massimo = OttieniLivelloMassimo(tipo);
        return livello >= massimo
            ? "LIVELLO MASSIMO"
            : "Livello " + livello + " / " + massimo;
    }

    void Applica(TipoPotenziamento tipo)
    {
        ShopBalanceSettings configurazione = Configurazione;
        switch (tipo)
        {
            case TipoPotenziamento.Movimento:
                livelloMovimento++;
                movimento.ImpostaBonusVelocita(
                    livelloMovimento *
                    Mathf.Max(0f, configurazione.incrementoMovimento)
                );
                break;
            case TipoPotenziamento.Resistenza:
                livelloResistenza++;
                salute.ImpostaBonusFrequenzaBlocco(
                    configurazione.frequenzeBlocco[livelloResistenza - 1]
                );
                break;
            case TipoPotenziamento.SaluteMassima:
                livelloSalute++;
                salute.ImpostaBonusVitaMassima(
                    livelloSalute * Mathf.Max(
                        0,
                        configurazione.incrementoSaluteMassima
                    ),
                    Mathf.Max(0, configurazione.curaSuIncrementoSalute)
                );
                break;
            case TipoPotenziamento.Cura:
                salute.Cura(Mathf.Max(0, configurazione.quantitaCura));
                break;
            case TipoPotenziamento.Danno:
                livelloDanno++;
                sparo.ImpostaBonusDanno(
                    livelloDanno *
                    Mathf.Max(0, configurazione.incrementoDanno)
                );
                break;
            case TipoPotenziamento.Cadenza:
                livelloCadenza++;
                sparo.ImpostaBonusRiduzioneIntervalloSparo(
                    livelloCadenza * Mathf.Max(
                        0f,
                        configurazione.riduzioneIntervalloSparo
                    )
                );
                break;
            case TipoPotenziamento.Penetrazione:
                livelloPenetrazione++;
                sparo.ImpostaBonusPenetrazione(
                    livelloPenetrazione * Mathf.Max(
                        0,
                        configurazione.incrementoPenetrazione
                    )
                );
                break;
        }
    }

    int OttieniLivello(TipoPotenziamento tipo)
    {
        switch (tipo)
        {
            case TipoPotenziamento.Movimento: return livelloMovimento;
            case TipoPotenziamento.Resistenza: return livelloResistenza;
            case TipoPotenziamento.SaluteMassima: return livelloSalute;
            case TipoPotenziamento.Danno: return livelloDanno;
            case TipoPotenziamento.Cadenza: return livelloCadenza;
            case TipoPotenziamento.Penetrazione: return livelloPenetrazione;
            default: return 0;
        }
    }

    int OttieniLivelloMassimo(TipoPotenziamento tipo)
    {
        ShopBalanceSettings configurazione = Configurazione;
        switch (tipo)
        {
            case TipoPotenziamento.Movimento:
                return Lunghezza(configurazione.costiMovimento);
            case TipoPotenziamento.Resistenza:
                return Mathf.Min(
                    Lunghezza(configurazione.costiResistenza),
                    Lunghezza(configurazione.frequenzeBlocco)
                );
            case TipoPotenziamento.SaluteMassima:
                return Lunghezza(configurazione.costiSalute);
            case TipoPotenziamento.Danno:
                return Lunghezza(configurazione.costiDanno);
            case TipoPotenziamento.Cadenza:
                return Lunghezza(configurazione.costiCadenza);
            case TipoPotenziamento.Penetrazione:
                return Lunghezza(configurazione.costiPenetrazione);
            default:
                return 0;
        }
    }

    string DescrizioneResistenza(ShopBalanceSettings configurazione)
    {
        int[] frequenze = configurazione.frequenzeBlocco;
        if (frequenze == null || frequenze.Length == 0)
        {
            return "Nessun blocco configurato";
        }

        int indice = Mathf.Clamp(
            livelloResistenza,
            0,
            frequenze.Length - 1
        );
        return "Blocca 1 colpo ogni " + Mathf.Max(1, frequenze[indice]);
    }

    static int CostoLivello(int[] costi, int livello)
    {
        return costi != null && livello >= 0 && livello < costi.Length
            ? Mathf.Max(0, costi[livello])
            : 0;
    }

    static int Lunghezza(int[] valori)
    {
        return valori != null ? valori.Length : 0;
    }

    static string FormattaDecimale(float valore)
    {
        return Mathf.Max(0f, valore)
            .ToString("0.##", CultureInfo.InvariantCulture)
            .Replace('.', ',');
    }
}
