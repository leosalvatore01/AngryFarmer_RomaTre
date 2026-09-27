# Angry Farmer

Angry Farmer è un survival 2D a ondate infinite: il contadino affronta da
solo branchi di volpi sempre più numerosi e specializzati. La partita termina
quando il contadino muore; ogni tentativo alimenta una progressione permanente
che rende più credibile raggiungere ondate superiori.

Questa repository contiene la release candidate `1.0.0-rc` per Windows del
progetto universitario.

## Caratteristiche principali

- ondate infinite organizzate in capitoli da cinque;
- otto famiglie di volpi con grafica e comportamento dedicati;
- incontri élite e boss con attacchi telegrafati e due fasi;
- shop iniziale gratuito e shop tra le ondate;
- quattro archetipi, sinergie ed evoluzioni delle build;
- sette drop temporanei, maialini bonus e raccolta magnetica;
- shop permanente, tre difficoltà e record locali separati;
- HUD minimale, tutorial contestuale, pausa e opzioni PC;
- salvataggi JSON con backup e ripristino del profilo dal menu;
- pooling degli oggetti e protezioni per le ondate alte.

## Controlli

| Azione | Tastiera e mouse | Gamepad |
|---|---|---|
| Movimento | WASD o frecce | levetta sinistra / D-pad |
| Mira | mouse | levetta destra |
| Fuoco | tasto sinistro | grilletto destro / dorsale destro |
| Schivata | spazio o Shift sinistro | pulsante Sud / A |
| Pausa / indietro | Esc | Start / pulsante Est / B |
| Conferma menu | Invio o spazio | pulsante Sud / A |
| Scelte rapide shop | 1–4 | D-pad |
| Riprova dopo la morte | R | pulsante Sud / A |

I comandi principali possono essere rimappati dal menu Opzioni.

## Avvio del progetto

1. Installare Unity `6000.3.4f1` tramite Unity Hub.
2. Aggiungere questa cartella come progetto e aprirla con quella versione.
3. Attendere l'importazione e aprire `Assets/Scenes/MenuIniziale.unity`.
4. Premere Play. Le Build Settings devono contenere prima `MenuIniziale` e poi
   `SampleScene`.

Per produrre la build Windows usare
`Tools > Angry Farmer > Release > Build Windows RC`. Istruzioni complete e
comando automatizzato sono in [Docs/ISTRUZIONI_BUILD.md](Docs/ISTRUZIONI_BUILD.md).

## Verifica

I test automatici si eseguono da `Window > General > Test Runner`, scheda
EditMode, con `Run All`. Prima di una consegna seguire anche
[Docs/CHECKLIST_DEMO.md](Docs/CHECKLIST_DEMO.md): i test non sostituiscono una
partita completa giocata sulla build.

## Documentazione

- [GDD breve](Docs/GDD_BREVE.md)
- [Architettura](Docs/ARCHITETTURA.md)
- [Bilanciamento della release candidate](Docs/BILANCIAMENTO_RC.md)
- [Istruzioni di build](Docs/ISTRUZIONI_BUILD.md)
- [Checklist della demo](Docs/CHECKLIST_DEMO.md)
- [Verifica della release candidate](Docs/VERIFICA_RELEASE.md)
- [Credits e licenze](Docs/CREDITS_E_LICENZE.md)

## Struttura essenziale

- `Assets/_Scripts`: gameplay, interfaccia, salvataggio e strumenti runtime;
- `Assets/Editor`: importatori e procedura di build della release;
- `Assets/Resources`: configurazione di bilanciamento e asset caricati a runtime;
- `Assets/Scenes`: menu iniziale e arena survival;
- `Assets/Tests`: regressioni EditMode e PlayMode;
- `Docs`: materiali per sviluppo, demo e consegna.

I file prodotti dentro `Builds/` e i salvataggi locali non sono versionati.

## Stato della release

Target verificato: Windows 64 bit. Il progetto usa già un sistema di input
unificato adatto anche a gamepad e touch, ma una release mobile richiede ancora
profilazione su dispositivi reali, layout touch definitivo e pacchettizzazione
Android/iOS.
