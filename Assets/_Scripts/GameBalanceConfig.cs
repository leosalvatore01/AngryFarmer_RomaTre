using System;
using UnityEngine;

[Serializable]
public sealed class PlayerBalanceSettings
{
    [Header("Movimento")]
    [Min(0f)] public float velocitaMovimento = 8f;
    [Min(0f)] public float accelerazione = 45f;
    [Min(0f)] public float decelerazione = 60f;
    [Min(1f)] public float moltiplicatoreInversione = 1.35f;

    [Header("Salute")]
    [Min(1)] public int vitaMassima = 5;
    [Min(0)] public int frequenzaBloccoBase;
    [Range(0f, 2f)] public float durataInvulnerabilitaDopoColpo = 0.65f;

    [Header("Schivata")]
    [Min(0.05f)] public float durataSchivata = 0.18f;
    [Min(1f)] public float velocitaSchivata = 18f;
    [Min(0.1f)] public float cooldownSchivata = 1.1f;
    [Min(0.05f)] public float durataInvulnerabilitaSchivata = 0.24f;

    [Header("Sparo")]
    [Min(0.01f)] public float intervalloSparo = 0.4f;
    [Min(0.01f)] public float intervalloSparoMinimo = 0.12f;
    [Min(0f)] public float velocitaProiettile = 10f;
    [Min(1)] public int dannoProiettile = 1;
    [Min(0)] public int penetrazioneProiettile;
    [Min(0.05f)] public float durataProiettile = 3f;
    [Min(0f)] public float distanzaMinimaMira = 0.2f;
    [Min(0f)] public float distanzaUscitaProiettile = 0.44f;

    [Header("Power-up temporanei")]
    [Min(0f)] public float durataBoostVelocita = 5f;
    [Min(1f)] public float moltiplicatoreBoostVelocita = 2f;
    [Min(0f)] public float durataTriploSparo = 5f;
    [Range(0f, 45f)] public float angoloLateraleTriploSparo = 10f;
    [Min(0.1f)] public float durataScudoTemporaneo = 8f;
    [Min(1)] public int caricheScudoTemporaneo = 1;
    [Min(1)] public int quantitaCuraDrop = 2;
    [Min(0.1f)] public float durataCalamita = 9f;
    [Min(0.5f)] public float raggioCalamita = 5.5f;
    [Min(0.1f)] public float velocitaAttrazioneCalamita = 9f;
    [Min(0.1f)] public float durataFuria = 7f;
    [Min(1f)] public float moltiplicatoreDannoFuria = 1.75f;
    [Range(0.2f, 1f)] public float moltiplicatoreIntervalloFuria = 0.68f;
    [Min(0.5f)] public float raggioEsplosioneDrop = 4.5f;
    [Min(1)] public int dannoEsplosioneDrop = 8;
}

[Serializable]
public sealed class FoxBalanceSettings
{
    [Header("Movimento e attacco")]
    [Min(0f)] public float velocita = 2.4f;
    [Min(0f)] public float accelerazione = 12f;
    [Min(0f)] public float decelerazione = 18f;
    [Min(1f)] public float moltiplicatoreInversione = 1.3f;
    [Min(0f)] public float distanzaRipresaInseguimento = 0.95f;
    [Min(0f)] public float distanzaAttacco = 0.8f;
    [Min(0)] public int danno = 1;
    [Min(0.01f)] public float intervalloAttacco = 1f;
    [Range(0.05f, 1f)] public float moltiplicatoreRallentamento = 0.5f;

    [Header("Progressione e ricompense")]
    [Min(1)] public int vitaPrimaOndata = 2;
    [Min(0)] public int vitaAggiuntivaPerOndata = 1;
    [Min(0)] public int monetePerEliminazione = 1;
    [Range(0f, 100f)] public float probabilitaDrop = 24f;
    [Range(0f, 1f)] public float probabilitaDenteSulDrop = 0.5f;
    [Min(0.1f)] public float durataDropSullaMappa = 12f;
    [Min(1)] public int uccisioniSenzaDropMassime = 6;
    [Min(1)] public int ripetizioniMassimeStessoDrop = 2;
    [Min(0f)] public float pesoDropTriploSparo = 1.2f;
    [Min(0f)] public float pesoDropVelocita = 1.15f;
    [Min(0f)] public float pesoDropScudo = 0.9f;
    [Min(0f)] public float pesoDropCura = 0.8f;
    [Min(0f)] public float pesoDropCalamita = 0.75f;
    [Min(0f)] public float pesoDropFuria = 0.7f;
    [Min(0f)] public float pesoDropEsplosione = 0.55f;
}

[Serializable]
public sealed class PigBalanceSettings
{
    [Min(0f)] public float velocitaPasseggio = 1.45f;
    [Min(0f)] public float velocitaFuga = 3.1f;
    [Min(0f)] public float accelerazione = 10f;
    [Min(0f)] public float decelerazione = 14f;
    [Min(1f)] public float moltiplicatoreInversione = 1.25f;
    [Min(0f)] public float raggioFuga = 2.6f;
    [Min(0.1f)] public float durataSullaMappa = 14f;
    [Min(1)] public int vitaBase = 1;
    [Min(0)] public int moneteBase = 3;
    [Min(0.05f)] public float cambioDirezioneMinimo = 1.4f;
    [Min(0.05f)] public float cambioDirezioneMassimo = 2.8f;
    [Min(0f)] public float ritardoDirezioneDopoFuga = 0.5f;
}

[Serializable]
public sealed class ShopBalanceSettings
{
    [Header("Offerte e ritmo della bottega")]
    [Range(3, 4)] public int numeroOfferte = 4;
    [Min(1)] public int costoRerollBase = 1;
    [Min(0)] public int incrementoCostoReroll = 1;
    [Min(0)] public int bonusCompletamentoOnda = 1;

    [Header("Pesi archetipi iniziali")]
    [Range(1f, 3f)] public float pesoArchetipoPercorsoPrincipale = 1.9f;
    [Range(1f, 2.5f)] public float pesoArchetipoPercorsoSecondario = 1.3f;
    [Range(1f, 3f)] public float pesoArchetipoSinergia = 1.5f;
    [Range(1f, 2f)] public float pesoPercorsoEmergente = 1.2f;

    [Header("Evoluzioni e crescita infinita")]
    [Min(1)] public int sogliaPrimaEvoluzione = 3;
    [Min(2)] public int sogliaSecondaEvoluzione = 6;
    [Range(0.45f, 0.9f)] public float esponenteRendimentoDecrescente = 0.65f;

    [Header("Evoluzione Raffica")]
    [Range(2, 4)] public int frammentiRafficaPrimoStadio = 2;
    [Range(2, 5)] public int frammentiRafficaSecondoStadio = 3;
    [Range(5f, 35f)] public float angoloDivisioneRaffica = 18f;
    [Range(0.2f, 0.8f)] public float dannoFrammentiPrimoStadio = 0.42f;
    [Range(0.2f, 0.8f)] public float dannoFrammentiSecondoStadio = 0.52f;

    [Header("Evoluzione Artiglieria")]
    [Min(0.2f)] public float raggioEsplosioneEvoluzione = 0.9f;
    [Range(0.1f, 1f)] public float dannoEsplosioneEvoluzione = 0.35f;
    [Range(1, 3)] public int esplosioniSecondariePrimoStadio = 1;
    [Range(1, 4)] public int esplosioniSecondarieSecondoStadio = 2;
    [Range(0.05f, 0.5f)] public float ritardoEsplosioniSecondarie = 0.16f;
    [Range(0.4f, 1.5f)] public float raggioEsplosioniSecondarie = 0.78f;
    [Range(0.1f, 1f)] public float dannoEsplosioniSecondarie = 0.32f;

    [Header("Evoluzione Perforazione")]
    [Range(1, 4)] public int penetrazioniEvoluzionePrimoStadio = 1;
    [Range(1, 6)] public int penetrazioniEvoluzioneSecondoStadio = 2;
    [Range(1, 3)] public int rimbalziEvoluzioneSecondoStadio = 1;

    [Header("Evoluzione Controllo")]
    [Range(0.4f, 0.95f)] public float rallentamentoBaseEvoluzione = 0.82f;
    [Range(0.2f, 4f)] public float durataRallentamentoEvoluzione = 1f;
    [Range(0.5f, 4f)] public float raggioContagioPrimoStadio = 1.6f;
    [Range(0.5f, 5f)] public float raggioContagioSecondoStadio = 2.4f;
    [Range(0.2f, 1f)] public float intensitaContagioPrimoStadio = 0.65f;
    [Range(0.2f, 1f)] public float intensitaContagioSecondoStadio = 0.9f;

    [Header("Prezzi per livello")]
    public int[] costiMovimento = { 3, 5, 8 };
    public int[] costiResistenza = { 4, 7, 10 };
    public int[] costiSalute = { 4, 7, 10 };
    public int[] costiDanno = { 10, 16 };
    public int[] costiCadenza = { 3, 6, 9 };
    public int[] costiPenetrazione = { 3, 5, 7 };
    [Min(1)] public int costoCura = 2;

    [Header("Prezzi modificatori speciali")]
    public int[] costiColpoAggiuntivo = { 6, 10 };
    public int[] costiRafficaRaccolto = { 11 };
    public int[] costiPatataGigante = { 4, 7 };
    public int[] costiPatataEsplosiva = { 12 };
    public int[] costiCritico = { 3, 5, 7 };
    public int[] costiRimbalzo = { 8, 14 };
    public int[] costiRallentamento = { 3, 6 };
    public int[] costiSpinta = { 3, 6, 9 };

    [Header("Effetti")]
    [Min(0f)] public float incrementoMovimento = 0.5f;
    [Min(2)] public int[] frequenzeBlocco = { 5, 4, 3 };
    [Min(0)] public int incrementoSaluteMassima = 1;
    [Min(0)] public int curaSuIncrementoSalute = 1;
    [Min(0)] public int quantitaCura = 2;
    [Min(0)] public int incrementoDanno = 1;
    [Min(0f)] public float riduzioneIntervalloSparo = 0.04f;
    [Min(0)] public int incrementoPenetrazione = 1;
    [Min(0)] public int moneteIniziali;

    [Header("Effetti build Raffica")]
    [Range(0f, 1f)] public float probabilitaColpoAggiuntivoPerLivello = 0.18f;
    [Min(2)] public int colpiPerRafficaRaccolto = 5;
    [Range(0f, 30f)] public float angoloColpoAggiuntivo = 7f;

    [Header("Effetti build Artiglieria")]
    [Range(0f, 1f)] public float incrementoScalaPatataGigante = 0.2f;
    [Range(0f, 0.4f)] public float riduzioneVelocitaPatataGigante = 0.06f;
    [Range(0f, 2f)] public float forzaSpintaPatataGigantePerLivello = 1.1f;
    [Min(0.2f)] public float raggioEsplosione = 1.25f;
    [Range(0.1f, 2f)] public float moltiplicatoreDannoEsplosione = 0.5f;

    [Header("Effetti build Perforazione")]
    [Range(0f, 1f)] public float probabilitaCriticoPerLivello = 0.12f;
    [Min(1f)] public float moltiplicatoreDannoCritico = 2f;
    [Range(1.5f, 3.2f)] public float raggioRicercaRimbalzo = 2.4f;
    [Range(0.5f, 1f)] public float moltiplicatoreDannoRimbalzo = 0.65f;

    [Header("Effetti build Controllo")]
    [Range(0.1f, 0.95f)] public float rallentamentoPrimoLivello = 0.72f;
    [Range(0f, 0.4f)] public float riduzioneRallentamentoPerLivello = 0.12f;
    [Min(0.1f)] public float durataRallentamentoBase = 1.1f;
    [Min(0f)] public float durataRallentamentoPerLivello = 0.35f;
    [Min(0f)] public float forzaSpintaPerLivello = 0.85f;
}

[Serializable]
public sealed class SpecialEncounterBalanceSettings
{
    [Header("Volpe elite")]
    [Min(1f)] public float moltiplicatoreVitaElite = 1.85f;
    [Min(1f)] public float moltiplicatoreScalaElite = 1.16f;
    [Min(0)] public int moneteEliteBase = 8;
    [Min(0)] public int moneteElitePerCapitolo = 2;
    [Range(1, 4)] public int dropGarantitiElite = 1;

    [Header("Il Re del Branco")]
    [Min(1f)] public float moltiplicatoreVitaBoss = 5.5f;
    [Min(0f)] public float crescitaVitaPerApparizione = 0.35f;
    [Min(1f)] public float moltiplicatoreScalaBoss = 1.12f;
    [Min(0)] public int moneteBossBase = 28;
    [Min(0)] public int moneteBossPerApparizione = 12;
    [Range(1, 8)] public int dropGarantitiBoss = 2;
    [Range(0.2f, 0.8f)] public float sogliaSecondaFase = 0.5f;

    [Header("Movimento e ritmo boss")]
    [Min(0f)] public float velocitaInseguimento = 1.55f;
    [Min(1f)] public float velocitaSecondaFase = 1.3f;
    [Min(0.25f)] public float intervalloAttacchi = 2.15f;
    [Range(0.35f, 1f)] public float intervalloSecondaFase = 0.72f;
    [Range(0.25f, 2f)] public float durataTransizioneFase = 1.15f;

    [Header("Attacchi telegrafati")]
    [Range(0.25f, 2f)] public float durataPreavvisoCarica = 0.8f;
    [Min(1f)] public float velocitaCarica = 10.5f;
    [Range(0.15f, 1.5f)] public float durataCarica = 0.52f;
    [Range(0.25f, 2f)] public float durataPreavvisoSchianto = 0.9f;
    [Min(0.5f)] public float raggioSchianto = 2.45f;
    [Range(0.25f, 2f)] public float durataPreavvisoRaffica = 0.75f;
    [Min(1f)] public float velocitaProiettili = 6.6f;
    [Range(3, 11)] public int proiettiliPrimaFase = 5;
    [Range(3, 13)] public int proiettiliSecondaFase = 7;
    [Range(5f, 90f)] public float aperturaRaffica = 52f;
    [Min(1)] public int dannoPrimaFase = 1;
    [Min(1)] public int dannoSecondaFase = 2;

    public void Normalizza()
    {
        moltiplicatoreVitaElite = Mathf.Max(1f, moltiplicatoreVitaElite);
        moltiplicatoreScalaElite = Mathf.Max(1f, moltiplicatoreScalaElite);
        moneteEliteBase = Mathf.Max(0, moneteEliteBase);
        moneteElitePerCapitolo = Mathf.Max(0, moneteElitePerCapitolo);
        dropGarantitiElite = Mathf.Clamp(dropGarantitiElite, 1, 4);

        moltiplicatoreVitaBoss = Mathf.Max(1f, moltiplicatoreVitaBoss);
        crescitaVitaPerApparizione = Mathf.Max(
            0f,
            crescitaVitaPerApparizione
        );
        moltiplicatoreScalaBoss = Mathf.Max(1f, moltiplicatoreScalaBoss);
        moneteBossBase = Mathf.Max(0, moneteBossBase);
        moneteBossPerApparizione = Mathf.Max(
            0,
            moneteBossPerApparizione
        );
        dropGarantitiBoss = Mathf.Clamp(dropGarantitiBoss, 1, 8);
        sogliaSecondaFase = Mathf.Clamp(sogliaSecondaFase, 0.2f, 0.8f);

        velocitaInseguimento = Mathf.Max(0f, velocitaInseguimento);
        velocitaSecondaFase = Mathf.Max(1f, velocitaSecondaFase);
        intervalloAttacchi = Mathf.Max(0.25f, intervalloAttacchi);
        intervalloSecondaFase = Mathf.Clamp(
            intervalloSecondaFase,
            0.35f,
            1f
        );
        durataTransizioneFase = Mathf.Clamp(
            durataTransizioneFase,
            0.25f,
            2f
        );

        durataPreavvisoCarica = Mathf.Clamp(
            durataPreavvisoCarica,
            0.25f,
            2f
        );
        velocitaCarica = Mathf.Max(1f, velocitaCarica);
        durataCarica = Mathf.Clamp(durataCarica, 0.15f, 1.5f);
        durataPreavvisoSchianto = Mathf.Clamp(
            durataPreavvisoSchianto,
            0.25f,
            2f
        );
        raggioSchianto = Mathf.Max(0.5f, raggioSchianto);
        durataPreavvisoRaffica = Mathf.Clamp(
            durataPreavvisoRaffica,
            0.25f,
            2f
        );
        velocitaProiettili = Mathf.Max(1f, velocitaProiettili);
        proiettiliPrimaFase = Mathf.Clamp(proiettiliPrimaFase, 3, 11);
        proiettiliSecondaFase = Mathf.Clamp(
            proiettiliSecondaFase,
            3,
            13
        );
        aperturaRaffica = Mathf.Clamp(aperturaRaffica, 5f, 90f);
        dannoPrimaFase = Mathf.Max(1, dannoPrimaFase);
        dannoSecondaFase = Mathf.Max(1, dannoSecondaFase);
    }
}

[Serializable]
public sealed class WaveBalanceSettings
{
    [Header("Spawn e ritmo")]
    [Min(0f)] public float distanzaSpawnVolpi = 10f;
    [Min(0f)] public float distanzaSpawnMaialini = 5.2f;
    [Min(0f)] public float durataBannerOndata = 0.85f;
    [Min(0.02f)] public float intervalloControlloFineOndata = 0.2f;
    [Min(0.05f)] public float durataMinimaDistribuzioneMaialini = 1f;

    [Header("Leggibilita e gruppi")]
    [Range(0.15f, 1f)] public float durataPreavvisoSpawn = 0.5f;
    [Range(1, 4)] public int sogliaUltimiNemici = 2;
    [Range(0f, 1f)] public float volumeSegnaleUltimiNemici = 0.26f;

    [Header("Capitoli e shop")]
    public WaveChapterSettings capitoli = new WaveChapterSettings();

    [Header("Budget di minaccia")]
    public ThreatBudgetSettings minaccia = new ThreatBudgetSettings();

    [Header("Incontri elite e boss")]
    public SpecialEncounterBalanceSettings incontriSpeciali =
        new SpecialEncounterBalanceSettings();

    public SpecialEncounterBalanceSettings IncontriSpeciali =>
        incontriSpeciali ??
        (incontriSpeciali = new SpecialEncounterBalanceSettings());

    [Header("Sequenza di riferimento")]
    public Wave[] ondate = CreaOndateRiferimento();

    private static Wave[] CreaOndateRiferimento()
    {
        return new[]
        {
            new Wave
            {
                nomeOndata = "Riscaldamento",
                numeroNemici = 3,
                sequenzaVolpi = new[]
                {
                    TipoVolpe.Comune,
                    TipoVolpe.Comune,
                    TipoVolpe.Comune
                },
                intervalloTraNemici = 0.7f,
                dimensioneMassimaGruppo = 2,
                intervalloTraGruppi = 2.9f,
                numeroMaialiniBonus = 0,
                vitaMaialinoBonus = 1,
                moneteMaialinoBonus = 0
            },
            new Wave
            {
                nomeOndata = "Passi veloci",
                numeroNemici = 4,
                sequenzaVolpi = new[]
                {
                    TipoVolpe.Comune,
                    TipoVolpe.Comune,
                    TipoVolpe.Agile,
                    TipoVolpe.Comune
                },
                intervalloTraNemici = 0.7f,
                dimensioneMassimaGruppo = 2,
                intervalloTraGruppi = 3f,
                numeroMaialiniBonus = 1,
                vitaMaialinoBonus = 2,
                moneteMaialinoBonus = 3
            },
            new Wave
            {
                nomeOndata = "Pelle dura",
                numeroNemici = 5,
                sequenzaVolpi = new[]
                {
                    TipoVolpe.Comune,
                    TipoVolpe.Agile,
                    TipoVolpe.Comune,
                    TipoVolpe.Robusta,
                    TipoVolpe.Comune
                },
                intervalloTraNemici = 0.68f,
                dimensioneMassimaGruppo = 2,
                intervalloTraGruppi = 2.7f,
                numeroMaialiniBonus = 1,
                vitaMaialinoBonus = 2,
                moneteMaialinoBonus = 3
            },
            new Wave
            {
                nomeOndata = "Salti imprevedibili",
                numeroNemici = 6,
                sequenzaVolpi = new[]
                {
                    TipoVolpe.Comune,
                    TipoVolpe.Agile,
                    TipoVolpe.Comune,
                    TipoVolpe.Robusta,
                    TipoVolpe.Schivatrice,
                    TipoVolpe.Comune
                },
                intervalloTraNemici = 0.66f,
                dimensioneMassimaGruppo = 2,
                intervalloTraGruppi = 2.55f,
                numeroMaialiniBonus = 1,
                vitaMaialinoBonus = 2,
                moneteMaialinoBonus = 4
            },
            new Wave
            {
                nomeOndata = "Carica alfa",
                numeroNemici = 7,
                sequenzaVolpi = new[]
                {
                    TipoVolpe.Comune,
                    TipoVolpe.Agile,
                    TipoVolpe.Robusta,
                    TipoVolpe.Comune,
                    TipoVolpe.Schivatrice,
                    TipoVolpe.Comune,
                    TipoVolpe.Alfa
                },
                intervalloTraNemici = 0.64f,
                dimensioneMassimaGruppo = 3,
                intervalloTraGruppi = 2.6f,
                numeroMaialiniBonus = 1,
                vitaMaialinoBonus = 3,
                moneteMaialinoBonus = 4
            },
            new Wave
            {
                nomeOndata = "Richiamo del branco",
                numeroNemici = 8,
                sequenzaVolpi = new[]
                {
                    TipoVolpe.Comune,
                    TipoVolpe.Agile,
                    TipoVolpe.Robusta,
                    TipoVolpe.Comune,
                    TipoVolpe.Schivatrice,
                    TipoVolpe.Ululatrice,
                    TipoVolpe.Comune,
                    TipoVolpe.Alfa
                },
                intervalloTraNemici = 0.62f,
                dimensioneMassimaGruppo = 3,
                intervalloTraGruppi = 2.5f,
                numeroMaialiniBonus = 2,
                vitaMaialinoBonus = 3,
                moneteMaialinoBonus = 5
            },
            new Wave
            {
                nomeOndata = "Branco misto",
                numeroNemici = 10,
                sequenzaVolpi = new[]
                {
                    TipoVolpe.Comune,
                    TipoVolpe.Agile,
                    TipoVolpe.Robusta,
                    TipoVolpe.Comune,
                    TipoVolpe.Schivatrice,
                    TipoVolpe.Alfa,
                    TipoVolpe.Comune,
                    TipoVolpe.Agile,
                    TipoVolpe.Ululatrice,
                    TipoVolpe.Robusta
                },
                intervalloTraNemici = 0.6f,
                dimensioneMassimaGruppo = 3,
                intervalloTraGruppi = 2.35f,
                numeroMaialiniBonus = 2,
                vitaMaialinoBonus = 3,
                moneteMaialinoBonus = 5
            },
            new Wave
            {
                nomeOndata = "Fango in arrivo",
                numeroNemici = 12,
                sequenzaVolpi = new[]
                {
                    TipoVolpe.Comune,
                    TipoVolpe.Agile,
                    TipoVolpe.Robusta,
                    TipoVolpe.Comune,
                    TipoVolpe.Schivatrice,
                    TipoVolpe.Ululatrice,
                    TipoVolpe.Comune,
                    TipoVolpe.Agile,
                    TipoVolpe.Alfa,
                    TipoVolpe.Robusta,
                    TipoVolpe.Comune,
                    TipoVolpe.Sputafango
                },
                intervalloTraNemici = 0.58f,
                dimensioneMassimaGruppo = 3,
                intervalloTraGruppi = 2.25f,
                numeroMaialiniBonus = 2,
                vitaMaialinoBonus = 4,
                moneteMaialinoBonus = 6
            },
            new Wave
            {
                nomeOndata = "Assedio del branco",
                numeroNemici = 14,
                sequenzaVolpi = new[]
                {
                    TipoVolpe.Comune,
                    TipoVolpe.Agile,
                    TipoVolpe.Robusta,
                    TipoVolpe.Comune,
                    TipoVolpe.Schivatrice,
                    TipoVolpe.Sputafango,
                    TipoVolpe.Comune,
                    TipoVolpe.Agile,
                    TipoVolpe.Alfa,
                    TipoVolpe.Robusta,
                    TipoVolpe.Comune,
                    TipoVolpe.Schivatrice,
                    TipoVolpe.Ululatrice,
                    TipoVolpe.Agile
                },
                intervalloTraNemici = 0.56f,
                dimensioneMassimaGruppo = 3,
                intervalloTraGruppi = 2.15f,
                numeroMaialiniBonus = 2,
                vitaMaialinoBonus = 4,
                moneteMaialinoBonus = 6
            },
            new Wave
            {
                nomeOndata = "Terra in movimento",
                numeroNemici = 16,
                sequenzaVolpi = new[]
                {
                    TipoVolpe.Comune,
                    TipoVolpe.Agile,
                    TipoVolpe.Robusta,
                    TipoVolpe.Comune,
                    TipoVolpe.Schivatrice,
                    TipoVolpe.Ululatrice,
                    TipoVolpe.Comune,
                    TipoVolpe.Agile,
                    TipoVolpe.Alfa,
                    TipoVolpe.Robusta,
                    TipoVolpe.Comune,
                    TipoVolpe.Schivatrice,
                    TipoVolpe.Sputafango,
                    TipoVolpe.Agile,
                    TipoVolpe.Comune,
                    TipoVolpe.Scavatrice
                },
                intervalloTraNemici = 0.54f,
                dimensioneMassimaGruppo = 3,
                intervalloTraGruppi = 2.05f,
                numeroMaialiniBonus = 2,
                vitaMaialinoBonus = 5,
                moneteMaialinoBonus = 7
            }
        };
    }
}

[Serializable]
public sealed class CombatFeedbackSettings
{
    [Header("Mirino")]
    public bool mirinoPixelAttivo = true;
    [Range(14f, 48f)] public float dimensioneMirino = 24f;

    [Header("Effetti visivi")]
    public bool effettiVisiviAttivi = true;
    [Range(3, 8)] public int particelleImpatto = 5;
    [Range(0.08f, 0.35f)] public float durataParticelle = 0.18f;
    [Range(0f, 0.3f)] public float distanzaRinculoBersaglio = 0.12f;
    [Range(0.03f, 0.25f)] public float durataRinculoBersaglio = 0.09f;
    [Range(0.03f, 0.2f)] public float durataFlashBersaglio = 0.075f;

    [Header("Audio")]
    public bool audioAttivo = true;
    [Range(0f, 1f)] public float volumeSparo = 0.22f;
    [Range(0f, 1f)] public float volumeImpatto = 0.28f;
    [Range(0f, 0.2f)] public float variazioneIntonazione = 0.055f;

    [Header("Vibrazione camera")]
    public bool vibrazioneCameraAttiva = true;
    [Range(0f, 0.15f)] public float intensitaVibrazione = 0.045f;
    [Range(0.02f, 0.25f)] public float durataVibrazione = 0.075f;
}

[Serializable]
public sealed class PoolingBalanceSettings
{
    [Header("Capacita dei pool prefab")]
    [Range(8, 96)] public int volpiComuni = 40;
    [Range(1, 12)] public int volpiElite = 4;
    [Range(1, 6)] public int boss = 2;
    [Range(32, 256)] public int proiettiliContadino = 160;
    [Range(8, 64)] public int drop = 32;
    [Range(2, 16)] public int maialini = 8;

    [Header("Capacita oggetti procedurali")]
    [Range(8, 64)] public int proiettiliNemici = 32;
    [Range(16, 128)] public int anelliVfx = 72;
    [Range(4, 32)] public int lineeVfx = 16;
    [Range(4, 32)] public int impulsiSecondari = 16;
    [Range(8, 64)] public int particelleRicompensa = 32;

    [Header("Obiettivi stabilita")]
    [Range(30, 120)] public int fpsObiettivo = 60;
    [Range(32, 256)] public int budgetMemoriaPoolMb = 96;

    public void Normalizza()
    {
        volpiComuni = Mathf.Clamp(volpiComuni, 8, 96);
        volpiElite = Mathf.Clamp(volpiElite, 1, 12);
        boss = Mathf.Clamp(boss, 1, 6);
        proiettiliContadino = Mathf.Clamp(proiettiliContadino, 32, 256);
        drop = Mathf.Clamp(drop, 8, 64);
        maialini = Mathf.Clamp(maialini, 2, 16);
        proiettiliNemici = Mathf.Clamp(proiettiliNemici, 8, 64);
        anelliVfx = Mathf.Clamp(anelliVfx, 16, 128);
        lineeVfx = Mathf.Clamp(lineeVfx, 4, 32);
        impulsiSecondari = Mathf.Clamp(impulsiSecondari, 4, 32);
        particelleRicompensa = Mathf.Clamp(particelleRicompensa, 8, 64);
        fpsObiettivo = Mathf.Clamp(fpsObiettivo, 30, 120);
        budgetMemoriaPoolMb = Mathf.Clamp(budgetMemoriaPoolMb, 32, 256);
    }
}

[CreateAssetMenu(
    fileName = "GameBalanceConfig",
    menuName = "Angry Farmer/Bilanciamento di riferimento"
)]
public sealed class GameBalanceConfig : ScriptableObject
{
    public const string PercorsoResources = "GameBalanceConfig";

    [SerializeField]
    private string versioneRiferimento =
        "Survival infinito - ritmo a capitoli - 2026-09-03";

    [SerializeField] private PlayerBalanceSettings giocatore =
        new PlayerBalanceSettings();
    [SerializeField] private FoxBalanceSettings volpe =
        new FoxBalanceSettings();
    [SerializeField] private FoxVariantsBalanceSettings variantiVolpe =
        new FoxVariantsBalanceSettings();
    [SerializeField] private PigBalanceSettings maialino =
        new PigBalanceSettings();
    [SerializeField] private ShopBalanceSettings shop =
        new ShopBalanceSettings();
    [SerializeField] private WaveBalanceSettings ondate =
        new WaveBalanceSettings();
    [SerializeField] private BilanciamentoDifficolta difficolta =
        new BilanciamentoDifficolta();
    [SerializeField] private CombatFeedbackSettings feedbackCombattimento =
        new CombatFeedbackSettings();
    [SerializeField] private PoolingBalanceSettings pooling =
        new PoolingBalanceSettings();

    private static GameBalanceConfig corrente;
    private static bool avvisoFallbackMostrato;

    public string VersioneRiferimento => versioneRiferimento;
    public PlayerBalanceSettings Giocatore => giocatore;
    public FoxBalanceSettings Volpe => volpe;
    public FoxVariantsBalanceSettings VariantiVolpe =>
        variantiVolpe ??
        (variantiVolpe = new FoxVariantsBalanceSettings());
    public PigBalanceSettings Maialino => maialino;
    public ShopBalanceSettings Shop => shop;
    public WaveBalanceSettings Ondate => ondate;
    public BilanciamentoDifficolta Difficolta =>
        difficolta ?? (difficolta = new BilanciamentoDifficolta());
    public CombatFeedbackSettings FeedbackCombattimento =>
        feedbackCombattimento ??
        (feedbackCombattimento = new CombatFeedbackSettings());
    public PoolingBalanceSettings Pooling =>
        pooling ?? (pooling = new PoolingBalanceSettings());

    public static GameBalanceConfig Corrente
    {
        get
        {
            if (corrente != null) return corrente;

            corrente = Resources.Load<GameBalanceConfig>(PercorsoResources);
            if (corrente != null) return corrente;

            corrente = CreateInstance<GameBalanceConfig>();
            corrente.name = "GameBalanceConfig_FallbackRuntime";
            corrente.hideFlags = HideFlags.HideAndDontSave;

            if (!avvisoFallbackMostrato)
            {
                Debug.LogWarning(
                    "GameBalanceConfig.asset non trovato in Resources: " +
                    "uso i valori di riferimento incorporati nel codice."
                );
                avvisoFallbackMostrato = true;
            }
            return corrente;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void AzzeraCache()
    {
        corrente = null;
        avvisoFallbackMostrato = false;
    }

    void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(versioneRiferimento))
        {
            versioneRiferimento = "Baseline senza nome";
        }

        if (feedbackCombattimento == null)
        {
            feedbackCombattimento = new CombatFeedbackSettings();
        }
        if (pooling == null)
        {
            pooling = new PoolingBalanceSettings();
        }
        if (variantiVolpe == null)
        {
            variantiVolpe = new FoxVariantsBalanceSettings();
        }
        if (difficolta == null)
        {
            difficolta = new BilanciamentoDifficolta();
        }
        if (ondate == null)
        {
            ondate = new WaveBalanceSettings();
        }
        if (ondate.capitoli == null)
        {
            ondate.capitoli = new WaveChapterSettings();
        }
        if (ondate.minaccia == null)
        {
            ondate.minaccia = new ThreatBudgetSettings();
        }
        if (ondate.incontriSpeciali == null)
        {
            ondate.incontriSpeciali =
                new SpecialEncounterBalanceSettings();
        }
        ondate.capitoli.Normalizza();
        ondate.minaccia.Normalizza();
        ondate.incontriSpeciali.Normalizza();
        difficolta.Normalizza();
        pooling.Normalizza();

        giocatore.intervalloSparoMinimo = Mathf.Max(
            0.01f,
            giocatore.intervalloSparoMinimo
        );
        giocatore.intervalloSparo = Mathf.Max(
            giocatore.intervalloSparoMinimo,
            giocatore.intervalloSparo
        );
        giocatore.durataInvulnerabilitaDopoColpo = Mathf.Clamp(
            giocatore.durataInvulnerabilitaDopoColpo,
            0f,
            2f
        );
        giocatore.durataSchivata = Mathf.Max(
            0.05f,
            giocatore.durataSchivata
        );
        giocatore.velocitaSchivata = Mathf.Max(
            1f,
            giocatore.velocitaSchivata
        );
        giocatore.cooldownSchivata = Mathf.Max(
            giocatore.durataSchivata,
            giocatore.cooldownSchivata
        );
        giocatore.durataInvulnerabilitaSchivata = Mathf.Clamp(
            giocatore.durataInvulnerabilitaSchivata,
            0.05f,
            giocatore.durataSchivata + 0.15f
        );
        giocatore.durataScudoTemporaneo = Mathf.Max(
            0.1f,
            giocatore.durataScudoTemporaneo
        );
        giocatore.caricheScudoTemporaneo = Mathf.Max(
            1,
            giocatore.caricheScudoTemporaneo
        );
        giocatore.quantitaCuraDrop = Mathf.Max(
            1,
            giocatore.quantitaCuraDrop
        );
        giocatore.durataCalamita = Mathf.Max(
            0.1f,
            giocatore.durataCalamita
        );
        giocatore.raggioCalamita = Mathf.Max(
            0.5f,
            giocatore.raggioCalamita
        );
        giocatore.velocitaAttrazioneCalamita = Mathf.Max(
            0.1f,
            giocatore.velocitaAttrazioneCalamita
        );
        giocatore.durataFuria = Mathf.Max(0.1f, giocatore.durataFuria);
        giocatore.moltiplicatoreDannoFuria = Mathf.Max(
            1f,
            giocatore.moltiplicatoreDannoFuria
        );
        giocatore.moltiplicatoreIntervalloFuria = Mathf.Clamp(
            giocatore.moltiplicatoreIntervalloFuria,
            0.2f,
            1f
        );
        giocatore.raggioEsplosioneDrop = Mathf.Max(
            0.5f,
            giocatore.raggioEsplosioneDrop
        );
        giocatore.dannoEsplosioneDrop = Mathf.Max(
            1,
            giocatore.dannoEsplosioneDrop
        );
        volpe.durataDropSullaMappa = Mathf.Max(
            0.1f,
            volpe.durataDropSullaMappa
        );
        volpe.uccisioniSenzaDropMassime = Mathf.Max(
            1,
            volpe.uccisioniSenzaDropMassime
        );
        volpe.ripetizioniMassimeStessoDrop = Mathf.Max(
            1,
            volpe.ripetizioniMassimeStessoDrop
        );
        volpe.pesoDropTriploSparo = Mathf.Max(0f, volpe.pesoDropTriploSparo);
        volpe.pesoDropVelocita = Mathf.Max(0f, volpe.pesoDropVelocita);
        volpe.pesoDropScudo = Mathf.Max(0f, volpe.pesoDropScudo);
        volpe.pesoDropCura = Mathf.Max(0f, volpe.pesoDropCura);
        volpe.pesoDropCalamita = Mathf.Max(0f, volpe.pesoDropCalamita);
        volpe.pesoDropFuria = Mathf.Max(0f, volpe.pesoDropFuria);
        volpe.pesoDropEsplosione = Mathf.Max(0f, volpe.pesoDropEsplosione);
        maialino.cambioDirezioneMassimo = Mathf.Max(
            maialino.cambioDirezioneMinimo,
            maialino.cambioDirezioneMassimo
        );
        NormalizzaArrayNonNegativo(shop.costiMovimento);
        NormalizzaArrayNonNegativo(shop.costiResistenza);
        NormalizzaArrayNonNegativo(shop.costiSalute);
        NormalizzaArrayNonNegativo(shop.costiDanno);
        NormalizzaArrayNonNegativo(shop.costiCadenza);
        NormalizzaArrayNonNegativo(shop.costiPenetrazione);
        NormalizzaArrayNonNegativo(shop.costiColpoAggiuntivo);
        NormalizzaArrayNonNegativo(shop.costiRafficaRaccolto);
        NormalizzaArrayNonNegativo(shop.costiPatataGigante);
        NormalizzaArrayNonNegativo(shop.costiPatataEsplosiva);
        NormalizzaArrayNonNegativo(shop.costiCritico);
        NormalizzaArrayNonNegativo(shop.costiRimbalzo);
        NormalizzaArrayNonNegativo(shop.costiRallentamento);
        NormalizzaArrayNonNegativo(shop.costiSpinta);
        shop.numeroOfferte = Mathf.Clamp(shop.numeroOfferte, 3, 4);
        shop.costoRerollBase = Mathf.Max(1, shop.costoRerollBase);
        shop.incrementoCostoReroll = Mathf.Max(
            1,
            shop.incrementoCostoReroll
        );
        shop.costoCura = Mathf.Max(1, shop.costoCura);
        shop.bonusCompletamentoOnda = Mathf.Max(
            0,
            shop.bonusCompletamentoOnda
        );
        shop.pesoArchetipoPercorsoPrincipale = Mathf.Clamp(
            shop.pesoArchetipoPercorsoPrincipale,
            1f,
            3f
        );
        shop.pesoArchetipoPercorsoSecondario = Mathf.Clamp(
            shop.pesoArchetipoPercorsoSecondario,
            1f,
            2.5f
        );
        shop.pesoArchetipoSinergia = Mathf.Clamp(
            shop.pesoArchetipoSinergia,
            1f,
            3f
        );
        shop.pesoPercorsoEmergente = Mathf.Clamp(
            shop.pesoPercorsoEmergente,
            1f,
            2f
        );
        shop.sogliaPrimaEvoluzione = Mathf.Max(
            1,
            shop.sogliaPrimaEvoluzione
        );
        shop.sogliaSecondaEvoluzione = Mathf.Max(
            shop.sogliaPrimaEvoluzione + 1,
            shop.sogliaSecondaEvoluzione
        );
        shop.esponenteRendimentoDecrescente = Mathf.Clamp(
            shop.esponenteRendimentoDecrescente,
            0.45f,
            0.9f
        );
        shop.frammentiRafficaPrimoStadio = Mathf.Clamp(
            shop.frammentiRafficaPrimoStadio,
            2,
            4
        );
        shop.frammentiRafficaSecondoStadio = Mathf.Clamp(
            shop.frammentiRafficaSecondoStadio,
            shop.frammentiRafficaPrimoStadio,
            5
        );
        shop.esplosioniSecondariePrimoStadio = Mathf.Clamp(
            shop.esplosioniSecondariePrimoStadio,
            1,
            3
        );
        shop.esplosioniSecondarieSecondoStadio = Mathf.Clamp(
            shop.esplosioniSecondarieSecondoStadio,
            shop.esplosioniSecondariePrimoStadio,
            4
        );
        shop.penetrazioniEvoluzionePrimoStadio = Mathf.Max(
            1,
            shop.penetrazioniEvoluzionePrimoStadio
        );
        shop.penetrazioniEvoluzioneSecondoStadio = Mathf.Max(
            shop.penetrazioniEvoluzionePrimoStadio,
            shop.penetrazioniEvoluzioneSecondoStadio
        );
        shop.rimbalziEvoluzioneSecondoStadio = Mathf.Max(
            1,
            shop.rimbalziEvoluzioneSecondoStadio
        );
        shop.raggioEsplosioneEvoluzione = Mathf.Max(
            0.2f,
            shop.raggioEsplosioneEvoluzione
        );
        shop.ritardoEsplosioniSecondarie = Mathf.Max(
            0.05f,
            shop.ritardoEsplosioniSecondarie
        );
        shop.raggioContagioSecondoStadio = Mathf.Max(
            shop.raggioContagioPrimoStadio,
            shop.raggioContagioSecondoStadio
        );
        shop.colpiPerRafficaRaccolto = Mathf.Max(
            2,
            shop.colpiPerRafficaRaccolto
        );
        shop.raggioEsplosione = Mathf.Max(0.2f, shop.raggioEsplosione);
        shop.forzaSpintaPatataGigantePerLivello = Mathf.Clamp(
            shop.forzaSpintaPatataGigantePerLivello,
            0f,
            2f
        );
        shop.moltiplicatoreDannoCritico = Mathf.Max(
            1f,
            shop.moltiplicatoreDannoCritico
        );
        shop.raggioRicercaRimbalzo = Mathf.Clamp(
            shop.raggioRicercaRimbalzo,
            1.5f,
            3.2f
        );
        shop.moltiplicatoreDannoRimbalzo = Mathf.Clamp(
            shop.moltiplicatoreDannoRimbalzo,
            0.5f,
            1f
        );
        shop.durataRallentamentoBase = Mathf.Max(
            0.1f,
            shop.durataRallentamentoBase
        );
        shop.durataRallentamentoPerLivello = Mathf.Max(
            0f,
            shop.durataRallentamentoPerLivello
        );
        shop.forzaSpintaPerLivello = Mathf.Max(
            0f,
            shop.forzaSpintaPerLivello
        );

        feedbackCombattimento.dimensioneMirino = Mathf.Clamp(
            feedbackCombattimento.dimensioneMirino,
            14f,
            48f
        );
        feedbackCombattimento.particelleImpatto = Mathf.Clamp(
            feedbackCombattimento.particelleImpatto,
            3,
            8
        );
        ondate.durataPreavvisoSpawn = Mathf.Clamp(
            ondate.durataPreavvisoSpawn,
            0.15f,
            1f
        );
        ondate.sogliaUltimiNemici = Mathf.Clamp(
            ondate.sogliaUltimiNemici,
            1,
            4
        );
        ondate.volumeSegnaleUltimiNemici = Mathf.Clamp01(
            ondate.volumeSegnaleUltimiNemici
        );

        NormalizzaVariante(variantiVolpe.comune);
        NormalizzaVariante(variantiVolpe.agile);
        NormalizzaVariante(variantiVolpe.robusta);
        NormalizzaVariante(variantiVolpe.schivatrice);
        NormalizzaVariante(variantiVolpe.alfa);
        NormalizzaVariante(variantiVolpe.ululatrice);
        NormalizzaVariante(variantiVolpe.sputafango);
        NormalizzaVariante(variantiVolpe.scavatrice);
        variantiVolpe.distanzaAttivazioneSchivata = Mathf.Max(
            0.25f,
            variantiVolpe.distanzaAttivazioneSchivata
        );
        variantiVolpe.durataSchivata = Mathf.Max(
            0.05f,
            variantiVolpe.durataSchivata
        );
        variantiVolpe.recuperoSchivata = Mathf.Max(
            0.1f,
            variantiVolpe.recuperoSchivata
        );
        variantiVolpe.moltiplicatoreVelocitaSchivata = Mathf.Max(
            1f,
            variantiVolpe.moltiplicatoreVelocitaSchivata
        );
        variantiVolpe.distanzaPreparazioneAlfa = Mathf.Max(
            0.5f,
            variantiVolpe.distanzaPreparazioneAlfa
        );
        variantiVolpe.durataPreparazioneAlfa = Mathf.Max(
            0.1f,
            variantiVolpe.durataPreparazioneAlfa
        );
        variantiVolpe.durataScattoAlfa = Mathf.Max(
            0.1f,
            variantiVolpe.durataScattoAlfa
        );
        variantiVolpe.moltiplicatoreScattoAlfa = Mathf.Max(
            1f,
            variantiVolpe.moltiplicatoreScattoAlfa
        );
        variantiVolpe.recuperoScattoAlfa = Mathf.Max(
            0.1f,
            variantiVolpe.recuperoScattoAlfa
        );
        variantiVolpe.raggioUlulato = Mathf.Max(
            0.5f,
            variantiVolpe.raggioUlulato
        );
        variantiVolpe.recuperoUlulato = Mathf.Max(
            0.1f,
            variantiVolpe.recuperoUlulato
        );
        variantiVolpe.durataPreparazioneUlulato = Mathf.Max(
            0.1f,
            variantiVolpe.durataPreparazioneUlulato
        );
        variantiVolpe.durataRallentamentoUlulato = Mathf.Max(
            0.1f,
            variantiVolpe.durataRallentamentoUlulato
        );
        variantiVolpe.moltiplicatoreRallentamentoUlulato = Mathf.Clamp(
            variantiVolpe.moltiplicatoreRallentamentoUlulato,
            0.1f,
            1f
        );
        variantiVolpe.distanzaTiroFango = Mathf.Max(
            1f,
            variantiVolpe.distanzaTiroFango
        );
        variantiVolpe.recuperoTiroFango = Mathf.Max(
            0.1f,
            variantiVolpe.recuperoTiroFango
        );
        variantiVolpe.velocitaProiettileFango = Mathf.Max(
            0.5f,
            variantiVolpe.velocitaProiettileFango
        );
        variantiVolpe.durataPozzaFango = Mathf.Max(
            0.25f,
            variantiVolpe.durataPozzaFango
        );
        variantiVolpe.raggioPozzaFango = Mathf.Max(
            0.25f,
            variantiVolpe.raggioPozzaFango
        );
        variantiVolpe.moltiplicatoreRallentamentoFango = Mathf.Clamp(
            variantiVolpe.moltiplicatoreRallentamentoFango,
            0.1f,
            1f
        );
        variantiVolpe.dannoFango = Mathf.Max(
            1,
            variantiVolpe.dannoFango
        );
        variantiVolpe.distanzaInizioScavo = Mathf.Max(
            1f,
            variantiVolpe.distanzaInizioScavo
        );
        variantiVolpe.durataScavo = Mathf.Max(
            0.1f,
            variantiVolpe.durataScavo
        );
        variantiVolpe.durataEmersione = Mathf.Max(
            0.1f,
            variantiVolpe.durataEmersione
        );
        variantiVolpe.recuperoScavo = Mathf.Max(
            0.1f,
            variantiVolpe.recuperoScavo
        );
        variantiVolpe.moltiplicatoreVelocitaScavo = Mathf.Max(
            1f,
            variantiVolpe.moltiplicatoreVelocitaScavo
        );
        variantiVolpe.volumeVersi = Mathf.Clamp01(
            variantiVolpe.volumeVersi
        );

        if (shop.frequenzeBlocco != null)
        {
            for (int i = 0; i < shop.frequenzeBlocco.Length; i++)
            {
                int minimo = 2 + shop.frequenzeBlocco.Length - 1 - i;
                int massimo = i == 0
                    ? int.MaxValue
                    : shop.frequenzeBlocco[i - 1] - 1;
                shop.frequenzeBlocco[i] = Mathf.Clamp(
                    shop.frequenzeBlocco[i],
                    minimo,
                    massimo
                );
            }
        }

        if (ondate.ondate == null) return;
        foreach (Wave onda in ondate.ondate)
        {
            if (onda == null) continue;
            onda.numeroNemici = Mathf.Max(0, onda.numeroNemici);
            if (onda.sequenzaVolpi != null)
            {
                for (int i = 0; i < onda.sequenzaVolpi.Length; i++)
                {
                    onda.sequenzaVolpi[i] = FoxVariantStyle.Normalizza(
                        onda.sequenzaVolpi[i]
                    );
                }
            }
            onda.intervalloTraNemici = Mathf.Max(
                0.05f,
                onda.intervalloTraNemici
            );
            onda.dimensioneMassimaGruppo = Mathf.Clamp(
                onda.dimensioneMassimaGruppo,
                1,
                4
            );
            onda.intervalloTraGruppi = Mathf.Max(
                onda.intervalloTraNemici,
                onda.intervalloTraGruppi
            );
            onda.numeroMaialiniBonus = Mathf.Max(
                0,
                onda.numeroMaialiniBonus
            );
            onda.vitaMaialinoBonus = Mathf.Max(1, onda.vitaMaialinoBonus);
            onda.moneteMaialinoBonus = Mathf.Max(
                0,
                onda.moneteMaialinoBonus
            );
        }
    }

    private static void NormalizzaArrayNonNegativo(int[] valori)
    {
        if (valori == null) return;
        for (int i = 0; i < valori.Length; i++)
        {
            int minimo = i == 0 ? 1 : valori[i - 1] + 1;
            valori[i] = Mathf.Max(minimo, valori[i]);
        }
    }

    private static void NormalizzaVariante(FoxVariantStats variante)
    {
        if (variante == null) return;
        variante.moltiplicatoreVelocita = Mathf.Max(
            0.1f,
            variante.moltiplicatoreVelocita
        );
        variante.moltiplicatoreAccelerazione = Mathf.Max(
            0.1f,
            variante.moltiplicatoreAccelerazione
        );
        variante.moltiplicatoreDecelerazione = Mathf.Max(
            0.1f,
            variante.moltiplicatoreDecelerazione
        );
        variante.moltiplicatoreVita = Mathf.Max(
            0.1f,
            variante.moltiplicatoreVita
        );
        variante.moltiplicatoreIntervalloAttacco = Mathf.Max(
            0.1f,
            variante.moltiplicatoreIntervalloAttacco
        );
        variante.scala = Mathf.Max(0.1f, variante.scala);
        variante.moltiplicatoreRinculo = Mathf.Clamp(
            variante.moltiplicatoreRinculo,
            0f,
            2f
        );
        variante.monetePerEliminazione = Mathf.Max(
            0,
            variante.monetePerEliminazione
        );
        variante.ampiezzaSerpentina = Mathf.Clamp(
            variante.ampiezzaSerpentina,
            0f,
            0.8f
        );
        variante.frequenzaSerpentina = Mathf.Max(
            0f,
            variante.frequenzaSerpentina
        );
    }
}
