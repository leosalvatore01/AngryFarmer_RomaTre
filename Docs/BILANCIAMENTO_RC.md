# Bilanciamento della release candidate

Riferimento: `1.0.0-rc`, 11 settembre 2026.

Questo documento sostituisce le baseline dei blocchi precedenti, conservate
nel repository come cronologia delle decisioni. La fonte eseguibile dei valori
resta `Assets/_Scripts/GameBalanceConfig.cs`.

## Baseline del giocatore

| Valore | Base |
|---|---:|
| Vita massima | 5 |
| Velocità | 8 |
| Intervallo di fuoco | 0,40 s |
| Intervallo minimo | 0,12 s |
| Danno del proiettile | 1 |
| Velocità del proiettile | 10 |
| Schivata | 0,18 s |
| Recupero schivata | 1,10 s |

## Curva delle ondate

- ondata 1: 3 volpi Comuni, nessun maialino;
- capitoli: 5 ondate;
- shop: dopo la seconda e la quarta posizione di ogni capitolo;
- boss: ogni 10 ondate;
- spawn volpi: anello a distanza 10 dalla posizione corrente del giocatore;
- vita base volpe: 2 alla prima ondata, poi +1 per ondata prima dei
  moltiplicatori di tipo e difficoltà;
- gruppi e intervalli si comprimono gradualmente, con tetti di sicurezza per
  impedire picchi illeggibili.

La prima ondata insegna il nemico base. Agile, Robusta e Schivatrice arrivano
prima delle varianti con attacchi speciali; Ululatrice, Sputafango e Scavatrice
allargano in seguito la pressione tattica.

## Ricompense della run

- moneta base per volpe Comune: 1;
- probabilità drop casuale: 24%;
- drop garantito dopo massimo 6 eliminazioni senza premio;
- massimo 2 ripetizioni consecutive dello stesso drop;
- permanenza drop sulla mappa: 12 secondi;
- maialino base: 1 vita, 3 monete, 14 secondi sulla mappa;
- primo boss: 28 monete e 2 drop garantiti;
- crescita premio boss: +12 per apparizione.

Prima del primo shop, il flusso garantito produce 9 monete oltre agli eventuali
maialini. Questo permette più acquisti economici oppure una scelta di risparmio,
senza completare immediatamente un percorso.

## Boss

| Valore | Configurazione |
|---|---:|
| Moltiplicatore vita | 5,5× |
| Crescita vita per apparizione | +35% |
| Soglia seconda fase | 50% |
| Intervallo attacchi | 2,15 s |
| Velocità carica | 10,5 |
| Proiettili raffica | 5 / 7 |
| Danno | 1 / 2 |

Carica, schianto e raffica hanno preavvisi distinti. La corsa usa quattro pose
dedicate e la sua cadenza segue la velocità reale, compresa la carica.

## Shop della run

Ogni apertura mostra 4 offerte. Il reroll parte da 1 moneta e aumenta di 1.
Le statistiche possono progredire senza un livello massimo artificiale; gli
effetti che potrebbero rompere il gioco usano limiti matematici o rendimenti
decrescenti. Le evoluzioni principali scattano alle soglie 3 e 6.

## Progressione permanente

Ogni ondata completata assegna `1 + floor((onda - 1) / 10)` gettoni. I primi
10 round danno 10 gettoni complessivi, i primi 20 ne danno 30, i primi 30 ne
danno 60 e i primi 50 ne danno 150.

I sei rami non hanno un tetto di livello rappresentativo. Vita, danno e
provviste crescono linearmente; cadenza, velocità e blocco tendono a limiti
sicuri. I costi crescono con potenze tra 1,30 e 1,40 per mantenere significativa
la progressione a lungo termine.

## Criteri del prossimo playtest

Su almeno 8 partite in Normale raccogliere:

1. ondata e causa della morte;
2. danni subiti per tipo di nemico;
3. acquisti e reroll per apertura;
4. drop raccolti e scaduti;
5. durata del boss e colpi subiti per attacco;
6. impressione del giocatore su noia, confusione e scelta obbligatoria.

Intervenire soltanto sui problemi ripetuti in più sessioni. Un singolo record
alto o basso non è sufficiente per cambiare la curva globale.
