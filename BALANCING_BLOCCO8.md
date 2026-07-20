# Angry Farmer — bilanciamento finale del Blocco 8

Versione di riferimento: **Blocco 8 — Fine partita e bilanciamento — 20 luglio 2026**.

Questo documento aggiorna la fotografia iniziale contenuta in
`BALANCING_BASELINE.md`. I valori attivi restano centralizzati in
`Assets/Resources/GameBalanceConfig.asset`.

## Difficoltà

Le tre modalità mantengono la stessa quantità e composizione di volpi. In
questo modo la difficoltà non riduce le monete disponibili in Tranquilla e non
regala più acquisti in Difficile.

| Modalità | Vita base per onda | HP totali volpi | Velocità | Intervalli spawn | Punteggio |
|---|---|---:|---:|---:|---:|
| Tranquilla | 2 / 3 / 3 / 4 / 5 / 6 | 163 | 90% | 115% | 85% |
| Normale | 2 / 3 / 4 / 5 / 6 / 7 | 194 | 100% | 100% | 100% |
| Difficile | 2 / 4 / 5 / 6 / 7 / 8 | 229 | 108% | 88% | 120% |

L'HP totale include i moltiplicatori delle varianti Agile, Robusta, Ladra e
Alfa. Nella prima ondata l'arrotondamento produce 6 HP in tutte le modalità;
velocità e ritmo distinguono comunque l'esperienza fin dall'inizio.

## Curva delle ondate

Legenda: **C** comune, **A** agile, **R** robusta, **L** ladra, **α** alfa.

| Onda | Composizione | Gruppi | Finestra spawn Normale | HP T / N / D | Monete volpi + bonus onda | Maialini opzionali |
|---:|---|---:|---:|---:|---:|---:|
| 1 | C×3 | 2 | 3,60 s | 6 / 6 / 6 | 3 + 1 | 0 |
| 2 | C×3, A×1 | 2 | 4,65 s | 11 / 11 / 15 | 4 + 1 | +2 |
| 3 | C×2, A×2, R×1 | 3 | 5,40 s | 15 / 21 / 26 | 6 + 1 | +2 |
| 4 | C×2, A×1, R×2, L×1 | 2 | 6,00 s | 29 / 35 / 42 | 9 + 1 | +3 |
| 5 | C×1, A×2, R×2, L×1, α×1 | 3 | 6,60 s | 44 / 52 / 62 | 14 + 1 | +6 |
| 6 | C×1, A×2, R×2, L×2, α×1 | 3 | 7,35 s | 58 / 69 / 78 | 16 + 1 | +10 |

Totale: **33 volpi**, con quantità 3 → 4 → 5 → 6 → 7 → 8 e una
composizione che introduce progressivamente le varianti più impegnative.

## Economia

Nello scenario di riferimento in cui tutte le volpi vengono eliminate, le
monete cumulative dopo ogni onda, incluso il bonus di completamento, sono:

`4 → 9 → 16 → 26 → 41 → 58`

Prima dell'ultima onda il giocatore dispone quindi di:

- 41 monete da eliminazioni e completamenti, prima delle spese;
- circa 49–50 includendo metà dei premi maialino e la cassa;
- 56 come massimo assoluto, salvando tutti i maialini e aprendo la cassa.

Il massimo teorico a fine partita è 83: 52 dalle volpi, 6 dai completamenti,
23 dai maialini e 2 dalla cassa. Una Volpe ladra che fugge non assegna la sua
ricompensa, quindi il guadagno reale può essere inferiore. Cure e reroll
riducono naturalmente il budget destinato alla build.

## Costi dello shop

| Potenziamento | Costi per livello | Totale |
|---|---:|---:|
| Movimento | 3 / 5 / 8 | 16 |
| Resistenza | 4 / 7 / 10 | 21 |
| Salute massima | 4 / 7 / 10 | 21 |
| Danno | 10 / 16 | 26 |
| Cadenza | 3 / 6 / 9 | 18 |
| Penetrazione | 3 / 5 / 7 | 15 |
| Colpo aggiuntivo | 6 / 10 | 16 |
| Raffica del raccolto | 11 | 11 |
| Patata gigante | 4 / 7 | 11 |
| Patata esplosiva | 12 | 12 |
| Critico | 3 / 5 / 7 | 15 |
| Rimbalzo | 8 / 14 | 22 |
| Rallentamento | 3 / 6 | 9 |
| Spinta | 3 / 6 / 9 | 18 |

Costo dei percorsi completi:

- Raffica: **45**;
- Artiglieria: **49**;
- Perforazione: **52**;
- Controllo: **27**;
- Utilità completa, esclusa la cura: **58**.

Nessun percorso offensivo completo è acquistabile soltanto con le 41 monete
di riferimento prima dell'ultima onda; premi opzionali e scelte ibride hanno
quindi un peso reale.

## Verifica delle combinazioni più forti

I valori seguenti sono tetti teorici con 40–41 monete e servono a confrontare
le build, non a promettere danno costante in partita.

| Combinazione | Costo | Risultato indicativo |
|---|---:|---|
| Danno II + Cadenza II + Critico I | 38 | circa 10,5 DPS affidabili su un bersaglio |
| Danno II + Esplosiva + Cadenza I | 41 | 8,33 DPS singoli; fino a 19,44 aggregati su tre volpi molto vicine |
| Danno II + Rimbalzo I + Cadenza I + Critico I | 40 | circa 15,56 DPS aggregati su due volpi entro 2,4 unità |
| Danno II + Raffica + Critico I | 40 | fino a 11,76 DPS se i colpi laterali trovano bersagli |
| Controllo completo + Danno I + Cadenza I | 40 | 5,56 DPS, rallentamento al 60% per 1,45 s e spinta 2,55 |

Le specializzazioni hanno condizioni diverse: Esplosiva richiede gruppi molto
stretti, Rimbalzo è più affidabile ma perde il 35% del danno a ogni salto,
Raffica dipende dalla dispersione e Controllo sacrifica danno per sicurezza.
Non emerge quindi una scelta migliore in ogni situazione.

Altri limiti applicati:

- il Rimbalzo cerca bersagli entro 2,4 unità e costa 8 / 14;
- il Colpo aggiuntivo non si somma allo stesso sparo che genera il ventaglio;
- Patata gigante aggiunge 1,1 di spinta per livello oltre a dimensione e peso;
- Resistenza migliora davvero da 1 blocco ogni 5 colpi a 1 ogni 4 e poi 1 ogni 3;
- cure e reroll non possono diventare gratuiti;
- lo shop garantisce almeno un'offerta acquistabile e due modificatori quando disponibili, favorendo leggermente il percorso già iniziato.

Il percorso Perforazione resta volutamente più efficace come ibrido con Danno:
la penetrazione si attiva quando il colpo elimina una volpe e diventa meno
frequente contro le alte vite delle ultime ondate.

## Fine partita, record e prestazioni

Il riepilogo finale mostra difficoltà, durata completa della run, volpi
eliminate, precisione, monete raccolte/spese/rimaste, galline, uova, obiettivi,
punteggio, record separati per difficoltà e build finale. La durata include
combattimento e bottega, ma esclude la selezione iniziale e la pausa manuale.
Il retry con **R** conserva la difficoltà e riparte senza un secondo menu.
Il punteggio usa le monete raccolte, non quelle rimaste: acquistare una build
non penalizza quindi il tentativo di record.

La precisione conta il primo bersaglio valido colpito da ogni proiettile,
inclusi maialini e oggetti interattivi; penetrazioni, esplosioni e rimbalzi non
possono gonfiare il dato contando più centri per lo stesso colpo.

Per contenere i picchi durante gruppi numerosi:

- gli impatti usano un pool fisso di 14 burst;
- i numeri di danno vengono riutilizzati e hanno un tetto di 28 elementi;
- le esplosioni della build hanno un tetto di 12 effetti simultanei;
- i feedback cosmetici oltre il tetto vengono saltati senza influire sul danno;
- la fine dell'ondata attende comunque la conclusione delle animazioni di morte.

## Controlli eseguiti

- compilazione completa degli assembly runtime ed Editor;
- validazione di curve, composizioni, vite e punteggi delle tre difficoltà;
- validazione di tutti i prezzi, dei guardrail e del budget Perforazione;
- Play Mode su `SampleScene`: selettore, time scale, offerte shop, impatti reali,
  falloff del Rimbalzo, statistiche, record, limite VFX, riepilogo e retry;
- controllo automatico dell'overflow dei testi nelle nuove schermate.

Tutti i controlli del Blocco 8 risultano superati.
