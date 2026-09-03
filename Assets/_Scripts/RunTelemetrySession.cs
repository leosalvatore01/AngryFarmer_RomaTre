using System;
using System.Globalization;

/// <summary>
/// Aggregatore deterministico della telemetria di una singola partita.
/// Non legge il tempo globale e può quindi essere verificato senza avviare
/// una scena Unity.
/// </summary>
public sealed class RunTelemetrySession
{
    private RunTelemetryWaveData ondaAttiva;
    private int tokenOndaAttiva = int.MinValue;
    private bool finalizzata;

    public RunTelemetryData Dati { get; }
    public bool Finalizzata => finalizzata;
    public bool HaOndaAttiva => ondaAttiva != null;

    public RunTelemetrySession(
        string idPartita,
        DateTime avvioUtc,
        string difficolta,
        int moneteIniziali,
        string versioneGioco,
        string piattaforma
    )
    {
        Dati = new RunTelemetryData
        {
            idPartita = string.IsNullOrWhiteSpace(idPartita)
                ? Guid.NewGuid().ToString("N")
                : idPartita.Trim(),
            avvioUtc = FormattaUtc(avvioUtc),
            versioneGioco = PulisciTesto(versioneGioco, "sconosciuta"),
            piattaforma = PulisciTesto(piattaforma, "sconosciuta"),
            difficolta = PulisciTesto(difficolta, "Sconosciuta"),
            causaMorte = string.Empty,
            tipoVolpeMorte = string.Empty,
            moneteIniziali = Math.Max(0, moneteIniziali)
        };
    }

    public void RegistraFrame(float deltaSecondi, bool validoPerFps = true)
    {
        if (finalizzata || !FloatValidoPositivo(deltaSecondi)) return;

        Dati.durataGameplaySecondi = SommaFloatSicura(
            Dati.durataGameplaySecondi,
            deltaSecondi
        );
        if (!validoPerFps) return;

        Dati.fotogrammiCampionati = SommaSaturata(
            Dati.fotogrammiCampionati,
            1
        );
        Dati.secondiFpsCampionati = SommaFloatSicura(
            Dati.secondiFpsCampionati,
            deltaSecondi
        );
    }

    public void RegistraProgressoOnda(
        int token,
        int indice,
        string nome,
        int nemiciPrevisti,
        int nemiciDaSpawnare,
        int nemiciAttivi
    )
    {
        if (finalizzata || indice <= 0) return;

        if (ondaAttiva == null || tokenOndaAttiva != token)
        {
            if (ondaAttiva != null)
            {
                TerminaOndaAttiva("Interrotta");
            }

            ondaAttiva = new RunTelemetryWaveData
            {
                indice = indice,
                nome = PulisciTesto(nome, "Ondata " + indice),
                esito = "InCorso",
                inizioGameplaySecondi = Dati.durataGameplaySecondi,
                nemiciPrevisti = Math.Max(0, nemiciPrevisti)
            };
            tokenOndaAttiva = token;
            Dati.ondate.Add(ondaAttiva);
            Dati.ondataRaggiunta = Math.Max(Dati.ondataRaggiunta, indice);
        }

        ondaAttiva.nemiciPrevisti = Math.Max(
            ondaAttiva.nemiciPrevisti,
            Math.Max(0, nemiciPrevisti)
        );
        int processati = Math.Max(
            0,
            ondaAttiva.nemiciPrevisti - Math.Max(0, nemiciDaSpawnare)
        );
        ondaAttiva.nemiciSpawnati = Math.Max(
            ondaAttiva.nemiciSpawnati,
            processati
        );
        CampionaNemiciAttivi(nemiciAttivi);
    }

    public void CampionaNemiciAttivi(int quantita)
    {
        if (finalizzata) return;

        int conteggio = Math.Max(0, quantita);
        Dati.piccoNemiciAttivi = Math.Max(
            Dati.piccoNemiciAttivi,
            conteggio
        );
        if (ondaAttiva != null)
        {
            ondaAttiva.piccoNemiciVivi = Math.Max(
                ondaAttiva.piccoNemiciVivi,
                conteggio
            );
            ondaAttiva.nemiciViviFinali = conteggio;
        }
    }

    public void RegistraRiepilogoOnda(
        RiepilogoDiagnosticaOndata riepilogo
    )
    {
        if (finalizzata || !riepilogo.Valido) return;

        RunTelemetryWaveData onda = TrovaOnda(riepilogo.IndiceOndata);
        if (onda == null)
        {
            onda = new RunTelemetryWaveData
            {
                indice = Math.Max(1, riepilogo.IndiceOndata),
                nome = PulisciTesto(
                    riepilogo.NomeOndata,
                    "Ondata " + Math.Max(1, riepilogo.IndiceOndata)
                ),
                inizioGameplaySecondi = Math.Max(
                    0f,
                    Dati.durataGameplaySecondi -
                    Math.Max(0f, riepilogo.DurataTotale)
                )
            };
            Dati.ondate.Add(onda);
        }

        onda.nome = PulisciTesto(riepilogo.NomeOndata, onda.nome);
        onda.esito = riepilogo.Esito.ToString();
        onda.durataTotaleSecondi = Math.Max(0f, riepilogo.DurataTotale);
        onda.durataBannerSecondi = Math.Max(0f, riepilogo.DurataBanner);
        onda.durataSpawnSecondi = Math.Max(0f, riepilogo.DurataSpawn);
        onda.durataCombattimentoSecondi = Math.Max(
            0f,
            riepilogo.DurataCombattimento
        );
        onda.nemiciPrevisti = Math.Max(0, riepilogo.NemiciPrevisti);
        onda.tentativiSpawn = Math.Max(0, riepilogo.TentativiSpawnNemici);
        onda.nemiciSpawnati = Math.Max(0, riepilogo.NemiciSpawnati);
        onda.nemiciViviFinali = Math.Max(0, riepilogo.NemiciViviFinali);
        onda.piccoNemiciVivi = Math.Max(0, riepilogo.PiccoNemiciVivi);
        onda.maialiniPrevisti = Math.Max(0, riepilogo.MaialiniPrevisti);
        onda.maialiniSpawnati = Math.Max(0, riepilogo.MaialiniSpawnati);

        Dati.ondataRaggiunta = Math.Max(Dati.ondataRaggiunta, onda.indice);
        Dati.piccoNemiciAttivi = Math.Max(
            Dati.piccoNemiciAttivi,
            onda.piccoNemiciVivi
        );
        if (ReferenceEquals(onda, ondaAttiva))
        {
            ondaAttiva = null;
            tokenOndaAttiva = int.MinValue;
        }
    }

    public void RegistraDanno(EventoDannoGiocatore evento)
    {
        if (finalizzata || evento.DannoEffettivo <= 0) return;

        Dati.danniSubiti = SommaSaturata(
            Dati.danniSubiti,
            evento.DannoEffettivo
        );
        if (!evento.Fatale) return;

        Dati.causaMorte = evento.TipoAttacco.ToString();
        Dati.tipoVolpeMorte = evento.HaTipoVolpe
            ? evento.TipoVolpe.ToString()
            : string.Empty;
    }

    public void RegistraReroll(int costo)
    {
        if (finalizzata) return;

        Dati.rerollEseguiti = SommaSaturata(Dati.rerollEseguiti, 1);
        Dati.costoTotaleReroll = SommaSaturata(
            Dati.costoTotaleReroll,
            Math.Max(0, costo)
        );
    }

    public void RegistraPowerUp(
        string id,
        string nome,
        SorgentePowerUpTelemetry sorgente,
        int costo,
        int livelloDopo,
        int dopoOndata
    )
    {
        if (finalizzata || string.IsNullOrWhiteSpace(id)) return;

        Dati.powerUp.Add(new RunTelemetryPowerUpData
        {
            tempoGameplaySecondi = Dati.durataGameplaySecondi,
            dopoOndata = Math.Max(0, dopoOndata),
            id = id.Trim(),
            nome = PulisciTesto(nome, id.Trim()),
            sorgente = sorgente.ToString(),
            costo = Math.Max(0, costo),
            livelloDopo = Math.Max(0, livelloDopo)
        });
    }

    public bool Finalizza(
        DateTime fineUtc,
        bool morteContadino,
        RunTelemetryFinalSnapshot riepilogo
    )
    {
        if (finalizzata) return false;

        if (ondaAttiva != null)
        {
            TerminaOndaAttiva(
                morteContadino ? "Sconfitta" : "Interrotta"
            );
        }

        Dati.fineUtc = FormattaUtc(fineUtc);
        Dati.conclusa = true;
        Dati.morteContadino = morteContadino;
        if (morteContadino && string.IsNullOrEmpty(Dati.causaMorte))
        {
            Dati.causaMorte = TipoAttaccoNemico.Sconosciuto.ToString();
        }

        Dati.durataGameplaySecondi = Math.Max(
            Dati.durataGameplaySecondi,
            riepilogo.DurataGameplaySecondi
        );
        Dati.ondateCompletate = riepilogo.OndateCompletate;
        Dati.volpiEliminate = riepilogo.VolpiEliminate;
        Dati.proiettiliSparati = riepilogo.ProiettiliSparati;
        Dati.proiettiliACentro = Math.Min(
            riepilogo.ProiettiliSparati,
            riepilogo.ProiettiliACentro
        );
        Dati.precisione = Dati.proiettiliSparati > 0
            ? Math.Min(
                1f,
                Dati.proiettiliACentro /
                (float)Dati.proiettiliSparati
            )
            : 0f;
        Dati.punteggioFinale = riepilogo.PunteggioFinale;
        Dati.moneteIniziali = riepilogo.MoneteIniziali;
        Dati.moneteGuadagnate = riepilogo.MoneteGuadagnate;
        Dati.moneteSpese = riepilogo.MoneteSpese;
        Dati.moneteNonSpese = riepilogo.MoneteNonSpese;
        Dati.gettoniPermanentiGuadagnati =
            riepilogo.GettoniPermanentiGuadagnati;
        Dati.fpsMedio = Dati.secondiFpsCampionati > 0f
            ? Dati.fotogrammiCampionati / Dati.secondiFpsCampionati
            : 0f;

        finalizzata = true;
        return true;
    }

    private void TerminaOndaAttiva(string esito)
    {
        if (ondaAttiva == null) return;

        float durata = Math.Max(
            0f,
            Dati.durataGameplaySecondi -
            ondaAttiva.inizioGameplaySecondi
        );
        ondaAttiva.esito = PulisciTesto(esito, "Interrotta");
        ondaAttiva.durataTotaleSecondi = durata;
        ondaAttiva.durataCombattimentoSecondi = durata;
        ondaAttiva = null;
        tokenOndaAttiva = int.MinValue;
    }

    private RunTelemetryWaveData TrovaOnda(int indice)
    {
        if (ondaAttiva != null && ondaAttiva.indice == indice)
        {
            return ondaAttiva;
        }

        for (int i = Dati.ondate.Count - 1; i >= 0; i--)
        {
            if (Dati.ondate[i].indice == indice)
            {
                return Dati.ondate[i];
            }
        }
        return null;
    }

    private static int SommaSaturata(int primo, int secondo)
    {
        long somma = (long)Math.Max(0, primo) + Math.Max(0, secondo);
        return somma >= int.MaxValue ? int.MaxValue : (int)somma;
    }

    private static float SommaFloatSicura(float primo, float secondo)
    {
        double somma = Math.Max(0f, primo) + Math.Max(0f, secondo);
        return somma >= float.MaxValue ? float.MaxValue : (float)somma;
    }

    private static bool FloatValidoPositivo(float valore)
    {
        return valore > 0f && !float.IsNaN(valore) &&
               !float.IsInfinity(valore);
    }

    private static string PulisciTesto(string valore, string fallback)
    {
        return string.IsNullOrWhiteSpace(valore) ? fallback : valore.Trim();
    }

    private static string FormattaUtc(DateTime valore)
    {
        DateTime utc = valore.Kind == DateTimeKind.Utc
            ? valore
            : valore.ToUniversalTime();
        return utc.ToString("O", CultureInfo.InvariantCulture);
    }
}
