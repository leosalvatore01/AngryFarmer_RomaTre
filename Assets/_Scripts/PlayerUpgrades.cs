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
    private static readonly int[] costiMovimento = { 3, 6, 10 };
    private static readonly int[] costiResistenza = { 4, 8, 13 };
    private static readonly int[] costiSalute = { 5, 9, 14 };
    private static readonly int[] costiDanno = { 9, 16 };
    private static readonly int[] costiCadenza = { 4, 7, 11 };
    private static readonly int[] costiPenetrazione = { 6, 10, 15 };
    private static readonly int[] frequenzeBlocco = { 5, 4, 3 };

    private const int CostoCura = 3;

    private PlayerMovement movimento;
    private PlayerHealth salute;
    private PlayerShooting sparo;

    private int livelloMovimento;
    private int livelloResistenza;
    private int livelloSalute;
    private int livelloDanno;
    private int livelloCadenza;
    private int livelloPenetrazione;

    void Awake()
    {
        movimento = GetComponent<PlayerMovement>();
        salute = GetComponent<PlayerHealth>();
        sparo = GetComponent<PlayerShooting>();
    }

    public int OttieniCosto(TipoPotenziamento tipo)
    {
        switch (tipo)
        {
            case TipoPotenziamento.Movimento:
                return CostoLivello(costiMovimento, livelloMovimento);
            case TipoPotenziamento.Resistenza:
                return CostoLivello(costiResistenza, livelloResistenza);
            case TipoPotenziamento.SaluteMassima:
                return CostoLivello(costiSalute, livelloSalute);
            case TipoPotenziamento.Cura:
                return CostoCura;
            case TipoPotenziamento.Danno:
                return CostoLivello(costiDanno, livelloDanno);
            case TipoPotenziamento.Cadenza:
                return CostoLivello(costiCadenza, livelloCadenza);
            case TipoPotenziamento.Penetrazione:
                return CostoLivello(costiPenetrazione, livelloPenetrazione);
            default:
                return 0;
        }
    }

    public bool PuoAcquistare(TipoPotenziamento tipo)
    {
        switch (tipo)
        {
            case TipoPotenziamento.Movimento:
                return movimento != null && livelloMovimento < costiMovimento.Length;
            case TipoPotenziamento.Resistenza:
                return salute != null && livelloResistenza < costiResistenza.Length;
            case TipoPotenziamento.SaluteMassima:
                return salute != null && livelloSalute < costiSalute.Length;
            case TipoPotenziamento.Cura:
                return salute != null && !salute.VitaPiena;
            case TipoPotenziamento.Danno:
                return sparo != null && livelloDanno < costiDanno.Length;
            case TipoPotenziamento.Cadenza:
                return sparo != null && livelloCadenza < costiCadenza.Length;
            case TipoPotenziamento.Penetrazione:
                return sparo != null &&
                       livelloPenetrazione < costiPenetrazione.Length;
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
            case TipoPotenziamento.Danno: return "PATATE PIÙ DURE";
            case TipoPotenziamento.Cadenza: return "CARICATORE RAPIDO";
            case TipoPotenziamento.Penetrazione: return "PATATA PERFORANTE";
            default: return "POTENZIAMENTO";
        }
    }

    public string OttieniDescrizione(TipoPotenziamento tipo)
    {
        switch (tipo)
        {
            case TipoPotenziamento.Movimento:
                return "+0,5 velocità di movimento";
            case TipoPotenziamento.Resistenza:
                return livelloResistenza < frequenzeBlocco.Length
                    ? "Blocca 1 colpo ogni " +
                      frequenzeBlocco[livelloResistenza]
                    : "Blocca 1 colpo ogni 3";
            case TipoPotenziamento.SaluteMassima:
                return "+1 salute massima e cura 1";
            case TipoPotenziamento.Cura:
                return "Recupera subito 2 salute";
            case TipoPotenziamento.Danno:
                return "+1 danno per ogni patata";
            case TipoPotenziamento.Cadenza:
                return "Riduce di 0,04 s il tempo tra i colpi";
            case TipoPotenziamento.Penetrazione:
                return "+1 volpe attraversata sul colpo finale";
            default:
                return string.Empty;
        }
    }

    public string OttieniStato(TipoPotenziamento tipo)
    {
        if (tipo == TipoPotenziamento.Cura)
        {
            return salute != null
                ? "Salute " + salute.VitaCorrente + " / " + salute.VitaMassima
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
        switch (tipo)
        {
            case TipoPotenziamento.Movimento:
                livelloMovimento++;
                movimento.AumentaVelocitaBase(0.5f);
                break;
            case TipoPotenziamento.Resistenza:
                livelloResistenza++;
                salute.ImpostaFrequenzaBlocco(
                    frequenzeBlocco[livelloResistenza - 1]
                );
                break;
            case TipoPotenziamento.SaluteMassima:
                livelloSalute++;
                salute.AumentaVitaMassima(1, 1);
                break;
            case TipoPotenziamento.Cura:
                salute.Cura(2);
                break;
            case TipoPotenziamento.Danno:
                livelloDanno++;
                sparo.AumentaDanno(1);
                break;
            case TipoPotenziamento.Cadenza:
                livelloCadenza++;
                sparo.RiduciIntervalloSparo(0.04f);
                break;
            case TipoPotenziamento.Penetrazione:
                livelloPenetrazione++;
                sparo.AumentaPenetrazione(1);
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

    static int OttieniLivelloMassimo(TipoPotenziamento tipo)
    {
        switch (tipo)
        {
            case TipoPotenziamento.Movimento: return costiMovimento.Length;
            case TipoPotenziamento.Resistenza: return costiResistenza.Length;
            case TipoPotenziamento.SaluteMassima: return costiSalute.Length;
            case TipoPotenziamento.Danno: return costiDanno.Length;
            case TipoPotenziamento.Cadenza: return costiCadenza.Length;
            case TipoPotenziamento.Penetrazione: return costiPenetrazione.Length;
            default: return 0;
        }
    }

    static int CostoLivello(int[] costi, int livello)
    {
        return livello >= 0 && livello < costi.Length ? costi[livello] : 0;
    }
}
