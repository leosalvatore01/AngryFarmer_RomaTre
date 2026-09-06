using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public sealed class InputAndDodgeTests
{
    private GameObject oggettoInput;
    private GameObject giocatore;
    private GameObject interfaccia;
    private GameManager gameManagerPrecedente;
    private Gamepad gamepadVirtuale;
    private InputSettings.UpdateMode modalitaInputPrecedente;
    private bool modalitaInputModificata;
    private bool aggiornamentiGiocatoreInEditMode;

    [SetUp]
    public void SetUp()
    {
        gameManagerPrecedente = GameManager.instance;
        GameManager.instance = null;
        if (FarmerInputController.Instance != null)
        {
            Object.DestroyImmediate(
                FarmerInputController.Instance.gameObject
            );
        }
    }

    [TearDown]
    public void TearDown()
    {
        GameManager.instance = gameManagerPrecedente;
        if (gamepadVirtuale != null)
        {
            InputSystem.RemoveDevice(gamepadVirtuale);
            gamepadVirtuale = null;
        }
        if (modalitaInputModificata)
        {
            InputSystem.settings.updateMode = modalitaInputPrecedente;
            modalitaInputModificata = false;
        }
        if (aggiornamentiGiocatoreInEditMode)
        {
            InputSystem.settings.SetInternalFeatureFlag(
                "RUN_PLAYER_UPDATES_IN_EDIT_MODE",
                false
            );
            aggiornamentiGiocatoreInEditMode = false;
        }
        if (giocatore != null) Object.DestroyImmediate(giocatore);
        if (interfaccia != null) Object.DestroyImmediate(interfaccia);
        if (oggettoInput != null) Object.DestroyImmediate(oggettoInput);
        if (FarmerInputController.Instance != null)
        {
            Object.DestroyImmediate(
                FarmerInputController.Instance.gameObject
            );
        }
    }

    [Test]
    public void InputCentralizzato_ContieneTutteLeAzioniRichieste()
    {
        FarmerInputController input = CreaInput();

        Assert.That(input.NumeroAzioniGameplay, Is.EqualTo(9));
        Assert.That(input.NumeroAzioniInterfaccia, Is.EqualTo(5));
        Assert.That(input.PossiedeBinding(
            "Movimento", "<Keyboard>/w"), Is.True);
        Assert.That(input.PossiedeBinding(
            "Movimento", "<Gamepad>/leftStick"), Is.True);
        Assert.That(input.PossiedeBinding(
            "MiraGamepad", "<Gamepad>/rightStick"), Is.True);
        Assert.That(input.PossiedeBinding(
            "Fuoco", "<Mouse>/leftButton"), Is.True);
        Assert.That(input.PossiedeBinding(
            "Fuoco", "<Gamepad>/rightTrigger"), Is.True);
        Assert.That(input.PossiedeBinding(
            "Schivata", "<Keyboard>/space"), Is.True);
        Assert.That(input.PossiedeBinding(
            "Schivata", "<Gamepad>/buttonSouth"), Is.True);
        Assert.That(input.PossiedeBinding(
            "Pausa", "<Gamepad>/start"), Is.True);
        Assert.That(input.PossiedeBinding(
            "Conferma", "<Gamepad>/buttonSouth"), Is.True);
        Assert.That(input.PossiedeBinding(
            "Annulla", "<Gamepad>/buttonEast"), Is.True);
    }

    [Test]
    public void Schivata_AttivaMovimentoInvulnerabilitaECooldown()
    {
        PlayerMovement movimento = CreaGiocatore();
        PlayerHealth salute = giocatore.GetComponent<PlayerHealth>();

        bool avviata = movimento.ProvaSchivata(Vector2.right);

        Assert.That(avviata, Is.True);
        Assert.That(movimento.SchivataAttiva, Is.True);
        Assert.That(movimento.SchivataPronta, Is.False);
        Assert.That(movimento.CooldownSchivataRimasto, Is.GreaterThan(0f));
        Assert.That(movimento.ProgressoCooldownSchivata, Is.LessThan(0.1f));
        Assert.That(movimento.DirezioneUltimaSchivata, Is.EqualTo(Vector2.right));
        Assert.That(salute.Invulnerabile, Is.True);
        Assert.That(
            giocatore.GetComponent<PlayerDodgeFeedback>().Visibile,
            Is.True
        );
        Assert.That(
            movimento.ProvaSchivata(Vector2.up),
            Is.False,
            "Il cooldown deve impedire una seconda schivata immediata."
        );
    }

    [Test]
    public void InputCentralizzato_LeggeDavveroStickGrillettoESchivataGamepad()
    {
        modalitaInputPrecedente = InputSystem.settings.updateMode;
        modalitaInputModificata = true;
        InputSystem.settings.updateMode =
            InputSettings.UpdateMode.ProcessEventsManually;
        InputSystem.settings.SetInternalFeatureFlag(
            "RUN_PLAYER_UPDATES_IN_EDIT_MODE",
            true
        );
        aggiornamentiGiocatoreInEditMode = true;
        gamepadVirtuale = InputSystem.AddDevice<Gamepad>();
        FarmerInputController input = CreaInput();
        RichiamaMetodo(input, "OnEnable");
        InputSystem.QueueStateEvent(
            gamepadVirtuale,
            new GamepadState().WithButton(GamepadButton.South)
        );
        InputSystem.QueueDeltaStateEvent(
            gamepadVirtuale.leftStick,
            new Vector2(0.6f, 0.8f)
        );
        InputSystem.QueueDeltaStateEvent(
            gamepadVirtuale.rightStick,
            Vector2.left
        );
        InputSystem.QueueDeltaStateEvent(
            gamepadVirtuale.rightTrigger,
            1f
        );
        InputSystem.Update();

        Assert.That(input.AzioniAbilitate, Is.True);
        Assert.That(
            gamepadVirtuale.leftStick.ReadValue().x,
            Is.EqualTo(0.6f).Within(0.01f)
        );
        Assert.That(input.Movimento.x, Is.EqualTo(0.6f).Within(0.03f));
        Assert.That(input.Movimento.y, Is.EqualTo(0.8f).Within(0.03f));
        Assert.That(input.MiraGamepad.x, Is.LessThan(-0.95f));
        Assert.That(input.FuocoPremuto, Is.True);
        Assert.That(input.SchivataPremutaQuestoFrame, Is.True);
        Assert.That(
            input.SchemaCorrente,
            Is.EqualTo(SchemaInputContadino.Gamepad)
        );
    }

    [Test]
    public void Schivata_SenzaMovimentoUsaLaDirezionePredefinita()
    {
        PlayerMovement movimento = CreaGiocatore();

        Assert.That(movimento.ProvaSchivata(Vector2.zero), Is.True);
        Assert.That(
            movimento.DirezioneUltimaSchivata.sqrMagnitude,
            Is.EqualTo(1f).Within(0.001f)
        );
    }

    [Test]
    public void HudSchivata_VieneCreatoUnaSolaVoltaSullInterfaccia()
    {
        interfaccia = new GameObject("Interfaccia");
        interfaccia.AddComponent<Canvas>();

        DodgeHudController primo = DodgeHudController.CreaOTrova();
        RichiamaMetodo(primo, "Awake");
        DodgeHudController secondo = DodgeHudController.CreaOTrova();

        Assert.That(primo, Is.Not.Null);
        Assert.That(secondo, Is.SameAs(primo));
        Assert.That(
            primo.transform.Find("IconaSchivata"),
            Is.Not.Null
        );
        Assert.That(primo.GetComponent<CanvasGroup>(), Is.Not.Null);
    }

    [Test]
    public void ComponentiDiGioco_NonLeggonoPiuInputLegacyDirettamente()
    {
        string[] fileDaControllare =
        {
            "PlayerMovement.cs",
            "PlayerShooting.cs",
            "GameManager.cs",
            "MenuInizialeController.cs",
            "PauseSettingsMenu.cs",
            "ShopInterOndata.cs",
            "ShopPermanentePrePartita.cs",
            "CombatFeedbackController.cs",
            "WaveRuntimeDiagnostics.cs"
        };

        for (int i = 0; i < fileDaControllare.Length; i++)
        {
            string percorso = Path.Combine(
                Application.dataPath,
                "_Scripts",
                fileDaControllare[i]
            );
            string sorgente = File.ReadAllText(percorso);
            StringAssert.DoesNotContain(
                "Input.Get",
                sorgente,
                fileDaControllare[i]
            );
            StringAssert.DoesNotContain(
                "Input.mousePosition",
                sorgente,
                fileDaControllare[i]
            );
        }
    }

    private FarmerInputController CreaInput()
    {
        oggettoInput = new GameObject("InputCentralizzato_Test");
        FarmerInputController input =
            oggettoInput.AddComponent<FarmerInputController>();
        RichiamaMetodo(input, "Awake");
        return input;
    }

    private PlayerMovement CreaGiocatore()
    {
        giocatore = new GameObject("Contadino_Schivata_Test");
        Rigidbody2D corpo = giocatore.AddComponent<Rigidbody2D>();
        corpo.bodyType = RigidbodyType2D.Kinematic;
        PlayerHealth salute = giocatore.AddComponent<PlayerHealth>();
        PlayerMovement movimento = giocatore.AddComponent<PlayerMovement>();
        RichiamaMetodo(salute, "Awake");
        RichiamaMetodo(movimento, "Awake");
        return movimento;
    }

    private static void RichiamaMetodo(object componente, string nome)
    {
        componente.GetType().GetMethod(
            nome,
            BindingFlags.Instance | BindingFlags.NonPublic
        ).Invoke(componente, null);
    }
}
