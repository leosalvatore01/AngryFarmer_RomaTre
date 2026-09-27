# Checklist della demo

## Il giorno prima

- [ ] Tutti i test EditMode sono verdi.
- [ ] La build Windows parte da una cartella con profilo vuoto.
- [ ] La build è stata copiata e avviata su un secondo computer Windows.
- [ ] Nessun errore rosso compare in `Player.log` dopo una partita.
- [ ] Tastiera/mouse e almeno un gamepad sono stati provati.
- [ ] Audio, fullscreen/finestra, risoluzione e pausa funzionano.
- [ ] Il profilo si salva, si riapre e si azzera con conferma.
- [ ] Il file ZIP si estrae e contiene `.exe`, cartella Data e librerie.
- [ ] Commit di consegna e tag indicano la stessa versione della build.

## Prova guidata da 6–8 minuti

1. **Menu (30 s):** mostra profilo locale, opzioni e shop permanente.
2. **Inizio (45 s):** scegli Normale; fai notare lo shop gratuito e la prima
   ondata composta solo da volpi standard.
3. **Combattimento (2 min):** movimento, mira, schivata, feedback dei colpi e
   spawn che segue il contadino.
4. **Build (1 min):** mostra icone, offerte non acquistabili e un'evoluzione o
   una sinergia se il profilo demo lo consente.
5. **Varietà (1 min):** evidenzia due volpi speciali e un maialino bonus.
6. **Boss (1–2 min):** usa un profilo o una run preparata per mostrare barra,
   animazione, preavvisi, carica e seconda fase.
7. **Chiusura (45 s):** mostra game over, record, gettoni permanenti e ritorno
   al menu.

## Piano di emergenza

- Conservare due copie della build: chiavetta USB e archivio locale/cloud.
- Portare un breve video del boss e di una partita completa, senza usarlo al
  posto della build se il gioco parte correttamente.
- Se il gamepad non viene rilevato, proseguire con tastiera e mouse.
- Se un vecchio profilo altera la demo, usare `Profilo > Azzera` oppure una
  cartella salvataggi pulita.
- Non aprire l'Editor durante la presentazione salvo richiesta del docente.

## Domande tecniche da saper rispondere

- Perché lo spawn segue il giocatore?
- Come vengono scalate ondate potenzialmente infinite?
- Perché profilo e impostazioni del dispositivo sono separati?
- Cosa viene riutilizzato dal pooling e quale problema evita?
- Come i test impediscono regressioni nel bilanciamento e nella build?
