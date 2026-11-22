# Schermata di accesso

Il gioco mostra una schermata iniziale con accesso, creazione account e recupero password. È un'anteprima: nessuna richiesta di rete, configurazione Firebase o creazione di account. Le password vengono validate soltanto in memoria e cancellate quando si cambia modulo, si prova un'operazione non disponibile o si esce dalla schermata.

**Gioca come ospite** apre il menu esistente e conserva il profilo e i salvataggi locali. La scelta vale fino alla chiusura del gioco. Il pulsante **Accesso account** del menu riapre la schermata.

## Collegamento futuro

`ILoginProvider` definisce le operazioni di accesso, registrazione e recupero. Attualmente viene usato esclusivamente `LoginDisattivatoProvider`, con `Disponibile == false`; la schermata non ne avvia nemmeno le operazioni.

Quando servirà un servizio online:

1. Implementare `ILoginProvider` con il servizio scelto e la gestione della sessione.
2. Collegarlo esplicitamente con `OnlineLoginController.ConfiguraProvider(provider, attivaProfilo)`.
3. Nel callback `attivaProfilo`, selezionare o caricare il profilo associato all'utente autenticato e restituire `true` solo quando è pronto. La schermata non chiude un accesso riuscito senza questo passaggio.
4. Aggiungere isolamento dei salvataggi per account, uscita dalla sessione e test dell'integrazione. Il salvataggio attuale resta locale e non è ancora associato ad account online.

Non sono presenti SDK, chiavi, endpoint o progetti Firebase. Non basta cambiare un flag per attivare l'accesso online.

## Verifica

`LoginPreviewFlowTests` controlla validazione, mascheramento password, passaggi fra i moduli, assenza di invocazioni del provider disattivato, ingresso come ospite e conservazione dei progressi. Genera anteprime PC, telefono e tablet in `Logs/MobilePreview`.
