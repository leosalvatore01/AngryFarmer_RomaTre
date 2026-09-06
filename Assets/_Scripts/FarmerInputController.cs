using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum SchemaInputContadino
{
    TastieraMouse,
    Gamepad,
    Touch
}

/// <summary>
/// Unica sorgente degli input del gioco. Le altre componenti leggono azioni
/// semantiche (movimento, fuoco, schivata, pausa e UI) senza interrogare
/// direttamente tastiera, mouse o gamepad.
/// </summary>
[DefaultExecutionOrder(-500)]
[DisallowMultipleComponent]
public sealed class FarmerInputController : MonoBehaviour
{
    private const float SogliaMiraGamepad = 0.18f;

    private InputActionMap gameplay;
    private InputActionMap interfaccia;
    private InputAction azioneMovimento;
    private InputAction azioneMiraGamepad;
    private InputAction azionePuntatore;
    private InputAction azioneFuoco;
    private InputAction azioneSchivata;
    private InputAction azionePausa;
    private InputAction azioneConferma;
    private InputAction azioneAnnulla;
    private InputAction azioneSceltaUno;
    private InputAction azioneSceltaDue;
    private InputAction azioneSceltaTre;
    private InputAction azioneRiprova;
    private InputAction azioneDebugFeedback;
    private InputAction azioneDebugOndata;
    private Vector2 ultimaMiraGamepad = Vector2.right;
    private bool costruito;
    private float prossimoControlloSelezione;

    public static FarmerInputController Instance { get; private set; }
    public SchemaInputContadino SchemaCorrente { get; private set; } =
        SchemaInputContadino.TastieraMouse;
    public Vector2 Movimento => azioneMovimento != null
        ? Vector2.ClampMagnitude(
            azioneMovimento.ReadValue<Vector2>(),
            1f
        )
        : Vector2.zero;
    public Vector2 MiraGamepad => azioneMiraGamepad != null
        ? Vector2.ClampMagnitude(
            azioneMiraGamepad.ReadValue<Vector2>(),
            1f
        )
        : Vector2.zero;
    public Vector2 PosizionePuntatoreSchermo => azionePuntatore != null
        ? azionePuntatore.ReadValue<Vector2>()
        : Vector2.zero;
    public bool FuocoPremuto => azioneFuoco != null &&
        azioneFuoco.IsPressed();
    public bool SchivataPremutaQuestoFrame => azioneSchivata != null &&
        azioneSchivata.WasPressedThisFrame();
    public bool PausaPremutaQuestoFrame => azionePausa != null &&
        azionePausa.WasPressedThisFrame();
    public bool ConfermaPremutaQuestoFrame => azioneConferma != null &&
        azioneConferma.WasPressedThisFrame();
    public bool AnnullaPremutaQuestoFrame => azioneAnnulla != null &&
        azioneAnnulla.WasPressedThisFrame();
    public bool RiprovaPremutoQuestoFrame => azioneRiprova != null &&
        azioneRiprova.WasPressedThisFrame();
    public bool DebugFeedbackPremutoQuestoFrame =>
        azioneDebugFeedback != null &&
        azioneDebugFeedback.WasPressedThisFrame();
    public bool DebugOndataPremutoQuestoFrame =>
        azioneDebugOndata != null &&
        azioneDebugOndata.WasPressedThisFrame();
    public int NumeroAzioniGameplay => gameplay != null
        ? gameplay.actions.Count
        : 0;
    public int NumeroAzioniInterfaccia => interfaccia != null
        ? interfaccia.actions.Count
        : 0;
    public bool AzioniAbilitate => gameplay != null && gameplay.enabled &&
        interfaccia != null && interfaccia.enabled;

    public event Action<SchemaInputContadino> SchemaCambiato;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void AzzeraStatoStatico()
    {
        Instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreaPrimaDellaScena()
    {
        CreaOTrova();
    }

    public static FarmerInputController CreaOTrova()
    {
        if (Instance != null) return Instance;

        FarmerInputController esistente =
            FindFirstObjectByType<FarmerInputController>();
        if (esistente != null)
        {
            Instance = esistente;
            return esistente;
        }

        GameObject oggetto = new GameObject("InputContadino");
        return oggetto.AddComponent<FarmerInputController>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        if (Application.isPlaying) DontDestroyOnLoad(gameObject);
        CostruisciAzioni();
    }

    private void OnEnable()
    {
        CostruisciAzioni();
        gameplay.Enable();
        interfaccia.Enable();
    }

    private void OnDisable()
    {
        gameplay?.Disable();
        interfaccia?.Disable();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (SchemaCorrente != SchemaInputContadino.Gamepad ||
            EventSystem.current == null)
        {
            return;
        }

        if (!InterfacciaModaleAttiva())
        {
            if (EventSystem.current.currentSelectedGameObject != null)
                EventSystem.current.SetSelectedGameObject(null);
            return;
        }

        if (Time.unscaledTime < prossimoControlloSelezione) return;
        prossimoControlloSelezione = Time.unscaledTime + 0.15f;
        GameObject selezionato = EventSystem.current.currentSelectedGameObject;
        if (selezionato != null && selezionato.activeInHierarchy) return;

        Selectable[] selezionabili = Selectable.allSelectablesArray;
        for (int i = 0; i < selezionabili.Length; i++)
        {
            Selectable candidato = selezionabili[i];
            if (candidato == null || !candidato.IsInteractable() ||
                !candidato.gameObject.activeInHierarchy)
            {
                continue;
            }
            EventSystem.current.SetSelectedGameObject(candidato.gameObject);
            break;
        }
    }

    public bool SceltaRapidaPremutaQuestoFrame(int indice)
    {
        switch (indice)
        {
            case 1:
                return azioneSceltaUno != null &&
                       azioneSceltaUno.WasPressedThisFrame();
            case 2:
                return azioneSceltaDue != null &&
                       azioneSceltaDue.WasPressedThisFrame();
            case 3:
                return azioneSceltaTre != null &&
                       azioneSceltaTre.WasPressedThisFrame();
            default:
                return false;
        }
    }

    public void ConfiguraTastoDebugOndata(KeyCode tasto)
    {
        CostruisciAzioni();
        string nomeControllo = NomeControlloTastiera(tasto);
        azioneDebugOndata.ApplyBindingOverride(
            0,
            "<Keyboard>/" + nomeControllo
        );
    }

    public bool ProvaOttieniDirezioneMira(
        Camera camera,
        Vector3 posizioneGiocatore,
        float distanzaMinima,
        out Vector2 direzione
    )
    {
        Vector2 miraStick = MiraGamepad;
        if (miraStick.sqrMagnitude >=
            SogliaMiraGamepad * SogliaMiraGamepad)
        {
            ultimaMiraGamepad = miraStick.normalized;
            ImpostaSchema(SchemaInputContadino.Gamepad);
        }

        if (SchemaCorrente == SchemaInputContadino.Gamepad &&
            ultimaMiraGamepad.sqrMagnitude > 0.001f)
        {
            direzione = ultimaMiraGamepad.normalized;
            return true;
        }

        if (camera == null)
        {
            direzione = Vector2.zero;
            return false;
        }

        Vector3 posizioneSchermo = PosizionePuntatoreSchermo;
        posizioneSchermo.z = Mathf.Abs(
            posizioneGiocatore.z - camera.transform.position.z
        );
        Vector3 posizioneMondo = camera.ScreenToWorldPoint(
            posizioneSchermo
        );
        Vector2 scarto =
            (Vector2)posizioneMondo - (Vector2)posizioneGiocatore;
        float minimo = Mathf.Max(0f, distanzaMinima);
        if (scarto.sqrMagnitude < minimo * minimo)
        {
            direzione = Vector2.zero;
            return false;
        }

        direzione = scarto.normalized;
        return true;
    }

    public bool PossiedeBinding(string nomeAzione, string percorso)
    {
        if (string.IsNullOrWhiteSpace(nomeAzione) ||
            string.IsNullOrWhiteSpace(percorso))
        {
            return false;
        }

        return MappaPossiedeBinding(gameplay, nomeAzione, percorso) ||
               MappaPossiedeBinding(interfaccia, nomeAzione, percorso);
    }

    private void CostruisciAzioni()
    {
        if (costruito) return;
        costruito = true;

        gameplay = new InputActionMap("Gameplay");
        azioneMovimento = gameplay.AddAction(
            "Movimento",
            InputActionType.Value
        );
        azioneMovimento.expectedControlType = "Vector2";
        azioneMovimento.AddBinding("<Gamepad>/leftStick")
            .WithProcessor("stickDeadzone(min=0.15)");
        azioneMovimento.AddBinding("<Gamepad>/dpad");
        AggiungiCompositoMovimento("w", "s", "a", "d");
        AggiungiCompositoMovimento(
            "upArrow",
            "downArrow",
            "leftArrow",
            "rightArrow"
        );

        azioneMiraGamepad = gameplay.AddAction(
            "MiraGamepad",
            InputActionType.Value
        );
        azioneMiraGamepad.expectedControlType = "Vector2";
        azioneMiraGamepad.AddBinding("<Gamepad>/rightStick")
            .WithProcessor("stickDeadzone(min=0.15)");

        azionePuntatore = gameplay.AddAction(
            "Puntatore",
            InputActionType.PassThrough
        );
        azionePuntatore.expectedControlType = "Vector2";
        azionePuntatore.AddBinding("<Pointer>/position");

        azioneFuoco = gameplay.AddAction(
            "Fuoco",
            InputActionType.Button
        );
        azioneFuoco.AddBinding("<Mouse>/leftButton");
        azioneFuoco.AddBinding("<Gamepad>/rightTrigger");
        azioneFuoco.AddBinding("<Gamepad>/rightShoulder");

        azioneSchivata = gameplay.AddAction(
            "Schivata",
            InputActionType.Button
        );
        azioneSchivata.AddBinding("<Keyboard>/space");
        azioneSchivata.AddBinding("<Keyboard>/leftShift");
        azioneSchivata.AddBinding("<Gamepad>/buttonSouth");

        azionePausa = gameplay.AddAction(
            "Pausa",
            InputActionType.Button
        );
        azionePausa.AddBinding("<Keyboard>/escape");
        azionePausa.AddBinding("<Gamepad>/start");

        azioneRiprova = gameplay.AddAction(
            "Riprova",
            InputActionType.Button
        );
        azioneRiprova.AddBinding("<Keyboard>/r");
        azioneRiprova.AddBinding("<Gamepad>/buttonSouth");

        azioneDebugFeedback = gameplay.AddAction(
            "DebugFeedback",
            InputActionType.Button,
            "<Keyboard>/f4"
        );
        azioneDebugOndata = gameplay.AddAction(
            "DebugOndata",
            InputActionType.Button,
            "<Keyboard>/f3"
        );

        interfaccia = new InputActionMap("Interfaccia");
        azioneConferma = interfaccia.AddAction(
            "Conferma",
            InputActionType.Button
        );
        azioneConferma.AddBinding("<Keyboard>/enter");
        azioneConferma.AddBinding("<Keyboard>/numpadEnter");
        azioneConferma.AddBinding("<Keyboard>/space");
        azioneConferma.AddBinding("<Gamepad>/buttonSouth");

        azioneAnnulla = interfaccia.AddAction(
            "Annulla",
            InputActionType.Button
        );
        azioneAnnulla.AddBinding("<Keyboard>/escape");
        azioneAnnulla.AddBinding("<Gamepad>/buttonEast");

        azioneSceltaUno = CreaSceltaRapida(
            "SceltaUno",
            "<Keyboard>/digit1",
            "<Keyboard>/numpad1",
            "<Gamepad>/dpad/left"
        );
        azioneSceltaDue = CreaSceltaRapida(
            "SceltaDue",
            "<Keyboard>/digit2",
            "<Keyboard>/numpad2",
            "<Gamepad>/dpad/up"
        );
        azioneSceltaTre = CreaSceltaRapida(
            "SceltaTre",
            "<Keyboard>/digit3",
            "<Keyboard>/numpad3",
            "<Gamepad>/dpad/right"
        );

        SottoscriviRilevamentoDispositivo(gameplay);
        SottoscriviRilevamentoDispositivo(interfaccia);
    }

    private void AggiungiCompositoMovimento(
        string su,
        string giu,
        string sinistra,
        string destra
    )
    {
        azioneMovimento.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/" + su)
            .With("Down", "<Keyboard>/" + giu)
            .With("Left", "<Keyboard>/" + sinistra)
            .With("Right", "<Keyboard>/" + destra);
    }

    private InputAction CreaSceltaRapida(
        string nome,
        string tastiera,
        string tastierino,
        string gamepad
    )
    {
        InputAction azione = interfaccia.AddAction(
            nome,
            InputActionType.Button
        );
        azione.AddBinding(tastiera);
        azione.AddBinding(tastierino);
        azione.AddBinding(gamepad);
        return azione;
    }

    private void SottoscriviRilevamentoDispositivo(InputActionMap mappa)
    {
        foreach (InputAction azione in mappa.actions)
        {
            azione.performed += RegistraDispositivo;
        }
    }

    private void RegistraDispositivo(InputAction.CallbackContext contesto)
    {
        InputDevice dispositivo = contesto.control?.device;
        if (dispositivo is Gamepad)
        {
            ImpostaSchema(SchemaInputContadino.Gamepad);
        }
        else if (dispositivo is Touchscreen)
        {
            ImpostaSchema(SchemaInputContadino.Touch);
        }
        else if (dispositivo is Keyboard || dispositivo is Mouse ||
                 dispositivo is Pointer)
        {
            ImpostaSchema(SchemaInputContadino.TastieraMouse);
        }
    }

    private void ImpostaSchema(SchemaInputContadino schema)
    {
        if (SchemaCorrente == schema) return;
        SchemaCorrente = schema;
        SchemaCambiato?.Invoke(schema);
    }

    private static bool MappaPossiedeBinding(
        InputActionMap mappa,
        string nomeAzione,
        string percorso
    )
    {
        InputAction azione = mappa?.FindAction(nomeAzione, false);
        if (azione == null) return false;
        foreach (InputBinding binding in azione.bindings)
        {
            if (string.Equals(
                binding.path,
                percorso,
                StringComparison.OrdinalIgnoreCase
            ))
            {
                return true;
            }
        }
        return false;
    }

    private static string NomeControlloTastiera(KeyCode tasto)
    {
        switch (tasto)
        {
            case KeyCode.F1: return "f1";
            case KeyCode.F2: return "f2";
            case KeyCode.F3: return "f3";
            case KeyCode.F4: return "f4";
            case KeyCode.F5: return "f5";
            case KeyCode.F6: return "f6";
            case KeyCode.F7: return "f7";
            case KeyCode.F8: return "f8";
            case KeyCode.F9: return "f9";
            case KeyCode.F10: return "f10";
            case KeyCode.F11: return "f11";
            case KeyCode.F12: return "f12";
            default: return "f3";
        }
    }

    private static bool InterfacciaModaleAttiva()
    {
        if (MenuInizialeController.Attivo ||
            ShopPermanentePrePartita.ApertoGlobale ||
            (PauseSettingsMenu.Instance != null &&
             PauseSettingsMenu.Instance.Aperto))
        {
            return true;
        }

        GameManager gestore = GameManager.instance;
        return gestore != null && !gestore.GameplayAttivo;
    }
}
