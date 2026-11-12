# Blocco 16 — Preparazione Android

## Stato e confini

Preparazione del progetto implementata con Unity **6000.3.4f1**. Su richiesta
del proprietario non sono stati installati moduli o SDK. Mancando Android Build
Support, **nessun APK/AAB è stato generato o provato su telefono**. Non è una
certificazione di pubblicabilità sullo store né di prestazioni reali.

## Cosa è stato aggiunto

- Nelle opzioni mobile: **60 FPS / fluidità** e **30 FPS / risparmio**, con
  preferenze separate da quelle PC. Si tratta di limiti desiderati, non di
  frame rate garantiti: serve misurarli sui dispositivi.
- **Grafica completa / leggera**: la modalità leggera ferma l'oscillazione
  decorativa delle piante e limita i numeri di danno sulle volpi a 10 simultanei.
  I danni sul contadino continuano ad avere priorità. Non cambia quantità,
  animazioni, attacchi o difficoltà dei nemici. Le altre opzioni per effetti,
  vibrazioni e numeri restano disponibili.
- Sospensione: salvataggio di profilo e preferenze, silenziamento dell'audio,
  azzeramento dell'input touch, pausa della partita. Gli eventi di perdita del
  focus e sospensione possono arrivare in ordine diverso: la ripresa aspetta
  entrambi e richiede il pulsante **Riprendi** della pausa per ricominciare.
- Se Android termina il processo, si riparte dal menu conservando miglioramenti
  permanenti, gettoni già accreditati e opzioni. **La run in corso non viene
  salvata** e la sospensione non accredita premi per ondate non concluse.
- Salvataggi v5: migrazione delle vecchie preferenze a 60 FPS/grafica completa,
  senza cancellare profilo, progressi o scelta della vibrazione.
- Pipeline separata Windows/Android: identificatore stabile
  `com.leosalvatore01.angryfarmer`, Android 8+ (API 26), ARM64, IL2CPP,
  OpenGL ES 3, orientamento orizzontale, frame pacing ottimizzato, icona.
- Override di importazione solo Android: sprite grandi in ETC2 RGBA8 con alpha;
  sprite con entrambi i lati sotto 256 pixel in RGBA32 senza perdita.
  Sorgenti, animazioni, dimensioni massime e importazione PC non vengono cambiati.
  Il comando di configurazione include anche gli sprite aggiunti in futuro.

## Quando installerai il modulo

1. In Unity Hub, sulla versione **6000.3.4f1**, scegliere Add modules:
   **Android Build Support**, **Android SDK & NDK Tools**, **OpenJDK**.
2. Riaprire il repository `AngryFarmer_RomaTre`, non una copia vecchia.
3. Fuori da Play usare `Tools > Angry Farmer > Android > 1 - Configura progetto`.
4. Usare `2 - Verifica requisiti`. Il controllo legge anche i percorsi personalizzati
   di Unity per SDK/NDK/JDK; non scarica né installa nulla.
5. In `File > Build Profiles` selezionare Android e **Switch Platform**;
   attendere l'importazione. Scene abilitate: `MenuIniziale`, poi `SampleScene`.
6. Usare `3 - Build APK di prova`.

Risultato atteso, **solo dopo una build riuscita**:
`Builds/Android/AngryFarmer-test.apk` e relativa ricevuta `.apk.json` con versione,
data, dimensione e SHA-256. `Builds/` è esclusa da Git. La build non è Development:
serve per provare il comportamento normale; per il Profiler creare separatamente
una Development Build dalle Build Profiles.

L'APK di prova usa la firma debug di Unity, senza sostituire il keystore di
distribuzione. Per installarlo, trasferirlo sul telefono e autorizzare
l'installazione da quella sorgente. Con strumenti Android configurati si può
anche usare `adb install -r Builds/Android/AngryFarmer-test.apk`.
Un aggiornamento mantiene i dati soltanto se firma e identificatore coincidono:
in caso di errore di firma **non disinstallare automaticamente**, perché questo
può cancellare i progressi locali. Le build debug non sono pacchetti di distribuzione.

## AAB per distribuzione

Configurare manualmente un keystore e un alias in
`Player Settings > Publishing Settings`, poi usare `4 - Build AAB distribuzione`.
Il comando rifiuta la build se mancano chiave o password; non crea chiavi e non
memorizza password in script o ricevute. Tenere chiave, password e backup fuori
dal repository; `.keystore` e `.jks` sono esclusi da Git.

L'output atteso è `Builds/Android/AngryFarmer.aab` con ricevuta `.aab.json`.
L'AAB non si installa toccandolo come un APK. Prima dello store verificare
requisiti aggiornati, firma e API richieste. Il target SDK è il più alto disponibile
nell'SDK installato: **non garantisce da solo il rispetto delle policy dello store**.
Incrementare manualmente `Bundle Version Code` per un nuovo caricamento;
la procedura non cambia da sola la versione del gioco.

## Automazione opzionale

Con Unity chiuso, da PowerShell (adattare solo il percorso del repository):

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.4f1\Editor\Unity.exe" `
  -batchmode -quit -buildTarget Android `
  -projectPath "C:\percorso\AngryFarmer_RomaTre" `
  -executeMethod AndroidBuildTools.BuildApk `
  -logFile "C:\percorso\android-build.log"
```

Per il solo controllo preliminare, anche senza modulo, omettere `-buildTarget
Android` e usare `AndroidBuildTools.VerificaRequisiti`: scrive un rapporto leggibile
nel log e non produce artefatti. Questo comando diagnostico non certifica una
build riuscita. Le build vere falliscono esplicitamente in caso di prerequisiti
mancanti, errori di compilazione o packaging.

## Checklist su dispositivi reali — tutta da eseguire

Annotare telefono, versione Android, memoria, commit, versione gioco e modalità
grafica per ogni prova. Servono almeno un dispositivo medio e uno meno potente.

- [ ] Primo avvio e avvio offline; profilo/nome con tastiera del telefono.
- [ ] Menu, shop permanente e due scelte iniziali senza tastiera o mouse.
- [ ] Joystick + schivata con due dita; dita spostate fuori dai pulsanti; nessun
      movimento o sparo residuo dopo pausa/shop/game over.
- [ ] Notch e bordi sicuri, formati 16:9/18:9/20:9 e pulsanti raggiungibili.
- [ ] Sprite, trasparenze, boss e testi senza artefatti di compressione.
- [ ] Almeno 15 minuti a 30 FPS e a 60 FPS; ondate 1, 10, 20 e boss, registrando
      frame time, memoria, scatti e riscaldamento. Confrontare grafica leggera.
- [ ] Home/ritorno dopo 5 e 30 secondi, blocco/sblocco, cambio app e interruzione
      mentre sono aperti shop, menu, pausa o banner tra ondate.
- [ ] Rientro in pausa senza danni immediati; ripresa esplicita; audio corretto.
- [ ] Chiusura forzata dopo un'ondata conclusa: gettoni, potenziamenti permanenti
      e preferenze conservati; ritorno al menu, senza promettere il recupero run.
- [ ] Aggiornamento APK con stessa firma senza perdita di dati.
- [ ] Vibrazione attivabile/disattivabile e assenza di regressioni PC.

## Verifiche locali

Verifica del **29 settembre 2026** con Unity 6000.3.4f1: **131/131 test superati**,
senza errori di compilazione. Inclusi 12 nuovi casi Android (normalizzazione FPS,
compressione, migrazione v4, persistenza e requisiti) e il flusso touch esteso con
opzioni 30 FPS/grafica leggera, sospensione/focus, salvataggio e ripresa esplicita.
I dati delle prove sono isolati dai progressi reali del giocatore.

Configurazione Android applicata a 49 importazioni sprite, senza cambiare i file
immagine o l'importazione PC. Controllate visivamente le anteprime della pausa e
della ripresa a 18:9. Test automatici e anteprime sono simulazioni su PC:
non sostituiscono la checklist su telefono.

Log locali: `Logs/android-config-block16.log`, `Logs/android-preflight-block16.log`,
`Logs/tests-block16.xml`; anteprime: `Logs/MobilePreview`.

Commit suggerito: `feat: prepara Android, opzioni prestazioni e pausa sicura su mobile`.

## Riferimenti delle scelte tecniche

- [Unity: targetFrameRate su mobile](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Application-targetFrameRate.html).
- [Unity: sospensione e focus Android](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/MonoBehaviour.OnApplicationPause.html).
- [Unity: formati texture per piattaforma](https://docs.unity3d.com/6000.3/Documentation/Manual/texture-choose-format-by-platform.html).
- [Unity: processo di build Android](https://docs.unity3d.com/6000.3/Documentation/Manual/android-BuildProcess.html).
