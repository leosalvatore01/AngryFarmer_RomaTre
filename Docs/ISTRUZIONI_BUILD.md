# Istruzioni di build

## Requisiti

- Windows 10/11 a 64 bit;
- Unity Hub;
- Unity Editor `6000.3.4f1` con modulo Windows Build Support;
- almeno 3 GB liberi per importazione, cache e build.

La versione esatta è registrata in `ProjectSettings/ProjectVersion.txt`.

## Preparazione

1. Aprire la cartella del repository con Unity Hub.
2. Attendere che Unity termini importazione e compilazione senza errori rossi.
3. In `File > Build Profiles` verificare Windows 64 bit.
4. Verificare l'ordine delle scene: `MenuIniziale`, poi `SampleScene`.
5. Eseguire `Window > General > Test Runner > EditMode > Run All`.

## Build dal menu

Usare `Tools > Angry Farmer > Release > Build Windows RC`. La procedura:

- applica nome prodotto, versione, identificatore, finestra ridimensionabile e
  icona ufficiale;
- usa tutte le scene abilitate nelle Build Settings;
- produce `Builds/Windows/AngryFarmer.exe`;
- interrompe la build se Unity segnala un errore.

La cartella `Builds` è intenzionalmente esclusa da Git.

## Build automatizzata

Da PowerShell, con Unity chiuso:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.4f1\Editor\Unity.exe" `
  -batchmode -quit `
  -projectPath "C:\percorso\AngryFarmer_RomaTre" `
  -executeMethod ReleaseBuildTools.BuildWindowsRcCommandLine `
  -logFile "C:\percorso\AngryFarmer-build.log"
```

Adattare soltanto i percorsi. Il codice di uscita deve essere `0`.

## Prova con profilo pulito

Usare una cartella temporanea vuota per evitare che i propri progressi
mascherino problemi di primo avvio:

```powershell
& ".\Builds\Windows\AngryFarmer.exe" `
  -batchmode -nographics `
  -angryFarmerReleaseSmoke `
  -angryFarmerSaveDirectory "$env:TEMP\AngryFarmer-CleanProfile" `
  -logFile "$env:TEMP\AngryFarmer-smoke.log"
```

Nel log deve comparire `[RELEASE_SMOKE_OK]` e il processo deve chiudersi da
solo con codice `0`. Per la prova finale su un altro PC copiare l'intera
cartella `Builds/Windows`, non il solo `.exe`, e seguire la checklist demo.

## Pacchetto da consegnare

Comprimere il contenuto della cartella Windows in un file chiamato
`AngryFarmer-v1.0-prof-rc-win64.zip`. Conservare anche il commit di consegna e
il tag `v1.0-prof-rc`, così sorgenti e binario restano riconducibili alla stessa
versione.
