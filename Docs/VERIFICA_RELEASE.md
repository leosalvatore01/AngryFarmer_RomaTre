# Verifica della release candidate

Data: 11 settembre 2026  
Versione: `1.0.0-rc`  
Target: Windows 64 bit

## Risultati automatici

- Suite Unity EditMode: **110/110 test superati**, 0 fallimenti.
- Test dedicati boss: asset trasparente, 4 frame e importazione point-filter.
- Test reset profilo: record, gettoni e livelli cancellati; impostazioni del
  dispositivo conservate; nuovo JSON persistito.
- Test release: ordine scene, icona quadrata ad alta risoluzione, nome prodotto,
  versione, finestra ridimensionabile e identificativo verificati.
- `git diff --check`: nessun errore di formato.

## Build

- Procedura: `ReleaseBuildTools.BuildWindowsRcCommandLine`.
- Esito Unity: **Succeeded**.
- Percorso locale: `Builds/Windows/AngryFarmer.exe`.
- Dimensione indicata dal rapporto Unity: **139.295.070 byte**.
- Prima scena: `MenuIniziale`.
- Pacchetto: `Builds/AngryFarmer-v1.0-prof-rc-win64.zip`, **49.033.365
  byte**, senza la cartella di debug Burst.
- SHA-256 dello ZIP:
  `AD4A0867C209994F5BCB6F5144160DCA5603A7D0D78DD3E86E1726FDA255B6B5`.

## Primo avvio pulito

La build è stata avviata in modalità automatica con una cartella temporanea
vuota passata tramite `-angryFarmerSaveDirectory`. Il controllo ha verificato:

- caricamento del menu iniziale;
- costruzione dell'interfaccia;
- creazione del profilo ospite e delle impostazioni del dispositivo;
- chiusura controllata con messaggio `[RELEASE_SMOKE_OK]`.

## Verifica manuale ancora esterna

Un test realmente indipendente richiede un secondo computer Windows che non
abbia Unity né file del progetto. Prima della consegna copiare lo ZIP su quella
macchina, estrarlo, avviare il gioco e completare almeno una partita breve. È
l'unica voce che non può essere certificata dal computer di sviluppo.
