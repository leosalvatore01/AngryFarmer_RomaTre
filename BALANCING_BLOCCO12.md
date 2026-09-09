# Angry Farmer — prima taratura quantitativa del Blocco 12

Versione di riferimento: **Blocco 12 — 9 settembre 2026**.

Questo passaggio confronta la configurazione corrente con la baseline del
Blocco 8 e aggiunge controlli automatici sulla curva. Non sostituisce il
playtest esterno: sul computer è disponibile una sola telemetria, registrata
prima dei più recenti sistemi di ondate, boss, drop ed evoluzioni.

## Modifiche applicate

- Drop casuale delle volpi comuni: **30% → 24%**.
- Protezione dalla sfortuna: drop garantito dopo al massimo **6** uccisioni
  senza premio, invece di 5.
- Dalla seconda ondata il budget riceve **+1**: l'introduzione resta composta
  da tre volpi standard, mentre l'ondata 2 torna ad avere quattro nemici e una
  finestra di spawn di circa 3,6 secondi invece di 1,15.
- Primo boss: **35 → 28 monete**, crescita per apparizione **15 → 12**.
- Drop garantiti del boss: **3 → 2**.
- Gettoni permanenti: la ricompensa aumenta ogni **10** ondate anziché ogni 5.

Il primo potenziamento permanente resta raggiungibile rapidamente: le prime
10 ondate assegnano complessivamente 10 gettoni. La curva successiva diventa
più graduale: 30 gettoni entro l'ondata 20, 60 entro la 30 e 150 entro la 50.
Acquistare una volta tutti e sei i rami richiede almeno l'ondata 24, senza
spese precedenti.

## Valori mantenuti

- Shop dopo la seconda e la quarta ondata di ogni capitolo.
- Quattro offerte per apertura.
- Prezzi dei power-up del Blocco 8.
- Elite ogni 5 ondate e boss ogni 10.
- Ritmo delle prime ondate.

I prezzi non sono stati alzati: prima del primo shop il giocatore guadagna 9
monete garantite, più l'eventuale maialino. Sono sufficienti per due o tre
scelte economiche oppure per conservare il denaro verso una carta premium,
ma non per completare un percorso.

## Controlli automatici

`BalanceCurveRegressionTests` blocca regressioni su:

- finestra di spawn delle prime due ondate;
- monete garantite prima del primo shop;
- quattro aperture nei primi dieci round e shop prima del boss;
- frequenza, anti-sfortuna e permanenza a terra dei drop;
- vita, premio e drop del primo boss;
- gettoni cumulativi e velocità del primo giro di progressione permanente.

## Validazione umana ancora necessaria

Per chiudere davvero il blocco servono **8–12 partite di persone diverse**, in
modalità Normale e senza spiegazioni durante il gioco. Dopo ogni prova vanno
raccolti il file di telemetria e tre risposte brevi:

1. In quale momento ti sei annoiato o sentito sopraffatto?
2. Quale acquisto ti è sembrato obbligatorio o inutile?
3. Quale morte ti è sembrata ingiusta e perché?

Con quei dati si potranno correggere i valori una seconda volta senza
confondere preferenze personali con problemi ricorrenti.
