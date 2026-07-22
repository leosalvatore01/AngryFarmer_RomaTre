using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

public enum TipoPotenziamento
{
    Movimento = 0,
    Resistenza = 1,
    SaluteMassima = 2,
    Cura = 3,
    Danno = 4,
    Cadenza = 5,
    Penetrazione = 6,
    ColpoAggiuntivo = 7,
    RafficaRaccolto = 8,
    PatataGigante = 9,
    PatataEsplosiva = 10,
    Critico = 11,
    Rimbalzo = 12,
    Rallentamento = 13,
    Spinta = 14
}

[DisallowMultipleComponent]
public class PlayerUpgrades : MonoBehaviour
{
    private const int MassimoColpiAggiuntiviFisici = 6;
    private const int MassimoColpiRafficaFisici = 7;
    private const int MassimoRimbalziFisici = 6;
    private const int MassimoLivelloRallentamentoFisico = 12;
    private const float ScalaMassimaProiettile = 2.35f;
    private const float ForzaSpintaMassima = 8f;
    private const float ProbabilitaBloccoAsintotica = 0.82f;

    private PlayerMovement movimento;
    private PlayerHealth salute;
    private PlayerShooting sparo;

    private int livelloMovimento;
    private int livelloResistenza;
    private int livelloSalute;
    private int livelloDanno;
    private int livelloCadenza;
    private int livelloPenetrazione;
    private int livelloColpoAggiuntivo;
    private int livelloRafficaRaccolto;
    private int livelloPatataGigante;
    private int livelloPatataEsplosiva;
    private int livelloCritico;
    private int livelloRimbalzo;
    private int livelloRallentamento;
    private int livelloSpinta;

    private ShopBalanceSettings Configurazione =>
        GameBalanceConfig.Corrente.Shop;

    public event Action<TipoPotenziamento> PotenziamentoAcquistato;

    public int LimiteColpiAggiuntiviFisici =>
        MassimoColpiAggiuntiviFisici;
    public int LimiteProiettiliRafficaFisici =>
        MassimoColpiRafficaFisici;
    public int LimiteRimbalziFisici => MassimoRimbalziFisici;
    public float LimiteScalaProiettile => ScalaMassimaProiettile;
    public float LimiteForzaSpinta => ForzaSpintaMassima;

    public float ValoreColpiAggiuntivi => Mathf.Max(
        0f,
        livelloColpoAggiuntivo *
        Mathf.Max(
            0.05f,
            Configurazione.probabilitaColpoAggiuntivoPerLivello
        )
    );
    public int ColpiAggiuntiviGarantiti => Mathf.Clamp(
        Mathf.FloorToInt(ValoreColpiAggiuntivi),
        0,
        MassimoColpiAggiuntiviFisici
    );
    public float ProbabilitaColpoAggiuntivo =>
        ColpiAggiuntiviGarantiti >= MassimoColpiAggiuntiviFisici
            ? 0f
            : Mathf.Repeat(ValoreColpiAggiuntivi, 1f);
    public bool HaRafficaRaccolto => livelloRafficaRaccolto > 0;
    public int ColpiPerRafficaRaccolto =>
        CalcolaIntervalloRaffica(livelloRafficaRaccolto);
    public int NumeroProiettiliRafficaRaccolto =>
        CalcolaNumeroProiettiliRaffica(livelloRafficaRaccolto);
    public float AngoloColpoAggiuntivo => Mathf.Clamp(
        Configurazione.angoloColpoAggiuntivo,
        0f,
        30f
    );
    public float ValoreCritico => Mathf.Max(
        0f,
        livelloCritico * Mathf.Max(
            0.01f,
            Configurazione.probabilitaCriticoPerLivello
        )
    );
    public float ProbabilitaCritico => Mathf.Clamp01(ValoreCritico);
    public float MoltiplicatoreDannoCriticoFinale =>
        CalcolaMoltiplicatoreDannoCritico(livelloCritico);

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
                return Mathf.Max(1, configurazione.costoCura);
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
            case TipoPotenziamento.ColpoAggiuntivo:
                return CostoLivello(
                    configurazione.costiColpoAggiuntivo,
                    livelloColpoAggiuntivo
                );
            case TipoPotenziamento.RafficaRaccolto:
                return CostoLivello(
                    configurazione.costiRafficaRaccolto,
                    livelloRafficaRaccolto
                );
            case TipoPotenziamento.PatataGigante:
                return CostoLivello(
                    configurazione.costiPatataGigante,
                    livelloPatataGigante
                );
            case TipoPotenziamento.PatataEsplosiva:
                return CostoLivello(
                    configurazione.costiPatataEsplosiva,
                    livelloPatataEsplosiva
                );
            case TipoPotenziamento.Critico:
                return CostoLivello(
                    configurazione.costiCritico,
                    livelloCritico
                );
            case TipoPotenziamento.Rimbalzo:
                return CostoLivello(
                    configurazione.costiRimbalzo,
                    livelloRimbalzo
                );
            case TipoPotenziamento.Rallentamento:
                return CostoLivello(
                    configurazione.costiRallentamento,
                    livelloRallentamento
                );
            case TipoPotenziamento.Spinta:
                return CostoLivello(
                    configurazione.costiSpinta,
                    livelloSpinta
                );
            default:
                return 0;
        }
    }

    public bool PuoAcquistare(TipoPotenziamento tipo)
    {
        if (tipo == TipoPotenziamento.Cura)
        {
            return salute != null && !salute.VitaPiena;
        }

        bool componenteDisponibile;
        switch (tipo)
        {
            case TipoPotenziamento.Movimento:
                componenteDisponibile = movimento != null;
                break;
            case TipoPotenziamento.Resistenza:
            case TipoPotenziamento.SaluteMassima:
                componenteDisponibile = salute != null;
                break;
            default:
                componenteDisponibile = sparo != null;
                break;
        }

        return componenteDisponibile;
    }

    public bool PuoComparire(TipoPotenziamento tipo)
    {
        return tipo != TipoPotenziamento.Cura && PuoAcquistare(tipo);
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
                : "Potenziamento non disponibile.";
            return false;
        }

        int costo = OttieniCosto(tipo);
        if (GameManager.instance == null ||
            !GameManager.instance.ProvaSpendiMonete(costo))
        {
            int possedute = GameManager.instance != null
                ? GameManager.instance.monete
                : 0;
            int mancanti = Mathf.Max(0, costo - possedute);
            messaggio = mancanti > 0
                ? "Ti mancano " + mancanti + " monete."
                : "Non hai abbastanza monete.";
            return false;
        }

        Applica(tipo);
        PotenziamentoAcquistato?.Invoke(tipo);
        messaggio = "Acquistato!";
        return true;
    }

    public bool ProvaApplicareGratis(
        TipoPotenziamento tipo,
        out string messaggio
    )
    {
        if (!PuoAcquistare(tipo))
        {
            messaggio = tipo == TipoPotenziamento.Cura &&
                        salute != null && salute.VitaPiena
                ? "La salute è già al massimo."
                : "Potenziamento non disponibile.";
            return false;
        }

        Applica(tipo);
        PotenziamentoAcquistato?.Invoke(tipo);
        messaggio = "Ottenuto gratis!";
        return true;
    }

    public string OttieniTitolo(TipoPotenziamento tipo)
    {
        switch (tipo)
        {
            case TipoPotenziamento.Movimento: return "STIVALI DA CAMPO";
            case TipoPotenziamento.Resistenza: return "GIACCA RINFORZATA";
            case TipoPotenziamento.SaluteMassima: return "COLAZIONE ROBUSTA";
            case TipoPotenziamento.Cura: return "RIMEDIO DELLA NONNA";
            case TipoPotenziamento.Danno: return "PATATE PIÙ DURE";
            case TipoPotenziamento.Cadenza: return "CARICATORE RAPIDO";
            case TipoPotenziamento.Penetrazione: return "PATATA PERFORANTE";
            case TipoPotenziamento.ColpoAggiuntivo:
                return "SECONDA CANNA";
            case TipoPotenziamento.RafficaRaccolto:
                return "RAFFICA DEL RACCOLTO";
            case TipoPotenziamento.PatataGigante:
                return "PATATA GIGANTE";
            case TipoPotenziamento.PatataEsplosiva:
                return "PATATA ESPLOSIVA";
            case TipoPotenziamento.Critico:
                return "CENTRO PERFETTO";
            case TipoPotenziamento.Rimbalzo:
                return "COLPO DI SPONDA";
            case TipoPotenziamento.Rallentamento:
                return "PATATA APPICCICOSA";
            case TipoPotenziamento.Spinta:
                return "IMPATTO PESANTE";
            default:
                return "POTENZIAMENTO";
        }
    }

    public string OttieniDescrizione(TipoPotenziamento tipo)
    {
        switch (tipo)
        {
            case TipoPotenziamento.Movimento:
                return "Aumenta la velocità del contadino.";
            case TipoPotenziamento.Resistenza:
                return "Blocca automaticamente alcuni colpi.";
            case TipoPotenziamento.SaluteMassima:
                return "Aumenta la vita massima e cura subito.";
            case TipoPotenziamento.Cura:
                return "Recupera immediatamente salute.";
            case TipoPotenziamento.Danno:
                return "Ogni patata infligge più danni.";
            case TipoPotenziamento.Cadenza:
                return "Riduce il tempo tra due spari.";
            case TipoPotenziamento.Penetrazione:
                return "Continua oltre una volpe eliminata.";
            case TipoPotenziamento.ColpoAggiuntivo:
                return "Aggiunge patate laterali e poi ne aumenta la potenza.";
            case TipoPotenziamento.RafficaRaccolto:
                return "Genera raffiche sempre più frequenti e potenti.";
            case TipoPotenziamento.PatataGigante:
                return "Proiettili più grandi, pesanti e leggermente lenti.";
            case TipoPotenziamento.PatataEsplosiva:
                return "Aumenta raggio e danno dell'esplosione all'impatto.";
            case TipoPotenziamento.Critico:
                return "Aumenta la probabilità e poi il danno dei critici.";
            case TipoPotenziamento.Rimbalzo:
                return "Aggiunge salti; oltre il limite ne migliora l'efficacia.";
            case TipoPotenziamento.Rallentamento:
                return "Le volpi colpite restano rallentate per un po'.";
            case TipoPotenziamento.Spinta:
                return "Gli impatti respingono realmente le volpi.";
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

        return "Livello " + OttieniLivello(tipo);
    }

    /// <summary>
    /// Descrive soltanto il vantaggio del prossimo acquisto, senza il
    /// confronto prima/dopo che rallentava la lettura delle carte.
    /// </summary>
    public string OttieniBonusProssimoLivello(TipoPotenziamento tipo)
    {
        ShopBalanceSettings configurazione = Configurazione;
        switch (tipo)
        {
            case TipoPotenziamento.Movimento:
            {
                float incremento =
                    CalcolaBonusMovimento(livelloMovimento + 1) -
                    CalcolaBonusMovimento(livelloMovimento);
                return "VELOCITÀ +" + FormattaDecimale(incremento);
            }
            case TipoPotenziamento.Resistenza:
            {
                float incremento =
                    CalcolaProbabilitaBlocco(livelloResistenza + 1) -
                    CalcolaProbabilitaBlocco(livelloResistenza);
                return "BLOCCO +" +
                    Mathf.Max(1, Mathf.RoundToInt(incremento * 100f)) + "%";
            }
            case TipoPotenziamento.SaluteMassima:
                return "VITA MASSIMA +" +
                    Mathf.Max(1, configurazione.incrementoSaluteMassima);
            case TipoPotenziamento.Cura:
                return "CURA +" + Mathf.Max(1, configurazione.quantitaCura);
            case TipoPotenziamento.Danno:
                return "DANNO +" + Mathf.Max(1, configurazione.incrementoDanno);
            case TipoPotenziamento.Cadenza:
            {
                float attuale = CalcolaIntervalloSparo(livelloCadenza);
                float prossimo = CalcolaIntervalloSparo(livelloCadenza + 1);
                int incremento = prossimo > 0f
                    ? Mathf.Max(
                        1,
                        Mathf.RoundToInt((attuale / prossimo - 1f) * 100f)
                    )
                    : 0;
                return "ATTACCO RAPIDO +" + incremento + "%";
            }
            case TipoPotenziamento.Penetrazione:
                return "PENETRAZIONE +" +
                    Mathf.Max(1, configurazione.incrementoPenetrazione);
            case TipoPotenziamento.ColpoAggiuntivo:
            {
                if (ValoreColpiAggiuntivi < MassimoColpiAggiuntiviFisici)
                {
                    return "COLPO EXTRA +" + Mathf.RoundToInt(
                        Mathf.Max(
                            0.05f,
                            configurazione.probabilitaColpoAggiuntivoPerLivello
                        ) * 100f
                    ) + "%";
                }
                return "POTENZA COLPI +3,5%";
            }
            case TipoPotenziamento.RafficaRaccolto:
            {
                int prossimoLivello = livelloRafficaRaccolto + 1;
                int colpiAttuali = CalcolaNumeroProiettiliRaffica(
                    livelloRafficaRaccolto
                );
                int colpiProssimi = CalcolaNumeroProiettiliRaffica(
                    prossimoLivello
                );
                if (livelloRafficaRaccolto <= 0)
                {
                    return "RAFFICA " + colpiProssimi + " COLPI";
                }
                if (colpiProssimi > colpiAttuali)
                {
                    return "RAFFICA +" + (colpiProssimi - colpiAttuali) +
                           " COLPI";
                }
                int intervallo = CalcolaIntervalloRaffica(prossimoLivello);
                if (intervallo < CalcolaIntervalloRaffica(
                    livelloRafficaRaccolto
                ))
                {
                    return "RAFFICA OGNI " + intervallo + " SPARI";
                }
                return "DANNO RAFFICA +10%";
            }
            case TipoPotenziamento.PatataGigante:
            {
                float attuale = CalcolaScalaPatataGigante(livelloPatataGigante);
                float prossimo = CalcolaScalaPatataGigante(
                    livelloPatataGigante + 1
                );
                if (prossimo > attuale + 0.001f)
                {
                    return "DIMENSIONE +" + Mathf.RoundToInt(
                        (prossimo - attuale) * 100f
                    ) + "%";
                }
                return "POTENZA GIGANTE +5%";
            }
            case TipoPotenziamento.PatataEsplosiva:
            {
                if (livelloPatataEsplosiva <= 0)
                    return "ESPLOSIONE SBLOCCATA";
                float attuale = CalcolaRaggioEsplosione(
                    livelloPatataEsplosiva
                );
                float prossimo = CalcolaRaggioEsplosione(
                    livelloPatataEsplosiva + 1
                );
                int incremento = attuale > 0f
                    ? Mathf.Max(
                        1,
                        Mathf.RoundToInt((prossimo / attuale - 1f) * 100f)
                    )
                    : 0;
                return "ESPLOSIONE +" + incremento + "%";
            }
            case TipoPotenziamento.Critico:
            {
                if (ValoreCritico < 1f)
                {
                    return "CRITICO +" + Mathf.RoundToInt(
                        Mathf.Max(
                            0.01f,
                            configurazione.probabilitaCriticoPerLivello
                        ) * 100f
                    ) + "%";
                }
                return "DANNO CRITICO +45%";
            }
            case TipoPotenziamento.Rimbalzo:
                return livelloRimbalzo < MassimoRimbalziFisici
                    ? "RIMBALZO +1"
                    : "POTENZA RIMBALZO +3,5%";
            case TipoPotenziamento.Rallentamento:
            {
                if (livelloRallentamento >=
                    MassimoLivelloRallentamentoFisico)
                {
                    return "POTENZA CONTROLLO +2,5%";
                }
                int prossimoLivello = livelloRallentamento + 1;
                int percentuale = Mathf.RoundToInt(
                    (1f - CalcolaMoltiplicatoreRallentamento(prossimoLivello)) *
                    100f
                );
                return "RALLENTA " + percentuale + "% / " +
                    FormattaDecimale(
                        CalcolaDurataRallentamento(prossimoLivello)
                    ) + " s";
            }
            case TipoPotenziamento.Spinta:
            {
                float attuale = CalcolaForzaSpintaTotale(livelloSpinta, 0);
                float prossimo = CalcolaForzaSpintaTotale(
                    livelloSpinta + 1,
                    0
                );
                return prossimo > attuale + 0.001f
                    ? "SPINTA +" + FormattaDecimale(prossimo - attuale)
                    : "POTENZA IMPATTO +2,5%";
            }
            default:
                return "POTENZIAMENTO +1";
        }
    }

    public string OttieniConfronto(TipoPotenziamento tipo)
    {
        ShopBalanceSettings configurazione = Configurazione;
        switch (tipo)
        {
            case TipoPotenziamento.Movimento:
            {
                float attuale = movimento != null
                    ? movimento.VelocitaFinale
                    : 0f;
                return Confronto(
                    FormattaDecimale(attuale),
                    FormattaDecimale(
                        CalcolaVelocitaMovimento(livelloMovimento + 1)
                    )
                );
            }
            case TipoPotenziamento.Resistenza:
            {
                float attuale = salute != null
                    ? salute.ProbabilitaBloccoFinale
                    : 0f;
                return ConfrontoPercentuale(
                    attuale,
                    CalcolaProbabilitaBlocco(livelloResistenza + 1)
                );
            }
            case TipoPotenziamento.SaluteMassima:
            {
                int attuale = salute != null ? salute.VitaMassimaFinale : 0;
                return Confronto(
                    attuale.ToString(),
                    SommaSicura(
                        attuale,
                        Mathf.Max(1, configurazione.incrementoSaluteMassima)
                    ).ToString()
                );
            }
            case TipoPotenziamento.Cura:
            {
                int attuale = salute != null ? salute.VitaCorrente : 0;
                int massimo = salute != null ? salute.VitaMassimaFinale : 0;
                return Confronto(
                    attuale.ToString(),
                    Mathf.Min(
                        massimo,
                        SommaSicura(attuale, configurazione.quantitaCura)
                    ).ToString()
                );
            }
            case TipoPotenziamento.Danno:
            {
                int attuale = sparo != null ? sparo.DannoFinale : 0;
                return Confronto(
                    attuale.ToString(),
                    SommaSicura(
                        attuale,
                        Mathf.Max(1, configurazione.incrementoDanno)
                    ).ToString()
                );
            }
            case TipoPotenziamento.Cadenza:
            {
                float attuale = sparo != null
                    ? sparo.IntervalloSparoFinale
                    : 0f;
                float prossimo = sparo != null
                    ? CalcolaIntervalloSparo(livelloCadenza + 1)
                    : 0f;
                return Confronto(
                    FormattaDecimale(attuale) + " s",
                    FormattaDecimale(prossimo) + " s"
                );
            }
            case TipoPotenziamento.Penetrazione:
            {
                int attuale = sparo != null ? sparo.PenetrazioneFinale : 0;
                return Confronto(
                    attuale.ToString(),
                    SommaSicura(
                        attuale,
                        Mathf.Max(1, configurazione.incrementoPenetrazione)
                    ).ToString()
                );
            }
            case TipoPotenziamento.ColpoAggiuntivo:
                return Confronto(
                    DescriviColpiAggiuntivi(livelloColpoAggiuntivo),
                    DescriviColpiAggiuntivi(livelloColpoAggiuntivo + 1)
                );
            case TipoPotenziamento.RafficaRaccolto:
                return Confronto(
                    DescriviRaffica(livelloRafficaRaccolto),
                    DescriviRaffica(livelloRafficaRaccolto + 1)
                );
            case TipoPotenziamento.PatataGigante:
                return Confronto(
                    DescriviPatataGigante(livelloPatataGigante),
                    DescriviPatataGigante(livelloPatataGigante + 1)
                );
            case TipoPotenziamento.PatataEsplosiva:
                return Confronto(
                    DescriviEsplosione(livelloPatataEsplosiva),
                    DescriviEsplosione(livelloPatataEsplosiva + 1)
                );
            case TipoPotenziamento.Critico:
                return Confronto(
                    DescriviCritico(livelloCritico),
                    DescriviCritico(livelloCritico + 1)
                );
            case TipoPotenziamento.Rimbalzo:
                return Confronto(
                    DescriviRimbalzo(livelloRimbalzo),
                    DescriviRimbalzo(livelloRimbalzo + 1)
                );
            case TipoPotenziamento.Rallentamento:
            {
                string attuale = livelloRallentamento > 0
                    ? DescriviRallentamento(livelloRallentamento)
                    : "nessuno";
                return Confronto(
                    attuale,
                    DescriviRallentamento(livelloRallentamento + 1)
                );
            }
            case TipoPotenziamento.Spinta:
                return Confronto(
                    DescriviSpinta(livelloSpinta),
                    DescriviSpinta(livelloSpinta + 1)
                );
            default:
                return string.Empty;
        }
    }

    private string DescriviValoreAttuale(
        TipoPotenziamento tipo,
        ShopBalanceSettings configurazione
    )
    {
        switch (tipo)
        {
            case TipoPotenziamento.Movimento:
                return FormattaDecimale(
                    movimento != null ? movimento.VelocitaFinale : 0f
                );
            case TipoPotenziamento.Resistenza:
            {
                int frequenza = salute != null
                    ? salute.FrequenzaBloccoFinale
                    : 0;
                return frequenza > 0
                    ? "1 ogni " + frequenza
                    : "nessun blocco";
            }
            case TipoPotenziamento.SaluteMassima:
                return (salute != null ? salute.VitaMassimaFinale : 0)
                    .ToString();
            case TipoPotenziamento.Danno:
                return (sparo != null ? sparo.DannoFinale : 0).ToString();
            case TipoPotenziamento.Cadenza:
                return FormattaDecimale(
                    sparo != null ? sparo.IntervalloSparoFinale : 0f
                ) + " s";
            case TipoPotenziamento.Penetrazione:
                return (sparo != null ? sparo.PenetrazioneFinale : 0)
                    .ToString();
            case TipoPotenziamento.ColpoAggiuntivo:
                return FormattaPercentuale(
                    livelloColpoAggiuntivo *
                    configurazione.probabilitaColpoAggiuntivoPerLivello
                );
            case TipoPotenziamento.RafficaRaccolto:
                return "ogni " +
                       configurazione.colpiPerRafficaRaccolto +
                       " spari";
            case TipoPotenziamento.PatataGigante:
                return FormattaPercentuale(
                    1f + livelloPatataGigante *
                    configurazione.incrementoScalaPatataGigante,
                    true
                ) + " / spinta " + FormattaDecimale(
                    livelloPatataGigante *
                    configurazione.forzaSpintaPatataGigantePerLivello
                );
            case TipoPotenziamento.PatataEsplosiva:
                return "raggio " +
                       FormattaDecimale(configurazione.raggioEsplosione);
            case TipoPotenziamento.Critico:
                return FormattaPercentuale(
                    livelloCritico *
                    configurazione.probabilitaCriticoPerLivello
                );
            case TipoPotenziamento.Rimbalzo:
                return livelloRimbalzo.ToString();
            case TipoPotenziamento.Rallentamento:
                return DescriviRallentamento(livelloRallentamento);
            case TipoPotenziamento.Spinta:
                return FormattaDecimale(
                    livelloSpinta * configurazione.forzaSpintaPerLivello
                );
            default:
                return string.Empty;
        }
    }

    public int OttieniLivello(TipoPotenziamento tipo)
    {
        switch (tipo)
        {
            case TipoPotenziamento.Movimento: return livelloMovimento;
            case TipoPotenziamento.Resistenza: return livelloResistenza;
            case TipoPotenziamento.SaluteMassima: return livelloSalute;
            case TipoPotenziamento.Danno: return livelloDanno;
            case TipoPotenziamento.Cadenza: return livelloCadenza;
            case TipoPotenziamento.Penetrazione: return livelloPenetrazione;
            case TipoPotenziamento.ColpoAggiuntivo:
                return livelloColpoAggiuntivo;
            case TipoPotenziamento.RafficaRaccolto:
                return livelloRafficaRaccolto;
            case TipoPotenziamento.PatataGigante:
                return livelloPatataGigante;
            case TipoPotenziamento.PatataEsplosiva:
                return livelloPatataEsplosiva;
            case TipoPotenziamento.Critico: return livelloCritico;
            case TipoPotenziamento.Rimbalzo: return livelloRimbalzo;
            case TipoPotenziamento.Rallentamento:
                return livelloRallentamento;
            case TipoPotenziamento.Spinta: return livelloSpinta;
            default: return 0;
        }
    }

    public int OttieniLivelloMassimo(TipoPotenziamento tipo)
    {
        return tipo == TipoPotenziamento.Cura ? 0 : int.MaxValue;
    }

    public int OttieniPuntiPercorso(PercorsoBuild percorso)
    {
        int punti = 0;
        IReadOnlyList<DefinizionePotenziamentoBuild> definizioni =
            CatalogoPotenziamentiBuild.Tutte;
        for (int i = 0; i < definizioni.Count; i++)
        {
            if (definizioni[i].Percorso == percorso)
            {
                punti += OttieniLivello(definizioni[i].Tipo);
            }
        }
        return punti;
    }

    public string DescriviBuildCompatta()
    {
        StringBuilder testo = new StringBuilder();
        AggiungiPercorso(testo, PercorsoBuild.Raffica);
        AggiungiPercorso(testo, PercorsoBuild.Artiglieria);
        AggiungiPercorso(testo, PercorsoBuild.Perforazione);
        AggiungiPercorso(testo, PercorsoBuild.Controllo);

        int utilita = OttieniPuntiPercorso(PercorsoBuild.Utilita);
        if (utilita > 0)
        {
            if (testo.Length > 0) testo.Append("  |  ");
            testo.Append("UTILITÀ ");
            testo.Append(utilita);
        }
        return testo.Length > 0 ? testo.ToString() : "NESSUNA BUILD";
    }

    public ProfiloProiettileBuild CreaProfiloProiettile(bool critico)
    {
        return CreaProfiloProiettile(critico, false);
    }

    public ProfiloProiettileBuild CreaProfiloProiettile(
        bool critico,
        bool colpoRaffica
    )
    {
        ShopBalanceSettings configurazione = Configurazione;
        int dannoBase = sparo != null ? sparo.DannoFinale : 1;
        double moltiplicatoreDanno =
            CalcolaMoltiplicatoreOverflowTecnico();
        if (colpoRaffica && livelloRafficaRaccolto > 0)
        {
            moltiplicatoreDanno *= 1d +
                0.1d * Math.Max(0, livelloRafficaRaccolto - 1);
        }

        ProfiloProiettileBuild profilo = new ProfiloProiettileBuild
        {
            Danno = SommaSicura(
                MoltiplicaDannoSicuro(dannoBase, moltiplicatoreDanno),
                CalcolaBonusDannoOverflowIntero()
            ),
            Penetrazioni = sparo != null ? sparo.PenetrazioneFinale : 0,
            Scala = CalcolaScalaPatataGigante(livelloPatataGigante),
            MoltiplicatoreVelocita =
                CalcolaVelocitaPatataGigante(livelloPatataGigante),
            Critico = critico,
            Rimbalzi = Mathf.Min(
                livelloRimbalzo,
                MassimoRimbalziFisici
            ),
            RaggioRimbalzo = CalcolaRaggioRimbalzo(livelloRimbalzo),
            MoltiplicatoreDannoRimbalzo =
                CalcolaDannoRimbalzo(livelloRimbalzo),
            RaggioEsplosione =
                CalcolaRaggioEsplosione(livelloPatataEsplosiva),
            MoltiplicatoreRallentamento =
                CalcolaMoltiplicatoreRallentamento(livelloRallentamento),
            DurataRallentamento =
                CalcolaDurataRallentamento(livelloRallentamento),
            ForzaSpinta = CalcolaForzaSpintaTotale(
                livelloSpinta,
                livelloPatataGigante
            )
        };

        if (critico)
        {
            profilo.Danno = SommaSicura(
                MoltiplicaDannoSicuro(
                    profilo.Danno,
                    CalcolaMoltiplicatoreDannoCritico(livelloCritico)
                ),
                CalcolaBonusDannoCriticoIntero(livelloCritico)
            );
        }
        if (colpoRaffica && livelloRafficaRaccolto > 1)
        {
            profilo.Danno = SommaSicura(
                profilo.Danno,
                livelloRafficaRaccolto - 1
            );
        }
        profilo.DannoEsplosione = profilo.RaggioEsplosione > 0f
            ? SommaSicura(
                MoltiplicaDannoSicuro(
                    profilo.Danno,
                    Math.Max(
                        0.1d,
                        configurazione.moltiplicatoreDannoEsplosione
                    ) *
                    (1d + 0.14d * Math.Max(
                        0,
                        livelloPatataEsplosiva - 1
                    ))
                ),
                Math.Max(0, livelloPatataEsplosiva - 1)
            )
            : 0;
        return profilo;
    }

    private void Applica(TipoPotenziamento tipo)
    {
        ShopBalanceSettings configurazione = Configurazione;
        switch (tipo)
        {
            case TipoPotenziamento.Movimento:
                livelloMovimento = IncrementaLivello(livelloMovimento);
                movimento.ImpostaBonusVelocita(
                    CalcolaBonusMovimento(livelloMovimento)
                );
                break;
            case TipoPotenziamento.Resistenza:
                livelloResistenza = IncrementaLivello(livelloResistenza);
                salute.ImpostaProbabilitaBlocco(
                    CalcolaProbabilitaBlocco(livelloResistenza)
                );
                break;
            case TipoPotenziamento.SaluteMassima:
                livelloSalute = IncrementaLivello(livelloSalute);
                salute.ImpostaBonusVitaMassima(
                    ProdottoSicuro(
                        livelloSalute,
                        Mathf.Max(1, configurazione.incrementoSaluteMassima)
                    ),
                    Mathf.Max(0, configurazione.curaSuIncrementoSalute)
                );
                break;
            case TipoPotenziamento.Cura:
                salute.Cura(Mathf.Max(0, configurazione.quantitaCura));
                break;
            case TipoPotenziamento.Danno:
                livelloDanno = IncrementaLivello(livelloDanno);
                sparo.ImpostaBonusDanno(
                    ProdottoSicuro(
                        livelloDanno,
                        Mathf.Max(1, configurazione.incrementoDanno)
                    )
                );
                break;
            case TipoPotenziamento.Cadenza:
                livelloCadenza = IncrementaLivello(livelloCadenza);
                sparo.ImpostaBonusRiduzioneIntervalloSparo(
                    Mathf.Max(
                        0f,
                        sparo.IntervalloSparoBase -
                        CalcolaIntervalloSparo(livelloCadenza)
                    )
                );
                break;
            case TipoPotenziamento.Penetrazione:
                livelloPenetrazione = IncrementaLivello(livelloPenetrazione);
                sparo.ImpostaBonusPenetrazione(
                    ProdottoSicuro(
                        livelloPenetrazione,
                        Mathf.Max(1, configurazione.incrementoPenetrazione)
                    )
                );
                break;
            case TipoPotenziamento.ColpoAggiuntivo:
                livelloColpoAggiuntivo =
                    IncrementaLivello(livelloColpoAggiuntivo);
                break;
            case TipoPotenziamento.RafficaRaccolto:
                livelloRafficaRaccolto =
                    IncrementaLivello(livelloRafficaRaccolto);
                break;
            case TipoPotenziamento.PatataGigante:
                livelloPatataGigante =
                    IncrementaLivello(livelloPatataGigante);
                break;
            case TipoPotenziamento.PatataEsplosiva:
                livelloPatataEsplosiva =
                    IncrementaLivello(livelloPatataEsplosiva);
                break;
            case TipoPotenziamento.Critico:
                livelloCritico = IncrementaLivello(livelloCritico);
                break;
            case TipoPotenziamento.Rimbalzo:
                livelloRimbalzo = IncrementaLivello(livelloRimbalzo);
                break;
            case TipoPotenziamento.Rallentamento:
                livelloRallentamento =
                    IncrementaLivello(livelloRallentamento);
                break;
            case TipoPotenziamento.Spinta:
                livelloSpinta = IncrementaLivello(livelloSpinta);
                break;
        }
    }

    private float CalcolaBonusMovimento(int livello)
    {
        if (livello <= 0) return 0f;

        float incremento = Mathf.Max(
            0.01f,
            Configurazione.incrementoMovimento
        );
        float velocitaBase = movimento != null
            ? movimento.VelocitaBase
            : 8f;
        float limite = Mathf.Max(
            incremento * 8f,
            velocitaBase * 0.5f
        );
        double costante = Math.Max(1d, limite / incremento - 1d);
        return (float)(limite * livello / (livello + costante));
    }

    private float CalcolaVelocitaMovimento(int livello)
    {
        float baseMovimento = movimento != null
            ? movimento.VelocitaBase
            : 0f;
        return baseMovimento + CalcolaBonusMovimento(livello);
    }

    private float CalcolaIntervalloSparo(int livello)
    {
        if (sparo == null) return 0f;

        float baseSparo = sparo.IntervalloSparoBase;
        float minimo = sparo.IntervalloSparoMinimo;
        if (livello <= 0 || baseSparo <= minimo) return baseSparo;

        float spazio = baseSparo - minimo;
        float primaRiduzione = Mathf.Clamp(
            Mathf.Max(0.001f, Configurazione.riduzioneIntervalloSparo),
            0.001f,
            spazio * 0.75f
        );
        double costante = Math.Max(
            1d / 3d,
            spazio / primaRiduzione - 1d
        );
        return minimo + (float)(
            spazio * costante / (livello + costante)
        );
    }

    private float CalcolaProbabilitaBlocco(int livello)
    {
        if (livello <= 0) return 0f;

        int[] frequenze = Configurazione.frequenzeBlocco;
        int configurati = frequenze != null ? frequenze.Length : 0;
        if (livello <= configurati)
        {
            return 1f / Mathf.Max(2, frequenze[livello - 1]);
        }

        float partenza = configurati > 0
            ? 1f / Mathf.Max(2, frequenze[configurati - 1])
            : 0f;
        int livelliOltreConfigurazione = livello - configurati;
        double progresso = livelliOltreConfigurazione /
            (livelliOltreConfigurazione + 8d);
        return Mathf.Clamp(
            partenza +
            (ProbabilitaBloccoAsintotica - partenza) * (float)progresso,
            0f,
            ProbabilitaBloccoAsintotica
        );
    }

    private float CalcolaMoltiplicatoreDannoCritico(int livello)
    {
        float baseCritico = Mathf.Max(
            1f,
            Configurazione.moltiplicatoreDannoCritico
        );
        if (livello <= 0) return baseCritico;

        double valoreCritico = livello * (double)Mathf.Max(
            0.01f,
            Configurazione.probabilitaCriticoPerLivello
        );
        double oltreCentoPercento = Math.Max(0d, valoreCritico - 1d);
        return (float)Math.Min(
            float.MaxValue,
            baseCritico * (1d + 0.45d * oltreCentoPercento)
        );
    }

    private float CalcolaScalaPatataGigante(int livello)
    {
        if (livello <= 0) return 1f;
        double incremento = Math.Max(
            0.02d,
            Configurazione.incrementoScalaPatataGigante
        );
        return (float)Math.Min(
            ScalaMassimaProiettile,
            1d + livello * incremento
        );
    }

    private float CalcolaVelocitaPatataGigante(int livello)
    {
        if (livello <= 0) return 1f;
        float incrementoScala = Mathf.Max(
            0.02f,
            Configurazione.incrementoScalaPatataGigante
        );
        int livelliFisici = Mathf.Max(
            1,
            Mathf.CeilToInt(
                (ScalaMassimaProiettile - 1f) / incrementoScala
            )
        );
        int livelloEffettivo = Mathf.Min(livello, livelliFisici);
        return Mathf.Clamp(
            1f - livelloEffettivo * Mathf.Max(
                0f,
                Configurazione.riduzioneVelocitaPatataGigante
            ),
            0.55f,
            1f
        );
    }

    private float CalcolaRaggioEsplosione(int livello)
    {
        if (livello <= 0) return 0f;
        float raggioBase = Mathf.Max(
            0.2f,
            Configurazione.raggioEsplosione
        );
        int livelliExtra = livello - 1;
        double crescita = livelliExtra / (livelliExtra + 4d);
        return raggioBase * (1f + 0.65f * (float)crescita);
    }

    private float CalcolaRaggioRimbalzo(int livello)
    {
        float raggioBase = Mathf.Max(
            0.5f,
            Configurazione.raggioRicercaRimbalzo
        );
        int overflow = Math.Max(0, livello - MassimoRimbalziFisici);
        double crescita = overflow / (overflow + 5d);
        return raggioBase * (1f + 0.35f * (float)crescita);
    }

    private float CalcolaDannoRimbalzo(int livello)
    {
        float baseRimbalzo = Mathf.Clamp(
            Configurazione.moltiplicatoreDannoRimbalzo,
            0.5f,
            0.9f
        );
        int overflow = Math.Max(0, livello - MassimoRimbalziFisici);
        double crescita = overflow / (overflow + 5d);
        return Mathf.Lerp(baseRimbalzo, 0.94f, (float)crescita);
    }

    private float CalcolaForzaSpintaTotale(
        int livelloSpinta,
        int livelloGigante
    )
    {
        double forza =
            livelloSpinta * (double)Mathf.Max(
                0.05f,
                Configurazione.forzaSpintaPerLivello
            ) +
            livelloGigante * (double)Mathf.Max(
                0.05f,
                Configurazione.forzaSpintaPatataGigantePerLivello
            );
        return (float)Math.Min(ForzaSpintaMassima, forza);
    }

    private double CalcolaMoltiplicatoreOverflowTecnico()
    {
        ShopBalanceSettings configurazione = Configurazione;
        double incrementoScala = Math.Max(
            0.02d,
            configurazione.incrementoScalaPatataGigante
        );
        double scalaDesiderata = 1d +
            livelloPatataGigante * incrementoScala;
        double overflowGigante = Math.Max(
            0d,
            (scalaDesiderata - ScalaMassimaProiettile) / incrementoScala
        );
        int overflowRimbalzo = Math.Max(
            0,
            livelloRimbalzo - MassimoRimbalziFisici
        );
        int overflowRallentamento = Math.Max(
            0,
            livelloRallentamento - MassimoLivelloRallentamentoFisico
        );
        double incrementoSpinta = Math.Max(
            0.05d,
            configurazione.forzaSpintaPerLivello
        );
        int livelliSpintaFisici = Math.Max(
            1,
            (int)Math.Ceiling(ForzaSpintaMassima / incrementoSpinta)
        );
        int overflowSpinta = Math.Max(
            0,
            livelloSpinta - livelliSpintaFisici
        );
        double overflowColpi = Math.Max(
            0d,
            ValoreColpiAggiuntivi - MassimoColpiAggiuntiviFisici
        );

        return 1d +
            overflowGigante * 0.05d +
            overflowRimbalzo * 0.035d +
            overflowRallentamento * 0.025d +
            overflowSpinta * 0.025d +
            overflowColpi * 0.035d;
    }

    private int CalcolaBonusDannoOverflowIntero()
    {
        long bonus = 0L;
        bonus += Math.Max(
            0,
            livelloPatataGigante - CalcolaLivelloCapPatataGigante()
        );
        bonus += Math.Max(
            0,
            livelloColpoAggiuntivo - CalcolaLivelloCapColpiAggiuntivi()
        );
        bonus += Math.Max(
            0,
            livelloRimbalzo - MassimoRimbalziFisici
        );
        bonus += Math.Max(
            0,
            livelloRallentamento - MassimoLivelloRallentamentoFisico
        );
        bonus += Math.Max(
            0,
            livelloSpinta - CalcolaLivelloCapSpinta()
        );
        return bonus >= int.MaxValue ? int.MaxValue : (int)bonus;
    }

    private int CalcolaLivelloCapColpiAggiuntivi()
    {
        double probabilitaPerLivello = Math.Max(
            0.05d,
            Configurazione.probabilitaColpoAggiuntivoPerLivello
        );
        return Math.Max(
            1,
            (int)Math.Ceiling(
                MassimoColpiAggiuntiviFisici / probabilitaPerLivello
            )
        );
    }

    private int CalcolaLivelloCapPatataGigante()
    {
        double incrementoScala = Math.Max(
            0.02d,
            Configurazione.incrementoScalaPatataGigante
        );
        int capScala = Math.Max(
            1,
            (int)Math.Ceiling(
                (ScalaMassimaProiettile - 1d) / incrementoScala
            )
        );
        double incrementoSpinta = Math.Max(
            0.05d,
            Configurazione.forzaSpintaPatataGigantePerLivello
        );
        int capSpinta = Math.Max(
            1,
            (int)Math.Ceiling(ForzaSpintaMassima / incrementoSpinta)
        );
        return Math.Max(capScala, capSpinta);
    }

    private int CalcolaLivelloCapSpinta()
    {
        double incremento = Math.Max(
            0.05d,
            Configurazione.forzaSpintaPerLivello
        );
        return Math.Max(
            1,
            (int)Math.Ceiling(ForzaSpintaMassima / incremento)
        );
    }

    private int CalcolaLivelloSogliaCriticoGarantito()
    {
        double probabilitaPerLivello = Math.Max(
            0.01d,
            Configurazione.probabilitaCriticoPerLivello
        );
        return Math.Max(
            1,
            (int)Math.Ceiling(1d / probabilitaPerLivello)
        );
    }

    private int CalcolaBonusDannoCriticoIntero(int livello)
    {
        return Math.Max(
            0,
            livello - CalcolaLivelloSogliaCriticoGarantito()
        );
    }

    private float CalcolaMoltiplicatoreRallentamento(int livello)
    {
        if (livello <= 0) return 1f;
        ShopBalanceSettings configurazione = Configurazione;
        int livelloFisico = Mathf.Min(
            livello,
            MassimoLivelloRallentamentoFisico
        );
        return Mathf.Clamp(
            configurazione.rallentamentoPrimoLivello -
            (livelloFisico - 1) *
            configurazione.riduzioneRallentamentoPerLivello,
            0.25f,
            0.95f
        );
    }

    private float CalcolaDurataRallentamento(int livello)
    {
        if (livello <= 0) return 0f;
        ShopBalanceSettings configurazione = Configurazione;
        int livelloFisico = Mathf.Min(
            livello,
            MassimoLivelloRallentamentoFisico
        );
        return Mathf.Clamp(
            configurazione.durataRallentamentoBase +
            (livelloFisico - 1) *
            configurazione.durataRallentamentoPerLivello,
            0.1f,
            6f
        );
    }

    private string DescriviRallentamento(int livello)
    {
        float moltiplicatore = CalcolaMoltiplicatoreRallentamento(livello);
        float durata = CalcolaDurataRallentamento(livello);
        int percentuale = Mathf.RoundToInt((1f - moltiplicatore) * 100f);
        string descrizione = "-" + percentuale + "% per " +
            FormattaDecimale(durata) + " s";
        int overflow = Math.Max(
            0,
            livello - MassimoLivelloRallentamentoFisico
        );
        return overflow > 0
            ? descrizione + " / potenza +" + (overflow * 2.5f)
                .ToString("0.#", CultureInfo.InvariantCulture) +
              "% / danno +" + overflow
            : descrizione;
    }

    private string DescriviColpiAggiuntivi(int livello)
    {
        if (livello <= 0) return "nessuno";
        double valore = livello * (double)Mathf.Max(
            0.05f,
            Configurazione.probabilitaColpoAggiuntivoPerLivello
        );
        int garantiti = (int)Math.Min(
            MassimoColpiAggiuntiviFisici,
            Math.Floor(valore)
        );
        double probabilita = garantiti >= MassimoColpiAggiuntiviFisici
            ? 0d
            : valore - Math.Floor(valore);
        double overflow = Math.Max(
            0d,
            valore - MassimoColpiAggiuntiviFisici
        );
        int bonusIntero = Math.Max(
            0,
            livello - CalcolaLivelloCapColpiAggiuntivi()
        );

        string risultato = garantiti > 0
            ? garantiti + " sicuri"
            : string.Empty;
        if (probabilita > 0.0001d)
        {
            if (risultato.Length > 0) risultato += " + ";
            risultato += Math.Round(probabilita * 100d) + "%";
        }
        if (overflow > 0d)
        {
            risultato += " / potenza +" +
                Math.Round(overflow * 3.5d, 1) + "%";
        }
        if (bonusIntero > 0)
        {
            risultato += " / danno +" + bonusIntero;
        }
        return risultato.Length > 0 ? risultato : "nessuno";
    }

    private string DescriviRaffica(int livello)
    {
        if (livello <= 0) return "disattiva";
        int intervallo = CalcolaIntervalloRaffica(livello);
        int proiettili = CalcolaNumeroProiettiliRaffica(livello);
        float bonusDanno = Mathf.Max(0, livello - 1) * 10f;
        int bonusIntero = Math.Max(0, livello - 1);
        return proiettili + " colpi ogni " + intervallo +
            " spari / danno +" + FormattaDecimale(bonusDanno) +
            "% e +" + bonusIntero;
    }

    private int CalcolaIntervalloRaffica(int livello)
    {
        int baseRaffica = Mathf.Max(
            2,
            Configurazione.colpiPerRafficaRaccolto
        );
        if (livello <= 0) return baseRaffica;
        long valore = (long)baseRaffica - Math.Max(0, livello - 1);
        return (int)Math.Max(2L, valore);
    }

    private int CalcolaNumeroProiettiliRaffica(int livello)
    {
        if (livello <= 0) return 0;
        long gruppi = Math.Max(0L, ((long)livello - 1L) / 3L);
        return (int)Math.Min(
            MassimoColpiRafficaFisici,
            3L + 2L * gruppi
        );
    }

    private string DescriviPatataGigante(int livello)
    {
        if (livello <= 0) return "normale";
        float scala = CalcolaScalaPatataGigante(livello);
        float spinta = CalcolaForzaSpintaTotale(0, livello);
        double incremento = Math.Max(
            0.02d,
            Configurazione.incrementoScalaPatataGigante
        );
        double overflow = Math.Max(
            0d,
            (1d + livello * incremento - ScalaMassimaProiettile) /
            incremento
        );
        string testo = FormattaPercentuale(scala, true) +
            " / spinta " + FormattaDecimale(spinta);
        int bonusIntero = Math.Max(
            0,
            livello - CalcolaLivelloCapPatataGigante()
        );
        return overflow > 0d
            ? testo + " / potenza +" + Math.Round(overflow * 5d, 1) +
              "% / danno +" + bonusIntero
            : testo;
    }

    private string DescriviEsplosione(int livello)
    {
        if (livello <= 0) return "nessuna";
        double moltiplicatore = Math.Max(
            0.1d,
            Configurazione.moltiplicatoreDannoEsplosione
        ) * (1d + 0.14d * Math.Max(0, livello - 1));
        int bonusIntero = Math.Max(0, livello - 1);
        return "raggio " +
            FormattaDecimale(CalcolaRaggioEsplosione(livello)) +
            " / danno " + Math.Round(moltiplicatore * 100d) +
            "% e +" + bonusIntero;
    }

    private string DescriviCritico(int livello)
    {
        if (livello <= 0) return "nessuno";
        double valore = livello * (double)Mathf.Max(
            0.01f,
            Configurazione.probabilitaCriticoPerLivello
        );
        int probabilita = (int)Math.Round(Math.Min(1d, valore) * 100d);
        int bonusIntero = CalcolaBonusDannoCriticoIntero(livello);
        return probabilita + "% / danno x" +
            FormattaDecimale(CalcolaMoltiplicatoreDannoCritico(livello)) +
            " e +" + bonusIntero;
    }

    private string DescriviRimbalzo(int livello)
    {
        if (livello <= 0) return "nessuno";
        int fisici = Mathf.Min(livello, MassimoRimbalziFisici);
        int bonusIntero = Math.Max(
            0,
            livello - MassimoRimbalziFisici
        );
        return fisici + " / potenza " +
            FormattaPercentuale(CalcolaDannoRimbalzo(livello), true) +
            " / raggio " + FormattaDecimale(
                CalcolaRaggioRimbalzo(livello)
            ) + " / danno +" + bonusIntero;
    }

    private string DescriviSpinta(int livello)
    {
        if (livello <= 0) return "nessuna";
        float forza = CalcolaForzaSpintaTotale(livello, 0);
        double incremento = Math.Max(
            0.05d,
            Configurazione.forzaSpintaPerLivello
        );
        int livelliFisici = Math.Max(
            1,
            (int)Math.Ceiling(ForzaSpintaMassima / incremento)
        );
        int overflow = Math.Max(0, livello - livelliFisici);
        return overflow > 0
            ? FormattaDecimale(forza) + " / potenza +" +
              Math.Round(overflow * 2.5d, 1) + "% / danno +" + overflow
            : FormattaDecimale(forza);
    }

    private void AggiungiPercorso(StringBuilder testo, PercorsoBuild percorso)
    {
        int punti = OttieniPuntiPercorso(percorso);
        if (punti <= 0) return;
        if (testo.Length > 0) testo.Append("  |  ");
        testo.Append(CatalogoPotenziamentiBuild.NomePercorso(percorso));
        testo.Append(' ');
        testo.Append(punti);
    }

    private static int CostoLivello(int[] costi, int livello)
    {
        int livelloValido = Mathf.Max(0, livello);
        int quantiConfigurati = costi != null ? costi.Length : 0;
        if (livelloValido < quantiConfigurati)
        {
            return Mathf.Max(1, costi[livelloValido]);
        }

        int ultimo = quantiConfigurati > 0
            ? Mathf.Max(1, costi[quantiConfigurati - 1])
            : 3;
        int penultimo = quantiConfigurati > 1
            ? Mathf.Max(1, costi[quantiConfigurati - 2])
            : Mathf.Max(1, ultimo / 2);
        int passo = Mathf.Max(1, ultimo - penultimo);
        int curvatura = Mathf.Max(1, passo / 3);
        long livelliExtra = quantiConfigurati > 0
            ? (long)livelloValido - quantiConfigurati + 1L
            : livelloValido;
        double costo = ultimo +
            passo * (double)livelliExtra +
            curvatura * (double)livelliExtra *
            Math.Max(0L, livelliExtra - 1L) * 0.5d;
        return costo >= int.MaxValue
            ? int.MaxValue
            : Math.Max(1, (int)Math.Ceiling(costo));
    }

    private static int IncrementaLivello(int livello)
    {
        return livello < int.MaxValue ? livello + 1 : int.MaxValue;
    }

    private static int ProdottoSicuro(int primo, int secondo)
    {
        long prodotto = (long)Mathf.Max(0, primo) * Mathf.Max(0, secondo);
        return prodotto >= int.MaxValue ? int.MaxValue : (int)prodotto;
    }

    private static int SommaSicura(int primo, int secondo)
    {
        long somma = (long)Mathf.Max(0, primo) + Mathf.Max(0, secondo);
        return somma >= int.MaxValue ? int.MaxValue : (int)somma;
    }

    private static int MoltiplicaDannoSicuro(
        int danno,
        double moltiplicatore
    )
    {
        if (double.IsNaN(moltiplicatore) || moltiplicatore <= 0d) return 1;
        double risultato = Math.Max(1, danno) * moltiplicatore;
        if (double.IsInfinity(risultato) || risultato >= int.MaxValue)
        {
            return int.MaxValue;
        }
        return Math.Max(1, (int)Math.Round(risultato));
    }

    private static int Lunghezza(int[] valori)
    {
        return valori != null ? valori.Length : 0;
    }

    private static int ValoreArray(
        int[] valori,
        int indice,
        int fallback
    )
    {
        return valori != null && indice >= 0 && indice < valori.Length
            ? valori[indice]
            : fallback;
    }

    private static string Confronto(string attuale, string prossimo)
    {
        return "ORA  " + attuale + "   >   DOPO  " + prossimo;
    }

    private static string ConfrontoPercentuale(
        float attuale,
        float prossimo,
        bool valoreAssoluto = false
    )
    {
        int percentualeAttuale = Mathf.RoundToInt(
            (valoreAssoluto ? attuale : Mathf.Clamp01(attuale)) * 100f
        );
        int percentualeProssima = Mathf.RoundToInt(
            (valoreAssoluto ? prossimo : Mathf.Clamp01(prossimo)) * 100f
        );
        return Confronto(
            percentualeAttuale + "%",
            percentualeProssima + "%"
        );
    }

    private static string FormattaPercentuale(
        float valore,
        bool valoreAssoluto = false
    )
    {
        return Mathf.RoundToInt(
            (valoreAssoluto ? valore : Mathf.Clamp01(valore)) * 100f
        ) + "%";
    }

    private static string FormattaDecimale(float valore)
    {
        return Mathf.Max(0f, valore)
            .ToString("0.##", CultureInfo.InvariantCulture)
            .Replace('.', ',');
    }
}
