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

    public float ProbabilitaColpoAggiuntivo => Mathf.Clamp01(
        livelloColpoAggiuntivo *
        Configurazione.probabilitaColpoAggiuntivoPerLivello
    );
    public bool HaRafficaRaccolto => livelloRafficaRaccolto > 0;
    public int ColpiPerRafficaRaccolto => Mathf.Max(
        2,
        Configurazione.colpiPerRafficaRaccolto
    );
    public float AngoloColpoAggiuntivo => Mathf.Clamp(
        Configurazione.angoloColpoAggiuntivo,
        0f,
        30f
    );
    public float ProbabilitaCritico => Mathf.Clamp01(
        livelloCritico * Configurazione.probabilitaCriticoPerLivello
    );

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

        return componenteDisponibile &&
               OttieniLivello(tipo) < OttieniLivelloMassimo(tipo);
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
                : "Potenziamento già al massimo.";
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
                return "Può aggiungere una seconda patata laterale.";
            case TipoPotenziamento.RafficaRaccolto:
                return "Ogni pochi spari libera un ventaglio di tre colpi.";
            case TipoPotenziamento.PatataGigante:
                return "Proiettili più grandi, pesanti e leggermente lenti.";
            case TipoPotenziamento.PatataEsplosiva:
                return "Il primo impatto danneggia anche le volpi vicine.";
            case TipoPotenziamento.Critico:
                return "Possibilità di infliggere danno doppio.";
            case TipoPotenziamento.Rimbalzo:
                return "Un colpo fermato cerca una nuova volpe vicina.";
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

        int livello = OttieniLivello(tipo);
        int massimo = OttieniLivelloMassimo(tipo);
        return livello >= massimo
            ? "LIVELLO MASSIMO"
            : "Livello " + livello + " / " + massimo;
    }

    public string OttieniConfronto(TipoPotenziamento tipo)
    {
        ShopBalanceSettings configurazione = Configurazione;
        if (tipo != TipoPotenziamento.Cura &&
            OttieniLivello(tipo) >= OttieniLivelloMassimo(tipo))
        {
            return "ORA  " +
                   DescriviValoreAttuale(tipo, configurazione) +
                   "   |   MASSIMO";
        }

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
                        attuale + configurazione.incrementoMovimento
                    )
                );
            }
            case TipoPotenziamento.Resistenza:
            {
                int attuale = salute != null
                    ? salute.FrequenzaBloccoFinale
                    : 0;
                int prossimo = ValoreArray(
                    configurazione.frequenzeBlocco,
                    livelloResistenza,
                    attuale
                );
                return Confronto(
                    attuale > 0 ? "1 ogni " + attuale : "nessun blocco",
                    "1 ogni " + Mathf.Max(1, prossimo)
                );
            }
            case TipoPotenziamento.SaluteMassima:
            {
                int attuale = salute != null ? salute.VitaMassimaFinale : 0;
                return Confronto(
                    attuale.ToString(),
                    (attuale + configurazione.incrementoSaluteMassima)
                    .ToString()
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
                        attuale + configurazione.quantitaCura
                    ).ToString()
                );
            }
            case TipoPotenziamento.Danno:
            {
                int attuale = sparo != null ? sparo.DannoFinale : 0;
                return Confronto(
                    attuale.ToString(),
                    (attuale + configurazione.incrementoDanno).ToString()
                );
            }
            case TipoPotenziamento.Cadenza:
            {
                float attuale = sparo != null
                    ? sparo.IntervalloSparoFinale
                    : 0f;
                float prossimo = sparo != null
                    ? Mathf.Max(
                        sparo.IntervalloSparoMinimo,
                        attuale - configurazione.riduzioneIntervalloSparo
                    )
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
                    (attuale + configurazione.incrementoPenetrazione)
                    .ToString()
                );
            }
            case TipoPotenziamento.ColpoAggiuntivo:
                return ConfrontoPercentuale(
                    livelloColpoAggiuntivo *
                    configurazione.probabilitaColpoAggiuntivoPerLivello,
                    (livelloColpoAggiuntivo + 1) *
                    configurazione.probabilitaColpoAggiuntivoPerLivello
                );
            case TipoPotenziamento.RafficaRaccolto:
                return Confronto(
                    "disattiva",
                    "ogni " + configurazione.colpiPerRafficaRaccolto +
                    " spari"
                );
            case TipoPotenziamento.PatataGigante:
                return ConfrontoPercentuale(
                    1f + livelloPatataGigante *
                    configurazione.incrementoScalaPatataGigante,
                    1f + (livelloPatataGigante + 1) *
                    configurazione.incrementoScalaPatataGigante,
                    true
                );
            case TipoPotenziamento.PatataEsplosiva:
                return Confronto(
                    "nessuna",
                    "raggio " + FormattaDecimale(
                        configurazione.raggioEsplosione
                    )
                );
            case TipoPotenziamento.Critico:
                return ConfrontoPercentuale(
                    livelloCritico *
                    configurazione.probabilitaCriticoPerLivello,
                    (livelloCritico + 1) *
                    configurazione.probabilitaCriticoPerLivello
                );
            case TipoPotenziamento.Rimbalzo:
                return Confronto(
                    livelloRimbalzo.ToString(),
                    (livelloRimbalzo + 1).ToString()
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
                    FormattaDecimale(
                        livelloSpinta * configurazione.forzaSpintaPerLivello
                    ),
                    FormattaDecimale(
                        (livelloSpinta + 1) *
                        configurazione.forzaSpintaPerLivello
                    )
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
            case TipoPotenziamento.ColpoAggiuntivo:
                return Lunghezza(configurazione.costiColpoAggiuntivo);
            case TipoPotenziamento.RafficaRaccolto:
                return Lunghezza(configurazione.costiRafficaRaccolto);
            case TipoPotenziamento.PatataGigante:
                return Lunghezza(configurazione.costiPatataGigante);
            case TipoPotenziamento.PatataEsplosiva:
                return Lunghezza(configurazione.costiPatataEsplosiva);
            case TipoPotenziamento.Critico:
                return Lunghezza(configurazione.costiCritico);
            case TipoPotenziamento.Rimbalzo:
                return Lunghezza(configurazione.costiRimbalzo);
            case TipoPotenziamento.Rallentamento:
                return Lunghezza(configurazione.costiRallentamento);
            case TipoPotenziamento.Spinta:
                return Lunghezza(configurazione.costiSpinta);
            default:
                return 0;
        }
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
        ShopBalanceSettings configurazione = Configurazione;
        ProfiloProiettileBuild profilo = new ProfiloProiettileBuild
        {
            Danno = sparo != null ? sparo.DannoFinale : 1,
            Penetrazioni = sparo != null ? sparo.PenetrazioneFinale : 0,
            Scala = Mathf.Max(
                0.25f,
                1f + livelloPatataGigante *
                configurazione.incrementoScalaPatataGigante
            ),
            MoltiplicatoreVelocita = Mathf.Clamp(
                1f - livelloPatataGigante *
                configurazione.riduzioneVelocitaPatataGigante,
                0.55f,
                1f
            ),
            Critico = critico,
            Rimbalzi = livelloRimbalzo,
            RaggioRimbalzo = configurazione.raggioRicercaRimbalzo,
            RaggioEsplosione = livelloPatataEsplosiva > 0
                ? configurazione.raggioEsplosione
                : 0f,
            MoltiplicatoreRallentamento =
                CalcolaMoltiplicatoreRallentamento(livelloRallentamento),
            DurataRallentamento =
                CalcolaDurataRallentamento(livelloRallentamento),
            ForzaSpinta =
                livelloSpinta * configurazione.forzaSpintaPerLivello
        };

        if (critico)
        {
            profilo.Danno = Mathf.Max(
                1,
                Mathf.RoundToInt(
                    profilo.Danno *
                    Mathf.Max(1f, configurazione.moltiplicatoreDannoCritico)
                )
            );
        }
        profilo.DannoEsplosione = profilo.RaggioEsplosione > 0f
            ? Mathf.Max(
                1,
                Mathf.RoundToInt(
                    profilo.Danno *
                    configurazione.moltiplicatoreDannoEsplosione
                )
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
            case TipoPotenziamento.ColpoAggiuntivo:
                livelloColpoAggiuntivo++;
                break;
            case TipoPotenziamento.RafficaRaccolto:
                livelloRafficaRaccolto++;
                break;
            case TipoPotenziamento.PatataGigante:
                livelloPatataGigante++;
                break;
            case TipoPotenziamento.PatataEsplosiva:
                livelloPatataEsplosiva++;
                break;
            case TipoPotenziamento.Critico:
                livelloCritico++;
                break;
            case TipoPotenziamento.Rimbalzo:
                livelloRimbalzo++;
                break;
            case TipoPotenziamento.Rallentamento:
                livelloRallentamento++;
                break;
            case TipoPotenziamento.Spinta:
                livelloSpinta++;
                break;
        }
    }

    private float CalcolaMoltiplicatoreRallentamento(int livello)
    {
        if (livello <= 0) return 1f;
        ShopBalanceSettings configurazione = Configurazione;
        return Mathf.Clamp(
            configurazione.rallentamentoPrimoLivello -
            (livello - 1) *
            configurazione.riduzioneRallentamentoPerLivello,
            0.25f,
            0.95f
        );
    }

    private float CalcolaDurataRallentamento(int livello)
    {
        if (livello <= 0) return 0f;
        ShopBalanceSettings configurazione = Configurazione;
        return Mathf.Max(
            0.1f,
            configurazione.durataRallentamentoBase +
            (livello - 1) *
            configurazione.durataRallentamentoPerLivello
        );
    }

    private string DescriviRallentamento(int livello)
    {
        float moltiplicatore = CalcolaMoltiplicatoreRallentamento(livello);
        float durata = CalcolaDurataRallentamento(livello);
        int percentuale = Mathf.RoundToInt((1f - moltiplicatore) * 100f);
        return "-" + percentuale + "% per " +
               FormattaDecimale(durata) + " s";
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
        return costi != null && livello >= 0 && livello < costi.Length
            ? Mathf.Max(0, costi[livello])
            : 0;
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
