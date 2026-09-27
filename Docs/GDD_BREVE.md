# Angry Farmer — GDD breve

Versione: `1.0.0-rc`  
Genere: survival arena 2D a ondate infinite  
Target corrente: PC Windows, tastiera/mouse e gamepad

## Visione

Il giocatore deve sentirsi sempre a un passo dal perdere il controllo della
fattoria, ma deve poter capire perché è stato colpito e quale scelta avrebbe
potuto salvarlo. Le partite iniziano subito, generano build differenti e
premiano sia l'abilità nella singola run sia la costanza tra più run.

## Tre pilastri

1. **Combattimento leggibile.** Mira manuale, schivata, preavvisi e identità
   visive distinte rendono evitabili gli attacchi pericolosi.
2. **Scelte frequenti.** Shop, archetipi, sinergie e drop cambiano il modo di
   giocare senza interrompere continuamente l'azione.
3. **Tensione crescente.** La composizione dei branchi evolve, le ondate non
   hanno un limite e ogni dieci ondate arriva un boss più resistente.

## Ciclo di gioco

Menu iniziale → scelta della difficoltà → shop gratuito → combattimento →
ricompense e drop → shop alle pause previste → élite/boss → morte → riepilogo,
record e gettoni permanenti → nuovo tentativo.

Non esiste una condizione di vittoria. Il risultato principale è l'ondata
massima raggiunta, accompagnata da punteggio e volpi eliminate.

## Giocatore

Il contadino si muove in otto direzioni, mira indipendentemente, spara patate e
può effettuare una schivata con breve invulnerabilità. Parte da 5 punti vita,
velocità 8, danno 1 e un colpo ogni 0,4 secondi nella configurazione base.

La build della partita può specializzarsi in Tiratore, Artigliere, Critico o
Controllore. I power-up non hanno un tetto artificiale: oltre le soglie di
evoluzione continuano a crescere con rendimenti controllati.

## Nemici

| Tipo | Ruolo leggibile |
|---|---|
| Comune | inseguitore di base e riferimento della prima ondata |
| Agile | rapido, riduce lo spazio di reazione |
| Robusta | lenta e resistente, occupa l'arena |
| Schivatrice | evita lateralmente i colpi in arrivo |
| Alfa | prepara uno scatto aggressivo |
| Ululatrice | supporta e potenzia il branco |
| Sputafango | controlla lo spazio con attacchi a distanza |
| Scavatrice | scompare e riemerge vicino al giocatore |

La prima ondata contiene solo tre volpi Comuni. I tipi speciali vengono
introdotti gradualmente. Lo spawn forma un anello intorno alla posizione
corrente del contadino, così l'azione segue il giocatore nella mappa.

## Ritmo delle ondate

- un capitolo dura 5 ondate;
- lo shop completo si apre dopo le posizioni 2 e 4 del capitolo;
- ogni quinta ondata è un climax élite, salvo gli incontri boss;
- ogni decima ondata compare Il Re del Branco;
- dopo la sequenza iniziale, budget, salute e composizione continuano a scalare.

Il boss alterna inseguimento, carica, schianto ad area e raffica di proiettili.
Al 50% della vita entra nella seconda fase e aumenta ritmo e pericolosità.

## Economie e ricompense

Le monete valgono solo nella partita corrente e finanziano lo shop della run.
I maialini sono bersagli bonus che scappano e rilasciano monete. I drop delle
volpi restano a terra 12 secondi e offrono effetti temporanei; un sistema
anti-sfortuna evita sequenze troppo lunghe senza drop.

I gettoni permanenti vengono assegnati completando ondate e finanziano sei rami
interpartita: vita, danno, cadenza, movimento, resistenza e provviste. Il loro
shop è accessibile soltanto dal menu, prima della partita.

## Interfaccia e accessibilità

L'HUD mostra solo stato vitale, ondata, nemici rimasti, risorse ed effetti
attivi. Le informazioni estese compaiono nelle schermate dedicate. Sono
disponibili volume separato, vibrazione camera, flash, numeri di danno,
dimensione mirino, risoluzione, modalità schermo, VSync e limite FPS.

## Persistenza

Il profilo locale conserva nome, record, gettoni e miglioramenti permanenti.
Le impostazioni del dispositivo sono separate. Ogni file JSON ha backup e
scrittura sicura; dal pannello Profilo è possibile azzerare i progressi con una
doppia conferma senza perdere audio, video o comandi.

## Criteri di successo della demo

- entro 30 secondi il giocatore comprende movimento, mira e fuoco;
- la prima ondata è leggibile senza spiegazione orale;
- almeno due scelte di build hanno conseguenze percepibili;
- élite e boss comunicano chiaramente gli attacchi;
- morte, record e progressione permanente invitano a riprovare.
