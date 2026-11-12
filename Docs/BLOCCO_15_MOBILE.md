# Blocco 15 — Interfaccia e controlli mobile

## Cosa cambia

- Schermo orizzontale fisso e interfaccia dentro la zona sicura del display,
  senza spostare la gerarchia usata dai sistemi esistenti.
- Joystick analogico sinistro, con movimento diagonale limitato e gestione di
  un dito alla volta. Un secondo dito può premere Schiva mentre ci si muove.
- Schivata alla pressione, con indicazione del tempo di ricarica.
- Mira e sparo automatici durante l'ondata. Priorità alle volpi visibili,
  esclusione di morte e sotterrate; maialini colpibili quando non ci sono volpi
  visibili. Una piccola preferenza al bersaglio corrente evita cambi continui.
- Comandi nascosti nei menu, shop, pausa e game over. Nessun movimento residuo
  alla ripresa o dopo la perdita del focus.
- Menu principale a due colonne, opzioni più grandi, carte dello shop toccabili
  su tutta la superficie. Un acquisto disabilitato rimane disabilitato anche
  toccando la carta.
- Vibrazione del telefono per danni e schivate, disattivabile nelle opzioni.
  È separata dalla vibrazione della camera e salvata sul dispositivo.
- Schema salvataggi v4: i vecchi progressi restano validi, viene aggiunta solo
  la preferenza per la vibrazione del telefono.

Tastiera, mouse e gamepad su PC conservano i comandi manuali del blocco 13.
Il supporto ai controller fisici su telefono non fa parte di questo blocco.

## Come provarlo in Unity sul PC

1. Fuori dalla modalità Play aprire `Tools > Angry Farmer > Anteprima controlli mobile`.
2. Aprire `MenuIniziale` e premere Play.
3. Provare menu, miglioramenti permanenti, scelta della difficoltà e due scelte
   gratuite iniziali senza usare la tastiera.
4. Durante l'ondata trascinare il joystick con il mouse. Lo sparo è automatico;
   premere Schiva e Pausa per controllare i relativi pulsanti.
5. Nelle opzioni verificare che “Vibrazione telefono” sia distinta da
   “Vibrazione camera”. Il PC non vibra durante l'anteprima.
6. Ripetere con la Game View a 1920×1080, 2160×1080 e 2400×1080.
   Per il notch usare il Device Simulator, se installato.
7. Fermare Play e disattivare la voce di anteprima per tornare ai comandi PC.

La scelta di anteprima dura solo nella sessione dell'Editor: non modifica le
scene, i salvataggi del profilo o il comportamento della build Windows.
Nelle build mobile i comandi touch si attivano automaticamente.

## Verifiche

Esecuzione del 28 settembre 2026 con Unity 6000.3.4f1: **119/119 test
superati**, inclusi 9 casi dedicati al mobile; nessun errore di compilazione.
Rapporto locale: `Logs/tests-block15.xml`. Anteprime di menu, shop iniziale,
shop permanente, gameplay e pausa controllate visivamente.

`MobileExperienceTests` controlla safe area ai formati 16:9, 18:9, 20:9 e tablet,
joystick, mira, pulsanti disabilitati e persistenza della preferenza aptica.
Il test in Play Mode percorre menu, shop, scelte gratuite, ondata, schivata,
pausa e fine partita; controlla anche che i pulsanti non siano coperti da altri
elementi. I dati dei test sono isolati dai progressi reali.

Le anteprime generate dai test sono in `Logs/MobilePreview` (escluse da Git).
Sono render di controllo della disposizione, non prove su un telefono reale.

## Confine con il blocco 16

Restano al blocco 16 la build Android, il pacchetto installabile e le prove su
dispositivo: multitouch reale, tastiera del telefono, notch, vibrazione,
prestazioni, sospensione/ripresa dell'app e qualità grafica. Questo blocco non
certifica una build Android né una prova fisica su iOS/Android.

Commit previsto: `feat: adatta interfaccia e controlli al gameplay mobile`.
