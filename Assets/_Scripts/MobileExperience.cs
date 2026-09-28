using UnityEngine;

/// <summary>Una sola decisione di piattaforma per input, HUD e menu.</summary>
public static class MobileExperience
{
    public const string ChiaveAnteprima = "AngryFarmer.AnteprimaMobile";
    public static bool Attiva
    {
        get
        {
#if UNITY_EDITOR
            return UnityEditor.SessionState.GetBool(ChiaveAnteprima, false);
#else
            return Application.isMobilePlatform;
#endif
        }
    }

    public static bool GameplayTouchAttivo => Attiva &&
        GameManager.instance != null && GameManager.instance.GameplayAttivo &&
        !MenuInizialeController.Attivo && !ShopPermanentePrePartita.ApertoGlobale &&
        !(PauseSettingsMenu.Instance != null && PauseSettingsMenu.Instance.Aperto);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Inizializza()
    {
        if (!Attiva) return;
        if (Application.isMobilePlatform)
        {
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = false;
            Screen.orientation = ScreenOrientation.LandscapeLeft;
        }
        var radice = new GameObject("EsperienzaMobile");
        Object.DontDestroyOnLoad(radice);
        radice.AddComponent<MobileControls>();
    }
}
