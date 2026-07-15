# Angry Farmer — baseline di bilanciamento

Versione di riferimento: **Baseline 1 — 15 luglio 2026**.

Questa baseline fotografa il comportamento giocabile precedente al lavoro di
bilanciamento. I valori attivi si modificano dall'asset Unity:

`Assets/Resources/GameBalanceConfig.asset`

I campi già serializzati nei vecchi componenti e prefab restano per
compatibilità con scene e Inspector esistenti, ma in Play Mode vengono
sovrascritti dalla configurazione centrale. Se l'asset non è disponibile, il
codice crea invece una configurazione di fallback con questi stessi valori.
Gli acquisti dello shop vengono registrati separatamente come bonus.

## Contadino

| Parametro | Base |
|---|---:|
| Velocità | 8 |
| Vita massima | 5 |
| Intervallo di sparo | 0,40 s |
| Velocità proiettile | 10 |
| Danno | 1 |
| Penetrazione | 0 |
| Durata proiettile | 3 s |
| Durata boost velocità | 5 s |
| Durata triplo sparo | 5 s |

## Volpe standard

| Parametro | Base |
|---|---:|
| Velocità | 2,4 |
| Danno | 1 |
| Intervallo attacco | 1 s |
| Monete per eliminazione | 1 |
| Probabilità drop | 30% |
| Vita prima ondata | 2 |
| Vita aggiuntiva per ondata | 1 |

## Ondate

| # | Nome | Volpi | Vita volpe | Intervallo | Maialini | Vita maialino | Premio maialino |
|---:|---|---:|---:|---:|---:|---:|---:|
| 1 | Riscaldamento | 3 | 2 | 1,80 s | 0 | 1 | 0 |
| 2 | Primi intrusi | 4 | 3 | 1,55 s | 1 | 2 | 3 |
| 3 | Branco in arrivo | 5 | 4 | 1,35 s | 1 | 2 | 3 |
| 4 | Assalto alla fattoria | 6 | 5 | 1,20 s | 1 | 3 | 4 |
| 5 | Furia della campagna | 7 | 6 | 1,10 s | 2 | 3 | 4 |
| 6 | Ultima difesa | 8 | 7 | 1,05 s | 2 | 4 | 5 |

Totale: **33 volpi** e **7 maialini bonus**.

## Economia di riferimento

- Monete garantite dalle volpi: 33.
- Monete massime dai maialini: 28.
- Massimo teorico a fine partita: 61.
- Massimo teorico prima dell'ultimo shop: 43.
- Costo per massimizzare tutti gli upgrade permanenti: 150.

Questa sproporzione è intenzionalmente conservata nel Blocco 0: verrà usata
come dato di partenza per il successivo bilanciamento dello shop.

## Shop permanente

| Potenziamento | Costi per livello | Effetto per livello |
|---|---|---|
| Movimento | 3 / 6 / 10 | +0,5 velocità |
| Resistenza | 4 / 8 / 13 | blocca ogni 5 / 4 / 3 colpi |
| Salute massima | 5 / 9 / 14 | +1 vita massima e cura 1 |
| Danno | 9 / 16 | +1 danno |
| Cadenza | 4 / 7 / 11 | -0,04 s all'intervallo di sparo |
| Penetrazione | 6 / 10 / 15 | +1 penetrazione |
| Cura istantanea | 3 | recupera 2 vita |

## Diagnostica delle ondate

Il componente `WaveRuntimeDiagnostics` sul `GestoreNemici` è disattivato per
default e non modifica RNG o gameplay.

- Attivazione rapida in Play Mode: **F3**.
- Overlay: onda, durata, volpi vive/picco, spawn e maialini attivi.
- Console: una riga riepilogativa alla conclusione o interruzione dell'ondata.

Per una misurazione comparabile, completare una partita senza modificare i
valori durante il Play Mode e annotare durata delle ondate, danni subiti e
potenziamenti acquistati.
